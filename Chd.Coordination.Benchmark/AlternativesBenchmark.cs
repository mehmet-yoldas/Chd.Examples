using BenchmarkDotNet.Attributes;
using Chd.Coordination.Abstractions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace Chd.Coordination.Benchmark;

/// <summary>
/// 🎯 Real-World Comparison: Chd.Coordination vs Popular Alternatives
/// 
/// STRATEGY:
/// 1. Fair Lock Comparison - Same features (fencing, retry)
/// 2. Unique Features - What competitors CAN'T do
/// 3. Real-World Scenario - Payment processing with full guarantees
/// 
/// ALTERNATIVES:
/// - RedLock.net (Distributed Lock only)
/// - MediatR (In-memory pipeline only)
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class AlternativesBenchmark
{
    private ICoordinator _coordinator = null!;
    private ServiceProvider _serviceProvider = null!;
    private IMediator _mediator = null!;
    private RedLockFactory _redLockFactory = null!;
    private IDatabase _redis = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        // Setup Chd.Coordination
        (_serviceProvider, _coordinator) = await BenchmarkHelper.SetupAsync();
        _redis = _serviceProvider.GetRequiredService<IDatabase>();

        // Setup MediatR
        var services = new ServiceCollection();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AlternativesBenchmark).Assembly));
        var mediatorProvider = services.BuildServiceProvider();
        _mediator = mediatorProvider.GetRequiredService<IMediator>();

        // Setup RedLock
        var redisConnection = await BenchmarkHelper.GetSharedRedisAsync();
        var multiplexers = new List<RedLockMultiplexer> 
        { 
            new RedLockMultiplexer(redisConnection) 
        };
        _redLockFactory = RedLockFactory.Create(multiplexers);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
        _redLockFactory?.Dispose();
    }

    // ==================== 1️⃣ FAIR COMPARISON: Basic Lock ====================
    // Both use distributed lock with similar guarantees

    [Benchmark(Baseline = true, Description = "Chd: Basic Lock (10ms work)")]
    public async Task Lock_Chd_Basic()
    {
        var resourceId = $"lock:{Guid.NewGuid()}";

        var lockHandle = await _coordinator.Lock.AcquireAsync(
            resourceId,
            ttl: TimeSpan.FromSeconds(5),
            waitTime: TimeSpan.FromSeconds(10),
            cancellationToken: CancellationToken.None
        );

        if (lockHandle == null)
            throw new InvalidOperationException("Failed to acquire lock");

        try
        {
            await Task.Delay(10); // Simulate work
        }
        finally
        {
            await lockHandle.ReleaseAsync();
        }
    }

    [Benchmark(Description = "RedLock: Basic Lock (10ms work)")]
    public async Task Lock_RedLock_Basic()
    {
        var resourceId = $"lock:{Guid.NewGuid()}";

        await using var redLock = await _redLockFactory.CreateLockAsync(
            resource: resourceId,
            expiryTime: TimeSpan.FromSeconds(5),
            waitTime: TimeSpan.FromSeconds(10),
            retryTime: TimeSpan.FromMilliseconds(100)
        );

        if (!redLock.IsAcquired)
            throw new InvalidOperationException("Failed to acquire lock");

        await Task.Delay(10); // Simulate work
    }

    // ==================== 2️⃣ UNIQUE FEATURES: What Competitors CAN'T Do ====================

    [Benchmark(Description = "Chd: Idempotency (Built-in, Automatic)")]
    public async Task Unique_Chd_Idempotency()
    {
        var operationId = $"payment:{Guid.NewGuid()}";

        // First execution - will run
        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () =>
            {
                await Task.Delay(10); // Payment processing
            },
            cancellationToken: CancellationToken.None
        );

        // Duplicate - will be skipped (idempotency!)
        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () =>
            {
                await Task.Delay(10);
            },
            cancellationToken: CancellationToken.None
        );
    }

    [Benchmark(Description = "RedLock: Idempotency (Manual Redis Checks)")]
    public async Task Unique_RedLock_NoIdempotency()
    {
        var operationId = $"payment:{Guid.NewGuid()}";

        // First execution
        var key = $"idempotency:{operationId}";
        var exists = await _redis.StringGetAsync(key);
        if (exists.IsNullOrEmpty)
        {
            await Task.Delay(10);
            await _redis.StringSetAsync(key, "done", TimeSpan.FromHours(1));
        }

        // Duplicate check
        exists = await _redis.StringGetAsync(key);
        if (exists.IsNullOrEmpty)
        {
            await Task.Delay(10);
            await _redis.StringSetAsync(key, "done", TimeSpan.FromHours(1));
        }
    }

    [Benchmark(Description = "Chd: Saga (Redis Persistence + Auto Rollback)")]
    public async Task Unique_Chd_Saga()
    {
        var sagaId = $"order:{Guid.NewGuid()}";

        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            // Step 1: Reserve inventory
            await saga.Step("reserve-inventory", async () => await Task.Delay(10));

            // Step 2: Charge payment
            await saga.Step("charge-payment", async () => await Task.Delay(10));

            // Step 3: Send confirmation
            await saga.Step("send-confirmation", async () => await Task.Delay(10));
        });
    }

    [Benchmark(Description = "MediatR: Saga (Manual Redis Persistence)")]
    public async Task Unique_MediatR_WithPersistence()
    {
        var sagaId = $"order:{Guid.NewGuid()}";

        // Manual state management required for persistence
        // Step 1: Reserve inventory + Save state
        await _redis.StringSetAsync($"saga:{sagaId}:step1", "pending", TimeSpan.FromMinutes(5));
        await _mediator.Send(new SampleRequest());
        await _redis.StringSetAsync($"saga:{sagaId}:step1", "completed", TimeSpan.FromMinutes(5));

        // Step 2: Charge payment + Save state
        await _redis.StringSetAsync($"saga:{sagaId}:step2", "pending", TimeSpan.FromMinutes(5));
        await _mediator.Send(new SampleRequest());
        await _redis.StringSetAsync($"saga:{sagaId}:step2", "completed", TimeSpan.FromMinutes(5));

        // Step 3: Send confirmation + Save state
        await _redis.StringSetAsync($"saga:{sagaId}:step3", "pending", TimeSpan.FromMinutes(5));
        await _mediator.Send(new SampleRequest());
        await _redis.StringSetAsync($"saga:{sagaId}:step3", "completed", TimeSpan.FromMinutes(5));
    }

    // ==================== 3️⃣ REAL-WORLD SCENARIO: Payment Processing ====================

    [Benchmark(Description = "Chd: Complete Payment (Lock+Idempotency+Retry)")]
    public async Task RealWorld_Chd_CompletePayment()
    {
        var paymentId = $"payment:{Guid.NewGuid()}";
        var userId = $"user:{Guid.NewGuid()}";

        // All-in-one: Lock + Idempotency + Automatic state management
        await _coordinator.Idempotency.RunAsync(
            paymentId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () =>
            {
                var lockHandle = await _coordinator.Lock.AcquireAsync(
                    $"user-balance:{userId}",
                    ttl: TimeSpan.FromSeconds(5),
                    waitTime: TimeSpan.FromSeconds(10),
                    cancellationToken: CancellationToken.None
                );

                if (lockHandle == null)
                    throw new InvalidOperationException("Failed to acquire lock");

                try
                {
                    // Process payment
                    await Task.Delay(10);
                }
                finally
                {
                    await lockHandle.ReleaseAsync();
                }
            },
            cancellationToken: CancellationToken.None
        );
    }

    [Benchmark(Description = "RedLock+Manual: Payment (3 libraries needed)")]
    public async Task RealWorld_Competitors_ManualImplementation()
    {
        var paymentId = $"payment:{Guid.NewGuid()}";
        var userId = $"user:{Guid.NewGuid()}";

        // Manual idempotency check
        var idempotencyKey = $"idempotency:{paymentId}";
        var processed = await _redis.StringGetAsync(idempotencyKey);
        if (!processed.IsNullOrEmpty)
            return; // Already processed

        // Acquire lock
        await using var redLock = await _redLockFactory.CreateLockAsync(
            resource: $"user-balance:{userId}",
            expiryTime: TimeSpan.FromSeconds(5),
            waitTime: TimeSpan.FromSeconds(10),
            retryTime: TimeSpan.FromMilliseconds(100)
        );

        if (!redLock.IsAcquired)
            throw new InvalidOperationException("Failed to acquire lock");

        // Process payment
        await Task.Delay(10);

        // Mark as processed
        await _redis.StringSetAsync(idempotencyKey, "done", TimeSpan.FromHours(1));
    }
}

// MediatR Request/Response definitions
public record SampleRequest : IRequest<string>;

public class SampleHandler : IRequestHandler<SampleRequest, string>
{
    public async Task<string> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return "completed";
    }
}

public class Step1Behavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return await next();
    }
}

public class Step2Behavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);
        return await next();
    }
}
