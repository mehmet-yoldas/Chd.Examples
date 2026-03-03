# Chd.Coordination - Usage Examples

This project contains comprehensive examples demonstrating all features of the [Chd.Coordination](https://www.nuget.org/packages/Chd.Coordination) library.

## 🚀 Getting Started

### Requirements

- .NET 8 SDK
- Redis server (default: `localhost:6379`)

### Redis Setup

**Windows (Docker):**
```bash
docker run -d -p 6379:6379 redis:latest
```

**macOS/Linux:**
```bash
# With Homebrew
brew install redis
redis-server

# With Docker
docker run -d -p 6379:6379 redis:latest
```

### Running

```bash
cd Chd.Coordination.Examples
dotnet run
```

## 📚 Examples

### 1. Distributed Lock Examples

The distributed lock feature ensures that critical code sections are executed by only one server/instance at a time in distributed systems.

**Example Scenarios:**
- ✅ Basic lock usage
- ✅ Lock with action execution
- ✅ Sequential lock acquisition
- ✅ Parallel locks on different keys

**Use Cases:**
- Critical data updates (inventory, balance, etc.)
- Unique job execution
- Rate limiting
- Cache updates

### 2. Idempotency Examples

The idempotency feature guarantees that when the same operation is called multiple times, it executes only once and returns the same result.

**Example Scenarios:**
- ✅ Basic idempotency
- ✅ Idempotency with caching behavior
- ✅ Payment processing (double-click protection)

**Use Cases:**
- Payment processing
- Email/SMS sending
- Webhook processing
- API retry mechanisms
- Event sourcing

### 3. Saga Examples

Saga pattern manages long-running distributed transactions consisting of multiple steps. Each step can have a compensation (rollback) defined.

**Example Scenarios:**
- ✅ Basic saga usage
- ✅ Saga with error handling
- ✅ Order processing saga

**Use Cases:**
- E-commerce order processing
- Reservation systems
- User onboarding
- Microservice orchestration

### 4. Real-World Scenarios

Complex scenarios combining multiple features.

**Example Scenarios:**
- ✅ Bank transfer (Saga + Lock + Idempotency)
- ✅ Concurrent job processing (Lock + Context)
- ✅ Event processing (Idempotency + deduplication)

## 🎯 Feature Comparison

| Feature | Purpose | When to Use |
|---------|---------|-------------|
| **Distributed Lock** | Critical section protection | Only one instance should run at a time |
| **Idempotency** | Retry safety | Operation may be called multiple times |
| **Saga** | Long transaction management | Multiple steps, rollback needed |
| **CoordinationContext** | Traceability | Request tracking, correlation |

## 🏗️ Architecture Notes

### Distributed Lock vs Idempotency

```csharp
// ❌ Wrong - Lock usage unnecessary
await coordinator.Lock.RunAsync("send-email:123", async ct => 
{
    await SendEmail(); // Should be idempotent already
});

// ✅ Right - Idempotency is sufficient
await coordinator.Idempotency.RunAsync("send-email:123", async () => 
{
    await SendEmail();
});
```

**Lock:** Concurrency control (mutual exclusion)  
**Idempotency:** Retry safety (duplicate protection)

### Saga Best Practices

```csharp
await coordinator.Saga.RunAsync("order-123", async saga =>
{
    // ✅ Define steps for important operations
    await saga.Step("charge-payment", async () => await ChargeCard());
    await saga.Step("reserve-inventory", async () => await ReserveItems());
    await saga.Step("create-shipment", async () => await CreateShipment());
    
    // ❌ Don't put all business logic in one step
    // Each logical operation should be a separate step
});
```

## 🔧 Configuration

```csharp
services.AddCoordination(opt =>
{
    // Redis connection string
    opt.RedisConnectionString = "localhost:6379";
    
    // Optional: Prefix
    opt.KeyPrefix = "myapp:";
});
```

## 📖 Related Resources

- [NuGet Package](https://www.nuget.org/packages/Chd.Coordination)
- [Source Code](https://github.com/mehmet-yoldas/library-core)
- [Unit Tests](../Chd.UnitTest/)

## 🤝 Contributing

Feel free to send pull requests for new example scenarios or improvements!

## 📝 License

These examples are provided under the MIT License.
