using BenchmarkDotNet.Running;
using Chd.Coordination.Benchmark;
using System.Diagnostics;
using System.Linq;

Console.WriteLine("=======================================================");
Console.WriteLine("  Chd.Coordination - Performance Benchmarks");
Console.WriteLine("  BenchmarkDotNet - Micro Performance Analysis");
Console.WriteLine("=======================================================");
Console.WriteLine();

// Auto-setup: Check and start Redis if needed
await AutoSetupRedisAsync();

try
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("📋 Available Options:");
        Console.WriteLine("  0. Test Connection        - Verify Redis and API compatibility");
        Console.WriteLine("  1. Lock Benchmarks        - Compare Chd vs alternatives (Lock)");
        Console.WriteLine("  2. Idempotency Benchmarks - Compare Chd vs alternatives (Idempotency)");
        Console.WriteLine("  3. Saga Benchmarks        - Compare Chd vs alternatives (Saga)");
        Console.WriteLine("  4. Exit                   - Close application");
        Console.WriteLine();
        Console.WriteLine("⚠️  Prerequisites:");
        Console.WriteLine("  • Redis must be running (localhost:6379)");
        Console.WriteLine("  • Docker: docker run -d -p 6379:6379 redis:7-alpine");
        Console.WriteLine();
        Console.Write("Select option (0-7): ");

        var choice = Console.ReadLine();

        Console.WriteLine();

            try
            {
                switch (choice)
                {
                case "0":
                    await TestConnection.RunAsync();
                    break;
                case "1":
                    // Run AlternativesBenchmark but filter to lock-related methods
                    await RunBenchmarkAsync("Lock Comparison", () => BenchmarkRunner.Run<AlternativesBenchmark>(), method => method.Contains("Lock", StringComparison.OrdinalIgnoreCase));
                    break;
                case "2":
                    // Run AlternativesBenchmark but filter to idempotency-related methods
                    await RunBenchmarkAsync("Idempotency Comparison", () => BenchmarkRunner.Run<AlternativesBenchmark>(), method => method.Contains("Idempotency", StringComparison.OrdinalIgnoreCase));
                    break;
                case "3":
                    // Run AlternativesBenchmark but filter to saga-related methods
                    await RunBenchmarkAsync("Saga Comparison", () => BenchmarkRunner.Run<AlternativesBenchmark>(), method => method.Contains("Saga", StringComparison.OrdinalIgnoreCase));
                    break;
                case "4":
                    Console.WriteLine("👋 Exiting...");
                    return;
                default:
                    Console.WriteLine("❌ Invalid option. Please try again.");
                    break;
                }
            }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine("💡 Make sure Redis is running and accessible");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}
finally
{
    await BenchmarkHelper.CloseSharedRedisAsync();
    Console.WriteLine();
    Console.WriteLine("✅ Resources cleaned up");
}

static async Task RunBenchmarkAsync(string name, Func<object?> runBenchmark, Func<string, bool>? filter = null)
{
    Console.WriteLine($"▶️  Running {name}...");
    Console.WriteLine();

    // Run benchmark and get Summary for post-processing
    var summary = await Task.Run(runBenchmark);

    Console.WriteLine();
    Console.WriteLine($"✅ {name} completed!");

    // Print a compact comparison including "times faster" relative to the fastest method
    try
    {
        PrintComparisonWithSpeed(summary, filter);
    }
    catch
    {
        // Best-effort: don't fail the whole program if printing fails
    }
}

static void PrintComparisonWithSpeed(object? summaryObj, Func<string, bool>? filter = null)
{
    if (summaryObj == null)
        return;

    // Try to read .Reports property via reflection (Summary type may be in a package-specific namespace)
    var summaryType = summaryObj.GetType();
    var reportsProp = summaryType.GetProperty("Reports");
    if (reportsProp == null) return;

    var reports = reportsProp.GetValue(summaryObj) as System.Collections.IEnumerable;
    if (reports == null) return;

    var rows = new List<(string Method, double MeanMs)>();

    foreach (var report in reports)
    {
        if (report == null) continue;

        var reportType = report.GetType();

        var statsProp = reportType.GetProperty("ResultStatistics");
        var stats = statsProp?.GetValue(report);
        if (stats == null) continue;

        var meanProp = stats.GetType().GetProperty("Mean");
        if (meanProp == null) continue;

        var meanObj = meanProp.GetValue(stats);
        if (meanObj == null) continue;

        double meanNs;
        try { meanNs = Convert.ToDouble(meanObj); } catch { continue; }

        // Try to get method/display name
        string methodName = "Unknown";
        var benchmarkCaseProp = reportType.GetProperty("BenchmarkCase");
        var benchmarkCase = benchmarkCaseProp?.GetValue(report);
        if (benchmarkCase != null)
        {
            var descriptorProp = benchmarkCase.GetType().GetProperty("Descriptor");
            var descriptor = descriptorProp?.GetValue(benchmarkCase);
            var displayInfoProp = benchmarkCase.GetType().GetProperty("DisplayInfo");

            string? displayInfo = displayInfoProp?.GetValue(benchmarkCase) as string;

            if (descriptor != null)
            {
                var workloadMethodProp = descriptor.GetType().GetProperty("WorkloadMethod");
                var workloadMethod = workloadMethodProp?.GetValue(descriptor);
                var nameProp = workloadMethod?.GetType().GetProperty("Name");
                var name = nameProp?.GetValue(workloadMethod) as string;
                if (!string.IsNullOrEmpty(name)) methodName = name;
            }

            if (methodName == "Unknown" && !string.IsNullOrEmpty(displayInfo))
                methodName = displayInfo!;
        }

        var meanMs = meanNs / 1_000_000.0;
        if (filter == null || filter(methodName))
            rows.Add((methodName, meanMs));
    }

    if (rows.Count == 0) return;

    var fastest = rows.Min(r => r.MeanMs);

    Console.WriteLine();
    Console.WriteLine("📊 Comparison Summary (includes how many times slower than fastest)");
    Console.WriteLine();
    Console.WriteLine("| Method | Mean (ms) | Times Slower |");
    Console.WriteLine("|-------:|----------:|-------------:|");

    foreach (var row in rows.OrderBy(r => r.MeanMs))
    {
        var timesSlower = row.MeanMs / fastest;
        Console.WriteLine($"| {row.Method} | {row.MeanMs:F3} | {timesSlower:F2}x |");
    }

    Console.WriteLine();
}

