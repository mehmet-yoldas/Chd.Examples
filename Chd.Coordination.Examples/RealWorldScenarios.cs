using Chd.Coordination.Abstractions;
using Chd.Coordination.Core;
using Microsoft.Extensions.Logging;

namespace Chd.Coordination.Examples;

/// <summary>
/// Gerçek dünya senaryolarını gösteren karmaşık örnekler
/// </summary>
public class RealWorldScenarios
{
    private readonly ICoordinator _coordinator;
    private readonly ILogger<RealWorldScenarios> _logger;

    public RealWorldScenarios(ICoordinator coordinator, ILogger<RealWorldScenarios> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task RunAllExamples()
    {
        Console.WriteLine("\n=== GERÇEK DÜNYA SENARYOLARI ===\n");

        await Scenario1_BankTransfer();
        await Scenario2_ConcurrentJobProcessing();
        await Scenario3_EventProcessing();
    }

    /// <summary>
    /// Senaryo 1: Banka havalesi (Saga + Lock + Idempotency)
    /// </summary>
    private async Task Scenario1_BankTransfer()
    {
        Console.WriteLine("💰 Senaryo 1: Banka Havalesi");
        Console.WriteLine("Özellikler: Distributed Lock, Saga, Idempotency\n");

        var transferId = Guid.NewGuid().ToString();
        var fromAccount = "ACC-001";
        var toAccount = "ACC-002";
        var amount = 1000m;

        var context = CoordinationContext.Create();
        _logger.LogInformation("Transfer ID: {TransferId}, Correlation: {CorrelationId}", 
            transferId, context.CorrelationId);

        try
        {
            // Idempotency ile aynı transferi birden fazla kez yapmayı engelle
            await _coordinator.Idempotency.RunAsync(
                key: $"transfer:{transferId}",
                ttl: TimeSpan.FromMinutes(10),
                async () =>
                {
                    // Saga ile adım adım işlem
                    await _coordinator.Saga.RunAsync($"transfer-saga-{transferId}", async saga =>
                    {
                        decimal withdrawnAmount = 0;
                        decimal depositedAmount = 0;

                        // Adım 1: Para çekme (Lock ile)
                        await saga.Step("withdraw", async () =>
                        {
                            await _coordinator.Lock.RunAsync(
                                key: $"account:lock:{fromAccount}",
                                ttl: TimeSpan.FromSeconds(10),
                                async ct =>
                                {
                                    _logger.LogInformation("1️⃣ {Account} hesabından {Amount:C} çekiliyor...", 
                                        fromAccount, amount);
                                    await Task.Delay(500, ct);
                                    withdrawnAmount = amount;
                                    _logger.LogInformation("✅ Para çekildi");
                                });
                        });

                        // Adım 2: Para yatırma (Lock ile)
                        await saga.Step("deposit", async () =>
                        {
                            await _coordinator.Lock.RunAsync(
                                key: $"account:lock:{toAccount}",
                                ttl: TimeSpan.FromSeconds(10),
                                async ct =>
                                {
                                    _logger.LogInformation("2️⃣ {Account} hesabına {Amount:C} yatırılıyor...", 
                                        toAccount, amount);
                                    await Task.Delay(500, ct);
                                    depositedAmount = amount;
                                    _logger.LogInformation("✅ Para yatırıldı");
                                });
                        });

                        // Adım 3: İşlem kaydı
                        await saga.Step("record-transaction", async () =>
                        {
                            _logger.LogInformation("3️⃣ İşlem kaydediliyor...");
                            await Task.Delay(300);
                            _logger.LogInformation("✅ İşlem kaydedildi");
                        });
                    });

                    _logger.LogInformation("🎉 Transfer başarıyla tamamlandı!");
                });
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Transfer başarısız: {Message}", ex.Message);
        }

        Console.WriteLine("\n");
    }

    /// <summary>
    /// Senaryo 2: Concurrent job processing (Lock + Context)
    /// </summary>
    private async Task Scenario2_ConcurrentJobProcessing()
    {
        Console.WriteLine("⚙️ Senaryo 2: Concurrent Job Processing");
        Console.WriteLine("Özellikler: Distributed Lock, CoordinationContext\n");

        var jobQueue = new[] { "job-1", "job-2", "job-3", "job-4", "job-5" };
        var processedJobs = new List<string>();
        var lockObject = new object();

        var tasks = Enumerable.Range(1, 3).Select(workerId =>
            Task.Run(async () =>
            {
                foreach (var jobId in jobQueue)
                {
                    try
                    {
                        await _coordinator.Lock.RunAsync(
                            key: $"job:process:{jobId}",
                            ttl: TimeSpan.FromSeconds(5),
                            async ct =>
                            {
                                var context = CoordinationContext.Create();
                                _logger.LogInformation("Worker-{WorkerId} processing {JobId} [Correlation: {CorrelationId}]", 
                                    workerId, jobId, context.CorrelationId);

                                await Task.Delay(Random.Shared.Next(200, 500), ct);

                                lock (lockObject)
                                {
                                    processedJobs.Add(jobId);
                                }

                                _logger.LogInformation("✅ Worker-{WorkerId} completed {JobId}", workerId, jobId);
                            });
                        break; // Job processed, move to next
                    }
                    catch (Exception)
                    {
                        // Job is locked by another worker, try next job
                    }
                }
            }));

        await Task.WhenAll(tasks);

        Console.WriteLine($"\n📊 Toplam {processedJobs.Count} job işlendi");
        Console.WriteLine($"Jobs: {string.Join(", ", processedJobs)}\n");
    }

    /// <summary>
    /// Senaryo 3: Event processing with idempotency
    /// </summary>
    private async Task Scenario3_EventProcessing()
    {
        Console.WriteLine("📨 Senaryo 3: Event Processing");
        Console.WriteLine("Özellikler: Idempotency (duplicate event handling)\n");

        var events = new[]
        {
            new Event("evt-1", "OrderCreated", new { OrderId = "ORD-001" }),
            new Event("evt-1", "OrderCreated", new { OrderId = "ORD-001" }), // Duplicate
            new Event("evt-2", "OrderShipped", new { OrderId = "ORD-001" }),
            new Event("evt-2", "OrderShipped", new { OrderId = "ORD-001" }), // Duplicate
            new Event("evt-3", "OrderDelivered", new { OrderId = "ORD-001" }),
        };

        int processedCount = 0;
        int skippedCount = 0;

        foreach (var evt in events)
        {
            bool wasSkipped = false;
            try
            {
                await _coordinator.Idempotency.RunAsync(
                    key: $"event:process:{evt.Id}",
                    ttl: TimeSpan.FromHours(24),
                    async () =>
                    {
                        _logger.LogInformation("📬 Processing event: {EventType} (ID: {EventId})", 
                            evt.Type, evt.Id);
                        await Task.Delay(200);
                        processedCount++;
                    });
            }
            catch (Exception)
            {
                // If idempotency prevents execution, it might throw or just skip
                wasSkipped = true;
                skippedCount++;
                _logger.LogWarning("⚠️ Duplicate event ignored: {EventType} (ID: {EventId})", 
                    evt.Type, evt.Id);
            }
        }

        Console.WriteLine($"\n📊 İstatistikler:");
        Console.WriteLine($"  Toplam event: {events.Length}");
        Console.WriteLine($"  İşlenen: {processedCount}");
        Console.WriteLine($"  Atlanan (duplicate): {skippedCount}\n");
    }

    private record Event(string Id, string Type, object Data);
}
