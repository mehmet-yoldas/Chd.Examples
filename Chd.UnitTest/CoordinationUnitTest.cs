using Chd.Coordination;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System;
using System.Threading.Tasks;
using Chd.Coordination.DependencyInjection;
using Chd.Coordination.Abstractions;
using Chd.Coordination.Core;

namespace Chd.UnitTest
{
    public class CoordinationUnitTest
    {
        [Fact]
        public async Task DistributedLock_Should_Acquire_And_Release()
        {
            var services = new ServiceCollection();
            services.AddCoordination(opt =>
            {
                opt.RedisConnectionString = "localhost:6379";
            });

            var provider = services.BuildServiceProvider();
            var coordinator = provider.GetRequiredService<ICoordinator>();

            bool lockAcquired = false;

            await coordinator.Lock.RunAsync(
                key: "unit-test-lock",
                ttl: TimeSpan.FromSeconds(5),
                async ct =>
                {
                    lockAcquired = true;
                    await Task.Delay(100, ct);
                });

            Assert.True(lockAcquired);
        }

        [Fact]
        public async Task Idempotency_Should_Run_Once()
        {
            var services = new ServiceCollection();
            services.AddCoordination(opt =>
            {
                opt.RedisConnectionString = "localhost:6379";
            });

            var provider = services.BuildServiceProvider();
            var coordinator = provider.GetRequiredService<ICoordinator>();

            int executionCount = 0;

            await coordinator.Idempotency.RunAsync(
                key: "unit-test-idempotency",
                ttl: TimeSpan.FromSeconds(5),
                async () =>
                {
                    executionCount++;
                    await Task.Delay(50);
                });

            // Tekrar çağrıldığında çalışmamalı
            await coordinator.Idempotency.RunAsync(
                key: "unit-test-idempotency",
                ttl: TimeSpan.FromSeconds(5),
                async () =>
                {
                    executionCount++;
                    await Task.Delay(50);
                });

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public async Task Saga_Should_Run_AllSteps()
        {
            var services = new ServiceCollection();
            services.AddCoordination(opt =>
            {
                opt.RedisConnectionString = "localhost:6379";
            });

            var provider = services.BuildServiceProvider();
            var coordinator = provider.GetRequiredService<ICoordinator>();

            int step1 = 0, step2 = 0, step3 = 0;

            await coordinator.Saga.RunAsync("unit-test-saga", async saga =>
            {
                await saga.Step("step1", async () => { step1++; await Task.Delay(10); });
                await saga.Step("step2", async () => { step2++; await Task.Delay(10); });
                await saga.Step("step3", async () => { step3++; await Task.Delay(10); });
            });

            Assert.Equal(1, step1);
            Assert.Equal(1, step2);
            Assert.Equal(1, step3);
        }

        [Fact]
        public void CoordinationContext_Should_Create_And_Trace()
        {
            var context = CoordinationContext.Create();
            Assert.False(string.IsNullOrEmpty(context.CorrelationId));

            var lockedContext = new CoordinationContext(context.CorrelationId, lockKey: "order:123");
            Assert.Equal("order:123", lockedContext.LockKey);

            var sagaContext = new CoordinationContext(context.CorrelationId, sagaId: "order:123");
            Assert.Equal("order:123", sagaContext.SagaId);
        }
    }
}