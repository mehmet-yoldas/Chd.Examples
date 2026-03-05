# Chd.LoadTests - NBomber Load Testing

Visual Studio-native load testing for Chd.Coordination library using NBomber.

## 🎯 Overview

This project provides production-grade load testing scenarios for:
- **Distributed Lock** - Concurrent lock acquisition and release
- **Idempotency** - Duplicate operation prevention under load
- **Saga** - Multi-step transaction performance
- **Mixed Workload** - Combined real-world scenarios

## ✨ Why NBomber?

✅ **Visual Studio Native** - Run with F5, debug with breakpoints  
✅ **Professional Reports** - HTML and Markdown output  
✅ **Grafana Integration** - Real-time metrics visualization  
✅ **Production Ready** - Used by companies worldwide  
✅ **No External Tools** - Pure .NET, no k6 or Node.js required  

## 🚀 Quick Start

### Super Easy Way (Recommended!) 🌟

**Just press F5 in Visual Studio!**

1. Open solution in Visual Studio 2022
2. Set `Chd.LoadTests` as startup project
3. Press **F5**
4. When asked: "Start monitoring stack automatically?" → Type **"y"**
5. When asked: "Open Grafana dashboard?" → Type **"y"**
6. **Done!** Dashboard opens automatically! 🎉

**📖 [Detailed Visual Studio Only Guide](./VISUAL_STUDIO_ONLY.md)**

---

### Prerequisites

1. **Redis** must be running:
   ```bash
   # Using Docker
   docker run -d -p 6379:6379 redis:7-alpine
   
   # OR use monitoring stack
   .\start-monitoring.bat
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

### Run Load Tests

**Option 1: Visual Studio**
- Press F5 to run
- Select scenario from menu
- View results in console + HTML report

**Option 2: Command Line**
```bash
dotnet run --project Chd.LoadTests
```

**Option 3: Release Mode (for accurate metrics)**
```bash
dotnet run --project Chd.LoadTests --configuration Release
```

## 📊 Available Scenarios

### 1. Distributed Lock Test
```
Duration: 3 minutes
Virtual Users: 10 → 30 (ramping)
Operation: Acquire lock, hold 50ms, release
Metrics: Lock acquisition time, contention rate
```

**What it tests:**
- Lock acquisition speed under load
- Lock contention handling
- Deadlock prevention
- TTL expiration behavior

### 2. Idempotency Test
```
Duration: 3 minutes
Virtual Users: 50 (ramping from 0)
Operation: Execute idempotent payment operation
Metrics: Cache hit rate, execution time
```

**What it tests:**
- Duplicate request filtering
- Cache performance
- TTL management
- Concurrent idempotency checks

### 3. Saga Test
```
Duration: 2 minutes
Virtual Users: 10 (constant)
Operation: 3-step saga (reserve → charge → ship)
Metrics: Saga completion time, step latency
```

**What it tests:**
- Multi-step transaction performance
- State persistence overhead
- Step execution time
- Saga completion rate

### 4. Mixed Workload Test
```
Duration: 3 minutes
Concurrent scenarios:
  - 5 VUs: Lock operations
  - 8 VUs: Idempotency operations
  - 3 VUs: Saga operations
```

**What it tests:**
- Real-world mixed workload
- Resource contention
- Cross-feature performance
- System stability under varied load

### 5. Run ALL Tests
Executes all scenarios sequentially (~10 minutes total).

## 📈 Understanding Results

### Console Output
```
Scenario: distributed_lock_test
┌────────────────┬───────┬────────┬─────────┐
│ Metric         │ Value │ StdDev │ Status  │
├────────────────┼───────┼────────┼─────────┤
│ RPS            │ 195   │ 12.3   │ OK      │
│ Latency (P95)  │ 52ms  │ 8ms    │ OK      │
│ Success Rate   │ 99.8% │ -      │ PASSED  │
└────────────────┴───────┴────────┴─────────┘
```

### HTML Report
Located at: `./reports/<timestamp>/report.html`

**Key Sections:**
- **Timeline Graph** - RPS and latency over time
- **Percentiles** - P50, P75, P95, P99 response times
- **Status Codes** - Success/failure distribution
- **Data Transfer** - Sent/received bytes (if applicable)

### Important Metrics

| Metric | Good | Warning | Critical |
|--------|------|---------|----------|
| **P95 Latency** | < 100ms | 100-500ms | > 500ms |
| **Success Rate** | > 99% | 95-99% | < 95% |
| **RPS** | Stable | Fluctuating ±20% | Dropping |

## 🔍 Debugging Load Tests

### Visual Studio (Recommended!) 👈

**NBomber'ın en büyük avantajı: Visual Studio'da debug edebilirsiniz!**

#### Quick Start
1. **Set as Startup Project**: Solution Explorer → Chd.LoadTests → Sağ tık → "Set as Startup Project"
2. **Press F5** to run in debug mode
3. **Set Breakpoints** (F9) anywhere in your scenarios
4. **Step Through** code (F10, F11)
5. **Watch variables** in real-time

#### Launch Profiles (Super Easy!)

Toolbar dropdown'da şunları göreceksiniz:
- **Lock Test (Debug)** - Otomatik lock test başlatır
- **Idempotency Test (Debug)** - Otomatik idempotency test
- **Saga Test (Debug)** - Otomatik saga test
- **Mixed Workload (Debug)** - Otomatik mixed test
- **Run All Tests** - Hepsini çalıştır

**Seçin → F5 basın → Test başlar!** (senaryo seçmek için beklemenize gerek yok)

📖 **[Detaylı Visual Studio Kullanım Kılavuzu](./VISUAL_STUDIO_GUIDE.md)**

---

### Enable Verbose Logging
```csharp
NBomberRunner
    .RegisterScenarios(scenario)
    .WithReportingInterval(TimeSpan.FromSeconds(5))
    .WithLogLevel(LogLevel.Debug)
    .Run();
