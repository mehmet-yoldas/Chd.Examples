# 🚀 Load Testing with k6

This directory contains k6 load testing scripts for CHD Coordination.

## Quick Start

### 1. Install k6

**Windows (Chocolatey):**
```bash
choco install k6
```

**macOS (Homebrew):**
```bash
brew install k6
```

**Linux (Debian/Ubuntu):**
```bash
sudo gpg -k
sudo gpg --no-default-keyring --keyring /usr/share/keyrings/k6-archive-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb [signed-by=/usr/share/keyrings/k6-archive-keyring.gpg] https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

### 2. Run Load Test

```bash
# Basic test (default settings)
k6 run loadtest.js

# Custom base URL
k6 run -e BASE_URL=http://localhost:5001 loadtest.js

# With output to InfluxDB (if configured)
k6 run --out influxdb=http://localhost:8086/k6 loadtest.js

# Quiet mode (less output)
k6 run --quiet loadtest.js

# Generate HTML report
k6 run --out json=results.json loadtest.js
```

### 3. Watch Grafana

While the test is running, watch the dashboard at: http://localhost:3000/d/chd-coordination

## Test Scenarios

### Scenario 1: Constant Load (2 minutes)
- **10 virtual users**
- **Constant load**
- Tests baseline performance

### Scenario 2: Ramp Up (2 minutes)
- **0 → 50 users** over 30s
- **Sustained 50 users** for 1 minute
- **50 → 0 users** over 30s
- Tests scalability

### Scenario 3: Spike Test (50 seconds)
- **0 → 100 users** in 10s
- **Sustained 100 users** for 30s
- **100 → 0 users** in 10s
- Tests system under stress

## Test Configuration

Edit `loadtest.js` to customize:

```javascript
export const options = {
  scenarios: {
    constant_load: {
      executor: 'constant-vus',
      vus: 10,           // Number of virtual users
      duration: '2m',     // Test duration
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% under 500ms
    http_req_failed: ['rate<0.01'],    // <1% error rate
  },
};
```

## Expected Results

### Good Performance ✅
```
✓ http_req_duration..............: avg=45ms  p(95)=120ms p(99)=250ms
✓ http_req_failed................: 0.05%
✓ lock_success_rate..............: 99.8%
✓ iterations.....................: 5000
```

### Poor Performance ⚠️
```
✗ http_req_duration..............: avg=850ms  p(95)=1500ms p(99)=3000ms
✗ http_req_failed................: 5.2%
✗ lock_success_rate..............: 85%
✓ iterations.....................: 2500
```

## Custom Metrics

The script tracks these CHD-specific metrics:

- `lock_success_rate` - % of successful lock acquisitions
- `lock_duration` - Time to acquire lock
- `idempotency_cache_hits` - Number of cached results

## Interpreting Results

### HTTP Request Duration
```
http_req_duration
  avg: 45ms       # Average response time
  min: 12ms       # Fastest request
  med: 38ms       # Median (50th percentile)
  max: 420ms      # Slowest request
  p(90): 85ms     # 90% of requests faster than this
  p(95): 120ms    # 95% of requests faster than this
  p(99): 250ms    # 99% of requests faster than this
```

**Targets:**
- p(95) < 500ms ✅ Good
- p(95) < 200ms ✅ Excellent
- p(95) > 1000ms ❌ Needs improvement

### Lock Success Rate
```
lock_success_rate: 99.8%
```

**Targets:**
- \>99% ✅ Excellent
- 95-99% ⚠️ Acceptable
- <95% ❌ Issue with Redis or configuration

### Error Rate
```
http_req_failed: 0.05% (25 out of 5000)
```

**Targets:**
- <1% ✅ Excellent
- 1-5% ⚠️ Investigate
- \>5% ❌ Critical issue

## Advanced Usage

### Stress Test
```javascript
export const options = {
  stages: [
    { duration: '1m', target: 100 },   // Ramp up to 100 users
    { duration: '5m', target: 100 },   // Stay at 100 users
    { duration: '1m', target: 200 },   // Ramp to 200 users
    { duration: '3m', target: 200 },   // Stay at 200 users
    { duration: '1m', target: 0 },     // Ramp down
  ],
};
```

### Soak Test (Endurance)
```javascript
export const options = {
  stages: [
    { duration: '2m', target: 50 },    // Ramp up
    { duration: '4h', target: 50 },    // Sustained load
    { duration: '2m', target: 0 },     // Ramp down
  ],
};
```

### Breakpoint Test
```javascript
export const options = {
  executor: 'ramping-arrival-rate',
  startRate: 50,
  timeUnit: '1s',
  preAllocatedVUs: 500,
  maxVUs: 1000,
  stages: [
    { target: 200, duration: '2m' },   // Linearly ramp up
    { target: 400, duration: '2m' },
    { target: 800, duration: '2m' },
    { target: 1600, duration: '2m' },  // Keep ramping until it breaks
  ],
};
```

## Troubleshooting

### High Error Rate?

1. **Check Redis:**
   ```bash
   redis-cli -h localhost -p 6379 info stats
   ```

2. **Check app logs:**
   ```bash
   docker-compose logs -f
   ```

3. **Reduce load:**
   Lower VUs or add delays in script

### Low Throughput?

1. **Check system resources:**
   ```bash
   docker stats
   ```

2. **Scale Redis:**
   Use Redis Cluster for higher throughput

3. **Tune connection pool:**
   Increase Redis connection pool size

### Timeout Errors?

1. **Increase timeouts in app**
2. **Check network latency**
3. **Verify Redis isn't overloaded**

## CI/CD Integration

### GitHub Actions Example

```yaml
name: Load Test

on:
  pull_request:
    branches: [main]

jobs:
  loadtest:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Start services
        run: |
          docker-compose -f docker-compose.monitoring.yml up -d
          sleep 10
      
      - name: Run load test
        uses: grafana/k6-action@v0.3.0
        with:
          filename: loadtest.js
          flags: --quiet
      
      - name: Check thresholds
        run: |
          if [ $? -ne 0 ]; then
            echo "Load test failed!"
            exit 1
          fi
```

## Resources

- **k6 Docs**: https://k6.io/docs/
- **Best Practices**: https://k6.io/docs/testing-guides/
- **Examples**: https://k6.io/docs/examples/

---

Happy load testing! 🚀
