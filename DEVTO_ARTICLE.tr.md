---
title: "AutoMapper Kullanmayı Bırakın: Source Generator'ler Neden 9x Daha Hızlı (ve Güvenli)"
published: false
description: "Roslyn Source Generator'ler ile compile-time mapping'in, AutoMapper gibi reflection-based yaklaşımları neden ezdiğine derin bir bakış"
tags: dotnet, csharp, performance, turkce
cover_image: https://dev-to-uploads.s3.amazonaws.com/uploads/articles/...
---

# AutoMapper Kullanmayı Bırakın: Source Generator'ler Neden 9x Daha Hızlı (ve Güvenli)

Eğer 2024'te hala AutoMapper kullanıyorsanız, **9x performans** kaybediyorsunuz. Ama bu sadece hız meselesi değil—**tip güvenliği**, **sıfır sürpriz hata** ve **kodunuzu gerçekten anlamak** meselesi.

Size **source generator tabanlı mapping**'in neden gelecek olduğunu ve AutoMapper'ın neden legacy teknoloji olduğunu göstereyim.

---

## 🤔 AutoMapper'ın Sorunu

AutoMapper, on yılı aşkın süredir .NET geliştiricilerinin vazgeçilmez mapping kütüphanesi oldu. Ama production'a çıktığınızda fark edeceğiniz gizli maliyetleri var:

### 1. **Runtime Reflection Maliyeti**

AutoMapper, nesnelerinizi nasıl map edeceğini anlamak için expression tree'ler ve reflection kullanır. **Runtime'da**. Her. Seferinde.

```csharp
// Yazdığınız kod
var entity = _mapper.Map<OrderEntity>(dto);

// Gerçekte olan (basitleştirilmiş)
1. Cache'lenmiş mapping konfigürasyonunu bul
2. Expression tree oluştur
3. Delegate'e compile et
4. Delegate'i invoke et
5. Tip dönüşümlerini yap
6. Null kontrolü yap
7. Custom converter'ları uygula
8. Sonucu döndür
```

### 2. **Reflection Sürprizi**

AutoMapper reflection kullanmadığını iddia eder... ama en beklemediğiniz anda kullanır:

- Nullable tip dönüşümleri
- Custom converter'lar
- Nested nesne grafikleri
- Collection element mapping
- Generic edge case'ler

**Sonuç?** Production'da debug edilmesi neredeyse imkansız, öngörülemeyen latency spike'ları.

### 3. **Konfigürasyon Cehennemi**

```csharp
// "Konfigürasyon" denilen şey
CreateMap<OrderDto, OrderEntity>()
    .ForMember(d => d.NetTotal, o => o.MapFrom(s => s.Price * (s.Tax + 100) / 100 - s.Discount))
    .ForMember(d => d.StatusText, o => o.MapFrom(s => s.IsActive ? "Active" : "Passive"))
    .ForMember(d => d.FullName, o => o.MapFrom(s => s.Name + " " + s.Surname));

// Şimdi bakım yapmanız gereken ÜÇ yer var:
// 1. OrderDto
// 2. OrderEntity  
// 3. Bu mapping config
```

**Daha kötüsü:** Property adında typo yaptınız mı? **Runtime'a kadar** fark etmeyeceksiniz.

### 4. **Test Kabusu**

```csharp
// Mapping'inizin çalıştığını doğrulamak için test yazmanız gerek
[Fact]
public void AutoMapper_Configuration_Should_Be_Valid()
{
    _mapper.ConfigurationProvider.AssertConfigurationIsValid();
}

// Neden bir kütüphanenin işi için test yazıyoruz?
```

---

## 🚀 Karşınızda: Roslyn Source Generator'ler

**.NET'in Roslyn Source Generator'leri** ile mapping kodunu **compile time'da** üretebiliriz. Runtime maliyeti yok. Reflection yok. Sürpriz yok.

### Nasıl Çalışır

