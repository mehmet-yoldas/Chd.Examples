using Chd.Coordination.Abstractions;
using Microsoft.Extensions.Logging;

namespace Chd.Coordination.Examples;

/// <summary>
/// Saga özelliğinin kullanım örnekleri
/// </summary>
public class SagaExample
{
    private readonly ICoordinator _coordinator;
    private readonly ILogger<SagaExample> _logger;

    public SagaExample(ICoordinator coordinator, ILogger<SagaExample> logger)
    {
        _coordinator = coordinator;
        _logger = logger;
    }

    public async Task RunAllExamples()
    {
        Console.WriteLine("\n=== SAGA ÖRNEKLERİ ===\n");

        await Example1_BasicSaga();
        await Example2_SagaWithErrorHandling();
        await Example3_OrderProcessingSaga();
    }

    /// <summary>
    /// Örnek 1: Temel saga kullanımı
    /// </summary>
    private async Task Example1_BasicSaga()
    {
        Console.WriteLine("📝 Örnek 1: Temel Saga");
        Console.WriteLine("Senaryo: Adım adım işlem yapma ve durumu takip etme\n");

        await _coordinator.Saga.RunAsync("user-onboarding-123", async saga =>
        {
            await saga.Step("create-account", async () =>
            {
                _logger.LogInformation("1️⃣ Kullanıcı hesabı oluşturuluyor...");
                await Task.Delay(500);
                _logger.LogInformation("✅ Hesap oluşturuldu");
            });

            await saga.Step("send-welcome-email", async () =>
            {
                _logger.LogInformation("2️⃣ Hoş geldin emaili gönderiliyor...");
                await Task.Delay(500);
                _logger.LogInformation("✅ Email gönderildi");
            });

            await saga.Step("setup-preferences", async () =>
            {
                _logger.LogInformation("3️⃣ Varsayılan tercihler ayarlanıyor...");
                await Task.Delay(500);
                _logger.LogInformation("✅ Tercihler ayarlandı");
            });
        });

        Console.WriteLine("🎉 Onboarding saga'sı tamamlandı!\n");
    }

    /// <summary>
    /// Örnek 2: Hata yönetimi ile saga
    /// </summary>
    private async Task Example2_SagaWithErrorHandling()
    {
        Console.WriteLine("📝 Örnek 2: Hata Yönetimi ile Saga");
        Console.WriteLine("Senaryo: İşlem sırasında hata oluşursa durumu yönetme\n");

        bool reservationMade = false;
        bool paymentProcessed = false;

        try
        {
            await _coordinator.Saga.RunAsync("booking-failed-456", async saga =>
            {
                await saga.Step("reserve-room", async () =>
                {
                    _logger.LogInformation("🏨 Oda rezerve ediliyor...");
                    await Task.Delay(300);
                    reservationMade = true;
                    _logger.LogInformation("✅ Oda rezerve edildi");
                });

                await saga.Step("process-payment", async () =>
                {
                    _logger.LogInformation("💳 Ödeme işleniyor...");
                    await Task.Delay(300);

                    // Simüle edilmiş hata
                    throw new Exception("Kredi kartı reddedildi!");
                });

                await saga.Step("send-confirmation", async () =>
                {
                    _logger.LogInformation("📧 Onay emaili gönderiliyor...");
                    await Task.Delay(300);
                    _logger.LogInformation("✅ Email gönderildi");
                });
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Saga başarısız: {Message}", ex.Message);
            _logger.LogWarning("⚠️ Manuel olarak geri alma işlemi gerekiyor");
        }

        Console.WriteLine($"\nSon durum:");
        Console.WriteLine($"  Rezervasyon yapıldı mı? {reservationMade}");
        Console.WriteLine($"  Ödeme işlendi mi? {paymentProcessed}\n");
    }

    /// <summary>
    /// Örnek 3: Gerçek dünya - sipariş işleme saga'sı
    /// </summary>
    private async Task Example3_OrderProcessingSaga()
    {
        Console.WriteLine("📝 Örnek 3: Sipariş İşleme Saga");
        Console.WriteLine("Senaryo: E-ticaret sipariş işleme süreci\n");

        var orderId = "ORD-789";
        var orderStatus = new OrderStatus();

        await _coordinator.Saga.RunAsync($"order-process-{orderId}", async saga =>
        {
            await saga.Step("validate-inventory", async () =>
            {
                _logger.LogInformation("📦 Stok kontrolü yapılıyor...");
                await Task.Delay(400);
                orderStatus.InventoryValidated = true;
                _logger.LogInformation("✅ Stok uygun");
            });

            await saga.Step("reserve-items", async () =>
            {
                _logger.LogInformation("🔒 Ürünler rezerve ediliyor...");
                await Task.Delay(400);
                orderStatus.ItemsReserved = true;
                _logger.LogInformation("✅ Ürünler rezerve edildi");
            });

            await saga.Step("charge-customer", async () =>
            {
                _logger.LogInformation("💳 Müşteri ücretlendiriliyor...");
                await Task.Delay(600);
                orderStatus.CustomerCharged = true;
                _logger.LogInformation("✅ Ödeme alındı");
            });

            await saga.Step("update-inventory", async () =>
            {
                _logger.LogInformation("📊 Stok güncelleniyor...");
                await Task.Delay(400);
                orderStatus.InventoryUpdated = true;
                _logger.LogInformation("✅ Stok güncellendi");
            });

            await saga.Step("create-shipment", async () =>
            {
                _logger.LogInformation("🚚 Kargo oluşturuluyor...");
                await Task.Delay(500);
                orderStatus.ShipmentCreated = true;
                _logger.LogInformation("✅ Kargo oluşturuldu");
            });

            await saga.Step("send-confirmation", async () =>
            {
                _logger.LogInformation("📧 Onay emaili gönderiliyor...");
                await Task.Delay(300);
                orderStatus.ConfirmationSent = true;
                _logger.LogInformation("✅ Onay emaili gönderildi");
            });
        });

        Console.WriteLine("\n🎉 Sipariş başarıyla işlendi!");
        Console.WriteLine($"Sipariş Durumu: {orderStatus}\n");
    }

    private class OrderStatus
    {
        public bool InventoryValidated { get; set; }
        public bool ItemsReserved { get; set; }
        public bool CustomerCharged { get; set; }
        public bool InventoryUpdated { get; set; }
        public bool ShipmentCreated { get; set; }
        public bool ConfirmationSent { get; set; }

        public override string ToString()
        {
            return $"✓ Stok doğrulandı: {InventoryValidated}, " +
                   $"✓ Rezerve: {ItemsReserved}, " +
                   $"✓ Ücretlendirildi: {CustomerCharged}, " +
                   $"✓ Stok güncellendi: {InventoryUpdated}, " +
                   $"✓ Kargo: {ShipmentCreated}, " +
                   $"✓ Onay: {ConfirmationSent}";
        }
    }
}
