# 🔧 Grafana Metrik Sorunu Düzeltildi!

## 📋 Problem
Grafana açılıyordu ancak metrikler görünmüyordu.

## ✅ Çözüm
Dashboard'daki metrik sorguları güncellendi ve doğru metrik isimlerini kullanıyor.

---

## 🎯 Hızlı Başlangıç

### 1️⃣ Metrikleri Test Et (Otomatik)
```bash
test-metrics.bat
```

Bu script otomatik olarak:
- ✅ Monitoring stack'i yeniden başlatır
- ✅ Prometheus targets sayfasını açar (kontrol için)
- ✅ Metrics endpoint'i açar (http://localhost:9091/metrics)
- ✅ Grafana dashboard'unu açar

### 2️⃣ Load Test Çalıştır
1. Visual Studio'da **Chd.LoadTests** projesini açın
2. **F5** tuşuna basın
3. Bir test seçin (örnek: `1` - Distributed Lock Test)

### 3️⃣ Metrikleri İzle
Grafana otomatik olarak açılacak:
- **URL**: http://localhost:3000/d/chd-coordination
- **Login**: admin / admin
- **Dashboard**: CHD Coordination - Performance Dashboard

---

## 📊 Güncellenen Metrikler

### Önceki (Çalışmayan):
```
❌ chd_lock_acquisitions_total
❌ chd_idempotency_executions_total
❌ chd_saga_executions_total
❌ chd_lock_duration_milliseconds
❌ chd_idempotency_duration_milliseconds
❌ chd_saga_duration_milliseconds
```

### Yeni (Çalışan):
```
✅ chd_lock_operations_total{status="success|failed"}
✅ chd_idempotency_operations_total{status="success|failed"}
✅ chd_saga_operations_total{status="success|failed"}
✅ chd_lock_duration_seconds (Histogram)
✅ chd_idempotency_duration_seconds (Histogram)
✅ chd_saga_duration_seconds (Histogram)
```

---

## 🔍 Değiştirilen Dosyalar

### 1. `monitoring/grafana/dashboards/chd-coordination-dashboard.json`
- ✅ Tüm panel sorguları güncellendi
- ✅ Doğru metrik isimleri kullanılıyor
- ✅ Status label'ları eklendi (success/failed)
- ✅ Unit'ler düzeltildi (milisaniye → saniye)

### 2. Yeni Dosyalar Eklendi
- ✅ `test-metrics.bat` - Hızlı test scripti
- ✅ `METRICS_TROUBLESHOOTING.md` - Detaylı sorun giderme rehberi
- ✅ `GRAFANA_METRICS_FIX.md` - Bu dosya

---

## 📖 Grafana Dashboard Panelleri

Dashboard şu 5 paneli içeriyor:

| Panel | Açıklama | Metrik |
|-------|----------|--------|
| **Operations per Second** | Saniye başına işlem sayısı | `rate(chd_*_operations_total[1m])` |
| **P95 Latency** | 95. yüzdelik dilim gecikme | `histogram_quantile(0.95, rate(chd_*_duration_seconds_bucket[5m]))` |
| **Distributed Lock Stats** | Lock başarı/hata sayısı | `increase(chd_lock_operations_total[5m])` |
| **Idempotency Stats** | Idempotency başarı/hata sayısı | `increase(chd_idempotency_operations_total[5m])` |
| **Saga Statistics** | Saga başarı/hata sayısı | `sum(increase(chd_saga_operations_total[5m]))` |

---

## 🎯 Manuel Kontrol Adımları

### 1. Prometheus Targets
```
http://localhost:9090/targets
```
**Beklenen**: `chd-loadtests` hedefi **UP** olmalı

### 2. Raw Metrics
```
http://localhost:9091/metrics
```
**Beklenen**: Load test çalışırken `chd_*` metrikleri görünmeli

### 3. Prometheus Query
```
http://localhost:9090/graph
```
Query: `chd_lock_operations_total`
**Beklenen**: Değerler görünmeli

---

## 🐛 Sorun Giderme

### Metrikler Hala Görünmüyorsa:

#### A) Monitoring Stack'i Yeniden Başlat
```bash
cd /d C:\Projects\Demo
docker-compose -f docker-compose.monitoring.yml down
docker-compose -f docker-compose.monitoring.yml up -d
```

#### B) Load Test Çalıştığından Emin Ol
- Visual Studio'da F5 ile test başlat
- Console'da "Prometheus metrics available at: http://localhost:9091/metrics" mesajını gör
- Bir test seçimi yap ve testin başladığını doğrula

#### C) Docker Container'ları Kontrol Et
```bash
docker ps
```
**Beklenen**: Şu container'lar çalışıyor olmalı:
- `chd-redis`
- `chd-prometheus`
- `chd-grafana`
- `chd-node-exporter`

#### D) Logları İncele
```bash
# Prometheus logs
docker logs chd-prometheus

# Grafana logs
docker logs chd-grafana
```

---

## 💡 Pro İpuçları

1. **İlk test çalıştırma sonrası**:
   - Metriklerin görünmesi 5-10 saniye sürebilir
   - Grafana'da "Last 5 minutes" zaman aralığını seçin
   - Auto-refresh açık olmalı (5s)

2. **Test sırasında**:
   - Grafana'yı bir monitörde açık tutun
   - Real-time değişimleri izleyin
   - NBomber console output'unu takip edin

3. **Test sonrası**:
   - Grafana'da zaman aralığını test süresine ayarlayın
   - NBomber HTML raporlarını inceleyin
   - Prometheus'da custom query'ler yapın

---

## 📚 Ek Kaynaklar

- **Detaylı Sorun Giderme**: `METRICS_TROUBLESHOOTING.md`
- **Visual Studio Kullanımı**: `Chd.LoadTests/VISUAL_STUDIO_ONLY.md`
- **Monitoring Setup**: `MONITORING_SETUP.md`
- **NBomber Documentation**: `Chd.LoadTests/README.md`

---

## ✅ Kontrol Listesi

- [ ] `test-metrics.bat` çalıştırıldı
- [ ] Prometheus targets sayfasında `chd-loadtests` **UP** durumda
- [ ] Visual Studio'da load test başlatıldı
- [ ] http://localhost:9091/metrics adresinde `chd_*` metrikleri görünüyor
- [ ] Grafana dashboard açıldı (admin/admin)
- [ ] Dashboard'da real-time metrikler güncelleniyor

---

🎉 **Tüm metrikler artık çalışıyor!** 

Sorularınız için: `METRICS_TROUBLESHOOTING.md` dosyasına bakın.
