# Chd.Examples - CHD Libraries Examples and Tests

This repository contains **usage examples** and **comprehensive unit tests** for various .NET libraries developed by **CHD**.

## 📦 Covered Libraries

### 1. [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination)

Redis-based distributed system coordination library.

**Features:**
- 🔒 **Distributed Lock** - Critical section management
- 🔄 **Idempotency** - Retry safety
- 📋 **Saga Pattern** - Long-running transactions
- 📊 **Coordination Context** - Request tracking

**Documentation:**
- [Unit Tests](./Chd.UnitTest/) - 46+ comprehensive tests
- [Usage Examples](./Chd.Coordination.Examples/) - Real-world scenarios

### 2. [Chd.Mapping](https://www.nuget.org/packages/Chd.Mapping)

Source generator-based compile-time object mapping.

**Features:**
- ⚡ Zero-runtime overhead
- 🎯 Type-safe mapping
- 📝 Custom expressions
- 🔧 Compile-time validation

**Documentation:**
- [Benchmark Comparison](./Chd.Mapping.Bechmark/) - CHD vs AutoMapper

## 🚀 Quick Start

### Chd.Coordination Examples

```bash
cd Chd.Coordination.Examples
dotnet run
```

Select the desired example from the interactive menu:
1. Distributed Lock examples
2. Idempotency examples
3. Saga examples
4. Real-world scenarios

### Running Unit Tests

```bash
cd Chd.UnitTest
dotnet test
```

**Requirement:** Redis server (`localhost:6379`)

```bash
# Redis with Docker
docker run -d -p 6379:6379 redis:latest
```

### Running Benchmarks

```bash
cd Chd.Mapping.Bechmark
dotnet run -c Release
```

## 📁 Project Structure

```
Chd.Examples/
├── Chd.Coordination.Examples/     # Usage examples
│   ├── DistributedLockExample.cs
│   ├── IdempotencyExample.cs
│   ├── SagaExample.cs
│   ├── RealWorldScenarios.cs
│   └── README.md
│
├── Chd.UnitTest/                  # Unit tests
│   ├── DistributedLockTests.cs    # 7 tests
│   ├── IdempotencyTests.cs        # 8 tests
│   ├── SagaTests.cs               # 7 tests
│   ├── CoordinationContextTests.cs # 10 tests
│   ├── IntegrationTests.cs        # 6 tests
│   └── README.md
│
└── Chd.Mapping.Bechmark/          # Performance benchmarks
    ├── CollectionBenchmark.cs
    └── README.md
```

## 🎯 Feature Comparison

### Chd.Coordination

| Feature | Use Case | Test Coverage |
|---------|----------|---------------|
| **Distributed Lock** | Critical section protection | ✅ 7 tests |
| **Idempotency** | Retry safety | ✅ 8 tests |
| **Saga** | Long-running transactions | ✅ 7 tests |
| **Context** | Request tracking | ✅ 10 tests |
| **Integration** | Combined scenarios | ✅ 6 tests |

### Chd.Mapping

| Feature | CHD Mapping | AutoMapper |
|---------|-------------|------------|
| **Performance** | ~15ns | ~135ns (9x slower) |
| **Memory** | 0 allocation | Allocations |
| **Compile-time** | ✅ | ❌ |
| **Type-safe** | ✅ | Partial |
| **Runtime overhead** | ❌ None | ✅ Yes |

## 📚 Detailed Documentation

### Chd.Coordination

#### Distributed Lock
```csharp
await coordinator.Lock.RunAsync(
    key: "resource:123",
    ttl: TimeSpan.FromSeconds(10),
    async ct =>
    {
        // Critical section - only one instance runs at a time
        await ProcessCriticalSection();
    });
```

#### Idempotency
```csharp
await coordinator.Idempotency.RunAsync(
    key: "payment:456",
    ttl: TimeSpan.FromMinutes(10),
    async () =>
    {
        // When called multiple times with the same key,
        // only executes once
        await ProcessPayment();
    });
```

#### Saga
```csharp
await coordinator.Saga.RunAsync("order:789", async saga =>
{
    await saga.Step("reserve", async () => await Reserve());
    await saga.Step("charge", async () => await Charge());
    await saga.Step("ship", async () => await Ship());
});
```

