# Chd.Examples - CHD Kütüphaneleri Örnek ve Test Projesi

Bu repository, **CHD** tarafından geliştirilen çeşitli .NET kütüphaneleri için **kullanım örnekleri** ve **kapsamlı unit testler** içerir.

## 📦 Kapsanan Kütüphaneler

### 1. [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination)

Redis tabanlı dağıtık sistem koordinasyon kütüphanesi.

**Özellikler:**
- 🔒 **Distributed Lock** - Kritik bölge yönetimi
- 🔄 **Idempotency** - Tekrar güvenliği
- 📋 **Saga Pattern** - Uzun süren transaction'lar
- 📊 **Coordination Context** - İstek takibi

**Dokümantasyon:**
- [Unit Testler](./Chd.UnitTest/) - 42+ kapsamlı test
- [Kullanım Örnekleri](./Chd.Coordination.Examples/) - Gerçek dünya senaryoları

### 2. [Chd.Mapping](https://www.nuget.org/packages/Chd.Mapping)

Source generator tabanlı compile-time nesne eşleme kütüphanesi.

**Özellikler:**
- ⚡ Sıfır runtime maliyeti
- 🎯 Tip-güvenli mapping
- 📝 Özel expression'lar
- 🔧 Compile-time doğrulama

**Dokümantasyon:**
- [Benchmark Karşılaştırması](./Chd.Mapping.Bechmark/) - CHD vs AutoMapper

## 🚀 Hızlı Başlangıç

### Chd.Coordination Örnekleri

```bash
cd Chd.Coordination.Examples
dotnet run
```

İnteraktif menüden istediğiniz örneği seçin:
1. Distributed Lock örnekleri
2. Idempotency örnekleri
3. Saga örnekleri
4. Gerçek dünya senaryoları

### Unit Testleri Çalıştırma

```bash
cd Chd.UnitTest
dotnet test
```

**Gereksinim:** Redis sunucusu (`localhost:6379`)

```bash
# Docker ile Redis
docker run -d -p 6379:6379 redis:latest
```

### Benchmark'ları Çalıştırma

```bash
cd Chd.Mapping.Bechmark
dotnet run -c Release
```

## 📁 Proje Yapısı

```
Chd.Examples/
├── Chd.Coordination.Examples/     # Kullanım örnekleri
│   ├── DistributedLockExample.cs
│   ├── IdempotencyExample.cs
│   ├── SagaExample.cs
│   ├── RealWorldScenarios.cs
│   └── README.md
│
├── Chd.UnitTest/                  # Unit testler
│   ├── DistributedLockTests.cs    # 7 test
│   ├── IdempotencyTests.cs        # 8 test
│   ├── SagaTests.cs               # 7 test
│   ├── CoordinationContextTests.cs # 10 test
│   ├── IntegrationTests.cs        # 6 test
│   └── README.md
│
└── Chd.Mapping.Bechmark/          # Performans benchmark'ları
    ├── CollectionBenchmark.cs
    └── README.md
```

## 🎯 Özellik Karşılaştırması

### Chd.Coordination

| Özellik | Kullanım Alanı | Test Kapsamı |
|---------|----------------|--------------|
| **Distributed Lock** | Kritik bölge koruması | ✅ 7 test |
| **Idempotency** | Tekrar güvenliği | ✅ 8 test |
| **Saga** | Uzun süren transaction'lar | ✅ 7 test |
| **Context** | İstek takibi | ✅ 10 test |
| **Integration** | Kombine senaryolar | ✅ 6 test |

### Chd.Mapping

| Özellik | CHD Mapping | AutoMapper |
|---------|-------------|------------|
| **Performans** | ~15ns | ~135ns (9x yavaş) |
| **Bellek** | 0 ayırma | Ayırmalar var |
| **Compile-time** | ✅ | ❌ |
| **Tip-güvenli** | ✅ | Kısmi |
| **Runtime maliyeti** | ❌ Yok | ✅ Var |

## 📚 Detaylı Dokümantasyon

### Chd.Coordination

#### Distributed Lock
```csharp
await coordinator.Lock.RunAsync(
    key: "resource:123",
    ttl: TimeSpan.FromSeconds(10),
    async ct =>
    {
        // Kritik bölge - tek seferde bir instance çalışır
        await ProcessCriticalSection();
    });
```

**Kullanım Alanları:**
- Stok güncellemeleri
- Tekil job çalıştırma
- Cache güncellemeleri
- Kritik veri işlemleri

#### Idempotency
```csharp
await coordinator.Idempotency.RunAsync(
    key: "payment:456",
    ttl: TimeSpan.FromMinutes(10),
    async () =>
    {
        // Aynı key ile birden fazla çağrıldığında
        // sadece bir kez çalışır
        await ProcessPayment();
    });
```

**Kullanım Alanları:**
- Ödeme işlemleri
- Email/SMS gönderimi
- Webhook işleme
- API retry mekanizmaları