```

### Breakpoint Debugging
1. Set breakpoint in scenario lambda
2. Press F5 in Visual Studio
3. Choose scenario
4. Breakpoint hits on first iteration

### Profile with dotTrace
```bash
# Install JetBrains profiler
dotnet tool install -g JetBrains.dotTrace.GlobalTools

# Run with profiling
dotnet trace Chd.LoadTests
```

## 📊 Grafana Integration

### View Live Metrics
1. Start monitoring stack: `.\start-monitoring.bat`
2. Run load test in separate terminal
3. Open dashboard: http://localhost:3000/d/chd-coordination
4. Watch metrics update in real-time

### Custom Metrics
Add custom NBomber metrics to Prometheus:

```csharp
var scenario = Scenario.Create("custom_test", async context =>
{
    var stopwatch = Stopwatch.StartNew();
    // ... your code ...
    stopwatch.Stop();
    
    context.Logger.Information(
        "custom_metric_duration_ms {Duration}", 
        stopwatch.ElapsedMilliseconds
    );
    
    return Response.Ok();
});
```

## 🎛️ Customizing Load Patterns

### Constant Load
```csharp
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 50, during: TimeSpan.FromMinutes(5))
)
```

### Ramping Load
```csharp
.WithLoadSimulations(
    Simulation.RampingInject(rate: 100, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(5))
)
```

### Spike Test
```csharp
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(1)),
    Simulation.KeepConstant(copies: 100, during: TimeSpan.FromSeconds(30)),
    Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(1))
)
```

### Stress Test
```csharp
.WithLoadSimulations(
    Simulation.RampingInject(rate: 500, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromMinutes(10))
)
```

## 🏆 Best Practices

### 1. Warm-Up Period
```csharp
.WithWarmUpDuration(TimeSpan.FromSeconds(30))
```
Allows system to stabilize before measurement.

### 2. Realistic Data
```csharp
var userId = $"user:{Random.Shared.Next(1, 10000)}";
```
Use random data to prevent cache optimization.

### 3. Think Time
```csharp
await Task.Delay(Random.Shared.Next(100, 500));
```
Simulate realistic user behavior.

### 4. Error Handling
```csharp
try 
{
    // Test code
    return Response.Ok();
}
catch (Exception ex)
{
    context.Logger.Error(ex, "Test failed");
    return Response.Fail(ex.Message);
}
```

### 5. Resource Cleanup
```csharp
var scenario = Scenario.Create("test", async context =>
{
    // Test code
})
.WithInit(async context =>
{
    // Setup resources
})
.WithClean(async context =>
{
    // Cleanup resources
});
```

## 🐛 Troubleshooting

### Redis Connection Failed
```
Error: It was not possible to connect to the redis server
```
**Solution:** Ensure Redis is running on localhost:6379
```bash
docker ps | findstr redis
docker run -d -p 6379:6379 redis:7-alpine
```

### High Latency (> 500ms)
**Possible causes:**
- Redis not optimized (use persistent connection)
- Network latency (run Redis locally)
- Resource contention (reduce VUs)

**Solution:** Check Redis performance
```bash
redis-cli --latency
```

### Test Crashes
```
Error: System.OutOfMemoryException
```
**Solution:** Reduce virtual users or test duration
```csharp
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(1))
)
```

### Inconsistent Results
**Causes:**
- Background processes consuming CPU
- Other applications using Redis
- Network instability

**Solution:** Run in Release mode, close other apps
```bash
dotnet run -c Release
```

## 📚 Additional Resources

- **NBomber Documentation**: https://nbomber.com/docs/overview
- **Chd.Coordination Examples**: ../Chd.Coordination.Examples/
- **Unit Tests**: ../Chd.UnitTest/
- **Monitoring Setup**: ../MONITORING_SETUP.md

## 🤝 Contributing

Found a performance issue? Have ideas for new scenarios?

1. Open an issue describing the problem
2. Submit a PR with new test scenarios
3. Share your benchmark results

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

---

**💡 Pro Tip:** Run load tests in CI/CD to catch performance regressions early!

```yaml
# Example GitHub Actions workflow
- name: Run Load Tests
  run: dotnet run --project Chd.LoadTests -c Release
  
- name: Upload Reports
  uses: actions/upload-artifact@v3
  with:
    name: load-test-reports
    path: ./reports/
```
