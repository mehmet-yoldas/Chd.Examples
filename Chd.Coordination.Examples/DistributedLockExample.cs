using Chd.Coordination.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chd.Coordination.Examples;

/// <summary>
/// Distributed Lock özelliğinin kullanım örnekleri
/// </summary>
public class DistributedLockExample
{
    private readonly ICoordinator _coordinator;
    private readonly ILogger<DistributedLockExample> _logger;

    public DistributedLockExample(ICoordinator coordinator, ILogger<DistributedLockExample> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task RunAllExamples()
    {
        Console.WriteLine("\n=== DISTRIBUTED LOCK ÖRNEKLERİ ===\n");

        await Example1_BasicLock();
        await Example2_LockWithReturnValue();
        await Example3_CompetingLocks();
        await Example4_LockTimeout();
    }

    /// <summary>
    /// Örnek 1: Temel kilit kullanımı
    /// </summary>
    private async Task Example1_BasicLock()
    {
        Console.WriteLine("📝 Örnek 1: Temel Distributed Lock");
        Console.WriteLine("Senaryo: Kritik bir işlemi sadece bir sunucunun yapmasını sağlama\n");

        await _coordinator.Lock.RunAsync(
            key: "inventory:update:product-123",
            ttl: TimeSpan.FromSeconds(10),
            async ct =>
            {
                _logger.LogInformation("🔒 Kilit alındı - Stok güncelleniyor...");
                await Task.Delay(2000, ct); // Simüle edilmiş iş
                _logger.LogInformation("✅ Stok güncellendi");
            });

        Console.WriteLine("\n");
    }

    /// <summary>
    /// Örnek 2: Kilit ile kritik hesaplama
    /// </summary>
    private async Task Example2_LockWithReturnValue()
    {
        Console.WriteLine("📝 Örnek 2: Kilit ile Kritik Hesaplama");
        Console.WriteLine("Senaryo: Kritik hesaplama sırasında kilitleme\n");

        int result = 0;
        await _coordinator.Lock.RunAsync(
            key: "calculation:monthly-report",
            ttl: TimeSpan.FromSeconds(10),
            async ct =>
            {
                _logger.LogInformation("🔒 Kilit alındı - Aylık rapor hesaplanıyor...");
                await Task.Delay(1000, ct);

                result = Random.Shared.Next(10000, 99999);
                _logger.LogInformation("📊 Hesaplama tamamlandı: {Total}", result);
            });

        Console.WriteLine($"✅ Sonuç: {result}\n");
    }

    /// <summary>
    /// Örnek 3: Sıralı kilit alma
    /// </summary>
    private async Task Example3_CompetingLocks()
    {
        Console.WriteLine("📝 Örnek 3: Sıralı Kilit Alma");
        Console.WriteLine("Senaryo: Birden fazla işlemin sırayla aynı kaynağa erişmesi\n");

        var tasks = new List<Task>();
        var startTime = DateTime.Now;

        for (int i = 1; i <= 3; i++)
        {
            var serverId = i;
            tasks.Add(Task.Run(async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: "payment:process:order-456",
                    ttl: TimeSpan.FromSeconds(5),
                    async ct =>
                    {
                        var elapsed = (DateTime.Now - startTime).TotalSeconds;
                        _logger.LogInformation("🏆 Server-{ServerId} kilidi aldı! (+{Elapsed:F1}s)", serverId, elapsed);
                        await Task.Delay(1000, ct);
                        _logger.LogInformation("✅ Server-{ServerId} işlemi tamamladı", serverId);
                    });
            }));

            // Stagger the start times
            await Task.Delay(100);
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("\n");
    }

    /// <summary>
    /// Örnek 4: Farklı anahtarlarla paralel çalışma
    /// </summary>
    private async Task Example4_LockTimeout()
    {
        Console.WriteLine("📝 Örnek 4: Paralel Kilitleme");
        Console.WriteLine("Senaryo: Farklı kaynaklar için eşzamanlı kilitleme\n");

        var tasks = new List<Task>
        {
            Task.Run(async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: "resource:database-1",
                    ttl: TimeSpan.FromSeconds(3),
                    async ct =>
                    {
                        _logger.LogInformation("🔒 Database-1 kilidi alındı");
                        await Task.Delay(1000, ct);
                        _logger.LogInformation("✅ Database-1 işlemi tamamlandı");
                    });
            }),
            Task.Run(async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: "resource:cache-1",
                    ttl: TimeSpan.FromSeconds(3),
                    async ct =>
                    {
                        _logger.LogInformation("🔒 Cache-1 kilidi alındı");
                        await Task.Delay(1000, ct);
                        _logger.LogInformation("✅ Cache-1 işlemi tamamlandı");
                    });
            }),
            Task.Run(async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: "resource:file-1",
                    ttl: TimeSpan.FromSeconds(3),
                    async ct =>
                    {
                        _logger.LogInformation("🔒 File-1 kilidi alındı");
                        await Task.Delay(1000, ct);
                        _logger.LogInformation("✅ File-1 işlemi tamamlandı");
                    });
            })
        };

        await Task.WhenAll(tasks);
        _logger.LogInformation("🎉 Tüm paralel işlemler tamamlandı");
        Console.WriteLine("\n");
    }
}