static void ShowResults()
{
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════════════════");
    Console.WriteLine("  📊 All Benchmarks Completed!");
    Console.WriteLine("═══════════════════════════════════════════════════");
    Console.WriteLine();
    Console.WriteLine("📁 Results Location:");
    Console.WriteLine("   ./BenchmarkDotNet.Artifacts/results/");
    Console.WriteLine();
    Console.WriteLine("📄 Report Files:");
    Console.WriteLine("   • HTML Reports  - Human-readable analysis");
    Console.WriteLine("   • MD Reports    - GitHub-compatible markdown");
    Console.WriteLine("   • CSV Files     - Excel/data analysis");
    Console.WriteLine();
    Console.WriteLine("💡 Tip: Open HTML reports in your browser for best visualization");
    Console.WriteLine();
}

static async Task AutoSetupRedisAsync()
{
    Console.WriteLine("🔍 Checking prerequisites...");
    Console.WriteLine();

    // Check if Redis is already running
    if (await IsRedisRunningAsync())
    {
        Console.WriteLine("✅ Redis is running");
        Console.WriteLine();
        return;
    }

    Console.WriteLine("⚠️  Redis is not running");
    Console.WriteLine();

    // Check if Docker is installed
    if (!IsDockerInstalled())
    {
        Console.WriteLine("❌ Docker is not installed");
        Console.WriteLine();
        Console.WriteLine("📦 Please install Docker:");
        Console.WriteLine("   • Windows/Mac: https://www.docker.com/products/docker-desktop");
        Console.WriteLine("   • Linux: sudo apt install docker.io");
        Console.WriteLine();
        Console.WriteLine("Or start Redis manually:");
        Console.WriteLine("   docker run -d -p 6379:6379 redis:7-alpine");
        Console.WriteLine();
        Console.Write("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
        return;
    }

    Console.WriteLine("✅ Docker is installed");
    Console.WriteLine();

    // Try to start Redis automatically
    Console.WriteLine("🚀 Starting Redis automatically...");

    if (await StartRedisDockerAsync())
    {
        Console.WriteLine("✅ Redis started successfully!");
        Console.WriteLine("   Container: chd-benchmark-redis");
        Console.WriteLine("   Port: 6379");
        Console.WriteLine();

        // Wait for Redis to be ready
        Console.Write("⏳ Waiting for Redis to be ready");
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(500);
            Console.Write(".");
            if (await IsRedisRunningAsync())
            {
                Console.WriteLine(" ✅");
                Console.WriteLine();
                return;
            }
        }
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine("⚠️  Could not start Redis automatically");
        Console.WriteLine();
        Console.WriteLine("💡 Please start Redis manually:");
        Console.WriteLine("   docker run -d -p 6379:6379 --name chd-benchmark-redis redis:7-alpine");
        Console.WriteLine();
        Console.Write("Press any key to exit...");
        Console.ReadKey();
        Environment.Exit(1);
    }
}

static async Task<bool> IsRedisRunningAsync()
{
    try
    {
        using var connection = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync("localhost:6379,connectTimeout=2000,abortConnect=false");
        var db = connection.GetDatabase();
        await db.PingAsync();
        return true;
    }
    catch
    {
        return false;
    }
}

static bool IsDockerInstalled()
{
    try
    {
        var psi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "--version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi);
        if (process == null) return false;

        process.WaitForExit();
        return process.ExitCode == 0;
    }
    catch
    {
        return false;
    }
}

static async Task<bool> StartRedisDockerAsync()
{
    try
    {
        // Check if container already exists (stopped)
        var checkPsi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "ps -a --filter name=chd-benchmark-redis --format {{.Names}}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var checkProcess = Process.Start(checkPsi);
        if (checkProcess != null)
        {
            var output = await checkProcess.StandardOutput.ReadToEndAsync();
            await checkProcess.WaitForExitAsync();

            if (output.Contains("chd-benchmark-redis"))
            {
                // Container exists, just start it
                var startPsi = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "start chd-benchmark-redis",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var startProcess = Process.Start(startPsi);
                if (startProcess != null)
                {
                    await startProcess.WaitForExitAsync();
                    return startProcess.ExitCode == 0;
                }
            }
        }

        // Container doesn't exist, create new one
        var createPsi = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "run -d -p 6379:6379 --name chd-benchmark-redis redis:7-alpine",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var createProcess = Process.Start(createPsi);
        if (createProcess == null) return false;

        await createProcess.WaitForExitAsync();
        return createProcess.ExitCode == 0;
    }
    catch
    {
        return false;
    }
}
