# CHD Coordination Unit Testleri

Bu dizin, [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination) kütüphanesinin .NET 8 ve xUnit ile yapılan **kapsamlı otomatik testlerini** içerir. Amaç, kütüphanenin tüm özelliklerinin (distributed lock, idempotency, saga, context) doğru, güvenilir ve performanslı bir şekilde çalıştığını doğrulamaktır.

## 📋 Test Sınıfları

### 1. DistributedLockTests (8 test)

Distributed lock özelliğinin tüm yönlerini test eder:

- ✅ Temel kilit alma ve serbest bırakma
- ✅ Dönüş değeri ile kilit kullanımı
- ✅ Eşzamanlı kilitleme senaryoları
- ✅ Timeout yönetimi
- ✅ Exception durumunda kilit serbest bırakma
- ✅ İptal (cancellation) desteği
- ✅ Farklı anahtarlar için paralel çalışma

**Test Senaryoları:**
```csharp
Lock_Should_Acquire_And_Release_Successfully()
Lock_Should_Return_Value()
Lock_Should_Prevent_Concurrent_Execution()
Lock_Should_Timeout_When_Cannot_Acquire()
Lock_Should_Release_On_Exception()
Lock_Should_Handle_Cancellation()
Lock_Should_Support_Different_Keys()
```

### 2. IdempotencyTests (9 test)

Idempotency özelliğinin garantilerini doğrular:

- ✅ Tekrar çağrıldığında sadece bir kez çalışma
- ✅ Cache'lenmiş değer döndürme
- ✅ TTL sonrası yeniden çalışma
- ✅ Eşzamanlı çağrılarda tek çalıştırma
- ✅ Exception durumunda tekrar deneme
- ✅ Farklı anahtarlar için bağımsız çalışma
- ✅ Karmaşık dönüş tipleri desteği
- ✅ Double-payment önleme (gerçek senaryo)

**Test Senaryoları:**
```csharp
Idempotency_Should_Execute_Once()
Idempotency_Should_Return_Cached_Value()
Idempotency_Should_Execute_Again_After_TTL()
Idempotency_Should_Handle_Concurrent_Calls()
Idempotency_Should_Handle_Exception()
Idempotency_Should_Support_Different_Keys()
Idempotency_Should_Handle_Complex_Return_Types()
Idempotency_Should_Prevent_Double_Payment()
```

### 3. SagaTests (8 test)

Saga pattern implementasyonunu test eder:

- ✅ Tüm adımların sıralı çalışması
- ✅ Compensation (geri alma) mekanizması
- ✅ Crash sonrası devam etme (resume)
- ✅ Compensation olmayan adımlar
- ✅ Karmaşık iş akışları
- ✅ Hata durumunda rollback
- ✅ Paralel farklı saga'lar

**Test Senaryoları:**
```csharp
Saga_Should_Execute_All_Steps()
Saga_Should_Execute_Compensation_On_Failure()
Saga_Should_Resume_After_Crash()
Saga_Should_Handle_Steps_Without_Compensation()
Saga_Should_Support_Complex_Workflow()
Saga_Should_Rollback_Complex_Workflow_On_Failure()
Saga_Should_Support_Parallel_Different_Sagas()
```

### 4. CoordinationContextTests (10 test)

Context yönetimi ve izlenebilirlik testleri:

- ✅ CorrelationId oluşturma
- ✅ Custom CorrelationId kullanımı
- ✅ LockKey ile context
- ✅ SagaId ile context
- ✅ Tüm özellikleri içeren context
- ✅ Benzersiz ID üretimi
- ✅ Immutability (değiştirilemezlik)
- ✅ ToString, Equality, Deconstruction
- ✅ Request flow tracking

**Test Senaryoları:**
```csharp
Context_Should_Create_With_CorrelationId()
Context_Should_Create_With_Custom_CorrelationId()
Context_Should_Create_With_LockKey()
Context_Should_Create_With_SagaId()
Context_Should_Create_With_All_Properties()
Context_Should_Generate_Unique_CorrelationIds()
// ... ve daha fazlası
```

### 5. IntegrationTests (7 test)

Özelliklerin birlikte kullanımını test eder:

- ✅ Lock + Idempotency kombinasyonu
- ✅ Saga + Lock kombinasyonu
- ✅ Idempotent Saga
- ✅ Banka havalesi senaryosu (tüm özellikler)
- ✅ Banka havalesi rollback senaryosu
- ✅ Job processing (3 özellik birlikte)