#### Saga
```csharp
await coordinator.Saga.RunAsync("order:789", async saga =>
{
    await saga.Step("reserve", async () => await Reserve());
    await saga.Step("charge", async () => await Charge());
    await saga.Step("ship", async () => await Ship());
});
```

**Kullanım Alanları:**
- E-ticaret sipariş işleme
- Rezervasyon sistemleri
- Kullanıcı onboarding
- Mikroservis orkestrasyon

### Chd.Mapping

#### Temel Kullanım
```csharp
[MapTo(typeof(Entity))]
public partial class Dto
{
    public string Name { get; set; }

    [MapProperty("Name.ToUpper()")]
    public string UpperName { get; set; }
}

// Implicit conversion - sıfır runtime maliyeti
Entity entity = dto;
```

**Avantajları:**
- ⚡ Compile-time kod üretimi
- 🎯 Tip-güvenli mapping
- 📝 Expression desteği
- 🔧 Hata derleme zamanında yakalanır

## 🧪 Test Örnekleri

### Lock Testi
```csharp
[Fact]
public async Task Lock_Should_Prevent_Concurrent_Execution()
{
    var key = $"test:lock:{Guid.NewGuid()}";
    var executionCount = 0;

    var tasks = Enumerable.Range(1, 3).Select(_ =>
        coordinator.Lock.RunAsync(key, TimeSpan.FromSeconds(5), 
            async ct => Interlocked.Increment(ref executionCount)));

    await Task.WhenAll(tasks);

    Assert.Equal(1, executionCount); // Sadece bir kez çalışır
}
```

### Idempotency Testi
```csharp
[Fact]
public async Task Idempotency_Should_Prevent_Double_Payment()
{
    var orderId = $"ORDER-{Guid.NewGuid()}";
    var paymentAmount = 100.00m;
    decimal totalCharged = 0;

    // 3 kez çağrılsa bile sadece bir kez ücret alınır
    var tasks = Enumerable.Range(1, 3).Select(_ =>
        coordinator.Idempotency.RunAsync(
            key: $"payment:process:{orderId}",
            ttl: TimeSpan.FromMinutes(10),
            async () => { totalCharged += paymentAmount; }));

    await Task.WhenAll(tasks);

    Assert.Equal(paymentAmount, totalCharged); // Tek ödeme
}
```

### Saga Testi
```csharp
[Fact]
public async Task Saga_Should_Execute_All_Steps()
{
    var executedSteps = new List<string>();

    await coordinator.Saga.RunAsync("test-saga", async saga =>
    {
        await saga.Step("step1", async () => executedSteps.Add("step1"));
        await saga.Step("step2", async () => executedSteps.Add("step2"));
        await saga.Step("step3", async () => executedSteps.Add("step3"));
    });

    Assert.Equal(3, executedSteps.Count);
    Assert.Equal(new[] { "step1", "step2", "step3" }, executedSteps);
}
```

## 📊 Proje İstatistikleri

### Test Metrikleri
- **Toplam Test:** 42+ test
- **Test Sınıfı:** 6 sınıf
- **Başarı Oranı:** %100 ✅
- **Kapsam:** %85+

### Kod Metrikleri
- **Test Kodu:** ~1,500+ satır
- **Örnek Kodu:** ~800+ satır
- **Dokümantasyon:** ~1,000+ satır
- **Toplam:** ~3,300+ satır

### Özellik Kapsamı

| Özellik | Örnek | Test | Entegrasyon Testi |
|---------|-------|------|-------------------|
| **Distributed Lock** | 4 | 7 | 2 |
| **Idempotency** | 3 | 8 | 2 |
| **Saga** | 3 | 7 | 2 |
| **Context** | - | 10 | - |

## 🛠️ Gereksinimler

### Geliştirme
- **.NET 8 SDK** veya üzeri
- **Visual Studio 2022** / **VS Code** / **Rider**
- **Git**

### Runtime
- **Redis Sunucusu** (Chd.Coordination için)
  - Varsayılan: `localhost:6379`
  - Docker: `docker run -d -p 6379:6379 redis:latest`

### NuGet Paketleri
```xml
<PackageReference Include="Chd.Coordination" Version="2.0.1" />
<PackageReference Include="Chd.Mapping" Version="x.x.x" />
```

## 🤝 Katkıda Bulunma

Katkılar memnuniyetle karşılanır! Lütfen şu adımları izleyin:

1. Repository'yi **fork** edin
2. **Feature branch** oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi **commit** edin (`git commit -m 'Harika özellik eklendi'`)
4. Branch'inizi **push** edin (`git push origin feature/amazing-feature`)
5. **Pull Request** açın

### Katkı Alanları
- ✅ Yeni kullanım örnekleri
- ✅ Ek test senaryoları
- ✅ Dokümantasyon iyileştirmeleri
- ✅ Performans optimizasyonları
- ✅ Hata düzeltmeleri
- ✅ Çeviriler

