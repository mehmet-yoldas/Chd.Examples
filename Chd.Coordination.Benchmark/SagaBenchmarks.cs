using BenchmarkDotNet.Attributes;
using Chd.Coordination.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Chd.Coordination.Benchmark;

[MemoryDiagnoser]
[RankColumn]
[MinColumn, MaxColumn]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class SagaBenchmarks
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

    [Benchmark(Description = "Saga: Single Step")]
    public async Task Saga_SingleStep()
    {
        var sagaId = $"order:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("single-step", async () =>
            {
                await Task.Delay(1);
            });
        });
    }

    [Benchmark(Description = "Saga: Three Steps (Success)")]
    public async Task Saga_ThreeSteps_Success()
    {
        var sagaId = $"order:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("reserve-inventory", async () =>
            {
                await Task.Delay(1);
            });

            await saga.Step("charge-payment", async () =>
            {
                await Task.Delay(1);
            });

            await saga.Step("create-shipment", async () =>
            {
                await Task.Delay(1);
            });
        });
    }

    [Benchmark(Description = "Saga: Three Steps (No Work)")]
    public async Task Saga_ThreeSteps_NoWork()
    {
        var sagaId = $"order:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () => await Task.CompletedTask);
            await saga.Step("step2", async () => await Task.CompletedTask);
            await saga.Step("step3", async () => await Task.CompletedTask);
        });
    }

    [Benchmark(Description = "Saga: Five Steps (Complex Workflow)")]
    public async Task Saga_FiveSteps_ComplexWorkflow()
    {
        var sagaId = $"order:{Guid.NewGuid()}";
        
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("validate-order", async () => await Task.Delay(1));
            await saga.Step("reserve-inventory", async () => await Task.Delay(1));
            await saga.Step("process-payment", async () => await Task.Delay(1));
            await saga.Step("update-inventory", async () => await Task.Delay(1));
            await saga.Step("notify-customer", async () => await Task.Delay(1));
        });
    }

    [Benchmark(Description = "Saga: 10 Sequential Sagas")]
    public async Task Saga_Sequential()
    {
        for (int i = 0; i < 10; i++)
        {
            var sagaId = $"order:{Guid.NewGuid()}";
            
            await _coordinator.Saga.RunAsync(sagaId, async saga =>
            {
                await saga.Step("step1", async () => await Task.CompletedTask);
                await saga.Step("step2", async () => await Task.CompletedTask);
            });
        }
    }
}
