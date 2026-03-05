# 📊 CHD Coordination - Grafana Monitoring Setup

Complete monitoring stack for CHD Coordination library with Prometheus metrics and Grafana dashboards.

---

## 🚀 Quick Start

### 1. Start Monitoring Stack

```bash
# Start Redis, Prometheus, and Grafana
docker-compose -f docker-compose.monitoring.yml up -d

# Check status
docker-compose -f docker-compose.monitoring.yml ps
```

### 2. Access Services

| Service | URL | Credentials |
|---------|-----|-------------|
| **Grafana** | http://localhost:3000 | admin / admin |
| **Prometheus** | http://localhost:9090 | - |
| **Redis** | localhost:6379 | - |

### 3. View Dashboard

1. Open Grafana: http://localhost:3000
2. Login: `admin` / `admin` (change on first login)
3. Navigate to: **Dashboards → CHD → CHD Coordination - Performance Dashboard**

---

## 📊 Dashboard Panels

### 1. **Operations per Second**
- Lock acquisitions/sec
- Idempotency executions/sec
- Saga executions/sec

### 2. **P95 Latency**
- Lock P95 latency
- Idempotency P95 latency
- Saga P95 latency

### 3. **Distributed Lock Stats**
- Total acquisitions
- Failed acquisitions
- Success rate

### 4. **Idempotency Stats**
- Executions
- Cache hits
- Cache hit rate

### 5. **Saga Statistics**
- Total saga executions
- Failed sagas
- Compensations executed

---

## 🔧 Adding Metrics to Your App

### Option 1: Using OpenTelemetry (Recommended)

```bash
# Install packages
dotnet add package OpenTelemetry.Exporter.Prometheus.AspNetCore
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
```

```csharp
// Program.cs
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddPrometheusExporter()
            .AddMeter("CHD.Coordination")
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation();
    });

// Add Coordination
builder.Services.AddCoordination(opt =>
{
    opt.RedisConnectionString = "localhost:6379";
});

var app = builder.Build();

// Map Prometheus metrics endpoint
app.MapPrometheusScrapingEndpoint();

app.Run();
```

### Option 2: Using prometheus-net

```bash
dotnet add package prometheus-net.AspNetCore
```

```csharp
// Program.cs
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCoordination(opt =>
{
    opt.RedisConnectionString = "localhost:6379";
});

var app = builder.Build();

// Use HTTP metrics middleware
app.UseHttpMetrics();

// Map metrics endpoint
app.MapMetrics();

app.Run();
```

---

## 📈 Custom Metrics Example

If you want to add custom CHD Coordination metrics:

```csharp
using Prometheus;

public class CoordinationMetrics
{
    // Counters
    public static readonly Counter LockAcquisitions = Metrics
        .CreateCounter("chd_lock_acquisitions_total", 
            "Total number of lock acquisitions");

    public static readonly Counter LockFailures = Metrics
        .CreateCounter("chd_lock_failures_total", 
            "Total number of lock acquisition failures");

    public static readonly Counter IdempotencyExecutions = Metrics
        .CreateCounter("chd_idempotency_executions_total", 
            "Total number of idempotency executions");

    public static readonly Counter IdempotencyCacheHits = Metrics
        .CreateCounter("chd_idempotency_cache_hits_total", 
            "Total number of idempotency cache hits");

    public static readonly Counter SagaExecutions = Metrics
        .CreateCounter("chd_saga_executions_total", 
            "Total number of saga executions");

    public static readonly Counter SagaFailures = Metrics
        .CreateCounter("chd_saga_failures_total", 
            "Total number of saga failures");

    public static readonly Counter SagaCompensations = Metrics
        .CreateCounter("chd_saga_compensations_total", 
            "Total number of saga compensations");

    // Histograms for latency
    public static readonly Histogram LockDuration = Metrics
        .CreateHistogram("chd_lock_duration_milliseconds", 
            "Lock acquisition duration in milliseconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(1, 2, 10)
            });

    public static readonly Histogram IdempotencyDuration = Metrics
        .CreateHistogram("chd_idempotency_duration_milliseconds", 
            "Idempotency execution duration in milliseconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(1, 2, 10)
            });

    public static readonly Histogram SagaDuration = Metrics
        .CreateHistogram("chd_saga_duration_milliseconds", 
            "Saga execution duration in milliseconds",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(10, 2, 10)
            });
}

// Usage in your coordinator wrapper
public class MonitoredCoordinator : ICoordinator
{
    private readonly ICoordinator _inner;

    public MonitoredCoordinator(ICoordinator inner)
    {
        _inner = inner;
    }

    public IDistributedLock Lock => new MonitoredLock(_inner.Lock);
    public IIdempotency Idempotency => new MonitoredIdempotency(_inner.Idempotency);
    public ISaga Saga => new MonitoredSaga(_inner.Saga);
}

public class MonitoredLock : IDistributedLock
{
    private readonly IDistributedLock _inner;

    public MonitoredLock(IDistributedLock inner)
    {
        _inner = inner;
    }

    public async Task RunAsync(string key, TimeSpan ttl, Func<CancellationToken, Task> action)
    {
        using (CoordinationMetrics.LockDuration.NewTimer())
        {
            try
            {
                CoordinationMetrics.LockAcquisitions.Inc();
                await _inner.RunAsync(key, ttl, action);
            }
            catch
            {
                CoordinationMetrics.LockFailures.Inc();
                throw;
            }
        }
    }
}
```

