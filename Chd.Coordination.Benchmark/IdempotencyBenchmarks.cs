using BenchmarkDotNet.Attributes;
using Chd.Coordination.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chd.Coordination.Benchmark;

[MemoryDiagnoser]
[RankColumn]
[MinColumn, MaxColumn]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class IdempotencyBenchmarks
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

    [Benchmark(Description = "Idempotency: First Execution")]
    public async Task Idempotency_FirstExecution()
    {
        var operationId = $"payment:{Guid.NewGuid()}";

        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () =>
            {
                await Task.Delay(1); // Simulate work
            }
        );
    }

    [Benchmark(Description = "Idempotency: Duplicate Detection (Same ID)")]
    public async Task Idempotency_DuplicateDetection()
    {
        var operationId = "duplicate-payment-123";

        // First execution will run the action
        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: async () =>
            {
                await Task.Delay(1);
            }
        );
    }

    [Benchmark(Description = "Idempotency: 10 Sequential Operations")]
    public async Task Idempotency_Sequential()
    {
        for (int i = 0; i < 10; i++)
        {
            var operationId = $"payment:{Guid.NewGuid()}";

            await _coordinator.Idempotency.RunAsync(
                operationId,
                ttl: TimeSpan.FromMinutes(5),
                action: async () =>
                {
                    await Task.CompletedTask;
                }
            );
        }
    }

    [Benchmark(Description = "Idempotency: With Return Value")]
    public async Task<string> Idempotency_WithReturnValue()
    {
        var operationId = $"payment:{Guid.NewGuid()}";

        await _coordinator.Idempotency.RunAsync(
            operationId,
            ttl: TimeSpan.FromMinutes(5),
            action: () =>
            {
                return Task.CompletedTask;
            }
        );

        return "payment-confirmed-123";
    }
}
