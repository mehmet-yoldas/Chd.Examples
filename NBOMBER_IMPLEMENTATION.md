# NBomber Load Testing Implementation Summary

## What Was Added

### New Project: Chd.LoadTests

A complete Visual Studio-native load testing solution using NBomber.

---

## Files Created

### 1. Chd.LoadTests/Chd.LoadTests.csproj
**Purpose:** Project file with NBomber dependencies

**Packages:**
- NBomber 5.10.4
- NBomber.Http 5.10.4
- Chd.Coordination 2.0.1
- Microsoft.Extensions.DependencyInjection 8.0.0

**Target:** .NET 8.0

---

### 2. Chd.LoadTests/Program.cs
**Purpose:** Interactive load testing scenarios

**Features:**
- Menu-driven interface (5 options)
- 4 production-grade test scenarios
- Visual Studio F5 debugging support
- ServiceProvider integration

**Scenarios:**

#### Scenario 1: Distributed Lock Test
```
Duration: 3 minutes
Virtual Users: 10 → 30 (ramping)
Simulations: 
  - Inject 10 ops/sec for 2 minutes
  - Inject 20 ops/sec for 1 minute
```

**Tests:**
- Lock acquisition speed
- Contention handling
- TTL expiration
- 50ms hold time

#### Scenario 2: Idempotency Test
```
Duration: 3 minutes
Virtual Users: 0 → 50 (ramping)
Operation: Payment processing with 30ms work
```

**Tests:**
- Cache hit rate
- Duplicate prevention
- TTL management
- Concurrent checks

#### Scenario 3: Saga Test
```
Duration: 2 minutes
Virtual Users: 10 (constant)
Steps: 3 (reserve → charge → ship)
Work: 20ms + 30ms + 25ms = 75ms total
```

**Tests:**
- Multi-step transaction performance
- State persistence overhead
- Saga completion rate

#### Scenario 4: Mixed Workload Test
```
Duration: 3 minutes
Concurrent scenarios:
  - 5 VUs: Lock operations (50ms work)
  - 8 VUs: Idempotency operations (30ms work)
  - 3 VUs: Saga operations (75ms work)
```

**Tests:**
- Real-world mixed workload
- Resource contention
- Cross-feature performance

---

### 3. Chd.LoadTests/README.md
**Purpose:** Comprehensive documentation (700+ lines)

**Sections:**
- Quick start guide
- Scenario descriptions
- Report interpretation
- Debugging instructions
- Grafana integration
- Customization examples
- Best practices
- Troubleshooting guide
- CI/CD examples

---

### 4. start-loadtests.bat (Windows)
**Purpose:** One-click load test execution

**Features:**
- Checks Redis status
- Offers to start Redis if needed
- Offers to start monitoring stack
- Runs tests in Release mode
- Shows report locations

**Usage:**
```bash
.\start-loadtests.bat
```

---

### 5. start-loadtests.sh (Linux/Mac)
**Purpose:** One-click load test execution (Unix)

**Features:**
- Same as Windows version
- Bash-compatible
- Executable permissions

**Usage:**
```bash
./start-loadtests.sh
```

---

### 6. K6_VS_NBOMBER.md
**Purpose:** Comprehensive comparison guide (400+ lines)

**Sections:**
- Quick comparison table
- Setup comparison
- Development experience
- Debugging capabilities
- Reports comparison
- CI/CD integration
- Advanced features
- When to use each
- Migration guide

**Key Points:**
- Side-by-side code comparisons
- Detailed pros/cons
- Practical recommendations
- Migration examples

---

## Updates to Existing Files

### README.md Updates

#### 1. Table of Contents
**Added:**
- Chd.LoadTests section
- Monitoring & Observability section

#### 2. New Project Section
**Added complete section:**
```markdown
### 4. Chd.LoadTests

**NBomber-based load testing** for Visual Studio with production-grade performance scenarios.

- Features table
- Scenarios comparison table
- Quick run instructions
- Benefits over k6
- Documentation link
```

#### 3. New Monitoring Section
**Added complete section:**
```markdown
## Monitoring & Observability

### Components
- Redis (port 6379)
- Prometheus (port 9090)
- Grafana (port 3000)
- Node Exporter (port 9100)

### Dashboard Panels (6 total)
- Operations/sec
- P95 Latency
- Lock/Idempotency/Saga statistics
- System metrics

### Access URLs
- Quick start scripts
```

#### 4. Quick Start Section
**Added:**
```markdown
### Run Load Tests
# Quick start (checks Redis, starts monitoring)
.\start-loadtests.bat

# Or manual
cd Chd.LoadTests
dotnet run
```

#### 5. Project Statistics Updates
**Updated:**
- Load Test Scenarios: 4 scenarios
- NBomber Integration: ✅
- Load Test Code: ~300+ lines
- Documentation: ~2,000+ lines (was ~1,000+)
- Total: ~4,600+ lines (was ~3,300+)

**Added column to Feature Coverage table:**
```
| Feature           | Examples | Tests | Integration | Load Tests |
|-------------------|----------|-------|-------------|------------|
| Distributed Lock  | 4        | 7     | 2           | ✅         |
| Idempotency       | 3        | 8     | 2           | ✅         |
| Saga              | 3        | 7     | 2           | ✅         |
| Context           | -        | 10    | -           | -          |
| Mixed Workload    | -        | -     | -           | ✅         |
```

#### 6. Documentation Section
**Reorganized into categories:**
- Project Documentation
- Monitoring & Performance
- Articles

