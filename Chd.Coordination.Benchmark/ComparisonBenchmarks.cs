using BenchmarkDotNet.Attributes;
using Chd.Coordination.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chd.Coordination.Benchmark;

/// <summary>
/// Comprehensive benchmark comparing all coordination patterns
/// Shows overhead differences between Lock, Idempotency, and Saga
/// </summary>
[MemoryDiagnoser]
[RankColumn]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class ComparisonBenchmarks
{
    private ICoordinator _coordinator = null!;
    private ServiceProvider _serviceProvider = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        (_serviceProvider, _coordinator) = await BenchmarkHelper.SetupAsync();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider?.Dispose();
        // Redis connection is shared and will be closed at app shutdown
    }

    // Baseline: Direct Redis operation overhead
    [Benchmark(Baseline = true, Description = "Baseline: Lock Only")]
    public async Task Baseline_LockOnly()
    {
        var resourceId = $"resource:{Guid.NewGuid()}";

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
            await Task.CompletedTask;
        }
        finally
        {
            await lockHandle.ReleaseAsync();
        }
    }

    [Benchmark(Description = "Pattern: Idempotency")]
    public async Task Pattern_Idempotency()
    {
        var operationId = $"operation:{Guid.NewGuid()}";
        
        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () => await Task.CompletedTask
        );
    }

    [Benchmark(Description = "Pattern: Saga (Single Step)")]
    public async Task Pattern_Saga_SingleStep()
    {
        var sagaId = $"saga:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () => await Task.CompletedTask);
        });
    }

    [Benchmark(Description = "Pattern: Saga (Three Steps)")]
    public async Task Pattern_Saga_ThreeSteps()
    {
        var sagaId = $"saga:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () => await Task.CompletedTask);
            await saga.Step("step2", async () => await Task.CompletedTask);
            await saga.Step("step3", async () => await Task.CompletedTask);
        });
    }

    // Real-world scenario: Simulate actual work
    [Benchmark(Description = "Real-World: Lock with 10ms Work")]
    public async Task RealWorld_LockWithWork()
    {
        var resourceId = $"resource:{Guid.NewGuid()}";

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
            await Task.Delay(10);
        }
        finally
        {
            await lockHandle.ReleaseAsync();
        }
    }

    [Benchmark(Description = "Real-World: Idempotency with 10ms Work")]
    public async Task RealWorld_IdempotencyWithWork()
    {
        var operationId = $"operation:{Guid.NewGuid()}";
        
        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () => await Task.Delay(10)
        );
    }

    [Benchmark(Description = "Real-World: Saga with 10ms per Step")]
    public async Task RealWorld_SagaWithWork()
    {
        var sagaId = $"saga:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () => await Task.Delay(10));
            await saga.Step("step2", async () => await Task.Delay(10));
            await saga.Step("step3", async () => await Task.Delay(10));
        });
    }
}
