# 🔧 Metrikler Görünmüyor mu? - Sorun Giderme

## ✅ Çözüm Uygulandı

Grafana dashboard'unuzdaki metrik sorguları güncellendi. Şimdi doğru metrik isimlerini kullanıyor:

### Güncellenen Metrikler:
- ✅ `chd_lock_operations_total{status="success|failed"}`
- ✅ `chd_idempotency_operations_total{status="success|failed"}`
- ✅ `chd_saga_operations_total{status="success|failed"}`
- ✅ `chd_lock_duration_seconds` (Histogram)
- ✅ `chd_idempotency_duration_seconds` (Histogram)
- ✅ `chd_saga_duration_seconds` (Histogram)

---

## 🚀 Hızlı Test (3 Adım)

### 1️⃣ Monitoring Stack'i Yeniden Başlat
```bash
test-metrics.bat
```
Bu script otomatik olarak:
- Monitoring stack'i yeniden başlatır
- Prometheus targets sayfasını açar
- Metrics endpoint'i açar
- Grafana dashboard'u açar

### 2️⃣ Visual Studio'da Load Test Çalıştır
1. **Chd.LoadTests** projesini **Startup Project** olarak seç
2. **F5** veya **Ctrl+F5** ile çalıştır
3. Herhangi bir test seçimi yap (örnek: `1` - Distributed Lock Test)

### 3️⃣ Grafana'da Metrikleri İzle
1. **Grafana**: http://localhost:3000/d/chd-coordination
2. **Login**: `admin` / `admin`
3. **Dashboard**: "CHD Coordination - Performance Dashboard"

---

## 🔍 Manuel Kontrol

### Prometheus Hedefleri Kontrolü
```
http://localhost:9090/targets
```
**Beklenen**: `chd-loadtests` hedefi **UP** durumunda olmalı

### Metriklerin Gerçek Zamanlı Görüntülenmesi
```
http://localhost:9091/metrics
```
**Beklenen**: Load test çalışırken bu sayfa şunları göstermeli:
```prometheus
# HELP chd_lock_operations_total Total number of lock operations
# TYPE chd_lock_operations_total counter
chd_lock_operations_total{status="success"} 150
chd_lock_operations_total{status="failed"} 2

# HELP chd_lock_duration_seconds Lock operation duration in seconds
# TYPE chd_lock_duration_seconds histogram
chd_lock_duration_seconds_bucket{le="0.001"} 0
chd_lock_duration_seconds_bucket{le="0.002"} 5
...
```

### Prometheus Query Testi
1. Prometheus'a git: http://localhost:9090
2. Query kutusuna yaz: `chd_lock_operations_total`
3. **Execute** tıkla
4. **Graph** sekmesinde çizgi görmelisin

---

## ❓ Hala Metrik Görünmüyorsa

### Senaryo 1: Load Test Henüz Çalışmadı
**Belirtiler:**
- Grafana dashboardu boş
- http://localhost:9091/metrics sadece process metrikleri gösteriyor
- `chd_*` metrikleri yok

**Çözüm:**
- Visual Studio'dan load test çalıştır (F5)
- En az bir test seçimi yap
- Test başladığında metrikler otomatik olarak görünmeye başlar

### Senaryo 2: Prometheus Scraping Yapamıyor
**Belirtiler:**
- http://localhost:9090/targets sayfasında `chd-loadtests` hedefi **DOWN**
- Hata mesajı: `context deadline exceeded` veya `connection refused`

**Çözüm:**
```bash
# 1. Prometheus'u yeniden başlat
docker restart chd-prometheus

# 2. Load test projesinin çalıştığından emin ol
# (Port 9091 açık olmalı)
```

### Senaryo 3: Grafana Dashboard Güncel Değil
**Belirtiler:**
- Prometheus'da metrikler var ama Grafana'da görünmüyor
- Dashboard boş

**Çözüm:**
```bash
# Monitoring stack'i tamamen yeniden başlat
docker-compose -f docker-compose.monitoring.yml down
docker-compose -f docker-compose.monitoring.yml up -d

# Dashboard'un güncellenmesi için 15 saniye bekle
```