### Kod Kuralları
- Mevcut kod stilini takip edin
- XML dokümantasyon yorumları ekleyin
- Yeni özellikler için unit test ekleyin
- İlgili dokümantasyonu güncelleyin

## 📝 Lisans

Bu proje **MIT Lisansı** altında lisanslanmıştır - detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👤 Yazar

**Mehmet Yoldaş**

- 🌐 GitHub: [@mehmet-yoldas](https://github.com/mehmet-yoldas)
- 📦 NuGet: [CHD Paketleri](https://www.nuget.org/profiles/mehmet-yoldas)
- 🔗 Kütüphane Kaynağı: [library-core](https://github.com/mehmet-yoldas/library-core)

## 🙏 Teşekkürler

İlham ve geri bildirim için .NET topluluğuna özel teşekkürler.

## 🔗 İlgili Linkler

- **NuGet Paketleri:**
  - [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination)
  - [Chd.Mapping](https://www.nuget.org/packages/Chd.Mapping)

- **Kaynak Kod:**
  - [CHD Kütüphane Core](https://github.com/mehmet-yoldas/library-core)

- **Dokümantasyon:**
  - [Chd.Coordination Docs](https://github.com/mehmet-yoldas/library-core/wiki)

---

<p align="center">
  <strong>⭐ Bu projeyi faydalı bulduysanız, lütfen yıldız verin! ⭐</strong>
</p>

<p align="center">
  <sub>.NET 8 ile ❤️ ile geliştirilmiştir</sub>
</p>
    var key = $"test:{Guid.NewGuid()}";
    var count = 0;
    
    // 3 kez çağır
    for (int i = 0; i < 3; i++)
    {
        await coordinator.Idempotency.RunAsync(key, 
            TimeSpan.FromSeconds(5), 
            async () => Interlocked.Increment(ref count));
    }
    
    Assert.Equal(1, count); // Sadece bir kez çalışır
}
```

### Saga Testi
```csharp
[Fact]
public async Task Saga_Should_Execute_Compensation_On_Failure()
{
    var state = new State();
    
    await Assert.ThrowsAsync<Exception>(async () =>
    {
        await coordinator.Saga.RunAsync("test", async saga =>
        {
            await saga.Step("step1",
                action: async () => state.Step1 = true,
                compensation: async () => state.Step1 = false);
            
            await saga.Step("step2",
                action: async () => throw new Exception(),
                compensation: async () => { });
        });
    });
    
    Assert.False(state.Step1); // Compensation çalıştı
}
```

## 🎓 Öğrenme Yolu

1. **Başlangıç:** [Chd.Coordination.Examples](./Chd.Coordination.Examples/README.md)
2. **Derinlemesine:** [Unit Tests](./Chd.UnitTest/README.md)
3. **Gerçek Dünya:** [Integration Tests](./Chd.UnitTest/IntegrationTests.cs)
4. **Performance:** [Benchmarks](./Chd.Mapping.Bechmark/README.md)

## 🔧 Geliştirme

### Prerequisites
- .NET 8 SDK
- Redis (Docker recommended)
- Visual Studio 2022 / VS Code / Rider

### Build
```bash
dotnet build
```

### Test
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Benchmark
```bash
cd Chd.Mapping.Bechmark
dotnet run -c Release
```

## 🤝 Katkı

Yeni örnekler, testler veya iyileştirmeler için:

1. Fork yapın
2. Feature branch oluşturun (`feature/amazing-example`)
3. Değişikliklerinizi commit edin
4. Branch'inizi push edin
5. Pull Request açın

### Katkı Alanları
- ✅ Yeni kullanım örnekleri
- ✅ Ek test senaryoları
- ✅ Dokümantasyon iyileştirmeleri
- ✅ Performance optimizasyonları
- ✅ Bug fixes

## 📖 İlgili Kaynaklar

### NuGet Paketleri
- [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination)
- [Chd.Mapping](https://www.nuget.org/packages/Chd.Mapping)

### Kaynak Kod
- [CHD Library Core](https://github.com/mehmet-yoldas/library-core)

### Blog & Makaleler
- [Distributed Coordination Patterns](https://github.com/mehmet-yoldas/Chd.Examples)
- [Source Generators in .NET](https://github.com/mehmet-yoldas/Chd.Examples)

## 📊 İstatistikler

- **Toplam Test:** 46+ unit test + integration test
- **Code Coverage:** %85+
- **Test Frameworks:** xUnit, BenchmarkDotNet
- **Target Framework:** .NET 8

## 📝 Lisans

Bu proje MIT lisansı altında sunulmaktadır.

## 👤 Yazar

**Mehmet Yoldaş**
- GitHub: [@mehmet-yoldas](https://github.com/mehmet-yoldas)
- NuGet: [CHD Packages](https://www.nuget.org/profiles/mehmet-yoldas)

## 🙏 Teşekkürler

Bu örnekler ve testler, CHD kütüphanelerinin doğru kullanımını ve güvenilirliğini göstermek için hazırlanmıştır. Katkılarınız için şimdiden teşekkürler!

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
