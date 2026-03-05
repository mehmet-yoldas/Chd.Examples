# 🎯 Sadece Visual Studio ile Metrik Görüntüleme

## ⚠️ ÖNEMLİ: Dashboard Güncellendi!

Metrikler görünmüyorsa, monitoring stack'i yeniden başlatın:
```bash
cd /d C:\Projects\Demo
docker-compose -f docker-compose.monitoring.yml down
docker-compose -f docker-compose.monitoring.yml up -d
```

**Veya** basitçe `test-metrics.bat` çalıştırın!

---

## ✨ Tek Tuşla Her Şey!

Artık **sadece F5 basarak** hem load test çalışır hem de Grafana'da metrikleri görebilirsiniz!

---

## 🚀 Kullanım (Süper Basit!)

### 1. Visual Studio'yu Aç

```
File → Open → Project/Solution
→ Chd.Examples.sln seç
```

### 2. Startup Project Ayarla

```
Solution Explorer → Chd.LoadTests → Sağ tık → Set as Startup Project
```

### 3. F5 Bas!

```
F5 tuşuna bas (veya Debug → Start Debugging)
```

### 4. İlk Çalıştırmada Ne Olur?

```
===========================================
CHD Coordination - Load Tests with NBomber
===========================================

========================================
   Monitoring Stack Check
========================================

✅ Docker is running
⚠️  Monitoring stack is not running

🚀 Start monitoring stack automatically? (y/n): _
```

**"y" yazın ve Enter basın** ✅

Program otomatik olarak:
1. ✅ Docker Compose ile monitoring stack'i başlatır
2. ✅ Grafana, Prometheus, Redis'i ayağa kaldırır
3. ✅ 10-15 saniye bekler (servisler hazır olsun diye)
4. ✅ Tarayıcıda Grafana dashboard'unu açar

### 5. Sonraki Çalıştırmalarda

Monitoring stack zaten çalışıyorsa:

```
========================================
   Monitoring Stack Check
========================================

✅ Docker is running
✅ Monitoring stack is already running

📊 View metrics at:
   - Grafana:    http://localhost:3000 (admin/admin)
   - Dashboard:  http://localhost:3000/d/chd-coordination
   - Prometheus: http://localhost:9090

Open Grafana dashboard in browser? (y/n): _
```

**"y" yazın** → Dashboard direkt açılır! 🎉

---

## 📺 Demo Workflow (Video Gibi)

```
1. Visual Studio'yu açın
   ↓
2. Chd.LoadTests → Set as Startup Project
   ↓
3. F5 basın
   ↓
4. Program başlar:
   "Start monitoring stack automatically? (y/n):"
   ↓
5. "y" yazın + Enter
   ↓
6. 10-15 saniye bekleme
   "Monitoring stack started successfully!"
   ↓
7. "Open Grafana dashboard in browser? (y/n):"
   ↓
8. "y" yazın + Enter
   ↓
9. Tarayıcıda Grafana açılır
   (Login: admin/admin)
   ↓
10. Console'da test senaryosu seçin
    "Select scenario (1-5):"
    ↓
11. "1" yazın (Lock Test)
    ↓
12. Test başlar
    ↓
13. Grafana'da metrikleri CANLI görürsünüz!
    📈 Operations/sec
    ⏱️  Latency
    ✅ Success rate
```

**Toplam süre:** ~30 saniye (ilk kez için)  
**Sonraki defalar:** ~5 saniye (zaten hazır)

---

## 🎯 Özet: Ne Değişti?

### Önceden (Manuel):
```sh
# Terminal 1
.\start-monitoring.bat

# Terminal 2
cd Chd.LoadTests
dotnet run

# Browser
http://localhost:3000
# Manuel login
# Manuel dashboard bulma
```

**3 adım, 3 araç, 2-3 dakika** 😫

### Şimdi (Otomatik):
```
Visual Studio → F5 → "y" → "y" → Hazır!
```

**1 adım, 1 araç, 30 saniye** 🚀

