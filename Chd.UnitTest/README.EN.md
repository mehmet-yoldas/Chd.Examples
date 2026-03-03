# CHD Coordination Unit Tests

This directory contains **comprehensive automated tests** for the [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination) library using .NET 8 and xUnit. The goal is to verify that all features (distributed lock, idempotency, saga, context) work correctly, reliably, and performantly.

## 📋 Test Classes

### 1. DistributedLockTests (7 tests)

Tests all aspects of the distributed lock feature:

- ✅ Basic lock acquisition and release
- ✅ Action execution with lock
- ✅ Concurrent locking scenarios
- ✅ Timeout management
- ✅ Lock release on exception
- ✅ Cancellation support
- ✅ Parallel execution with different keys

**Test Scenarios:**
```csharp
Lock_Should_Acquire_And_Release_Successfully()
Lock_Should_Execute_Action()
Lock_Should_Prevent_Concurrent_Execution()
Lock_Should_Timeout_When_Cannot_Acquire()
Lock_Should_Release_On_Exception()
Lock_Should_Handle_Cancellation()
Lock_Should_Support_Different_Keys()
```

### 2. IdempotencyTests (8 tests)

Validates idempotency guarantees:

- ✅ Execute only once when called multiple times
- ✅ Cache execution results
- ✅ Re-execute after TTL expiration
- ✅ Single execution under concurrent calls
- ✅ Retry on exception
- ✅ Independent execution for different keys
- ✅ Complex state handling
- ✅ Double-payment prevention (real scenario)

**Test Scenarios:**
```csharp
Idempotency_Should_Execute_Once()
Idempotency_Should_Cache_Execution()
Idempotency_Should_Execute_Again_After_TTL()
Idempotency_Should_Handle_Concurrent_Calls()
Idempotency_Should_Handle_Exception()
Idempotency_Should_Support_Different_Keys()
Idempotency_Should_Handle_Complex_State()
Idempotency_Should_Prevent_Double_Payment()
```

### 3. SagaTests (7 tests)

Tests saga pattern implementation:

- ✅ Sequential execution of all steps
- ✅ Error handling
- ✅ Resume after crash/failure
- ✅ Steps without compensation
- ✅ Complex workflows
- ✅ Failure handling in complex workflows
- ✅ Parallel execution of different sagas

**Test Scenarios:**
```csharp
Saga_Should_Execute_All_Steps()
Saga_Should_Handle_Failure()
Saga_Should_Resume_After_Crash()
Saga_Should_Handle_Steps_Without_Compensation()
Saga_Should_Support_Complex_Workflow()
Saga_Should_Handle_Failure_In_Complex_Workflow()
Saga_Should_Support_Parallel_Different_Sagas()
```

### 4. CoordinationContextTests (10 tests)

Context management and traceability tests:

- ✅ CorrelationId creation
- ✅ Custom CorrelationId usage
- ✅ Context with LockKey
- ✅ Context with SagaId
- ✅ Context with all properties
- ✅ Unique ID generation
- ✅ Immutability
- ✅ ToString, Equality, GetHashCode
- ✅ Request flow tracking

**Test Scenarios:**
```csharp
Context_Should_Create_With_CorrelationId()
Context_Should_Create_With_Custom_CorrelationId()
Context_Should_Create_With_LockKey()
Context_Should_Create_With_SagaId()
Context_Should_Create_With_All_Properties()
Context_Should_Generate_Unique_CorrelationIds()
// ... and more
```

### 5. IntegrationTests (6 tests)

Tests combining multiple features:

- ✅ Lock + Idempotency combination
- ✅ Saga + Lock combination
- ✅ Idempotent Saga
- ✅ Bank transfer scenario (all features)
- ✅ Bank transfer failure scenario
- ✅ Job processing (3 features combined)

