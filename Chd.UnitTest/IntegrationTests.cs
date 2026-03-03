using Chd.Coordination;
using Chd.Coordination.Abstractions;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Chd.UnitTest;

/// <summary>
/// Entegrasyon testleri - Birden fazla özelliğin birlikte kullanımı
/// </summary>
public class IntegrationTests : IAsyncLifetime
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
    public async Task Integration_Lock_And_Idempotency()
    {
        // Arrange
        var resourceId = Guid.NewGuid().ToString();
        int updateCount = 0;

        // Act - Multiple concurrent attempts to update
        var tasks = Enumerable.Range(1, 5).Select(_ =>
            Task.Run(async () =>
            {
                await _coordinator.Idempotency.RunAsync(
                    key: $"update:{resourceId}",
                    ttl: TimeSpan.FromSeconds(10),
                    async () =>
                    {
                        await _coordinator.Lock.RunAsync(
                            key: $"resource:lock:{resourceId}",
                            ttl: TimeSpan.FromSeconds(5),
                            async ct =>
                            {
                                Interlocked.Increment(ref updateCount);
                                await Task.Delay(100, ct);
                            });
                    });
            })
        );

        await Task.WhenAll(tasks);

        // Assert - Should execute only once due to idempotency
        Assert.Equal(1, updateCount);
    }

    [Fact]
    public async Task Integration_Saga_With_Locked_Steps()
    {
        // Arrange
        var sagaId = $"saga:locked:{Guid.NewGuid()}";
        var account1 = "ACC-001";
        var account2 = "ACC-002";
        var executedSteps = new List<string>();

        // Act - Saga with locked steps
        await _coordinator.Saga.RunAsync(sagaId, async saga =>
        {
            await saga.Step("withdraw-from-account1", async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: $"account:lock:{account1}",
                    ttl: TimeSpan.FromSeconds(5),
                    async ct =>
                    {
                        executedSteps.Add("withdraw");
                        await Task.Delay(50, ct);
                    });
            });

            await saga.Step("deposit-to-account2", async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: $"account:lock:{account2}",
                    ttl: TimeSpan.FromSeconds(5),
                    async ct =>
                    {
                        executedSteps.Add("deposit");
                        await Task.Delay(50, ct);
                    });
            });
        });

        // Assert
        Assert.Equal(2, executedSteps.Count);
        Assert.Equal("withdraw", executedSteps[0]);
        Assert.Equal("deposit", executedSteps[1]);
    }

    [Fact]
    public async Task Integration_Idempotent_Saga()
    {
        // Arrange
        var orderId = Guid.NewGuid().ToString();
        int sagaExecutionCount = 0;

        // Act - Same saga called multiple times (should be idempotent)
        for (int i = 0; i < 3; i++)
        {
            await _coordinator.Idempotency.RunAsync(
                key: $"order:process:{orderId}",
                ttl: TimeSpan.FromMinutes(5),
                async () =>
                {
                    await _coordinator.Saga.RunAsync($"order:saga:{orderId}", async saga =>
                    {
                        await saga.Step("step1", async () =>
                        {
                            Interlocked.Increment(ref sagaExecutionCount);
                            await Task.Delay(10);
                        });
                    });
                });
        }

        // Assert - Saga should execute only once
        Assert.Equal(1, sagaExecutionCount);
    }

    [Fact]
    public async Task Integration_BankTransfer_Scenario()
    {
        // Arrange - Real-world bank transfer scenario
        var transferId = Guid.NewGuid().ToString();
        var fromAccount = "ACC-123";
        var toAccount = "ACC-456";
        decimal amount = 100.00m;
        
        var transferResult = new TransferResult();

        // Act
        await _coordinator.Idempotency.RunAsync(
            key: $"transfer:{transferId}",
            ttl: TimeSpan.FromMinutes(10),
            async () =>
            {
                await _coordinator.Saga.RunAsync($"transfer:saga:{transferId}", async saga =>
                {
                    // Step 1: Withdraw
                    await saga.Step("withdraw", async () =>
                    {
                        await _coordinator.Lock.RunAsync(
                            key: $"account:{fromAccount}",
                            ttl: TimeSpan.FromSeconds(10),
                            async ct =>
                            {
                                transferResult.Withdrawn = amount;
                                await Task.Delay(50, ct);
                            });
                    });

                    // Step 2: Deposit
                    await saga.Step("deposit", async () =>
                    {
                        await _coordinator.Lock.RunAsync(
                            key: $"account:{toAccount}",
                            ttl: TimeSpan.FromSeconds(10),
                            async ct =>
                            {
                                transferResult.Deposited = amount;
                                await Task.Delay(50, ct);
                            });
                    });

                    // Step 3: Record transaction
                    await saga.Step("record", async () =>
                    {
                        transferResult.Recorded = true;
                        await Task.Delay(20);
                    });
                });
            });

        // Assert
        Assert.Equal(amount, transferResult.Withdrawn);
        Assert.Equal(amount, transferResult.Deposited);
        Assert.True(transferResult.Recorded);
    }

    [Fact]
    public async Task Integration_BankTransfer_Failure_Scenario()
    {
        // Arrange
        var transferId = Guid.NewGuid().ToString();
        var fromAccount = "ACC-789";
        var toAccount = "ACC-012";
        decimal amount = 200.00m;

        var transferResult = new TransferResult();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _coordinator.Saga.RunAsync($"transfer:saga:{transferId}", async saga =>
            {
                // Step 1: Withdraw (succeeds)
                await saga.Step("withdraw", async () =>
                {
                    await _coordinator.Lock.RunAsync(
                        key: $"account:{fromAccount}",
                        ttl: TimeSpan.FromSeconds(10),
                        async ct =>
                        {
                            transferResult.Withdrawn = amount;
                            await Task.Delay(50, ct);
                        });
                });

                // Step 2: Deposit (fails)
                await saga.Step("deposit", async () =>
                {
                    await _coordinator.Lock.RunAsync(
                        key: $"account:{toAccount}",
                        ttl: TimeSpan.FromSeconds(10),
                        async ct =>
                        {
                            await Task.Delay(50, ct);
                            throw new InvalidOperationException("Account frozen");
                        });
                });
            });
        });

        // Assert - Withdrawal happened but deposit failed
        Assert.Equal(amount, transferResult.Withdrawn);
        Assert.Equal(0, transferResult.Deposited);
        Assert.False(transferResult.Recorded);
    }

    [Fact]
    public async Task Integration_JobProcessing_With_All_Features()
    {
        // Arrange
        var jobId = Guid.NewGuid().ToString();
        var processedSteps = new List<string>();

        // Act - Job processing with lock, idempotency, and saga
        await _coordinator.Idempotency.RunAsync(
            key: $"job:execute:{jobId}",
            ttl: TimeSpan.FromHours(1),
            async () =>
            {
                await _coordinator.Lock.RunAsync(
                    key: $"job:lock:{jobId}",
                    ttl: TimeSpan.FromMinutes(5),
                    async ct =>
                    {
                        await _coordinator.Saga.RunAsync($"job:saga:{jobId}", async saga =>
                        {
                            await saga.Step("validate", async () =>
                            {
                                processedSteps.Add("validate");
                                await Task.Delay(10);
                            });

                            await saga.Step("process", async () =>
                            {
                                processedSteps.Add("process");
                                await Task.Delay(10);
                            });

                            await saga.Step("complete", async () =>
                            {
                                processedSteps.Add("complete");
                                await Task.Delay(10);
                            });
                        });
                    });
            });

        // Assert
        Assert.Equal(3, processedSteps.Count);
        Assert.Equal(new[] { "validate", "process", "complete" }, processedSteps);
    }

    private class TransferResult
    {
        public decimal Withdrawn { get; set; }
        public decimal Deposited { get; set; }
        public bool Recorded { get; set; }
    }
}
