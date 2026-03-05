# 📊 Grafana'da Metrikleri Görme - Adım Adım Rehber

## 🎯 Hızlı Başlangıç (3 Adım)

### 1️⃣ Monitoring Stack'i Başlat

```sh
# Windows
.\start-monitoring.bat

# Linux/Mac
./start-monitoring.sh
```

**Ne başlatır:**
- ✅ Redis (port 6379)
- ✅ Prometheus (port 9090)
- ✅ Grafana (port 3000)
- ✅ Node Exporter (port 9100)

**Çıktı:**
```
======================================
Monitoring stack is ready!
======================================

Access services:
  - Grafana:    http://localhost:3000
    Username: admin
    Password: admin

  - Prometheus: http://localhost:9090
  - Redis:      localhost:6379

View dashboard:
  http://localhost:3000/d/chd-coordination
```

### 2️⃣ Load Test'i Başlat

**Terminal 1'de monitoring çalışırken, yeni bir terminal açın:**

```sh
# Option 1: Visual Studio'da
F5 bas → Senaryo seç

# Option 2: Command line
cd Chd.LoadTests
dotnet run

# Option 3: Otomatik
.\start-loadtests.bat
```

**Çıktı:**
```
===========================================
CHD Coordination - Load Tests with NBomber
===========================================

[Prometheus] Metrics server started at http://localhost:9091/metrics
[INFO] Prometheus metrics available at: http://localhost:9091/metrics

Available scenarios:
  1. Distributed Lock Performance Test
  2. Idempotency Performance Test
  3. Saga Performance Test
  4. Mixed Workload Test
  5. Run ALL Tests

Select scenario (1-5): 1
```

### 3️⃣ Grafana'da Metrikleri Gör

1. **Tarayıcıda aç:** http://localhost:3000
2. **Login:**
   - Username: `admin`
   - Password: `admin`
3. **Dashboard'u aç:** http://localhost:3000/d/chd-coordination

**🎉 Artık metrikleri real-time görüyorsunuz!**

---

## 📈 Grafana Dashboard Panelleri

### Panel 1: Operations Per Second
**Ne gösterir:** Saniyede kaç işlem yapılıyor

**Metrikler:**
```promql
# Lock operations
rate(chd_lock_operations_total[1m])

# Idempotency operations
rate(chd_idempotency_operations_total[1m])

# Saga operations
rate(chd_saga_operations_total[1m])
```

**Nasıl görünür:**
- Yeşil çizgi: Lock operations
- Mavi çizgi: Idempotency operations
- Turuncu çizgi: Saga operations

### Panel 2: P95 Latency
**Ne gösterir:** İşlemlerin %95'i ne kadar sürede tamamlanıyor

**Metrikler:**
```promql
# Lock P95 latency
histogram_quantile(0.95, rate(chd_lock_duration_seconds_bucket[1m]))

# Idempotency P95 latency
histogram_quantile(0.95, rate(chd_idempotency_duration_seconds_bucket[1m]))

# Saga P95 latency
histogram_quantile(0.95, rate(chd_saga_duration_seconds_bucket[1m]))
```

**İyi değerler:**
- 🟢 < 100ms - Mükemmel
- 🟡 100-500ms - İyi
- 🔴 > 500ms - Problem var

### Panel 3: Success Rate
**Ne gösterir:** İşlemlerin başarı oranı

**Metrikler:**
```promql
# Lock success rate
rate(chd_lock_operations_total{status="success"}[1m]) /
rate(chd_lock_operations_total[1m]) * 100
```

**İyi değerler:**
- 🟢 > 99% - Harika
- 🟡 95-99% - Kabul edilebilir
- 🔴 < 95% - Sorun var

### Panel 4: Active Tests
**Ne gösterir:** Şu anda kaç test çalışıyor

**Metrik:**
```promql
chd_active_tests
```

### Panel 5: Operation Latency (P50, P90, P95, P99)
**Ne gösterir:** Farklı percentile'larda latency

