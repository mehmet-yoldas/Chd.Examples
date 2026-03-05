# Visual Studio'da NBomber Load Tests Kullanımı

## 🎯 Hızlı Başlangıç

### 1. Startup Project Olarak Ayarla

![Set Startup Project](https://via.placeholder.com/600x100/0078D4/FFFFFF?text=Solution+Explorer+%E2%86%92+Chd.LoadTests+%E2%86%92+Sağ+Tık+%E2%86%92+Set+as+Startup+Project)

1. **Solution Explorer**'ı açın (Ctrl+Alt+L)
2. **Chd.LoadTests** projesine **sağ tıklayın**
3. **"Set as Startup Project"** seçin
4. Proje **kalın** olur (startup project olduğunu gösterir)

### 2. Çalıştır

**F5** - Debug modda çalıştır  
**Ctrl+F5** - Debug olmadan çalıştır (daha hızlı)

### 3. Senaryo Seç

Console açılır, istediğiniz senaryoyu seçin (1-5).

---

## 🚀 Launch Profiles (Önerilen)

Artık **her senaryo için ayrı launch profile** var!

### Launch Profile Nasıl Kullanılır?

1. **Toolbar**'da **"Chd.LoadTests"** dropdown'ını bulun
2. Açın, şunları göreceksiniz:

```
Chd.LoadTests                    ← Manuel seçim (interactive)
Lock Test (Debug)                ← Otomatik Lock Test
Idempotency Test (Debug)         ← Otomatik Idempotency Test
Saga Test (Debug)                ← Otomatik Saga Test
Mixed Workload (Debug)           ← Otomatik Mixed Test
Run All Tests                    ← Hepsini çalıştır
```

3. İstediğinizi seçin
4. **F5** basın

**Avantaj:** Senaryo seçmek için beklemenize gerek yok, direkt başlar!

---

## 🐛 Debug Yapma

### Breakpoint Koyma

1. **`Program.cs`** dosyasını açın
2. İstediğiniz satırın **sol kenarına tıklayın** (kırmızı nokta görünür)
3. Veya satırda iken **F9** basın

**Örnek:**

```csharp
private static void RunLockTest()
{
    Console.WriteLine("\n[Running] Distributed Lock Load Test...\n");
    
    var serviceProvider = CreateServiceProvider();
    
    var scenario = Scenario.Create("distributed_lock_test", async context =>
    {
        var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
        var resourceId = $"resource:{context.ScenarioInfo.ThreadNumber}";
        
        // ← Buraya breakpoint koy (F9 veya sol kenara tıkla)
        try
        {
            await coordinator.Lock.RunAsync(
                resourceId,
                ttl: TimeSpan.FromSeconds(5),
                action: async () =>
                {
                    // ← Buraya da breakpoint koyabilirsin
                    await Task.Delay(50);
                    // Program burada duracak!
                }
            );
            return Response.Ok();
        }
        catch (Exception ex)
        {
            // ← Hata olursa buraya breakpoint koy
            return Response.Fail($"Lock failed: {ex.Message}");
        }
    })
    // ...
```

### Debug Sırasında Yapabilecekleriniz

#### 1. Step Through (Adım Adım İlerle)

| Tuş | İşlem | Açıklama |
|-----|-------|----------|
| **F10** | Step Over | Bir sonraki satıra geç |
| **F11** | Step Into | Metodun içine gir |
| **Shift+F11** | Step Out | Metoddan çık |
| **F5** | Continue | Bir sonraki breakpoint'e kadar devam et |

#### 2. Watch Window (Değişken İzleme)

1. **Debug → Windows → Watch → Watch 1**
2. İzlemek istediğiniz değişkeni yazın:
   - `coordinator`
   - `resourceId`
   - `context.ScenarioInfo.ThreadNumber`

#### 3. Immediate Window (Hemen Kod Çalıştır)

1. **Debug → Windows → Immediate** (Ctrl+Alt+I)
2. Kod yazın ve Enter basın:

```csharp
// Değişken değerini gör
? resourceId
// Sonuç: "resource:1"

// Metod çağır
? coordinator.Lock.IsLockedAsync("test").Result
// Sonuç: false

// Yeni değişken oluştur
var testKey = "test:123"
```

#### 4. Locals Window (Yerel Değişkenler)

1. **Debug → Windows → Locals**
2. Tüm yerel değişkenleri otomatik gösterir

#### 5. Call Stack (Çağrı Yığını)

1. **Debug → Windows → Call Stack** (Ctrl+Alt+C)
2. Buraya nasıl geldiğinizi görün

---

## 🎨 Visual Studio Özellikleri

### 1. IntelliSense

Kod yazarken otomatik tamamlama:

```csharp
var scenario = Scenario.
    // ← IntelliSense açılır:
    //   - Create
    //   - CreatePause
    // vs.
```

### 2. Go to Definition (F12)

Herhangi bir metoda sağ tıklayın → **"Go to Definition"** → NBomber kaynak kodunu görün

### 3. Find All References (Shift+F12)

Bir metod/sınıf nerede kullanılıyor? → Shift+F12

### 4. Refactoring

Bir değişkeni yeniden adlandırmak isterseniz:
1. Değişkenin üzerine sağ tıklayın
2. **"Rename"** (Ctrl+R, Ctrl+R)
3. Her yerde güncellenir

---

## 📊 Test Sonuçlarını Görme

### Console Output

Test çalışırken **Output Window**'da real-time görebilirsiniz:

```
===========================================
CHD Coordination - Load Tests with NBomber
===========================================

[Running] Distributed Lock Load Test...

┌────────────────┬─────────┬─────────┬─────────┐
│ Metric         │ Value   │ StdDev  │ Status  │
├────────────────┼─────────┼─────────┼─────────┤
│ RPS            │ 195     │ 12.3    │ OK      │
│ Latency (P95)  │ 52ms    │ 8ms     │ OK      │
│ Success Rate   │ 99.8%   │ -       │ PASSED  │
└────────────────┴─────────┴─────────┴─────────┘
```

### HTML Rapor

Test bitince şunu göreceksiniz:

```
[Completed] Lock test finished. Check HTML report.

Report location: ./reports/2024-01-15_14-30-00/report.html
```

**HTML raporunu açın:**
1. Solution Explorer → Chd.LoadTests → reports klasörü
2. En son tarihli klasör
3. `report.html` dosyasına çift tıklayın
4. Tarayıcıda açılır (interaktif grafikler!)

---

## 🎯 Önerilen Workflow

### İlk Kez Çalıştırma

1. ✅ **Redis'in çalıştığından emin olun:**
   ```sh
   docker ps | findstr redis
   ```
   Çalışmıyorsa:
   ```sh
   .\start-monitoring.bat
   ```

2. ✅ **Solution'ı build edin:**
   - **Build → Build Solution** (Ctrl+Shift+B)

3. ✅ **Chd.LoadTests'i startup project yapın:**
   - Solution Explorer → Chd.LoadTests → Sağ tık → Set as Startup Project

4. ✅ **İlk testi çalıştırın:**
   - Launch profile: **"Lock Test (Debug)"** seçin
   - **F5** basın

### Normal Kullanım

1. **Launch profile seçin** (toolbar dropdown)
2. **F5** basın
3. Sonuçları inceleyin

### Debug İçin

1. **Breakpoint koyun** (F9)
2. **Launch profile seçin**
3. **F5** basın
4. Kod breakpoint'te durur
5. **Watch/Locals** pencerelerini kullanın
6. **F10** ile adım adım ilerleyin

---

## 🔥 Pro Tips

### Tip 1: Conditional Breakpoint

Sadece belirli durumlarda durmasını isterseniz:

1. Breakpoint'e **sağ tıklayın**
2. **"Conditions..."** seçin
3. Koşul yazın:

```csharp
context.ScenarioInfo.ThreadNumber == 5
```

Şimdi sadece Thread #5'te duracak!

### Tip 2: Hot Reload

.NET 8'de **Hot Reload** var:
1. Test çalışırken
2. Kod değiştirin
3. **Ctrl+Shift+F5** (Hot Reload)
4. Test durmadan güncellenir!

**Not:** NBomber senaryoları için tam desteklenmeyebilir, ama denemeye değer.

### Tip 3: Multiple Startup Projects

İsterseniz:
1. Solution'a sağ tıklayın
2. **"Set Startup Projects..."**
3. **"Multiple startup projects"** seçin
4. Hem **Chd.Coordination.Examples** hem **Chd.LoadTests** seçin

Şimdi F5 basınca ikisi de açılır!

### Tip 4: Output Window Filtreleme

**View → Output** → Dropdown'dan **"Chd.LoadTests"** seçin

Sadece load test çıktılarını gösterir.

### Tip 5: Task List

Kod içinde TODO yazarsanız:

```csharp
// TODO: Add more test scenarios
// HACK: Temporary workaround
```

**View → Task List** → Tümünü görün

---

## 🆚 k6 ile Karşılaştırma

| Özellik | k6 | NBomber (Visual Studio) |
|---------|----|--------------------------| 
| **Debugging** | ❌ console.log | ✅ Breakpoints, Watch, Call Stack |
| **IntelliSense** | ❌ Yok | ✅ Tam destek |
| **F12 (Go to Definition)** | ❌ Yok | ✅ Var |
| **Refactoring** | ❌ Manuel | ✅ Otomatik |
| **Watch Değişkenler** | ❌ Yok | ✅ Var |
| **Call Stack** | ❌ Yok | ✅ Var |
| **Step Through** | ❌ Yok | ✅ F10, F11 |
| **Immediate Window** | ❌ Yok | ✅ Var |

**Sonuç:** NBomber + Visual Studio = **Game Changer** 🚀

---

## 🐛 Sorun Giderme

### "Redis connection failed"

**Çözüm:**
```sh
# Redis çalışıyor mu?
docker ps | findstr redis

# Değilse başlat
docker run -d -p 6379:6379 --name chd-redis redis:7-alpine
```

### "Cannot find Chd.Coordination"

**Çözüm:**
```sh
# NuGet paketlerini restore et
dotnet restore
```

Visual Studio'da:
- **Tools → NuGet Package Manager → Restore NuGet Packages**

### Breakpoint Hit Olmuyor

**Çözüm:**
1. **Debug** modda çalıştığınızdan emin olun (F5, Ctrl+F5 değil)
2. **Configuration**: **"Debug"** olmalı (Release değil)
3. Toolbar → Configuration dropdown → **"Debug"** seçin

### Test Çok Yavaş

**Çözüm:**
1. **Release** modda çalıştırın (daha hızlı):
   - Configuration: **"Release"**
   - **Ctrl+F5** (Debug olmadan)

---

## 📚 Daha Fazla Bilgi

- **NBomber Docs**: https://nbomber.com/docs/overview
- **Visual Studio Debugging**: https://learn.microsoft.com/en-us/visualstudio/debugger/
- **k6 vs NBomber**: `../K6_VS_NBOMBER.md`
- **Load Test README**: `./README.md`

---

**🎉 Artık Visual Studio'da load test yapabilirsiniz! F5 basın ve başlayın!**
