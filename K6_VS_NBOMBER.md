# Load Testing: k6 vs NBomber Comparison

## Overview

This project includes **two load testing options**:

1. **k6** (JavaScript-based) - `loadtest.js`
2. **NBomber** (.NET-based) - `Chd.LoadTests/`

Both are production-ready tools with similar capabilities. Choose based on your team's preferences and tooling.

---

## Quick Comparison

| Feature | k6 | NBomber |
|---------|----|---------| 
| **Language** | JavaScript | C# |
| **Installation** | External tool required | NuGet package |
| **VS Debugging** | ❌ No | ✅ Yes (F5, breakpoints) |
| **IDE Integration** | ❌ External | ✅ Native Visual Studio |
| **Reports** | Text + JSON | HTML + Markdown |
| **Learning Curve** | Low (JS familiar) | Low (C# familiar) |
| **Grafana Support** | ✅ Native | ✅ Via Prometheus |
| **CI/CD** | ✅ Docker image | ✅ dotnet run |
| **Distributed Load** | ✅ Yes (k6 cloud) | ⚠️ Requires setup |
| **Community** | Large (Grafana Labs) | Growing |

---

## Detailed Comparison

### 1. Setup & Installation

#### k6
```bash
# Windows (via Chocolatey)
choco install k6

# Mac
brew install k6

# Linux
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

**Pros:**
- One-time installation
- No project dependencies

**Cons:**
- Separate tool to maintain
- Version management across team
- Not in solution dependencies

#### NBomber
```bash
# No separate installation needed
dotnet restore
```

**Pros:**
- Managed via NuGet (version controlled)
- Same installation process as other packages
- Works everywhere .NET works

**Cons:**
- Adds NuGet dependencies to solution

---

### 2. Development Experience

#### k6
```javascript
import http from 'k6/http';
import { check } from 'k6';

export default function () {
    let response = http.post('http://localhost:5000/api/lock', 
        JSON.stringify({ key: 'resource:1', ttl: 5 }),
        { headers: { 'Content-Type': 'application/json' } }
    );
    
    check(response, {
        'status is 200': (r) => r.status === 200
    });
}
```

**Pros:**
- Simple JavaScript syntax
- Excellent documentation
- Large ecosystem (npm packages)

**Cons:**
- No type safety
- No IntelliSense for your APIs
- Can't reuse C# DTOs/models
- No debugging in VS

#### NBomber
```csharp
var scenario = Scenario.Create("lock_test", async context =>
{
    var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
    
    try
    {
        await coordinator.Lock.RunAsync(
            "resource:1",
            ttl: TimeSpan.FromSeconds(5),
            action: async () => await Task.Delay(50)
        );
        return Response.Ok();
    }
    catch (Exception ex)
    {
        return Response.Fail(ex.Message);
    }
});
```

**Pros:**
- Full type safety
- IntelliSense support
- Reuse existing C# code
- Debug with breakpoints (F5)
- Same language as application

**Cons:**
- Slightly more verbose

---

### 3. Debugging

#### k6
```javascript
// No debugging - only console.log
console.log('Response:', response.body);
```

**Limitations:**
- Cannot set breakpoints
- Cannot inspect variables in real-time
- Cannot step through code
- Must rely on logging

#### NBomber
```csharp
var scenario = Scenario.Create("test", async context =>
{
    var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
    
    // Set breakpoint here - press F5
    await coordinator.Lock.RunAsync(
        "resource:1",
        ttl: TimeSpan.FromSeconds(5),
        action: async () => 
        {
            // Inspect variables here
            var data = await FetchData();
            // Step through this code
            return data;
        }
    );
    
    return Response.Ok();
});
```

**Advantages:**
- Full Visual Studio debugging
- Breakpoints work normally
- Inspect variables in watch window
- Use Immediate window
- Step through code (F10, F11)
- **Game changer for complex scenarios**

---

### 4. Reports & Visualization

#### k6
```
     ✓ status is 200

     checks.........................: 100.00% ✓ 5940       ✗ 0   
     data_received..................: 1.2 MB  19 kB/s
     data_sent......................: 713 kB  12 kB/s
     http_req_blocked...............: avg=12.45µs  min=0s       med=0s       max=2.01ms   p(90)=0s       p(95)=0s      
     http_req_connecting............: avg=8.05µs   min=0s       med=0s       max=1.99ms   p(90)=0s       p(95)=0s      
     http_req_duration..............: avg=50.21ms  min=41.67ms  med=49.03ms  max=123.45ms p(90)=54.32ms  p(95)=57.89ms 
       { expected_response:true }...: avg=50.21ms  min=41.67ms  med=49.03ms  max=123.45ms p(90)=54.32ms  p(95)=57.89ms 
     http_req_failed................: 0.00%   ✓ 0          ✗ 5940
     http_req_receiving.............: avg=41.23µs  min=0s       med=0s       max=3.01ms   p(90)=0s       p(95)1.01ms 
     http_reqs......................: 5940    99/s
```

**Output:**
- Terminal output (text)
- JSON export (`k6 run --out json=test.json`)
- CSV export
- InfluxDB integration
- Grafana dashboards (via Prometheus)

#### NBomber
```
┌─────────────────────────────────────────────────────────────────────┐
│ Scenario: lock_test                                                 │
├────────────────────┬─────────┬─────────┬─────────┬─────────┬────────┤
│ Load Simulation    │ Value   │ Mean    │ StdDev  │ Min     │ Max    │
├────────────────────┼─────────┼─────────┼─────────┼─────────┼────────┤
│ Inject             │ -       │ -       │ -       │ -       │ -      │
├────────────────────┼─────────┼─────────┼─────────┼─────────┼────────┤
│ Request count      │ 5,940   │ -       │ -       │ -       │ -      │
│ OK                 │ 5,940   │ -       │ -       │ -       │ -      │
│ Failed             │ 0       │ -       │ -       │ -       │ -      │
│ RPS                │ 99      │ -       │ -       │ -       │ -      │
│ Latency (P50)      │ -       │ 49ms    │ 8ms     │ 42ms    │ 123ms  │
│ Latency (P75)      │ -       │ 52ms    │ -       │ -       │ -      │
│ Latency (P95)      │ -       │ 58ms    │ -       │ -       │ -      │
│ Latency (P99)      │ -       │ 78ms    │ -       │ -       │ -      │
└────────────────────┴─────────┴─────────┴─────────┴─────────┴────────┘
```

**Output:**
- Beautiful terminal tables
- **HTML report** (interactive, with graphs)
- **Markdown report** (for documentation)
- TXT report
- CSV export
- Custom plugins (Grafana, InfluxDB)

**HTML Report Features:**
- Interactive timeline graph
- Detailed percentile breakdown
- Status code distribution
- Sortable tables
- **Can be shared with stakeholders**

---

### 5. CI/CD Integration

#### k6
```yaml
# GitHub Actions
- name: Run k6 load test
  uses: grafana/k6-action@v0.3.1
  with:
    filename: loadtest.js
    
- name: Upload results
  uses: actions/upload-artifact@v3
  with:
    name: k6-results
    path: summary.json
```

**Pros:**
- Official Docker image
- Official GitHub Action
- Well-documented CI integrations

#### NBomber
```yaml
# GitHub Actions
- name: Setup .NET
  uses: actions/setup-dotnet@v3
  with:
    dotnet-version: '8.0.x'
    
- name: Run load tests
  run: dotnet run --project Chd.LoadTests -c Release
  
- name: Upload reports
  uses: actions/upload-artifact@v3
  with:
    name: nbomber-reports
    path: ./Chd.LoadTests/reports/
```

**Pros:**
- Standard .NET workflow
- No special actions needed
- Works in any .NET CI environment

---

### 6. Advanced Features

#### k6

**Strengths:**
- ✅ **Cloud execution** (k6 Cloud service)
- ✅ **Distributed load** (built-in)
- ✅ **Thresholds** (pass/fail criteria)
- ✅ **Scenarios** (multiple load patterns)
- ✅ **Browser testing** (k6 browser)

```javascript
export let options = {
    thresholds: {
        'http_req_duration': ['p(95)<500'],
        'http_req_failed': ['rate<0.01']
    }
};
```

#### NBomber

**Strengths:**
- ✅ **Plugins ecosystem** (HTTP, WebSockets, gRPC, MongoDB, Redis)
- ✅ **Custom reporting** (write your own)
- ✅ **.NET integration** (use any .NET library)
- ✅ **Data feeds** (from CSV, DB, etc.)
- ✅ **Dependency injection** (ServiceProvider)

```csharp
var dataFeed = DataFeed.Constant(users);

var scenario = Scenario.Create("test", async context =>
{
    var user = context.FeedItem; // From data feed
    // Use any .NET library here
    return Response.Ok();
})
.WithDataFeed(dataFeed);
```

---

## When to Use Each

### Use k6 if:
- ✅ Team is primarily JavaScript
- ✅ Need distributed load generation (k6 Cloud)
- ✅ Testing external APIs (not your C# code)
- ✅ Want industry-standard tool (Grafana Labs backing)
- ✅ Need browser automation (k6 browser)

### Use NBomber if:
- ✅ Team is primarily .NET/C#
- ✅ Need Visual Studio debugging
- ✅ Want to reuse C# code/models
- ✅ Testing your own .NET APIs
- ✅ Want everything in solution (no external tools)
- ✅ Need integration with .NET DI container
- ✅ Want professional HTML reports

---

## Our Recommendation

**For this project: Use NBomber**

**Why:**
1. **Same language** - Team already knows C#
2. **Debugging** - Critical for complex scenarios
3. **Code reuse** - Can use `ICoordinator` directly (no HTTP overhead)
4. **Reports** - HTML output is stakeholder-friendly
5. **Simplicity** - One less tool to install

**Keep k6 for:**
- Testing from external perspective (HTTP API)
- Comparing results (validation)
- Learning industry-standard tool

---

## Running Both

You can run both side-by-side for comparison!

### k6 (HTTP-based, external perspective)
```bash
k6 run loadtest.js
```

### NBomber (In-process, internal perspective)
```bash
cd Chd.LoadTests
dotnet run
```

**Compare results:**
- k6 includes network overhead
- NBomber shows pure library performance
- Both should show similar trends

---

## Migration Guide

### From k6 to NBomber

**k6 scenario:**
```javascript
export default function () {
    let response = http.post('http://localhost:5000/api/lock',
        JSON.stringify({ key: 'resource:1', ttl: 5 })
    );
}

export let options = {
    vus: 10,
    duration: '2m'
};
```

**NBomber equivalent:**
```csharp
var scenario = Scenario.Create("lock_test", async context =>
{
    var coordinator = serviceProvider.GetRequiredService<ICoordinator>();
    await coordinator.Lock.RunAsync("resource:1", TimeSpan.FromSeconds(5), async () => {});
    return Response.Ok();
})
.WithLoadSimulations(
    Simulation.KeepConstant(copies: 10, during: TimeSpan.FromMinutes(2))
);

NBomberRunner.RegisterScenarios(scenario).Run();
```

---

## Conclusion

Both tools are excellent. Choose based on:
- **Team skills** (JS vs C#)
- **Debugging needs** (critical → NBomber)
- **Tooling preference** (VS integration → NBomber)
- **Distributed load** (critical → k6)

**This project includes both**, so you can use what works best for each situation! 🚀

---

## Additional Resources

- **k6 Documentation**: https://k6.io/docs/
- **NBomber Documentation**: https://nbomber.com/docs/overview
- **k6 Examples**: https://k6.io/docs/examples/
- **NBomber Examples**: https://github.com/PragmaticFlow/NBomber/tree/dev/examples
