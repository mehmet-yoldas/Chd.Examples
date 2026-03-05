using BenchmarkDotNet.Attributes;
using Chd.Coordination.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chd.Coordination.Benchmark;

[MemoryDiagnoser]
[RankColumn]
[MinColumn, MaxColumn]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class LockBenchmarks
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

    [Benchmark(Description = "Lock: Acquire & Release (No Contention)")]
    public async Task Lock_NoContention()
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
            await Task.Delay(1); // Minimal work
        }
        finally
        {
            await lockHandle.ReleaseAsync();
        }
    }

    [Benchmark(Description = "Lock: Acquire & Release (With Work)")]
    public async Task Lock_WithWork()
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
            // Simulate actual work
            await Task.Delay(10);
        }
        finally
        {
            await lockHandle.ReleaseAsync();
        }
    }

    [Benchmark(Description = "Lock: Same Resource (Contention Simulation)")]
    public async Task Lock_SameResource()
    {
        // Multiple calls to same resource - measures lock acquisition overhead
        var resourceId = "shared-resource";

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
            await Task.Delay(1);
        }
        finally
        {
            await lockHandle.ReleaseAsync();
        }
    }

    [Benchmark(Description = "Lock: Rapid Acquire/Release (10 sequential)")]
    public async Task Lock_RapidSequential()
    {
        var resourceId = $"resource:{Guid.NewGuid()}";

        for (int i = 0; i < 10; i++)
        {
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
    }
}