**Metrikler:**
```promql
# P50 (Median)
chd_operation_latency_seconds{quantile="0.5"}

# P90
chd_operation_latency_seconds{quantile="0.9"}

# P95
chd_operation_latency_seconds{quantile="0.95"}

# P99
chd_operation_latency_seconds{quantile="0.99"}
```

---

## 🔍 Prometheus'ta Metrikleri Görme

### 1. Prometheus Web UI'a Gir

http://localhost:9090

### 2. PromQL Query'leri

**Tüm metrikler:**
```promql
{job="chd-loadtests"}
```

**Lock operations:**
```promql
chd_lock_operations_total
```

**Lock latency (son 5 dakika):**
```promql
rate(chd_lock_duration_seconds_sum[5m]) /
rate(chd_lock_duration_seconds_count[5m])
```

**Success rate:**
```promql
sum(rate(chd_lock_operations_total{status="success"}[1m])) /
sum(rate(chd_lock_operations_total[1m])) * 100
```

### 3. Graph Görünümü

1. **Graph** tab'ına geç
2. Query yaz
3. **Execute** bas
4. Grafiği gör

---

## 🎨 Custom Queries (İleri Seviye)

### Lock Contention Rate
```promql
# Başarısız lock denemeleri / Toplam
rate(chd_lock_operations_total{status="failed"}[1m]) /
rate(chd_lock_operations_total[1m]) * 100
```

### Average Latency by Operation Type
```promql
avg by (operation_type) (
  rate(chd_operation_latency_seconds_sum[5m]) /
  rate(chd_operation_latency_seconds_count[5m])
)
```

### Total Operations (son 1 saat)
```promql
increase(chd_lock_operations_total[1h]) +
increase(chd_idempotency_operations_total[1h]) +
increase(chd_saga_operations_total[1h])
```

### Peak RPS (son 1 saat)
```promql
max_over_time(
  rate(chd_lock_operations_total[1m])[1h:]
)
```

---

## 🐛 Sorun Giderme

### "No data" Gösteriyorsa

#### Kontrol 1: Load test çalışıyor mu?
```sh
# Başka bir terminal'de
curl http://localhost:9091/metrics
```

**Görmeli:**
```
# HELP chd_lock_operations_total Total number of lock operations
# TYPE chd_lock_operations_total counter
chd_lock_operations_total{status="success"} 1234
chd_lock_operations_total{status="failed"} 5
...
```

#### Kontrol 2: Prometheus scrape ediyor mu?
1. http://localhost:9090/targets açın
2. `chd-loadtests` job'unu bulun
3. **State:** UP olmalı

**Eğer DOWN ise:**
- Load test çalışmıyor olabilir
- Port 9091 kullanımda olabilir
- Firewall engelliyor olabilir

#### Kontrol 3: Prometheus config doğru mu?
```sh
# monitoring/prometheus.yml dosyasına bakın
# Şunu görmeli:
- job_name: 'chd-loadtests'
  static_configs:
    - targets: ['host.docker.internal:9091']
```

**Eğer yoksa:**
```sh
# Monitoring stack'i yeniden başlatın
docker-compose -f docker-compose.monitoring.yml down
docker-compose -f docker-compose.monitoring.yml up -d
```

### Metrikler Eski Gösteriyorsa

**Sebep:** Prometheus cache

**Çözüm:**
1. Grafana dashboard'da sağ üstte **refresh icon** var
2. Tıklayın veya
3. **Auto-refresh** açın (5s, 10s, 30s seçenekleri var)

### Dashboard Boş Görünüyorsa

**Çözüm:**
1. **Time range** kontrolü yapın (sağ üst köşe)
2. **Last 5 minutes** veya **Last 15 minutes** seçin
3. Load test çalıştırın
4. 10-15 saniye bekleyin

---

## 📊 Gerçek Zamanlı İzleme (Recommended Workflow)

### Setup

1. **3 terminal açın:**

