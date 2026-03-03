# Chd.Examples - CHD Libraries Example and Test Project

This repository contains **usage examples** and **comprehensive unit tests** for various .NET libraries developed by **CHD**.

## 📦 Covered Libraries

### 1. [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination)

Redis-based distributed system coordination library.

**Features:**
- 🔒 **Distributed Lock** - Critical section management
- 🔄 **Idempotency** - Retry safety
- 📋 **Saga Pattern** - Long-running transactions
- 📊 **Coordination Context** - Request tracking

**Documentation:**
- [Unit Tests](./Chd.UnitTest/) - 46+ comprehensive tests
- [Usage Examples](./Chd.Coordination.Examples/) - Real-world scenarios

### 2. [Chd.Mapping](https://www.nuget.org/packages/Chd.Mapping)

Source generator-based compile-time object mapping.

**Features:**
- ⚡ Zero-runtime overhead
- 🎯 Type-safe mapping
- 📝 Custom expressions
- 🔧 Compile-time validation

**Documentation:**
- [Benchmark Comparison](./Chd.Mapping.Bechmark/) - CHD vs AutoMapper

## 🚀 Quick Start

### Chd.Coordination Examples

```bash
cd Chd.Coordination.Examples
dotnet run
```

Select the desired example from the interactive menu:
1. Distributed Lock examples
2. Idempotency examples
3. Saga examples
4. Real-world scenarios

### Running Unit Tests

```bash
cd Chd.UnitTest
dotnet test
```

**Requirement:** Redis server (`localhost:6379`)

```bash
# Redis with Docker
docker run -d -p 6379:6379 redis:latest
```

### Running Benchmarks

```bash
cd Chd.Mapping.Bechmark
dotnet run -c Release
```

## 📁 Project Structure

```
Chd.Examples/
├── Chd.Coordination.Examples/     # Usage examples
│   ├── DistributedLockExample.cs
│   ├── IdempotencyExample.cs
│   ├── SagaExample.cs
│   ├── RealWorldScenarios.cs
│   └── README.md
│
├── Chd.UnitTest/                  # Unit tests
│   ├── DistributedLockTests.cs    # 8 tests
│   ├── IdempotencyTests.cs        # 9 tests
│   ├── SagaTests.cs               # 8 tests
│   ├── CoordinationContextTests.cs # 10 tests
│   ├── IntegrationTests.cs        # 7 tests
│   └── README.md
│
└── Chd.Mapping.Bechmark/          # Performance benchmarks
    ├── CollectionBenchmark.cs
    └── README.md
```

## 🎯 Feature Comparison

### Chd.Coordination

| Feature | Use Case | Test Coverage |
|---------|----------|---------------|
| **Distributed Lock** | Critical section protection | ✅ 8 tests |
| **Idempotency** | Retry safety | ✅ 9 tests |
| **Saga** | Long-running transactions | ✅ 8 tests |
| **Context** | Request tracking | ✅ 10 tests |
| **Integration** | Combined scenarios | ✅ 7 tests |

### Chd.Mapping

| Feature | CHD Mapping | AutoMapper |
|---------|-------------|------------|
| **Performance** | ~15ns | ~135ns (9x slower) |
| **Memory** | 0 allocation | Allocations |
| **Compile-time** | ✅ | ❌ |
| **Type-safe** | ✅ | Partial |
| **Runtime overhead** | ❌ None | ✅ Yes |

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

#### Saga
```csharp
await coordinator.Saga.RunAsync("order:789", async saga =>
{
    await saga.Step("reserve", 
        action: async () => await Reserve(),
        compensation: async () => await CancelReservation());
    
    await saga.Step("charge", 
        action: async () => await Charge(),
        compensation: async () => await Refund());
});
```

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

// Implicit conversion
Entity entity = dto;
```

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
public async Task Idempotency_Should_Execute_Once()
{
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