### Senaryo 4: Redis Çalışmıyor
**Belirtiler:**
- Load testler başlamıyor
- Console'da Redis connection hatası

**Çözüm:**
```bash
# Redis container'ını kontrol et
docker ps | findstr redis

# Eğer yoksa monitoring stack'i başlat
docker-compose -f docker-compose.monitoring.yml up -d
```

---

## 📊 Grafana Dashboard Panelleri

Dashboard şu 6 paneli içeriyor:

1. **Operations per Second** (Line Chart)
   - Lock, Idempotency, Saga işlem hızları
   - Real-time rate calculation

2. **P95 Latency** (Gauge)
   - 95. yüzdelik dilim latency
   - Sarı: >100ms, Kırmızı: >500ms

3. **Distributed Lock Stats** (Line Chart)
   - Başarılı/başarısız lock operasyonları
   - 5 dakikalık artış

4. **Idempotency Stats** (Line Chart)
   - Başarılı/başarısız idempotency operasyonları
   - 5 dakikalık artış

5. **Saga Statistics** (Stat Panel)
   - Toplam saga sayısı
   - Başarısız saga sayısı

6. **Active Tests** (Stats)
   - Şu anda çalışan test sayısı

---

## 🎯 Beklenen Davranış

### Test Çalıştırırken:
1. **İlk 5 saniye**: Metrikler görünmeye başlar
2. **30 saniye sonra**: Grafiklerde çizgiler belirgin hale gelir
3. **1 dakika sonra**: Rate ve latency hesaplamaları stabilize olur
4. **Test bittiğinde**: Metrikler sıfırlanmaz, kümülatif olarak kalır

### Dashboard Refresh:
- **Otomatik yenileme**: 5 saniye
- **Zaman aralığı**: Son 15 dakika
- **Metrikler**: 15 saniyelik scrape interval ile güncellenir

---

## 💡 Pro İpuçları

1. **Test çalıştırmadan önce**:
   - `test-metrics.bat` ile tüm servisleri kontrol et
   - Prometheus targets sayfasını aç

2. **Test sırasında**:
   - Grafana dashboard'unu bir monitörde aç
   - Visual Studio'yu diğer monitörde kullan
   - Real-time metrikleri izle

3. **Test sonrası**:
   - NBomber HTML raporlarını kontrol et
   - Grafana'da zaman aralığını değiştirerek tüm test süresini gör
   - Prometheus Query'leriyle custom analizler yap

---

## 📝 Metrik İsimleri Değişti!

**Önceki (ÇALIŞMIYOR):**
- ❌ `chd_lock_acquisitions_total`
- ❌ `chd_idempotency_executions_total`
- ❌ `chd_saga_executions_total`

**Yeni (ÇALIŞIYOR):**
- ✅ `chd_lock_operations_total{status="success|failed"}`
- ✅ `chd_idempotency_operations_total{status="success|failed"}`
- ✅ `chd_saga_operations_total{status="success|failed"}`

Dashboard artık doğru metrikleri kullanıyor!

---

## 🔗 Yararlı Linkler

- **Grafana Dashboard**: http://localhost:3000/d/chd-coordination
- **Prometheus Targets**: http://localhost:9090/targets
- **Prometheus Query**: http://localhost:9090/graph
- **Load Test Metrics**: http://localhost:9091/metrics
- **Redis**: localhost:6379

---

## ⚡ Hızlı Komutlar

```bash
# Monitoring stack'i başlat
docker-compose -f docker-compose.monitoring.yml up -d

# Monitoring stack'i durdur
docker-compose -f docker-compose.monitoring.yml down

# Logları görüntüle
docker-compose -f docker-compose.monitoring.yml logs -f

# Sadece Prometheus'u yeniden başlat
docker restart chd-prometheus

# Sadece Grafana'yı yeniden başlat
docker restart chd-grafana
```

---

🎉 **Artık metrikler çalışıyor!** Visual Studio'da F5'e basın ve Grafana'da real-time metrikleri izleyin!