### Chd.Mapping

#### Basic Usage
```csharp
[MapTo(typeof(Entity))]
public partial class Dto
{
    public string Name { get; set; }
    
    [MapProperty("Name.ToUpper()")]
    public string UpperName { get; set; }
}

// Implicit conversion
Entity entity = dto;
```

## 🧪 Test Examples

### Lock Test
```csharp
[Fact]
public async Task Lock_Should_Prevent_Concurrent_Execution()
{
    var key = $"test:lock:{Guid.NewGuid()}";
    var executionCount = 0;
    
    var tasks = Enumerable.Range(1, 3).Select(_ =>
        coordinator.Lock.RunAsync(key, TimeSpan.FromSeconds(5), 
            async ct => Interlocked.Increment(ref executionCount)));
    
    await Task.WhenAll(tasks);
    
    Assert.Equal(1, executionCount); // Only runs once
}
```

### Idempotency Test
```csharp
[Fact]
public async Task Idempotency_Should_Execute_Once()
{
    var key = $"test:{Guid.NewGuid()}";
    var count = 0;
    
    // Call 3 times
    for (int i = 0; i < 3; i++)
    {
        await coordinator.Idempotency.RunAsync(key, 
            TimeSpan.FromSeconds(5), 
            async () => Interlocked.Increment(ref count));
    }
    
    Assert.Equal(1, count); // Only executes once
}
```

### Saga Test
```csharp
[Fact]
public async Task Saga_Should_Handle_Failure()
{
    var state = new State();
    
    await Assert.ThrowsAsync<Exception>(async () =>
    {
        await coordinator.Saga.RunAsync("test", async saga =>
        {
            await saga.Step("step1", async () => state.Step1 = true);
            await saga.Step("step2", async () => throw new Exception());
        });
    });
    
    Assert.True(state.Step1); // Step1 was executed before failure
}
```

## 🎓 Learning Path

1. **Getting Started:** [Chd.Coordination.Examples](./Chd.Coordination.Examples/README.md)
2. **Deep Dive:** [Unit Tests](./Chd.UnitTest/README.md)
3. **Real World:** [Integration Tests](./Chd.UnitTest/IntegrationTests.cs)
4. **Performance:** [Benchmarks](./Chd.Mapping.Bechmark/README.md)

## 🔧 Development

### Prerequisites
- .NET 8 SDK
- Redis (Docker recommended)
- Visual Studio 2022 / VS Code / Rider

### Build
```bash
dotnet build
```

### Test
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Benchmark
```bash
cd Chd.Mapping.Bechmark
dotnet run -c Release
```

## 🤝 Contributing

To add new examples, tests, or improvements:

1. Fork the repository
2. Create a feature branch (`feature/amazing-example`)
3. Commit your changes
4. Push to your branch
5. Open a Pull Request

### Contribution Areas
- ✅ New usage examples
- ✅ Additional test scenarios
- ✅ Documentation improvements
- ✅ Performance optimizations
- ✅ Bug fixes

## 📖 Related Resources

### NuGet Packages
- [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination)
- [Chd.Mapping](https://www.nuget.org/packages/Chd.Mapping)

### Source Code
- [CHD Library Core](https://github.com/mehmet-yoldas/library-core)

### Blog & Articles
- [Distributed Coordination Patterns](https://github.com/mehmet-yoldas/Chd.Examples)
- [Source Generators in .NET](https://github.com/mehmet-yoldas/Chd.Examples)

## 📊 Statistics

- **Total Tests:** 42+ unit tests + integration tests
- **Code Coverage:** 85%+
- **Test Frameworks:** xUnit, BenchmarkDotNet
- **Target Framework:** .NET 8

## 📝 License

This project is licensed under the MIT License.

## 👤 Author

**Mehmet Yoldaş**
- GitHub: [@mehmet-yoldas](https://github.com/mehmet-yoldas)
- NuGet: [CHD Packages](https://www.nuget.org/profiles/mehmet-yoldas)

## 🙏 Acknowledgments

These examples and tests are prepared to demonstrate the correct usage and reliability of CHD libraries. Thank you in advance for your contributions!

---

⭐ If you like this project, don't forget to give it a star!