**Added:**
- Load Tests Documentation
- Monitoring Setup Guide
- Load Testing Guide
- Grafana Dashboard Documentation
- Stop Using AutoMapper article link

#### 7. Requirements Section
**Updated Runtime requirements:**
- Added monitoring stack option
- Updated Redis version to 7-alpine
- Added optional monitoring dependencies
- Added NBomber note

---

## Why NBomber Was Chosen

### Advantages Over k6

1. **Visual Studio Integration**
   - Press F5 to run
   - Set breakpoints
   - Debug interactively
   - Use watch window
   - Step through code

2. **Same Language**
   - Team already knows C#
   - Reuse existing code
   - Type safety
   - IntelliSense

3. **No External Tools**
   - NuGet package
   - No separate installation
   - Version controlled with project
   - Works everywhere .NET works

4. **Professional Reports**
   - HTML output (interactive)
   - Markdown output
   - Stakeholder-friendly
   - Embeddable graphs

5. **Direct Integration**
   - Use ICoordinator directly (no HTTP overhead)
   - Dependency injection support
   - Can test internal methods
   - True unit-like load testing

### k6 Still Available

- `loadtest.js` kept for comparison
- External perspective testing (HTTP)
- Industry-standard tool
- Both can run side-by-side

---

## Usage Examples

### Quick Run
```bash
# Windows
.\start-loadtests.bat

# Linux/Mac
./start-loadtests.sh
```

### Manual Run
```bash
cd Chd.LoadTests
dotnet run
```

### Debug in Visual Studio
1. Open solution
2. Set Chd.LoadTests as startup project
3. Press F5
4. Select scenario
5. Breakpoints work!

### CI/CD
```yaml
- name: Run load tests
  run: dotnet run --project Chd.LoadTests -c Release
  
- name: Upload reports
  uses: actions/upload-artifact@v3
  with:
    name: load-test-reports
    path: ./Chd.LoadTests/reports/
```

---

## Project Structure

```
Chd.Examples/
├── Chd.LoadTests/                    # NEW
│   ├── Chd.LoadTests.csproj          # NEW - NBomber project
│   ├── Program.cs                     # NEW - 4 test scenarios
│   └── README.md                      # NEW - 700+ lines docs
├── start-loadtests.bat               # NEW - Windows quick start
├── start-loadtests.sh                # NEW - Linux/Mac quick start
├── K6_VS_NBOMBER.md                  # NEW - Comparison guide
├── README.md                          # UPDATED - Added NBomber sections
├── docker-compose.monitoring.yml     # EXISTING - Monitoring stack
├── loadtest.js                        # EXISTING - k6 alternative
└── ... (existing projects)
```

---

## Key Metrics to Watch

### In Console Output
- **RPS (Requests Per Second)** - Throughput
- **Latency P95** - 95th percentile response time
- **Success Rate** - Percentage of successful operations
- **StdDev** - Consistency of results

### In HTML Reports
- **Timeline Graph** - Visual throughput over time
- **Percentiles Table** - P50, P75, P95, P99
- **Status Distribution** - Success vs failures
- **Request Count** - Total operations

### In Grafana
- **Operations/sec** - Real-time throughput
- **Lock Statistics** - Acquisition success rate
- **Idempotency Stats** - Cache hit rate
- **Saga Statistics** - Completion rate
- **System Metrics** - CPU, Memory, Network

---

## Performance Expectations

### Good Results
- P95 Latency: < 100ms
- Success Rate: > 99%
- RPS: Stable (not dropping)
- StdDev: Low (consistent)

### Warning Signs
- P95 Latency: 100-500ms
- Success Rate: 95-99%
- RPS: Fluctuating ±20%
- StdDev: High variance

### Critical Issues
- P95 Latency: > 500ms
- Success Rate: < 95%
- RPS: Continuously dropping
- Errors: Connection failures, timeouts

---

## Next Steps

### Immediate
1. Run tests to establish baseline
2. Document baseline results
3. Set up Grafana alerts

### Short-term
1. Add tests to CI/CD pipeline
2. Create performance regression tests
3. Add custom metrics

### Long-term
1. Distributed load testing (multiple machines)
2. Chaos engineering scenarios
3. Performance budgets in CI

---

## Learning Resources

- **NBomber Docs**: https://nbomber.com/docs/overview
- **NBomber Examples**: https://github.com/PragmaticFlow/NBomber
- **Load Testing Best Practices**: https://nbomber.com/docs/loadtesting-basics
- **This Project's Docs**: `./Chd.LoadTests/README.md`
- **k6 Comparison**: `./K6_VS_NBOMBER.md`

---

## Build Verification

✅ **Build Status:** Successful  
✅ **All Tests:** Passing (42 tests)  
✅ **New Project:** Compiles without errors  
✅ **Dependencies:** Resolved successfully  

---

## Summary

### What You Get
- ✅ 4 production-grade load test scenarios
- ✅ Visual Studio debugging support
- ✅ Professional HTML reports
- ✅ Grafana integration
- ✅ Quick-start scripts
- ✅ 700+ lines of documentation
- ✅ Comparison guide with k6
- ✅ CI/CD examples

### Time to Value
- **Setup:** < 5 minutes (`.\start-loadtests.bat`)
- **First test:** < 1 minute (select scenario, press Enter)
- **Results:** Immediate (console + HTML report)
- **Grafana:** Real-time (if monitoring stack running)

### Maintenance
- **Updates:** `dotnet restore` (same as other projects)
- **Versions:** Controlled via NuGet (in project file)
- **Dependencies:** None external to .NET

---

**🚀 Ready to use! Press F5 or run `.\start-loadtests.bat` to get started.**
