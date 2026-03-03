using Chd.Coordination.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chd.Coordination.Examples;

/// <summary>
/// Idempotency özelliğinin kullanım örnekleri
/// </summary>
public class IdempotencyExample
{
    private readonly ICoordinator _coordinator;
    private readonly ILogger<IdempotencyExample> _logger;

    public IdempotencyExample(ICoordinator coordinator, ILogger<IdempotencyExample> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task RunAllExamples()
    {
        Console.WriteLine("\n=== IDEMPOTENCY ÖRNEKLERİ ===\n");

        await Example1_BasicIdempotency();
        await Example2_IdempotencyWithReturnValue();
        await Example3_PaymentIdempotency();
    }

    /// <summary>
    /// Örnek 1: Temel idempotency kullanımı
    /// </summary>
    private async Task Example1_BasicIdempotency()
    {
        Console.WriteLine("📝 Örnek 1: Temel Idempotency");
        Console.WriteLine("Senaryo: Aynı email bildirimini birden fazla kez göndermeme\n");

        var notificationId = Guid.NewGuid().ToString();
        int sendCount = 0;

        // İlk çağrı - çalışacak
        await _coordinator.Idempotency.RunAsync(
            key: $"notification:send:{notificationId}",
            ttl: TimeSpan.FromMinutes(5),
            async () =>
            {
                sendCount++;
                _logger.LogInformation("📧 Email gönderiliyor... (Çağrı #{Count})", sendCount);
                await Task.Delay(500);
                _logger.LogInformation("✅ Email gönderildi");
            });

        // İkinci çağrı - çalışmayacak (idempotent)
        await _coordinator.Idempotency.RunAsync(
            key: $"notification:send:{notificationId}",
            ttl: TimeSpan.FromMinutes(5),
            async () =>
            {
                sendCount++;
                _logger.LogWarning("⚠️ Bu çalışmamalı! (Çağrı #{Count})", sendCount);
                await Task.Delay(500);
            });

        Console.WriteLine($"ℹ️ Toplam gönderim sayısı: {sendCount} (olması gereken: 1)\n");
    }

    /// <summary>
    /// Örnek 2: Idempotency ile cache benzeri davranış
    /// </summary>
    private async Task Example2_IdempotencyWithReturnValue()
    {
        Console.WriteLine("📝 Örnek 2: Idempotency ile Cache");
        Console.WriteLine("Senaryo: Aynı işlemi birden fazla kez çağırmayı engelleme\n");

        var reportId = "monthly-sales-2024-01";
        string? result1 = null;
        string? result2 = null;

        // İlk çağrı - hesaplama yapılacak
        await _coordinator.Idempotency.RunAsync(
            key: $"report:generate:{reportId}",
            ttl: TimeSpan.FromHours(1),
            async () =>
            {
                _logger.LogInformation("📊 Rapor oluşturuluyor...");
                await Task.Delay(1000); // Simüle edilmiş hesaplama
                result1 = $"Rapor Data - Oluşturulma: {DateTime.Now:HH:mm:ss}";
                _logger.LogInformation("✅ Rapor oluşturuldu");
            });

        Console.WriteLine($"İlk sonuç: {result1}");
        await Task.Delay(500);

        // İkinci çağrı - çalışmayacak (idempotent)
        await _coordinator.Idempotency.RunAsync(
            key: $"report:generate:{reportId}",
            ttl: TimeSpan.FromHours(1),
            async () =>
            {
                _logger.LogWarning("⚠️ Bu çalışmamalı!");
                await Task.Delay(1000);
                result2 = "Yeni hesaplama";
            });

        Console.WriteLine($"İkinci sonuç: {result2 ?? "null (çalışmadı)"}");
        Console.WriteLine($"ℹ️ İkinci çağrı atlandı - idempotency çalıştı!\n");
    }

    /// <summary>
    /// Örnek 3: Ödeme idempotency'si (gerçek dünya senaryosu)
    /// </summary>
    private async Task Example3_PaymentIdempotency()
    {
        Console.WriteLine("📝 Örnek 3: Ödeme İşlemi Idempotency");
        Console.WriteLine("Senaryo: Kullanıcının birden fazla ödeme butonuna tıklaması\n");

        var orderId = "ORDER-123";
        var amount = 150.50m;
        decimal totalCharged = 0;

        // Kullanıcı hızlıca 3 kez tıklıyor
        var tasks = new List<Task>();
        for (int i = 1; i <= 3; i++)
        {
            var clickNumber = i;
            tasks.Add(Task.Run(async () =>
            {
                await _coordinator.Idempotency.RunAsync(
                    key: $"payment:process:{orderId}",
                    ttl: TimeSpan.FromMinutes(10),
                    async () =>
                    {
                        _logger.LogInformation("💳 Ödeme işleniyor... (Tıklama #{Click})", clickNumber);
                        await Task.Delay(500);
                        
                        totalCharged += amount;
                        
                        _logger.LogInformation("✅ Ödeme başarılı: {Amount:C} (Tıklama #{Click})", 
                            amount, clickNumber);
                    });
            }));
        }

        await Task.WhenAll(tasks);
        
        Console.WriteLine($"\n💰 Toplam tahsil edilen: {totalCharged:C} (olması gereken: {amount:C})");
        Console.WriteLine($"✅ Müşteri sadece bir kez ücretlendirildi!\n");
    }
}