---

## 🔧 Nasıl Çalışıyor? (Teknik Detay)

### MonitoringHelper.cs

**3 ana fonksiyon:**

#### 1. IsDockerRunning()
```csharp
// Docker çalışıyor mu kontrol eder
docker ps
```

#### 2. IsMonitoringStackRunning()
```csharp
// Grafana container'ı var mı kontrol eder
docker ps --filter name=chd-grafana
```

#### 3. StartMonitoringStack()
```csharp
// Docker Compose ile stack'i başlatır
docker-compose -f docker-compose.monitoring.yml up -d
```

#### 4. OpenGrafanaDashboard()
```csharp
// OS'e göre tarayıcıda açar
// Windows: start http://...
// Mac: open http://...
// Linux: xdg-open http://...
```

### Program.cs

**Main() başında:**

```csharp
public static void Main(string[] args)
{
    // Header
    Console.WriteLine("CHD Coordination - Load Tests...");
    
    // YENİ: Otomatik monitoring kontrolü
    CheckAndStartMonitoring();
    
    // Prometheus metrics server
    _prometheusServer = new PrometheusServer(port: 9091);
    _prometheusServer.Start();
    
    // Test seçimi
    // ...
}
```

**CheckAndStartMonitoring():**

```csharp
1. Docker çalışıyor mu? → Hayır ise uyar
2. Monitoring stack çalışıyor mu?
   → Evet ise: "Open dashboard?" sor
   → Hayır ise: "Start automatically?" sor
3. Başlat ve dashboard'u aç
```

---

## 🎨 Kullanıcı Deneyimi

### İlk Kez (Docker kapalı)

```
========================================
   Monitoring Stack Check
========================================

⚠️  [WARNING] Docker is not running!
   Metrics will NOT be available in Grafana.
   Please start Docker Desktop to see metrics.

   Load tests will still run, but without visualization.

Continue without monitoring? (y/n): n
Exiting...
```

**Çözüm:** Docker Desktop'ı başlatın, tekrar F5 basın.

### İlk Kez (Docker açık, Stack yok)

```
========================================
   Monitoring Stack Check
========================================

✅ Docker is running
⚠️  Monitoring stack is not running

🚀 Start monitoring stack automatically? (y/n): y
   Solution directory: C:\Projects\Demo

[INFO] Starting monitoring stack...
       This may take 10-15 seconds...

[SUCCESS] Monitoring stack started!
          - Grafana:    http://localhost:3000 (admin/admin)
          - Prometheus: http://localhost:9090
          - Dashboard:  http://localhost:3000/d/chd-coordination

[INFO] Waiting for services to be ready...
[INFO] Services should be ready now!

Open Grafana dashboard in browser? (y/n): y
[INFO] Opening Grafana dashboard in browser...
       URL: http://localhost:3000/d/chd-coordination

========================================
```

### Sonraki Defalar (Her şey hazır)

```
========================================
   Monitoring Stack Check
========================================

✅ Docker is running
✅ Monitoring stack is already running

📊 View metrics at:
   - Grafana:    http://localhost:3000 (admin/admin)
   - Dashboard:  http://localhost:3000/d/chd-coordination
   - Prometheus: http://localhost:9090

Open Grafana dashboard in browser? (y/n): y
[INFO] Opening Grafana dashboard in browser...
       URL: http://localhost:3000/d/chd-coordination

========================================
```

---

## 💡 Pro Tips

### Tip 1: Otomatik Dashboard Açılsın

Launch settings'e ekleyin:

```json
{
  "profiles": {
    "Lock Test (Auto Dashboard)": {
      "commandName": "Project",
      "commandLineArgs": "1",
      "environmentVariables": {
        "AUTO_OPEN_DASHBOARD": "true"
      }
    }
  }
}
```

### Tip 2: Soru Sormadan Başlatsın

Environment variable ekleyin:

```json
"environmentVariables": {
  "AUTO_START_MONITORING": "true",
  "AUTO_OPEN_DASHBOARD": "true"
}
```

