# CHD Coordination Testleri

Bu dizin, [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination) kütüphanesinin .NET 8 ve xUnit ile yapılan otomatik testlerini içerir. Amaç, kütüphanenin ana özelliklerinin (distributed lock, idempotency, saga, context) doğru ve güvenilir şekilde çalıştığını doğrulamaktır.

## Test Edilen Özellikler

- **Distributed Lock:** Redis tabanlı kilit ile kritik bölge yönetimi.
- **Idempotency:** Aynı işlemin tekrar çağrıldığında sadece bir kez çalışması.
- **Saga:** Adım adım, crash recovery destekli iş akışı yönetimi.
- **CoordinationContext:** CorrelationId, LockKey ve SagaId ile izlenebilir context oluşturma.

## Test Örnekleri

### Distributed Lock

```markdown
Redis tabanlı distributed lock mekanizmasının doğruluğunu ve güvenilirliğini test eden senaryolar içerir.
```

### Idempotency
```markdown
İşlemlerin idempotentliğini test eden senaryolar içerir. Aynı işlemin birden fazla kez yapılmasının sistem üzerinde yarattığı etkileri sınırlandırır.
```

### Saga

```markdown
Uzun süreli ve birden fazla adım içeren işlemlerin yönetimini test eder. Her adımın başarılı bir şekilde tamamlandığından emin olunur.
```

### CoordinationContext

```markdown
Farklı işlemler ve talepler arasında tutarlı bir izleme ve bağlam yönetimi sağlamak için CoordinationContext kullanımını test eder.
```

## Test Ortamı

- .NET 8
- xUnit
- Redis sunucusu test sırasında çalışıyor olmalı (`localhost:6379`)

## Katkı

Yeni testler eklemek veya mevcutları geliştirmek için PR gönderebilirsiniz.

---
Daha fazla bilgi için [NuGet sayfası](https://www.nuget.org/packages/Chd.Coordination) veya [kaynak kodu](https://github.com/mehmet-yoldas/library-core) inceleyebilirsiniz.
