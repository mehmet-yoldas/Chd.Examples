# Chd.Coordination - Kullanım Örnekleri

Bu proje, [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination) kütüphanesinin tüm özelliklerini gösteren kapsamlı örnekler içerir.

## 🚀 Başlangıç

### Gereksinimler

- .NET 8 SDK
- Redis sunucusu (varsayılan: `localhost:6379`)

### Redis Kurulumu

**Windows (Docker):**
```bash
docker run -d -p 6379:6379 redis:latest
```

**macOS/Linux:**
```bash
# Homebrew ile
brew install redis
redis-server

# Docker ile
docker run -d -p 6379:6379 redis:latest
```

### Çalıştırma

```bash
cd Chd.Coordination.Examples
dotnet run
```

## 📚 Örnekler

### 1. Distributed Lock Örnekleri

Distributed lock özelliği, dağıtık sistemlerde kritik kod bölümlerinin sadece bir sunucu/instance tarafından aynı anda çalıştırılmasını sağlar.

**Örnek Senaryolar:**
- ✅ Temel kilit kullanımı
- ✅ Dönüş değeri ile kilit
- ✅ Rekabet eden kilitler (multiple servers)
- ✅ Timeout yönetimi

**Kullanım Alanları:**
- Kritik veri güncellemeleri (stok, bakiye vb.)
- Tekil job execution
- Rate limiting
- Cache güncellemeleri

### 2. Idempotency Örnekleri

Idempotency özelliği, aynı işlemin birden fazla kez çağrılması durumunda sadece bir kez çalışmasını ve aynı sonucu döndürmesini garanti eder.

**Örnek Senaryolar:**
- ✅ Temel idempotency
- ✅ Dönüş değeri ile idempotency (caching)
- ✅ Ödeme işlemi (double-click protection)

**Kullanım Alanları:**
- Ödeme işlemleri
- Email/SMS gönderimi
- Webhook işleme
- API retry mekanizmaları
- Event sourcing

### 3. Saga Örnekleri

Saga pattern, uzun süren ve birden fazla adımdan oluşan distributed transaction'ları yönetir. Her adım için compensation (geri alma) tanımlanabilir.

**Örnek Senaryolar:**
- ✅ Temel saga kullanımı
- ✅ Compensation ile saga (rollback)
- ✅ Sipariş işleme saga'sı

**Kullanım Alanları:**
- E-ticaret sipariş işleme
- Rezervasyon sistemleri
- Kullanıcı onboarding
- Mikroservis orchestration

### 4. Gerçek Dünya Senaryoları

Birden fazla özelliği kombine eden karmaşık senaryolar.

**Örnek Senaryolar:**
- ✅ Banka havalesi (Saga + Lock + Idempotency)
- ✅ Concurrent job processing (Lock + Context)
- ✅ Event processing (Idempotency + deduplication)

## 🎯 Özelliklerin Karşılaştırması

| Özellik | Amaç | Ne Zaman Kullanılır |
|---------|------|---------------------|
| **Distributed Lock** | Kritik bölge koruması | Aynı anda sadece bir instance çalışmalı |
| **Idempotency** | Tekrar güvenliği | İşlem birden fazla kez çağrılabilir |
| **Saga** | Uzun transaction yönetimi | Birden fazla adım, rollback gerekli |
| **CoordinationContext** | İzlenebilirlik | Request tracking, correlation |

## 🏗️ Mimari Notlar

### Distributed Lock vs Idempotency

```csharp
// ❌ Yanlış - Lock kullanımı gereksiz
await coordinator.Lock.RunAsync("send-email:123", async ct => 
{
    await SendEmail(); // Zaten idempotent olmalı
});

// ✅ Doğru - Idempotency yeterli
await coordinator.Idempotency.RunAsync("send-email:123", async () => 
{
    await SendEmail();
});
```

**Lock:** Eşzamanlılık kontrolü (mutual exclusion)  
**Idempotency:** Tekrar güvenliği (duplicate protection)

### Saga Best Practices

```csharp
await coordinator.Saga.RunAsync("order-123", async saga =>
{
    // ✅ Her önemli adım için compensation tanımla
    await saga.Step("charge-payment", 
        action: async () => await ChargeCard(),
        compensation: async () => await RefundCard());
    
    // ✅ Compensation gerektirmeyen adımlar
    await saga.Step("send-notification", 
        action: async () => await SendEmail());
    
    // ❌ Tüm iş mantığını tek step'e koymayın
    // Her mantıksal adım ayrı step olmalı
});
```

## 🔧 Yapılandırma

```csharp
services.AddCoordination(opt =>
{
    // Redis bağlantı dizesi
    opt.RedisConnectionString = "localhost:6379";
    
    // Opsiyonel: Prefix
    opt.KeyPrefix = "myapp:";
});
```

## 📖 İlgili Kaynaklar

- [NuGet Paketi](https://www.nuget.org/packages/Chd.Coordination)
- [Kaynak Kodu](https://github.com/mehmet-yoldas/library-core)
- [Unit Testler](../Chd.UnitTest/)

## 🤝 Katkı

Yeni örnek senaryolar veya iyileştirmeler için pull request göndermekten çekinmeyin!

## 📝 Lisans

Bu örnekler MIT lisansı altında sunulmaktadır.