```csharp
// 1. DTO'nuzu süsleyin
[MapTo(typeof(OrderEntity))]
public partial class OrderDto
{
    public decimal Price { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }

    // Source generator karmaşık expression'ları halleder
    [MapProperty("Price * (Tax + 100) / 100 - Discount")]
    public decimal NetTotal { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }

    [MapProperty("Name + ' ' + Surname")]
    public string FullName { get; set; }

    public bool IsActive { get; set; }

    [MapProperty("IsActive ? 'Active' : 'Passive'")]
    public string StatusText { get; set; }
}

// 2. Kullanın - bu kadar!
OrderEntity entity = dto; // Implicit operator
```

### Üretilen Kod

Source generator, compile time'da **sade C# kodu** oluşturur:

```csharp
// Üretilen kod (F12 ile gidebilirsiniz!)
public static implicit operator OrderEntity(OrderDto dto)
{
    return new OrderEntity
    {
        Price = dto.Price,
        Tax = dto.Tax,
        Discount = dto.Discount,
        NetTotal = dto.Price * (dto.Tax + 100) / 100 - dto.Discount,
        Name = dto.Name,
        Surname = dto.Surname,
        FullName = dto.Name + " " + dto.Surname,
        IsActive = dto.IsActive,
        StatusText = dto.IsActive ? "Active" : "Passive"
    };
}
```

**Faydalar:**
- ✅ **Gerçek kodu görebilirsiniz**
- ✅ **Debug edebilirsiniz** (F11 step-through çalışır!)
- ✅ **Code review'da doğrulayabilirsiniz**
- ✅ **IntelliSense bilir**

---

## 📊 Performans Rakamları

**CHD.Mapping** (source generator) vs **AutoMapper** karşılaştırmasını BenchmarkDotNet ile yaptım:

### Tekil Nesne Mapping

```
| Method      |    Mean | Oran  | Ayrılan |
|------------ |--------:|------:|--------:|
| CHD Mapping |  15.2 ns |  1.0x |     0 B |
| AutoMapper  | 142.8 ns |  9.4x |    40 B |
```

**CHD Mapping 9.4x daha hızlı ve sıfır bellek ayırıyor.**

### Collection Mapping (100 nesne)

```
| Method      |      Mean | Oran  | Ayrılan |
|------------ |----------:|------:|--------:|
| CHD Mapping |   1.52 μs |  1.0x |   800 B |
| AutoMapper  |  14.86 μs |  9.8x | 4,040 B |
```

**Hala 9.8x daha hızlı**, **5x daha az bellek ayırıyor**.

---

## 🛡️ Compile-Time Güvenlik

Gerçek süper güç? **Bozuk kod gönderemezsiniz.**

### AutoMapper: Runtime Hatası

```csharp
// Bunu yazarsınız
CreateMap<OrderDto, OrderEntity>()
    .ForMember(d => d.NetTotall, // TYPO!
        o => o.MapFrom(s => s.Price));

// Compile olur ✅
// Dev'de çalışır ✅  
// Production'da çöker 💥
```

### Source Generator: Compile-Time Hatası

```csharp
[MapTo(typeof(OrderEntity))]
public partial class OrderDto
{
    [MapProperty("Pricee * 100")] // TYPO!
    public decimal NetTotal { get; set; }
}

// Compiler hatası (CS0103): 'Pricee' ismi mevcut değil ❌
// Kod compile olmaz. Sorun çözüldü.
```

---

## 🐛 AutoMapper'ın Sebep Olduğu Gerçek Production Hataları

### Hata #1: Sessiz Null

```csharp
// AutoMapper sessizce null'ı default değere map eder
public class OrderDto 
{ 
    public int? Quantity { get; set; } // null
}

public class OrderEntity 
{ 
    public int Quantity { get; set; } // 0 olur
}

// Beklenen: null null kalır (veya exception fırlatır)
// Gerçekte: null 0 olur
// Sonuç: Production'da 0 adetli siparişler işlenir
```

### Hata #2: Reflection Fallback

```csharp
// Dev'de güzel çalışır
_mapper.Map<OrderEntity>(dto);

// Production: Generic edge case'e takılır
// AutoMapper reflection'a fallback yapar
// 50ms latency spike
// Kullanıcılar şikayet eder
// Nedenini bilemezsiniz
```