### Tip 3: İki Monitör Kullanın

- **Monitör 1:** Visual Studio (kod, console)
- **Monitör 2:** Grafana (real-time metrikler)

Mükemmel developer experience! 🚀

### Tip 4: Debug Sırasında Metrik İzleyin

1. Breakpoint koyun
2. F5 basın
3. Grafana'da metrikleri izleyin
4. Breakpoint'te durun
5. Grafana'da ne olduğunu görün

**Örnek:**
```csharp
await coordinator.Lock.RunAsync(...);
// ← Breakpoint burada

// Grafana'da:
// - Lock operations spike görülür
// - Latency artar
// - Sonra düşer (breakpoint'te beklediği için)
```

---

## 🐛 Sorun Giderme

### "docker-compose.monitoring.yml not found"

**Sebep:** Solution dizini bulunamadı

**Çözüm:**
```csharp
// MonitoringHelper.cs'te kontrol edin
var solutionDir = MonitoringHelper.GetSolutionDirectory();
Console.WriteLine($"Solution dir: {solutionDir}");

// Manuel ayarlayın
var composeFile = @"C:\Projects\Demo\docker-compose.monitoring.yml";
```

### "Docker is not running"

**Çözüm:**
1. Docker Desktop'ı başlatın
2. 30 saniye bekleyin (Docker tam başlasın)
3. Visual Studio'da tekrar F5

### "Port 9091 already in use"

**Sebep:** Prometheus metrics server zaten çalışıyor

**Çözüm:**
```sh
# Önceki instance'ı kapat
Get-Process -Name "Chd.LoadTests" | Stop-Process

# Veya port değiştir
var server = new PrometheusServer(port: 9092);
```

### Grafana Dashboard Boş

**Sebep:** Test henüz başlamadı

**Çözüm:**
1. Senaryo seçin (1-5)
2. 10-15 saniye bekleyin
3. Grafana'da refresh (F5)

---

## 📊 Karşılaştırma

### Manuel Yöntem
```
Adımlar: 7-8
Araçlar: Terminal, Browser, Visual Studio
Süre: 2-3 dakika
Hata riski: Yüksek (komutları unutma)
```

### Otomatik Yöntem (Şimdi)
```
Adımlar: 3
Araçlar: Sadece Visual Studio
Süre: 30 saniye (ilk kez), 5 saniye (sonraki)
Hata riski: Düşük (otomatik kontrol)
```

**Verimlilik artışı: %80** 🚀

---

## ✅ Checklist

Hazır mısınız?

- [ ] Docker Desktop kurulu ve çalışıyor
- [ ] Visual Studio 2022 açık
- [ ] Chd.LoadTests startup project olarak ayarlı
- [ ] F5 basabilecek kadar cesaret var 😄

Hepsi ✅ ise → **F5 basın!** 🎉

---

## 🎬 Video Gibi Anlatım

```
┌─────────────────────────────────────┐
│   Visual Studio 2022                │
│                                     │
│   [F5] ← BAS                        │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Console                           │
│                                     │
│   Start monitoring? (y/n): y        │
└─────────────────────────────────────┘
              ↓
      [10 saniye bekleme]
              ↓
┌─────────────────────────────────────┐
│   Browser                           │
│                                     │
│   Grafana Dashboard 📊              │
│   ┌─────────────────────────┐       │
│   │ Operations/sec          │       │
│   │ ▁▂▃▅▇█ (Real-time!)     │       │
│   └─────────────────────────┘       │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Console                           │
│                                     │
│   Select scenario (1-5): 1          │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   Grafana'da metrikler CANLI        │
│   görünüyor! 🎉                     │
└─────────────────────────────────────┘
```

---

**🎉 Artık sadece Visual Studio'da F5 basıp metrikleri görebilirsiniz!**

**Ekran görüntüsü alın ve paylaşın!** 📸

```
Visual Studio (kod) + Grafana (metrikler) = Developer Heaven 🚀
```
