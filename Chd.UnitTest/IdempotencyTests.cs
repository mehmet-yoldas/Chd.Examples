using Chd.Coordination;
using Chd.Coordination.Abstractions;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chd.UnitTest;

/// <summary>
/// Idempotency özelliği için kapsamlı testler
/// </summary>
public class IdempotencyTests : IAsyncLifetime
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
    public async Task Idempotency_Should_Execute_Once()
    {
        // Arrange
        var key = $"test:idempotency:{Guid.NewGuid()}";
        int executionCount = 0;

        // Act - Call multiple times
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            await Task.Delay(50);
        });

        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            await Task.Delay(50);
        });

        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            await Task.Delay(50);
        });

        // Assert
        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task Idempotency_Should_Cache_Execution()
    {
        // Arrange
        var key = $"test:idempotency:{Guid.NewGuid()}";
        int executionCount = 0;
        var testValue = Guid.NewGuid().ToString();
        string? capturedValue = null;

        // Act - First execution
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            capturedValue = testValue;
            await Task.Delay(50);
        });

        // Second execution should be skipped
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            capturedValue = "different-value";
            await Task.Delay(50);
        });

        // Assert
        Assert.Equal(1, executionCount);
        Assert.Equal(testValue, capturedValue); // Should keep first value
    }

    [Fact]
    public async Task Idempotency_Should_Execute_Again_After_TTL()
    {
        // Arrange
        var key = $"test:idempotency:{Guid.NewGuid()}";
        int executionCount = 0;

        // Act - First execution
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromMilliseconds(500), async () =>
        {
            executionCount++;
            await Task.Delay(10);
        });

        // Wait for TTL to expire
        await Task.Delay(600);

        // Second execution after TTL
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromMilliseconds(500), async () =>
        {
            executionCount++;
            await Task.Delay(10);
        });

        // Assert
        Assert.Equal(2, executionCount);
    }

    [Fact]
    public async Task Idempotency_Should_Handle_Concurrent_Calls()
    {
        // Arrange
        var key = $"test:idempotency:{Guid.NewGuid()}";
        int executionCount = 0;
        var tasks = new List<Task>();

        // Act - Concurrent calls
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
                {
                    Interlocked.Increment(ref executionCount);
                    await Task.Delay(100);
                });
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Should execute only once
        Assert.Equal(1, executionCount);
    }

    [Fact]
    public async Task Idempotency_Should_Handle_Exception()
    {
        // Arrange
        var key = $"test:idempotency:{Guid.NewGuid()}";
        int executionCount = 0;

        // Act & Assert - First call throws exception
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
            {
                executionCount++;
                await Task.Delay(50);
                throw new InvalidOperationException("Test exception");
            });
        });

        // Second call should execute (first one failed)
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            await Task.Delay(50);
        });

        // Assert - Both executions happened (failures don't cache)
        Assert.Equal(2, executionCount);
    }

    [Fact]
    public async Task Idempotency_Should_Support_Different_Keys()
    {
        // Arrange
        var key1 = $"test:idempotency:{Guid.NewGuid()}";
        var key2 = $"test:idempotency:{Guid.NewGuid()}";
        int executionCount = 0;

        // Act
        await _coordinator.Idempotency.RunAsync(key1, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            await Task.Delay(50);
        });

        await _coordinator.Idempotency.RunAsync(key2, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            await Task.Delay(50);
        });

        // Assert - Different keys should both execute
        Assert.Equal(2, executionCount);
    }

    [Fact]
    public async Task Idempotency_Should_Handle_Complex_State()
    {
        // Arrange
        var key = $"test:idempotency:{Guid.NewGuid()}";
        var testData = new TestData
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Value = 42,
            CreatedAt = DateTime.UtcNow
        };
        var executionCount = 0;
        TestData? capturedData = null;

        // Act - First execution
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            capturedData = testData;
            await Task.Delay(50);
        });

        // Second execution should be skipped
        await _coordinator.Idempotency.RunAsync(key, TimeSpan.FromSeconds(5), async () =>
        {
            executionCount++;
            capturedData = new TestData { Id = Guid.NewGuid(), Name = "Different", Value = 99 };
            await Task.Delay(50);
        });

        // Assert
        Assert.Equal(1, executionCount);
        Assert.NotNull(capturedData);
        Assert.Equal(testData.Id, capturedData.Id);
        Assert.Equal(testData.Name, capturedData.Name);
    }

    [Fact]
    public async Task Idempotency_Should_Prevent_Double_Payment()
    {
        // Arrange - Real world scenario
        var orderId = $"ORDER-{Guid.NewGuid()}";
        var paymentAmount = 100.00m;
        decimal totalCharged = 0;

        // Act - Simulate user clicking payment button 3 times
        var tasks = Enumerable.Range(1, 3).Select(_ =>
            _coordinator.Idempotency.RunAsync(
                key: $"payment:process:{orderId}",
                ttl: TimeSpan.FromMinutes(10),
                async () =>
                {
                    await Task.Delay(100);
                    totalCharged += paymentAmount; // Decimal addition
                })
        );

        await Task.WhenAll(tasks);

        // Assert - Should charge only once
        Assert.Equal(paymentAmount, totalCharged);
    }

    private class TestData
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
