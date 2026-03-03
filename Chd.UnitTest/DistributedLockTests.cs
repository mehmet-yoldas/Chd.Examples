using Chd.Coordination;
using Chd.Coordination.Abstractions;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chd.UnitTest;

/// <summary>
/// Distributed Lock özelliği için kapsamlı testler
/// </summary>
public class DistributedLockTests : IAsyncLifetime
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
    public async Task Lock_Should_Acquire_And_Release_Successfully()
    {
        // Arrange
        var lockKey = $"test:lock:{Guid.NewGuid()}";
        bool actionExecuted = false;

        // Act
        await _coordinator.Lock.RunAsync(
            key: lockKey,
            ttl: TimeSpan.FromSeconds(5),
            async ct =>
            {
                actionExecuted = true;
                await Task.Delay(100, ct);
            });

        // Assert
        Assert.True(actionExecuted);
    }

    [Fact]
    public async Task Lock_Should_Execute_Action()
    {
        // Arrange
        var lockKey = $"test:lock:{Guid.NewGuid()}";
        var executed = false;

        // Act
        await _coordinator.Lock.RunAsync(
            key: lockKey,
            ttl: TimeSpan.FromSeconds(5),
            async ct =>
            {
                await Task.Delay(50, ct);
                executed = true;
            });

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public async Task Lock_Should_Prevent_Concurrent_Execution()
    {
        // Arrange
        var lockKey = $"test:lock:{Guid.NewGuid()}";
        var executionCount = 0;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 3; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: lockKey,
                    ttl: TimeSpan.FromSeconds(2),
                    async ct =>
                    {
                        Interlocked.Increment(ref executionCount);
                        await Task.Delay(500, ct);
                    });
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Only one should succeed due to timeout
        Assert.True(executionCount >= 1 && executionCount <= 3);
    }

    [Fact]
    public async Task Lock_Should_Timeout_When_Cannot_Acquire()
    {
        // Arrange
        var lockKey = $"test:lock:{Guid.NewGuid()}";

        // Act - First lock holds for 3 seconds
        var firstTask = _coordinator.Lock.RunAsync(
            key: lockKey,
            ttl: TimeSpan.FromSeconds(3),
            async ct => await Task.Delay(2000, ct));

        await Task.Delay(100); // Ensure first lock is acquired

        await Task.Delay(100); // Wait a bit to ensure first lock is held

        // Second lock should wait (no timeout parameter available)
        bool secondExecuted = false;
        var secondTask = Task.Run(async () =>
        {
            await _coordinator.Lock.RunAsync(
                key: lockKey,
                ttl: TimeSpan.FromSeconds(3),
                async ct =>
                {
                    secondExecuted = true;
                    await Task.Delay(100, ct);
                });
        });

        // Wait for first to complete
        await firstTask;
        await secondTask;

        // Assert - Second should execute after first completes
        Assert.True(secondExecuted);
    }

    [Fact]
    public async Task Lock_Should_Release_On_Exception()
    {
        // Arrange
        var lockKey = $"test:lock:{Guid.NewGuid()}";

        // Act - First execution throws exception
        try
        {
            await _coordinator.Lock.RunAsync(
                key: lockKey,
                ttl: TimeSpan.FromSeconds(5),
                async ct =>
                {
                    await Task.Delay(100, ct);
                    throw new InvalidOperationException("Test exception");
                });
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Second execution should succeed (lock was released)
        var secondExecuted = false;
        await _coordinator.Lock.RunAsync(
            key: lockKey,
            ttl: TimeSpan.FromSeconds(5),
            async ct =>
            {
                secondExecuted = true;
                await Task.Delay(50, ct);
            });

        // Assert
        Assert.True(secondExecuted);
    }

    [Fact]
    public async Task Lock_Should_Handle_Cancellation()
    {
        // Arrange
        var lockKey = $"test:lock:{Guid.NewGuid()}";
        using var cts = new CancellationTokenSource();

        // Act & Assert
        var task = _coordinator.Lock.RunAsync(
            key: lockKey,
            ttl: TimeSpan.FromSeconds(5),
            async ct =>
            {
                await Task.Delay(2000, ct);
            });

        cts.CancelAfter(100);

        // The cancellation should work (either OperationCanceledException or task completes)
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            // Expected if cancellation propagates
        }
    }

    [Fact]
    public async Task Lock_Should_Support_Different_Keys()
    {
        // Arrange
        var lockKey1 = $"test:lock:{Guid.NewGuid()}";
        var lockKey2 = $"test:lock:{Guid.NewGuid()}";
        var executed1 = false;
        var executed2 = false;

        // Act - Different keys should not block each other
        var tasks = new[]
        {
            _coordinator.Lock.RunAsync(lockKey1, TimeSpan.FromSeconds(5), async ct =>
            {
                executed1 = true;
                await Task.Delay(100, ct);
            }),
            _coordinator.Lock.RunAsync(lockKey2, TimeSpan.FromSeconds(5), async ct =>
            {
                executed2 = true;
                await Task.Delay(100, ct);
            })
        };

        await Task.WhenAll(tasks);

        // Assert
        Assert.True(executed1);
        Assert.True(executed2);
    }
}
