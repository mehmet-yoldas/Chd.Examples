using NBomber.CSharp;
using NBomber.Plugins.Network.Ping;
using Chd.Coordination;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Chd.Coordination.Abstractions;

namespace Chd.LoadTests;

public class Program
{
    private static PrometheusServer? _prometheusServer;

    public static void Main(string[] args)
    {
        Console.WriteLine("===========================================");
        Console.WriteLine("CHD Coordination - Load Tests with NBomber");
        Console.WriteLine("===========================================");
        Console.WriteLine();

        // Check Docker and Monitoring Stack
        CheckAndStartMonitoring();

        // Start Prometheus metrics server
        _prometheusServer = new PrometheusServer(port: 9091);
        _prometheusServer.Start();
        Console.WriteLine("[INFO] Prometheus metrics available at: http://localhost:9091/metrics");
        Console.WriteLine();

        // Check for auto-select from launch settings or command line
        string? choice = null;

        if (args.Length > 0)
        {
            choice = args[0];
            Console.WriteLine($"[Auto-selected] Scenario {choice}");
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("Available scenarios:");
            Console.WriteLine("  1. Distributed Lock Performance Test");
            Console.WriteLine("  2. Idempotency Performance Test");
            Console.WriteLine("  3. Saga Performance Test");
            Console.WriteLine("  4. Mixed Workload Test");
            Console.WriteLine("  5. Run ALL Tests");
            Console.WriteLine();
            Console.Write("Select scenario (1-5): ");
            choice = Console.ReadLine();
        }

        try
        {
            PrometheusMetrics.ActiveTests.Inc();

            switch (choice)
            {
                case "1":
                    RunLockTest();
                    break;
                case "2":
                    RunIdempotencyTest();
                    break;
                case "3":
                    RunSagaTest();
                    break;
                case "4":
                    RunMixedTest();
                    break;
                case "5":
                    RunAllTests();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Exiting...");
                    break;
            }

            // Test completed - keep server running so user can check Grafana
            if (choice is "1" or "2" or "3" or "4" or "5")
            {
                Console.WriteLine();
                Console.WriteLine("========================================");
                Console.WriteLine("   ✅ Test Completed Successfully!");
                Console.WriteLine("========================================");
                Console.WriteLine();
                Console.WriteLine("📊 View your results:");
                Console.WriteLine("   • Grafana Dashboard: http://localhost:3000/d/chd-coordination");
                Console.WriteLine("   • Prometheus Metrics: http://localhost:9091/metrics");
                Console.WriteLine("   • NBomber HTML Report: Check the reports folder");
                Console.WriteLine();
                Console.WriteLine("💡 PrometheusServer is still running so you can explore metrics in Grafana.");
                Console.WriteLine("   Take your time to analyze the dashboard!");
                Console.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                Console.WriteLine();
            }
        }
        finally
        {
            PrometheusMetrics.ActiveTests.Dec();
            Console.WriteLine("\n[INFO] Shutting down PrometheusServer...");
            _prometheusServer?.Stop();
            Console.WriteLine("[INFO] Cleanup completed. Goodbye!");
        }
    }