**Terminal 1: Monitoring**
```sh
.\start-monitoring.bat
```

**Terminal 2: Load Test**
```sh
cd Chd.LoadTests
dotnet run
```

**Terminal 3: Logs**
```sh
# Docker logs izle
docker logs -f chd-prometheus
```

2. **2 browser tab açın:**

**Tab 1: Grafana Dashboard**
```
http://localhost:3000/d/chd-coordination
```

**Tab 2: Prometheus Targets**
```
http://localhost:9090/targets
```

### Test Sırasında İzlenecekler

✅ **Grafana'da:**
- Operations/sec artıyor mu?
- Latency stabil mi?
- Success rate %99+ mı?

✅ **Prometheus'ta:**
- chd-loadtests target UP mu?
- Last scrape başarılı mı?

✅ **Console'da:**
- NBomber progress gösteriyormu?
- Hata mesajı var mı?

---

## 🎯 İdeal Metrik Değerleri

| Metrik | İyi | Uyarı | Kötü |
|--------|-----|-------|------|
| **P95 Latency** | < 100ms | 100-500ms | > 500ms |
| **Success Rate** | > 99% | 95-99% | < 95% |
| **RPS** | Stabil | ±20% fluktuasyon | Düşüyor |
| **Lock Contention** | < 1% | 1-5% | > 5% |
| **Memory** | < 500MB | 500MB-1GB | > 1GB |
| **CPU** | < 50% | 50-80% | > 80% |

---

## 🚀 Pro Tips

### Tip 1: Alert Kuralları

Grafana'da **Alerting** → **Alert rules** → **New alert rule**

**Örnek: High Latency Alert**
```promql
histogram_quantile(0.95, 
  rate(chd_lock_duration_seconds_bucket[1m])
) > 0.5
```

**Uyarı:** P95 latency 500ms'yi geçerse

### Tip 2: Dashboard Variables

Dashboard'da değişken kullanın:

```promql
# $operation variable ile
rate(chd_${operation}_operations_total[1m])
```

Dropdown'dan `lock`, `idempotency`, `saga` seçebilirsiniz.

### Tip 3: Annotations

Test başlangıcını işaretleyin:

1. Grafana → Dashboard
2. Grafikte sağ tık → **Add annotation**
3. "Load test started" yazın

### Tip 4: Snapshot

Sonuçları paylaşmak için:

1. Dashboard → **Share**
2. **Snapshot** → **Publish to snapshots.raintank.io**
3. Link'i paylaşın

### Tip 5: Export

Metrikleri export edin:

1. Dashboard → **Panel** → **Inspect**
2. **Data** tab
3. **Download CSV**

---

## 📚 Daha Fazla

- **PromQL Tutorial**: https://prometheus.io/docs/prometheus/latest/querying/basics/
- **Grafana Docs**: https://grafana.com/docs/
- **NBomber Metrics**: https://nbomber.com/docs/reporting-sinks
- **This Project**: `../MONITORING_SETUP.md`

---

## ✅ Checklist

Test öncesi kontrol listesi:

- [ ] Redis çalışıyor (`docker ps | findstr redis`)
- [ ] Monitoring stack ayakta (`docker ps | findstr grafana`)
- [ ] Prometheus targets UP (`http://localhost:9090/targets`)
- [ ] Grafana açılıyor (`http://localhost:3000`)
- [ ] Dashboard görünüyor (`http://localhost:3000/d/chd-coordination`)
- [ ] Load test başlatılabilir (`cd Chd.LoadTests && dotnet run`)
- [ ] Metrik endpoint çalışıyor (`curl http://localhost:9091/metrics`)

Hepsi ✅ ise, test başlatabilirsiniz! 🚀

---

**🎉 Artık Grafana'da real-time metrikleri izleyebilirsiniz!**

Ekran görüntüsünü almak için:
1. Load test'i başlatın
2. Grafana dashboard'u açın
3. 30 saniye bekleyin
4. Ekran görüntüsü alın ve paylaşın! 📸
