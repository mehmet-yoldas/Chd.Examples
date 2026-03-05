using System.Diagnostics;

namespace Chd.LoadTests;

public static class MonitoringHelper
{
    public static bool IsMonitoringStackRunning()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "ps --filter name=chd-grafana --format {{.Names}}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output.Contains("chd-grafana");
        }
        catch
        {
            return false;
        }
    }

    public static bool IsDockerRunning()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "ps",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool StartMonitoringStack(string solutionDirectory)
    {
        try
        {
            Console.WriteLine("[INFO] Starting monitoring stack...");
            Console.WriteLine("       This may take 10-15 seconds...");
            Console.WriteLine();

            var composeFile = Path.Combine(solutionDirectory, "docker-compose.monitoring.yml");

            if (!File.Exists(composeFile))
            {
                Console.WriteLine($"[ERROR] docker-compose.monitoring.yml not found at: {composeFile}");
                return false;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = $"-f \"{composeFile}\" up -d",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("[SUCCESS] Monitoring stack started!");
                Console.WriteLine("          - Grafana:    http://localhost:3000 (admin/admin)");
                Console.WriteLine("          - Prometheus: http://localhost:9090");
                Console.WriteLine("          - Dashboard:  http://localhost:3000/d/chd-coordination");
                Console.WriteLine();
                
                // Wait for services to be ready
                Console.WriteLine("[INFO] Waiting for services to be ready...");
                Thread.Sleep(10000);
                Console.WriteLine("[INFO] Services should be ready now!");
                Console.WriteLine();
                
                return true;
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to start monitoring stack:");
                Console.WriteLine(error);
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception while starting monitoring stack: {ex.Message}");
            return false;
        }
    }

    public static void OpenGrafanaDashboard()
    {
        try
        {
            var url = "http://localhost:3000/d/chd-coordination";
            
            // Windows
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            // macOS
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", url);
            }
            // Linux
            else if (OperatingSystem.IsLinux())
            {
                Process.Start("xdg-open", url);
            }
            
            Console.WriteLine($"[INFO] Opening Grafana dashboard in browser...");
            Console.WriteLine($"       URL: {url}");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Could not open browser: {ex.Message}");
            Console.WriteLine("          Please open manually: http://localhost:3000/d/chd-coordination");
            Console.WriteLine();
        }
    }

    public static string GetSolutionDirectory()
    {
        // Start from current directory and go up to find solution
        var currentDir = Directory.GetCurrentDirectory();
        var dir = new DirectoryInfo(currentDir);

        while (dir != null)
        {
            // Check if docker-compose.monitoring.yml exists
            if (File.Exists(Path.Combine(dir.FullName, "docker-compose.monitoring.yml")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        // Fallback: assume we're in Chd.LoadTests, so go up one level
        return Path.GetFullPath(Path.Combine(currentDir, ".."));
    }

    public static bool AreDockerImagesDownloaded()
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "images --format {{.Repository}}:{{.Tag}}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Check if required images exist
            var hasRedis = output.Contains("redis:7-alpine") || output.Contains("redis:7");
            var hasPrometheus = output.Contains("prom/prometheus");
            var hasGrafana = output.Contains("grafana/grafana");

            return hasRedis && hasPrometheus && hasGrafana;
        }
        catch
        {
            return false;
        }
    }

    public static List<string> GetMissingImages()
    {
        var missing = new List<string>();

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "images --format {{.Repository}}:{{.Tag}}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (!output.Contains("redis:7")) missing.Add("Redis 7");
            if (!output.Contains("prom/prometheus")) missing.Add("Prometheus");
            if (!output.Contains("grafana/grafana")) missing.Add("Grafana");
            if (!output.Contains("prom/node-exporter")) missing.Add("Node Exporter");
        }
        catch
        {
            missing.AddRange(new[] { "Redis 7", "Prometheus", "Grafana", "Node Exporter" });
        }

        return missing;
    }

    public static bool StartMonitoringStackWithProgress(string solutionDirectory)
    {
        try
        {
            Console.WriteLine("📥 [FIRST-TIME SETUP] Downloading Docker images...");
            Console.WriteLine();
            Console.WriteLine("   This will download:");
            Console.WriteLine("   • Redis 7 Alpine (~10 MB)");
            Console.WriteLine("   • Prometheus Latest (~100 MB)");
            Console.WriteLine("   • Grafana Latest (~150 MB)");
            Console.WriteLine("   • Node Exporter (~10 MB)");
            Console.WriteLine();
            Console.WriteLine("   Estimated time: 2-5 minutes (depending on your connection)");
            Console.WriteLine("   Images will be cached for future use.");
            Console.WriteLine();
            Console.WriteLine("   Starting download...");
            Console.WriteLine();

            var composeFile = Path.Combine(solutionDirectory, "docker-compose.monitoring.yml");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker-compose",
                    Arguments = $"-f \"{composeFile}\" up -d",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = false // Show output so user sees progress
                }
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Console.WriteLine();
                Console.WriteLine("✅ [SETUP COMPLETE] All images downloaded and cached!");
                Console.WriteLine("   Future startups will be instant.");
                Console.WriteLine();
                Console.WriteLine("   Services started:");
                Console.WriteLine("   ✅ Redis - Port 6379");
                Console.WriteLine("   ✅ Prometheus - Port 9090");
                Console.WriteLine("   ✅ Grafana - Port 3000");
                Console.WriteLine("   ✅ Node Exporter - Port 9100");
                Console.WriteLine();

                // Wait for services to be ready
                Console.WriteLine("   Waiting for services to initialize (10 seconds)...");
                Thread.Sleep(10000);
                Console.WriteLine("   ✅ All services ready!");
                Console.WriteLine();

                return true;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("❌ [ERROR] Failed to start monitoring stack");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [ERROR] Exception: {ex.Message}");
            return false;
        }
    }
}