**Test Senaryoları:**
```csharp
Integration_Lock_And_Idempotency()
Integration_Saga_With_Locked_Steps()
Integration_Idempotent_Saga()
Integration_BankTransfer_Scenario()
Integration_BankTransfer_Rollback_Scenario()
Integration_JobProcessing_With_All_Features()
```

### 6. CoordinationUnitTest (Legacy - 4 test)

Orijinal temel testler (geriye dönük uyumluluk için korunmuştur).

## 📊 Test İstatistikleri

- **Toplam Test Sayısı:** 46+ test
- **Kapsanan Özellikler:** 4 ana özellik
- **Test Türleri:** Unit tests + Integration tests
- **Framework:** xUnit with .NET 8

## 🚀 Testleri Çalıştırma

### Gereksinimler

- .NET 8 SDK
- Redis sunucusu (`localhost:6379`)

### Redis Kurulumu

**Docker ile:**
```bash
docker run -d -p 6379:6379 redis:latest
```

**Yerel kurulum:**
```bash
# Windows (Chocolatey)
choco install redis-64

# macOS (Homebrew)
brew install redis
redis-server

# Linux (Ubuntu/Debian)
sudo apt-get install redis-server
sudo service redis-server start
```

### Test Komutları

```bash
# Tüm testleri çalıştır
dotnet test

# Detaylı çıktı ile
dotnet test --logger "console;verbosity=detailed"

# Belirli bir test sınıfı
dotnet test --filter "FullyQualifiedName~DistributedLockTests"

# Code coverage ile
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🎯 Test Metodolojisi

### Test Yapısı (AAA Pattern)

```csharp
[Fact]
public async Task TestName_Should_ExpectedBehavior_When_Condition()
{
    // Arrange - Test verilerini hazırla
    var key = $"test:{Guid.NewGuid()}";

    // Act - Test edilecek işlemi çalıştır
    var result = await _coordinator.Lock.RunAsync(...);

    // Assert - Sonuçları doğrula
    Assert.True(result);
}
```

### Test İsimlendirme

- `Feature_Should_Behavior` formatı
- Açıklayıcı ve anlaşılır
- Örnek: `Lock_Should_Release_On_Exception()`

### Test Bağımsızlığı

- Her test kendi benzersiz anahtarlarını kullanır (`Guid.NewGuid()`)
- `IAsyncLifetime` ile proper setup/teardown
- Test'ler paralel çalışabilir

## 🔍 Test Coverage

### Distributed Lock
- ✅ Normal akış
- ✅ Hata durumları
- ✅ Eşzamanlılık
- ✅ Timeout
- ✅ Cancellation

### Idempotency
- ✅ Temel idempotency
- ✅ Caching
- ✅ TTL yönetimi
- ✅ Concurrent access
- ✅ Exception handling

### Saga
- ✅ Step execution
- ✅ Compensation
- ✅ Resume/recovery
- ✅ Complex workflows
- ✅ Rollback

### Integration
- ✅ Feature combinations
- ✅ Real-world scenarios
- ✅ Error handling
- ✅ Rollback scenarios

## 🐛 Test Debugging

### Visual Studio
```
Test Explorer → Right Click → Debug
```

### VS Code
```json
// launch.json
{
  "name": ".NET Core Test",
  "type": "coreclr",
  "request": "launch",
  "program": "dotnet",
  "args": ["test", "--filter", "TestName"],
  "cwd": "${workspaceFolder}/Chd.UnitTest"
}
```

### Command Line
```bash
# Belirli bir testi debug modda çalıştır
dotnet test --filter "Lock_Should_Acquire" --logger "console;verbosity=detailed"
```

## 📝 Yeni Test Ekleme

```csharp
[Fact]
public async Task NewFeature_Should_DoSomething()
{
    // Arrange
    var key = $"test:newfeature:{Guid.NewGuid()}";

    // Act
    var result = await _coordinator.NewFeature.ExecuteAsync(key);

    // Assert
    Assert.NotNull(result);
}
```

## 🤝 Katkı

Yeni testler veya iyileştirmeler için:

1. Fork yapın
2. Test ekleyin/güncelleyin
3. `dotnet test` ile doğrulayın
4. Pull request gönderin

## 📖 İlgili Kaynaklar

- [Kullanım Örnekleri](../Chd.Coordination.Examples/)
- [NuGet Paketi](https://www.nuget.org/packages/Chd.Coordination)
- [Kaynak Kodu](https://github.com/mehmet-yoldas/library-core)

## 📄 Lisans

MIT License
