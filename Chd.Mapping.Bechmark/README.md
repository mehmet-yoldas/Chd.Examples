# CHD Mapping vs AutoMapper – Detaylı Benchmark ve Karşılaştırma

Bu repository, **CHD Mapping (source‑generated / compile‑time mapping)** ile **AutoMapper (runtime mapping)** arasındaki **performans, bellek kullanımı ve mimari farkları** objektif bir şekilde göstermek amacıyla hazırlanmıştır.

Amaç bir "kazanan" ilan etmek değil; **hangi senaryoda hangi yaklaşımın mantıklı olduğunu** net rakamlarla ortaya koymaktır.

---

## 📌 Karşılaştırılan Yaklaşımlar

### 1️⃣ CHD Mapping (Source Generator)

* DTO üzerine **attribute** yazılır
* Mapping kodu **compile‑time’da üretilir**
* Sonuçta elde edilen kod:

  * Düz C# kodu
  * Reflection yok
  * Expression Tree yok
  * Runtime configuration yok

Örnek:

```csharp
[MapTo(typeof(OrderEntity))]
public partial class OrderDto
{
    public decimal Price { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }

    [MapProperty("Price *(Tax+100)/100 - Discount")]
    public decimal NetTotal { get; set; }

    public bool IsActive { get; set; }

    [MapProperty("IsActive ? 'Active' : 'Passive'")]
    public string StatusText { get; set; }
}
```

Kullanım:

```csharp
OrderEntity entity = dto; // implicit operator
```

---

### 2️⃣ AutoMapper (Runtime Mapping)

* Mapping kuralları **runtime’da tanımlanır**
* Configuration aşaması zorunludur
* Mapping çağrısı sırasında:

  * Expression Tree çalışır
  * Delegate invoke edilir
  * Bazı senaryolarda reflection fallback oluşabilir

Örnek:

```csharp
CreateMap<OrderDto, OrderEntity>()
    .ForMember(d => d.NetTotal,
        o => o.MapFrom(s => s.Price * (s.Tax + 100) / 100 - s.Discount))
    .ForMember(d => d.StatusText,
        o => o.MapFrom(s => s.IsActive ? "Active" : "Passive"));
```

Kullanım:

```csharp
_mapper.Map<OrderEntity>(dto);
```

---

## ⚖️ Adil Karşılaştırma İçin Alınan Önlemler

Bu benchmark **AutoMapper’a adil davranacak şekilde** hazırlanmıştır:

* ✅ `MapperConfiguration` **benchmark dışında** oluşturulmuştur
* ✅ Aynı business logic her iki tarafta da uygulanmıştır
* ❌ Configuration süresi ölçüme dahil edilmemiştir
* ❌ İlk çalıştırma (cold start) maliyeti ölçülmemiştir

Yani ölçülen fark **saf mapping maliyetidir**.

---

## 📊 Benchmark Sonuçları (Single Object)

| Method      |    Mean |    Ratio |
| ----------- | ------: | -------: |
| CHD Mapping |  ~15 ns |     1.00 |
| AutoMapper  | ~135 ns | **9.0x** |

### Yorum:

* Tek nesne için bile **order‑of‑magnitude fark** oluşuyor
* Mapping logic karmaşıklaştıkça fark **kapanmıyor**, aksine artıyor

---

## ⚠️ AutoMapper’da Görülmeyen Ama Yaşanan Gerçek: Reflection Sürprizi

AutoMapper çoğu zaman "reflection kullanmıyor" gibi algılansa da:

* Complex mapping
* Nullable / converter zincirleri
* Generic edge‑case’ler
* Collection içi nested mapping

senaryolarında **reflection fallback** devreye girebilir.

Bu:

* Predict edilemeyen latency
* GC spike
* Production’da sürpriz performans düşüşü

olarak geri döner.

CHD Mapping’te bu **teorik olarak mümkün değildir**, çünkü:

> Reflection içeren kod compile‑time’da zaten reddedilir.

---

## 🧠 Konfigürasyon Karmaşıklığı Karşılaştırması

### AutoMapper

* Profile
* Configuration
* DI entegrasyonu
* Validation
* Startup cost

### CHD Mapping

* DTO üstüne attribute
* Başka hiçbir şey yok

Bu fark özellikle:

* Microservice
* Serverless
* Cold‑start hassas sistemler

için kritiktir.

---

## ✅ Ne Zaman Hangisi?

### AutoMapper'in daha mantıklı olduğu senaryolar

- Dinamik mapping senaryoları varsa
- Mapping kuralları sık değişiyorsa
- Convention-based yaklaşım tercih ediliyorsa
- Takım AutoMapper ekosistemine hâkimse
- Performans kritik bir gereksinim değilse

> Esneklik öncelikliyse AutoMapper hâlâ güçlü bir araçtır.

---

### CHD Mapping daha mantıklı  olduğu senaryolar

- CRUD ağırlıklı uygulamalar
- Mapping noktaları sabit ve öngörülebilir ise
- Mapping işlemi uygulamanın en sık çalışan akışında yer alıyorsa
- Runtime sürprizi istenmiyorsa
- Mapping hatalarının compile-time’da yakalanması isteniyorsa
- Okunabilir, debug edilebilir düz C# kodu tercih ediliyorsa

> Deterministik davranış ve düşük runtime maliyeti hedefleniyorsa  
> compile-time mapping net bir avantaj sağlar.


---

## 🎯 Sonuç

Bu benchmark’ın iddiası şudur:

> ** Mapping kodu derleme aşamasında (compile-time) üretilebilir, bunu uygulama çalışırken (runtime) üretmeye çalışmak kaçınılmaz olarak ek maliyet oluşturur.
Runtime mapping; reflection, expression tree oluşturma, mapping planı hazırlama ve önbellekleme gibi işlemler yapar. Bu işlemler iş mantığının bir parçası değildir ve her uygulama başlatıldığında ya da ilk kullanımda performans bedeli ödetir.
Compile-time mapping ise bu hazırlıkların tamamını derleme sırasında çözer ve runtime’da yalnızca düz, doğrudan C# kodu çalıştırır.  **

Bu maliyet:

* Tek nesnede bile ölçülebilir
* Collection’da büyür
* Production’da sürpriz yapabilir

Bu repo, bu farkı **rakamlarla** gösterir.

---

📌 Benchmarklar: BenchmarkDotNet
📌 .NET: 8.0
📌 AutoMapper: 16.x

---

> Sorular, katkılar ve karşı benchmark PR’ları memnuniyetle karşılanır.