    private static void RunLockTest()
    {
        Console.WriteLine("\n[Running] Distributed Lock Load Test...\n");

        var serviceProvider = CreateServiceProvider();

        var scenario = Scenario.Create("distributed_lock_test", async context =>
        {
            var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
            var resourceId = $"resource:{context.ScenarioInfo.ThreadNumber}";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await coordinator.Lock.RunAsync(
                    resourceId,
                    ttl: TimeSpan.FromSeconds(5),
                    action: async ct =>
                    {
                        await Task.Delay(50, ct); // Simulate work
                    }
                );

                stopwatch.Stop();

                // Record metrics
                PrometheusMetrics.LockOperations.WithLabels("success").Inc();
                PrometheusMetrics.LockLatency.Observe(stopwatch.Elapsed.TotalSeconds);
                PrometheusMetrics.OperationLatency.WithLabels("lock").Observe(stopwatch.Elapsed.TotalSeconds);

                return Response.Ok();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                PrometheusMetrics.LockOperations.WithLabels("failed").Inc();
                Console.WriteLine($"Lock failed: {ex.Message}");
                return Response.Fail();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.Inject(rate: 10, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(2)),
            Simulation.Inject(rate: 20, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(1))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Console.WriteLine("\n[Completed] Lock test finished. Check HTML report.\n");
    }

    private static void RunIdempotencyTest()
    {
        Console.WriteLine("\n[Running] Idempotency Load Test...\n");

        var serviceProvider = CreateServiceProvider();

        var scenario = Scenario.Create("idempotency_test", async context =>
        {
            var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
            var operationId = $"payment:{context.ScenarioInfo.ThreadNumber}";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await coordinator.Idempotency.RunAsync(
                    operationId,
                    ttl: TimeSpan.FromMinutes(5),
                    action: async () =>
                    {
                        await Task.Delay(30); // Simulate payment processing
                    }
                );

                stopwatch.Stop();

                // Record metrics
                PrometheusMetrics.IdempotencyOperations.WithLabels("success").Inc();
                PrometheusMetrics.IdempotencyLatency.Observe(stopwatch.Elapsed.TotalSeconds);
                PrometheusMetrics.OperationLatency.WithLabels("idempotency").Observe(stopwatch.Elapsed.TotalSeconds);

                return Response.Ok();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                PrometheusMetrics.IdempotencyOperations.WithLabels("failed").Inc();
                Console.WriteLine($"Idempotency failed: {ex.Message}");
                return Response.Fail();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.RampingInject(rate: 50, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(3))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Console.WriteLine("\n[Completed] Idempotency test finished. Check HTML report.\n");
    }

    private static void RunSagaTest()
    {
        Console.WriteLine("\n[Running] Saga Load Test...\n");

        var serviceProvider = CreateServiceProvider();

        var scenario = Scenario.Create("saga_test", async context =>
        {
            var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
            var sagaId = $"order:{context.ScenarioInfo.ThreadNumber}";

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await coordinator.Saga.RunAsync(sagaId, async saga =>
                {
                    await saga.Step("reserve-inventory", async () =>
                    {
                        await Task.Delay(20); // Simulate reserve inventory
                    });

                    await saga.Step("charge-payment", async () =>
                    {
                        await Task.Delay(30); // Simulate charge payment
                    });

                    await saga.Step("create-shipment", async () =>
                    {
                        await Task.Delay(25); // Simulate create shipment
                    });
                });

                stopwatch.Stop();

                // Record metrics
                PrometheusMetrics.SagaOperations.WithLabels("success").Inc();
                PrometheusMetrics.SagaLatency.Observe(stopwatch.Elapsed.TotalSeconds);
                PrometheusMetrics.OperationLatency.WithLabels("saga").Observe(stopwatch.Elapsed.TotalSeconds);

                return Response.Ok();
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                PrometheusMetrics.SagaOperations.WithLabels("failed").Inc();
                Console.WriteLine($"Saga failed: {ex.Message}");
                return Response.Fail();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(
            Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(2))
        );

        NBomberRunner
            .RegisterScenarios(scenario)
            .Run();

        Console.WriteLine("\n[Completed] Saga test finished. Check HTML report.\n");
    }

    private static void RunMixedTest()
    {
        Console.WriteLine("\n[Running] Mixed Workload Test...\n");

        var serviceProvider = CreateServiceProvider();

        var lockScenario = Scenario.Create("mixed_lock", async context =>
        {
            var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
            var resourceId = $"resource:{context.ScenarioInfo.ThreadNumber}";

            try
            {
                await coordinator.Lock.RunAsync(
                    resourceId,
                    ttl: TimeSpan.FromSeconds(5),
                    action: async ct => await Task.Delay(50, ct)
                );
                return Response.Ok();
            }
            catch
            {
                return Response.Fail();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.KeepConstant(copies: 5, during: TimeSpan.FromMinutes(3)));

        var idempotencyScenario = Scenario.Create("mixed_idempotency", async context =>
        {
            var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
            var operationId = $"payment:{context.ScenarioInfo.ThreadNumber}";

            try
            {
                await coordinator.Idempotency.RunAsync(
                    operationId,
                    ttl: TimeSpan.FromMinutes(5),
                    action: async () => await Task.Delay(30)
                );
                return Response.Ok();
            }
            catch
            {
                return Response.Fail();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.KeepConstant(copies: 8, during: TimeSpan.FromMinutes(3)));

        var sagaScenario = Scenario.Create("mixed_saga", async context =>
        {
            var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
            var sagaId = $"order:{context.ScenarioInfo.ThreadNumber}";

            try
            {
                await coordinator.Saga.RunAsync(sagaId, async saga =>
                {
                    await saga.Step("step1", async () => await Task.Delay(20));
                    await saga.Step("step2", async () => await Task.Delay(30));
                });
                return Response.Ok();
            }
            catch
            {
                return Response.Fail();
            }
        })
        .WithoutWarmUp()
        .WithLoadSimulations(Simulation.KeepConstant(copies: 3, during: TimeSpan.FromMinutes(3)));

        NBomberRunner
            .RegisterScenarios(lockScenario, idempotencyScenario, sagaScenario)
            .Run();

        Console.WriteLine("\n[Completed] Mixed workload test finished. Check HTML report.\n");
    }

    private static void RunAllTests()
    {
        Console.WriteLine("\n[Running] ALL Performance Tests (this may take ~10 minutes)...\n");

        RunLockTest();
        Thread.Sleep(2000);

        RunIdempotencyTest();
        Thread.Sleep(2000);

        RunSagaTest();
        Thread.Sleep(2000);

        RunMixedTest();

        Console.WriteLine("\n[Completed] All tests finished!\n");
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        // Add Chd.Coordination with Redis
        services.AddCoordination(opt =>
        {
            opt.RedisConnectionString = "localhost:6379";
        });

        return services.BuildServiceProvider();
    }

    private static void CheckAndStartMonitoring()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("   Monitoring Stack Check");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Check if Docker is running
        if (!MonitoringHelper.IsDockerRunning())
        {
            Console.WriteLine("❌ [REQUIRED] Docker Desktop is not running!");
            Console.WriteLine();
            Console.WriteLine("   📥 Please install and start Docker Desktop:");
            Console.WriteLine("   → Windows/Mac: https://www.docker.com/products/docker-desktop");
            Console.WriteLine("   → Linux: https://docs.docker.com/engine/install/");
            Console.WriteLine();
            Console.WriteLine("   ⚠️  Without Docker:");
            Console.WriteLine("   • Metrics will NOT be available");
            Console.WriteLine("   • Grafana dashboards will NOT work");
            Console.WriteLine("   • Only basic load tests will run");
            Console.WriteLine();

            Console.Write("Continue without monitoring? (y/n): ");
            var answer = Console.ReadLine();
            if (answer?.ToLower() != "y")
            {
                Console.WriteLine("Exiting... Please install Docker and try again.");
                Environment.Exit(0);
            }

            Console.WriteLine();
            Console.WriteLine("⚠️  [WARNING] Continuing without monitoring...");
            Console.WriteLine();
            return;
        }

        Console.WriteLine("✅ Docker Desktop is running");
        Console.WriteLine();

        // Check if monitoring stack is running
        if (MonitoringHelper.IsMonitoringStackRunning())
        {
            Console.WriteLine("✅ Monitoring stack is already running");
            Console.WriteLine();
            Console.WriteLine("📊 Metrics available at:");
            Console.WriteLine("   • Grafana:    http://localhost:3000 (admin/admin)");
            Console.WriteLine("   • Dashboard:  http://localhost:3000/d/chd-coordination");
            Console.WriteLine("   • Prometheus: http://localhost:9090");
            Console.WriteLine();

            // Ask if user wants to open Grafana
            Console.Write("Open Grafana dashboard in browser? (y/n): ");
            var openBrowser = Console.ReadLine();
            if (openBrowser?.ToLower() == "y")
            {
                MonitoringHelper.OpenGrafanaDashboard();
            }
        }
        else
        {
            // Check if Docker images are downloaded
            var missingImages = MonitoringHelper.GetMissingImages();

            if (missingImages.Count > 0)
            {
                Console.WriteLine("📦 [FIRST-TIME SETUP] Missing Docker images detected:");
                foreach (var image in missingImages)
                {
                    Console.WriteLine($"   ❌ {image}");
                }
                Console.WriteLine();
                Console.WriteLine("   These will be downloaded automatically.");
                Console.WriteLine("   Estimated time: 2-5 minutes");
                Console.WriteLine("   ✨ This only happens ONCE - future runs are instant!");
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("⚠️  Monitoring stack is not running");
                Console.WriteLine();
            }

            Console.Write("🚀 Start monitoring stack now? (y/n): ");
            var answer = Console.ReadLine();

            if (answer?.ToLower() == "y")
            {
                var solutionDir = MonitoringHelper.GetSolutionDirectory();
                Console.WriteLine($"   Solution directory: {solutionDir}");
                Console.WriteLine();

                bool success;
                if (missingImages.Count > 0)
                {
                    // First-time setup with progress
                    success = MonitoringHelper.StartMonitoringStackWithProgress(solutionDir);
                }
                else
                {
                    // Quick start (images already cached)
                    success = MonitoringHelper.StartMonitoringStack(solutionDir);
                }

                if (success)
                {
                    Console.WriteLine("✅ Monitoring stack started successfully!");
                    Console.WriteLine();
                    Console.WriteLine("📊 Access your metrics:");
                    Console.WriteLine("   • Grafana Dashboard: http://localhost:3000/d/chd-coordination");
                    Console.WriteLine("   • Login: admin / admin");
                    Console.WriteLine();

                    // Ask if user wants to open Grafana
                    Console.Write("Open Grafana dashboard in browser? (y/n): ");
                    var openBrowser = Console.ReadLine();
                    if (openBrowser?.ToLower() == "y")
                    {
                        MonitoringHelper.OpenGrafanaDashboard();
                    }
                }
                else
                {
                    Console.WriteLine("❌ Could not start monitoring stack automatically");
                    Console.WriteLine("   📖 Manual setup: run start-monitoring.bat");
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("ℹ️  Continuing without monitoring");
                Console.WriteLine("   💡 You can start it later:");
                Console.WriteLine("   • Run: start-monitoring.bat");
                Console.WriteLine("   • Or restart this program and select 'y'");
                Console.WriteLine();
            }
        }

        Console.WriteLine("========================================");
        Console.WriteLine();
    }
}
    