---

## 🔍 Testing the Metrics

### 1. Run Unit Tests with Metrics

```bash
# Terminal 1: Start monitoring stack
docker-compose -f docker-compose.monitoring.yml up

# Terminal 2: Run a test app with metrics
cd YourTestApp
dotnet run --urls=http://localhost:5000

# Terminal 3: Run load tests
dotnet test Chd.UnitTest --filter "FullyQualifiedName~IntegrationTests"
```

### 2. Generate Load

```bash
# Use k6 for load testing
k6 run loadtest.js

# Or use Apache Bench
ab -n 10000 -c 100 http://localhost:5000/api/test

# Or simple curl loop
for i in {1..1000}; do
  curl http://localhost:5000/api/test &
done
```

### 3. Watch Grafana Dashboard

Open: http://localhost:3000/d/chd-coordination

You should see:
- ✅ Operations/sec increasing
- ✅ Latency staying low
- ✅ Success rates near 100%

---

## 📊 Prometheus Queries

Useful PromQL queries for debugging:

```promql
# Lock acquisition rate (per second)
rate(chd_lock_acquisitions_total[1m])

# Lock failure rate
rate(chd_lock_failures_total[1m])

# Lock success rate (percentage)
100 * (
  rate(chd_lock_acquisitions_total[1m]) / 
  (rate(chd_lock_acquisitions_total[1m]) + rate(chd_lock_failures_total[1m]))
)

# P95 lock latency
histogram_quantile(0.95, rate(chd_lock_duration_milliseconds_bucket[5m]))

# P99 lock latency
histogram_quantile(0.99, rate(chd_lock_duration_milliseconds_bucket[5m]))

# Average lock latency
rate(chd_lock_duration_milliseconds_sum[5m]) / 
rate(chd_lock_duration_milliseconds_count[5m])

# Idempotency cache hit rate
rate(chd_idempotency_cache_hits_total[1m]) / 
rate(chd_idempotency_executions_total[1m])

# Saga failure rate
rate(chd_saga_failures_total[1m]) / 
rate(chd_saga_executions_total[1m])
```

---

## 🎯 Alerting Rules (Optional)

Create `monitoring/prometheus/alerts.yml`:

```yaml
groups:
  - name: chd_coordination_alerts
    interval: 30s
    rules:
      # High lock failure rate
      - alert: HighLockFailureRate
        expr: rate(chd_lock_failures_total[5m]) > 10
        for: 2m
        labels:
          severity: warning
        annotations:
          summary: "High lock failure rate detected"
          description: "Lock failures: {{ $value }} per second"

      # High latency
      - alert: HighLockLatency
        expr: histogram_quantile(0.95, rate(chd_lock_duration_milliseconds_bucket[5m])) > 500
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High lock latency detected"
          description: "P95 latency: {{ $value }}ms"

      # Low idempotency cache hit rate
      - alert: LowIdempotencyCacheHitRate
        expr: |
          rate(chd_idempotency_cache_hits_total[5m]) / 
          rate(chd_idempotency_executions_total[5m]) < 0.5
        for: 10m
        labels:
          severity: info
        annotations:
          summary: "Low idempotency cache hit rate"
          description: "Cache hit rate: {{ $value | humanizePercentage }}"

      # High saga failure rate
      - alert: HighSagaFailureRate
        expr: |
          rate(chd_saga_failures_total[5m]) / 
          rate(chd_saga_executions_total[5m]) > 0.1
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High saga failure rate detected"
          description: "Saga failure rate: {{ $value | humanizePercentage }}"
```

Then update `prometheus.yml`:

```yaml
rule_files:
  - "alerts.yml"
```

---

## 🛠️ Troubleshooting

### Metrics not appearing?

1. **Check app is exposing metrics:**
   ```bash
   curl http://localhost:5000/metrics
   ```

2. **Check Prometheus can scrape:**
   - Open: http://localhost:9090/targets
   - Status should be "UP"

3. **Check Prometheus has data:**
   ```bash
   # Query Prometheus API
   curl 'http://localhost:9090/api/v1/query?query=chd_lock_acquisitions_total'
   ```

### Grafana dashboard not loading?

1. **Check datasource connection:**
   - Grafana → Configuration → Data Sources → Prometheus
   - Click "Test" - should be green

2. **Manually import dashboard:**
   - Grafana → Dashboards → Import
   - Upload `chd-coordination-dashboard.json`

### Redis connection issues?

```bash
# Check Redis is running
docker ps | grep redis

# Test Redis connection
redis-cli -h localhost -p 6379 ping
```

---

## 🎓 Next Steps

1. **Add custom metrics** for your specific use cases
2. **Set up alerting** with Alertmanager
3. **Export dashboards** for different environments
4. **Integrate with Slack/Teams** for notifications
5. **Add distributed tracing** with Jaeger/Zipkin

---

## 📚 Resources

- **Prometheus Docs**: https://prometheus.io/docs/
- **Grafana Docs**: https://grafana.com/docs/
- **prometheus-net**: https://github.com/prometheus-net/prometheus-net
- **OpenTelemetry**: https://opentelemetry.io/docs/instrumentation/net/

---

## 🤝 Contributing

Found an issue with the monitoring setup? Want to add more dashboards? PRs welcome!

---

<p align="center">
  <strong>⭐ Monitor your coordination! ⭐</strong>
</p>

<p align="center">
  <sub>Built with Prometheus & Grafana</sub>
</p>