**Test Scenarios:**
```csharp
Integration_Lock_And_Idempotency()
Integration_Saga_With_Locked_Steps()
Integration_Idempotent_Saga()
Integration_BankTransfer_Scenario()
Integration_BankTransfer_Failure_Scenario()
Integration_JobProcessing_With_All_Features()
```

### 6. CoordinationUnitTest (Legacy - 4 tests)

Original basic tests (kept for backward compatibility).

## 📊 Test Statistics

- **Total Test Count:** 42+ tests
- **Covered Features:** 4 main features
- **Test Types:** Unit tests + Integration tests
- **Framework:** xUnit with .NET 8

## 🚀 Running Tests

### Requirements

- .NET 8 SDK
- Redis server (`localhost:6379`)

### Redis Setup

**With Docker:**
```bash
docker run -d -p 6379:6379 redis:latest
```

**Local installation:**
```bash
# Windows (Chocolatey)
choco install redis-64

# macOS (Homebrew)
brew install redis
redis-server

# Linux (Ubuntu/Debian)
sudo apt-get install redis-server
sudo service redis-server start
```

### Test Commands

```bash
# Run all tests
dotnet test

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# Specific test class
dotnet test --filter "FullyQualifiedName~DistributedLockTests"

# With code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🎯 Test Methodology

### Test Structure (AAA Pattern)

```csharp
[Fact]
public async Task TestName_Should_ExpectedBehavior_When_Condition()
{
    // Arrange - Prepare test data
    var key = $"test:{Guid.NewGuid()}";
    
    // Act - Execute the operation being tested
    var result = await _coordinator.Lock.RunAsync(...);
    
    // Assert - Verify results
    Assert.True(result);
}
```

### Test Naming

- `Feature_Should_Behavior` format
- Descriptive and clear
- Example: `Lock_Should_Release_On_Exception()`

### Test Independence

- Each test uses its own unique keys (`Guid.NewGuid()`)
- Proper setup/teardown with `IAsyncLifetime`
- Tests can run in parallel

## 🔍 Test Coverage

### Distributed Lock
- ✅ Normal flow
- ✅ Error conditions
- ✅ Concurrency
- ✅ Timeout
- ✅ Cancellation

### Idempotency
- ✅ Basic idempotency
- ✅ Caching
- ✅ TTL management
- ✅ Concurrent access
- ✅ Exception handling

### Saga
- ✅ Step execution
- ✅ Error handling
- ✅ Resume/recovery
- ✅ Complex workflows
- ✅ Failure scenarios

### Integration
- ✅ Feature combinations
- ✅ Real-world scenarios
- ✅ Error handling
- ✅ Failure scenarios

## 🐛 Test Debugging

### Visual Studio
```
Test Explorer → Right Click → Debug
```

### VS Code
```json
// launch.json
{
  "name": ".NET Core Test",
  "type": "coreclr",
  "request": "launch",
  "program": "dotnet",
  "args": ["test", "--filter", "TestName"],
  "cwd": "${workspaceFolder}/Chd.UnitTest"
}
```

### Command Line
```bash
# Debug specific test with detailed output
dotnet test --filter "Lock_Should_Acquire" --logger "console;verbosity=detailed"
```

## 📝 Adding New Tests

```csharp
[Fact]
public async Task NewFeature_Should_DoSomething()
{
    // Arrange
    var key = $"test:newfeature:{Guid.NewGuid()}";
    
    // Act
    var result = await _coordinator.NewFeature.ExecuteAsync(key);
    
    // Assert
    Assert.NotNull(result);
}
```

## 🤝 Contributing

To add new tests or improvements:

1. Fork the repository
2. Add/update tests
3. Verify with `dotnet test`
4. Submit a pull request

## 📖 Related Resources

- [Usage Examples](../Chd.Coordination.Examples/)
- [NuGet Package](https://www.nuget.org/packages/Chd.Coordination)
- [Source Code](https://github.com/mehmet-yoldas/library-core)

## 📄 License

MIT License
