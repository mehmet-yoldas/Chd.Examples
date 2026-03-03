using Chd.Coordination;
using Chd.Coordination.Abstractions;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chd.UnitTest;

/// <summary>
/// Saga özelliği için kapsamlı testler
/// </summary>
public class SagaTests : IAsyncLifetime
{
    private ServiceProvider _serviceProvider = null!;
    private ICoordinator _coordinator = null!;

    public Task InitializeAsync()
    {
        var services = new ServiceCollection();
        services.AddCoordination(opt =>
        {
            opt.RedisConnectionString = "localhost:6379";
        });

        _serviceProvider = services.BuildServiceProvider();
        _coordinator = _serviceProvider.GetRequiredService<ICoordinator>();

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Saga_Should_Execute_All_Steps()
    {
        // Arrange
        var sagaId = $"test:saga:{Guid.NewGuid()}";
        var executedSteps = new List<string>();

        // Act
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () =>
            {
                executedSteps.Add("step1");
                await Task.Delay(10);
            });

            await saga.Step("step2", async () =>
            {
                executedSteps.Add("step2");
                await Task.Delay(10);
            });

            await saga.Step("step3", async () =>
            {
                executedSteps.Add("step3");
                await Task.Delay(10);
            });
        });

        // Assert
        Assert.Equal(3, executedSteps.Count);
        Assert.Equal("step1", executedSteps[0]);
        Assert.Equal("step2", executedSteps[1]);
        Assert.Equal("step3", executedSteps[2]);
    }

    [Fact]
    public async Task Saga_Should_Handle_Failure()
    {
        // Arrange
        var sagaId = $"test:saga:{Guid.NewGuid()}";
        var executedSteps = new List<string>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _coordinator.Saga.RunAsync(sagaId, async saga =>
            {
                await saga.Step("step1", async () =>
                {
                    executedSteps.Add("step1");
                    await Task.Delay(10);
                });

                await saga.Step("step2", async () =>
                {
                    executedSteps.Add("step2");
                    await Task.Delay(10);
                });

                await saga.Step("step3", async () =>
                {
                    executedSteps.Add("step3");
                    throw new InvalidOperationException("Step3 failed");
                });
            });
        });

        // Assert - All steps were executed until failure
        Assert.Equal(3, executedSteps.Count);
    }

    [Fact]
    public async Task Saga_Should_Resume_After_Crash()
    {
        // Arrange
        var sagaId = $"test:saga:{Guid.NewGuid()}";
        var executedSteps = new List<string>();

        // Act - First execution with partial completion
        try
        {
            await _coordinator.Saga.RunAsync(sagaId, async saga =>
            {
                await saga.Step("step1", async () =>
                {
                    executedSteps.Add("step1-first");
                    await Task.Delay(10);
                });

                await saga.Step("step2", async () =>
                {
                    executedSteps.Add("step2-first");
                    throw new InvalidOperationException("Simulated crash");
                });
            });
        }
        catch
        {
            // Expected
        }

        // Resume saga
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () =>
            {
                executedSteps.Add("step1-second");
                await Task.Delay(10);
            });

            await saga.Step("step2", async () =>
            {
                executedSteps.Add("step2-second");
                await Task.Delay(10);
            });

            await saga.Step("step3", async () =>
            {
                executedSteps.Add("step3-second");
                await Task.Delay(10);
            });
        });

        // Assert - Should not re-execute completed steps
        Assert.Contains("step1-first", executedSteps);
        Assert.DoesNotContain("step1-second", executedSteps);
        Assert.Contains("step2-second", executedSteps);
        Assert.Contains("step3-second", executedSteps);
    }

    [Fact]
    public async Task Saga_Should_Handle_Steps_Without_Compensation()
    {
        // Arrange
        var sagaId = $"test:saga:{Guid.NewGuid()}";
        var executedSteps = new List<string>();

        // Act
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("step1", async () =>
            {
                executedSteps.Add("step1");
                await Task.Delay(10);
            });

            await saga.Step("step2", async () =>
            {
                executedSteps.Add("step2");
                await Task.Delay(10);
            });
        });

        // Assert
        Assert.Equal(2, executedSteps.Count);
    }

    [Fact]
    public async Task Saga_Should_Support_Complex_Workflow()
    {
        // Arrange - Simulating order processing
        var sagaId = $"test:saga:order:{Guid.NewGuid()}";
        var orderState = new OrderState();

        // Act
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("validate-inventory", async () =>
            {
                orderState.InventoryValidated = true;
                await Task.Delay(10);
            });

            await saga.Step("reserve-items", async () =>
            {
                orderState.ItemsReserved = true;
                await Task.Delay(10);
            });

            await saga.Step("charge-payment", async () =>
            {
                orderState.PaymentCharged = true;
                await Task.Delay(10);
            });

            await saga.Step("create-shipment", async () =>
            {
                orderState.ShipmentCreated = true;
                await Task.Delay(10);
            });
        });

        // Assert
        Assert.True(orderState.InventoryValidated);
        Assert.True(orderState.ItemsReserved);
        Assert.True(orderState.PaymentCharged);
        Assert.True(orderState.ShipmentCreated);
    }

    [Fact]
    public async Task Saga_Should_Handle_Failure_In_Complex_Workflow()
    {
        // Arrange
        var sagaId = $"test:saga:order:{Guid.NewGuid()}";
        var orderState = new OrderState();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _coordinator.Saga.RunAsync(sagaId, async saga =>
            {
                await saga.Step("validate-inventory", async () =>
                {
                    orderState.InventoryValidated = true;
                    await Task.Delay(10);
                });

                await saga.Step("reserve-items", async () =>
                {
                    orderState.ItemsReserved = true;
                    await Task.Delay(10);
                });

                await saga.Step("charge-payment", async () =>
                {
                    orderState.PaymentCharged = true;
                    await Task.Delay(10);
                    throw new InvalidOperationException("Payment failed");
                });
            });
        });

        // Assert - Steps before failure were executed
        Assert.True(orderState.InventoryValidated);
        Assert.True(orderState.ItemsReserved);
        Assert.True(orderState.PaymentCharged); // Set before exception
        Assert.False(orderState.ShipmentCreated);
    }

    [Fact]
    public async Task Saga_Should_Support_Parallel_Different_Sagas()
    {
        // Arrange
        var tasks = new List<Task>();
        var executionCounts = new System.Collections.Concurrent.ConcurrentDictionary<string, int>();

        // Act - Run multiple different sagas in parallel
        for (int i = 0; i < 5; i++)
        {
            var sagaId = $"test:saga:parallel:{Guid.NewGuid()}";
            tasks.Add(Task.Run(async () =>
            {
                await _coordinator.Saga.RunAsync(sagaId, async saga =>
                {
                    await saga.Step("step1", async () =>
                    {
                        executionCounts.AddOrUpdate(sagaId, 1, (_, count) => count + 1);
                        await Task.Delay(10);
                    });
                });
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(5, executionCounts.Count);
        Assert.All(executionCounts.Values, count => Assert.Equal(1, count));
    }

    private class OrderState
    {
        public bool InventoryValidated { get; set; }
        public bool ItemsReserved { get; set; }
        public bool PaymentCharged { get; set; }
        public bool ShipmentCreated { get; set; }
    }
}