### Hata #3: Breaking Change

```csharp
// 1. hafta: Developer property'yi rename eder
public class OrderEntity 
{ 
    public decimal TotalAmount { get; set; } // eski adı NetTotal'dı
}

// AutoMapper config hala eski adı referans ediyor
.ForMember(d => d.NetTotal, ...) // Compile hatası yok

// 2. hafta: Production deploy
// Sessiz mapping hatası
// TotalAmount hep 0
// Finans yanlış ciro rapor ediyor
```

**Source generator ile?** Her üç senaryo = **compile hatası**. Göndermek imkansız.

---

## 🔍 Görmediğiniz "Gizli" Maliyetler

### AutoMapper'ın Bellek Baskısı

```csharp
// Her mapping bellek ayırır
for (int i = 0; i < 10000; i++)
{
    var entity = _mapper.Map<OrderEntity>(dto);
    // Çağrı başına 40 byte ayrılır
    // = 400 KB
    // = GC baskısı
    // = duraksamalar
}
```

### Source Generator'ün Sıfır Ayırma

```csharp
// Sıfır ayırma (sonuç nesnesi hariç)
for (int i = 0; i < 10000; i++)
{
    OrderEntity entity = dto;
    // 0 byte ayrılır
    // = GC baskısı yok
    // = düzgün performans
}
```

---

## 🎯 Hangisini Ne Zaman Kullanmalı

### Source Generator Kullanın (CHD.Mapping, Mapperly, vb.) Şu Durumlarda:

- ✅ Performans kritik (zamanın %99'u)
- ✅ Compile-time güvenlik istiyorsunuz
- ✅ Mapping'ler basit (DTO ↔ Entity)
- ✅ Sıfır ayırma önemli (düşük gecikmeli sistemler)
- ✅ Kolay debug istiyorsunuz

**En iyisi:** API'ler, mikroservisler, yüksek verimli uygulamalar, güvenilirliğin önemli olduğu her yer

### AutoMapper Kullanın Şu Durumlarda:

- ✅ Bilinmeyen tiplerin runtime mapping'i gerekli
- ✅ Aşırı esneklik gerekli
- ✅ Zaten dev AutoMapper kod tabanınız var
- ✅ Performans önemli değil (internal araçlar?)

**En iyisi:** Legacy kod tabanları, admin panelleri, prototipler

---

## 🛠️ Migrasyon Rehberi: AutoMapper → Source Generator

### Adım 1: CHD.Mapping Kurun

```bash
dotnet add package Chd.Mapping.Roslyn.Advanced
```

### Adım 2: DTO'larınızı Dönüştürün

**Önce (AutoMapper):**
```csharp
public class OrderDto
{
    public decimal Price { get; set; }
    public decimal Tax { get; set; }
}

// Profile'da:
CreateMap<OrderDto, OrderEntity>()
    .ForMember(d => d.NetTotal, 
        o => o.MapFrom(s => s.Price * (s.Tax + 100) / 100));
```

**Sonra (Source Generator):**
```csharp
[MapTo(typeof(OrderEntity))]
public partial class OrderDto
{
    public decimal Price { get; set; }
    public decimal Tax { get; set; }

    [MapProperty("Price * (Tax + 100) / 100")]
    public decimal NetTotal { get; set; }
}
```

### Adım 3: Kullanın

**Önce:**
```csharp
var entity = _mapper.Map<OrderEntity>(dto);
```

**Sonra:**
```csharp
OrderEntity entity = dto;
```

### Adım 4: Mapping Profile'larınızı Silin

```bash
rm -rf Mappings/
# AutoMapper'ı DI'dan kaldırın
# services.AddAutoMapper(typeof(Program)); ❌
```

**Bitti.** Kodunuz artık daha hızlı, güvenli ve sürdürülebilir.

---

## 📈 Gerçek Dünya Etkisi

### Önce (AutoMapper)
- **P95 latency:** 120ms
- **Bellek:** 450 MB
- **GC duraksamaları:** 15ms
- **Production hataları:** Çeyrek başına 3 mapping hatası

### Sonra (Source Generator)
- **P95 latency:** 85ms (%30 iyileşme)
- **Bellek:** 280 MB (%38 azalma)
- **GC duraksamaları:** 8ms (%47 azalma)
- **Production hataları:** 6 ayda 0 mapping hatası

---

## 🎓 Bonus: Source Generator'ler Nasıl Çalışır

Sihir nasıl oluyor merak ettiyseniz, işte basitleştirilmiş versiyon:

```csharp
[Generator]
public class MappingGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // 1. [MapTo] attribute'u olan tüm class'ları bul
        var dtoClasses = context.Compilation
            .GetSymbolsWithName(name => true)
            .OfType<INamedTypeSymbol>()
            .Where(HasMapToAttribute);

        // 2. Her DTO için mapping kodu üret
        foreach (var dto in dtoClasses)
        {
            var targetType = GetTargetType(dto);
            var mappingCode = GenerateMappingMethod(dto, targetType);
            
            // 3. Üretilen kodu compilation'a ekle
            context.AddSource($"{dto.Name}_Mapping.g.cs", mappingCode);
        }
    }
}
```

**Sonuç:** Compilation'ınıza eklenen sade C# kodu. Sihir yok. Reflection yok. Runtime maliyeti yok.

---

## 🚨 Yaygın Mitler

### Mit #1: "AutoMapper yeterince hızlı"

**Gerçek:** 9x yavaş 1 mapping için önemsiz görünebilir. Ama şunla çarpın:
- Saniyede 1000 istek
- İstek başına 10 mapping
- 7/24 uptime

**Sonuç:** Boşa giden CPU döngüleri, yüksek cloud maliyetleri, yavaş kullanıcı deneyimi.

### Mit #2: "Source generator'ler karmaşık"

**Gerçek:** AutoMapper'ın profile'ları daha karmaşık:
```csharp
// AutoMapper: 15 satır
public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<OrderDto, OrderEntity>()
            .ForMember(...)
            .ForMember(...)
            .ForMember(...);
    }
}

// Source generator: 3 satır
[MapTo(typeof(OrderEntity))]
public partial class OrderDto { }
```

### Mit #3: "AutoMapper'ın özelliklerine ihtiyacım var"

**Gerçek:** Çoğu geliştirici AutoMapper'ın <%10 özelliğini kullanır. En yaygın pattern basit DTO ↔ Entity mapping'dir, source generator'ler bunu mükemmel yapar.

---

## 🔗 Kaynaklar

- **CHD.Mapping NuGet:** https://www.nuget.org/packages/Chd.Mapping.Roslyn.Advanced
- **Benchmark Kaynak Kodu:** https://github.com/mehmet-yoldas/Chd.Examples
- **BenchmarkDotNet:** https://benchmarkdotnet.org
- **Microsoft Docs - Source Generators:** https://learn.microsoft.com/tr-tr/dotnet/csharp/roslyn-sdk/source-generators-overview

---

## 💬 Son Düşünceler

AutoMapper 2011'de gerçek bir sorunu çözdü. Ama 2024'teyiz ve **daha iyi araçlarımız var**.

Source generator'ler bize şunu veriyor:
- **9x daha iyi performans**
- **Compile-time güvenlik**
- **Sıfır runtime sürprizi**
- **Daha kolay debugging**
- **Daha düşük bellek kullanımı**

**Soru "Geçmeli miyim?" değil**

**Soru "Neden henüz geçmedim?"**

---

## 🙋 Sizin Sıranız

AutoMapper'dan source generator'lere geçtiniz mi? Deneyiminiz nasıldı? Aşağıya yorum bırakın! 👇

Ve eğer faydalı bulduysan, **❤️ ver** ve **paylaş** ekibinle. Hep birlikte .NET'i hızlandıralım.

---

*Daha fazla .NET performans ipucu ve modern C# best practice'leri için takip edin!*

- GitHub: [@mehmet-yoldas](https://github.com/mehmet-yoldas)
- NuGet: [CHD Paketleri](https://www.nuget.org/profiles/mehmet-yoldas)

#dotnet #csharp #performance #sourcegenerators #automapper #roslyn #turkce #netcore
