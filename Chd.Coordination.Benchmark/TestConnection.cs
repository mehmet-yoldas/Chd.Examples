using Chd.Coordination.Abstractions;

namespace Chd.Coordination.Benchmark;

public static class TestConnection
{
    public static async Task RunAsync()
    {
        Console.WriteLine("🧪 Testing Redis connection and Lock API...\n");

        try
        {
            Console.WriteLine("🔌 Setting up shared Redis connection and services...");
            var (serviceProvider, coordinator) = await BenchmarkHelper.SetupAsync();
            Console.WriteLine("✅ Setup complete");

            // Test Lock API
            var resourceId = $"test-resource:{Guid.NewGuid()}";
            Console.WriteLine($"🔒 Attempting to acquire lock for: {resourceId}");

            var lockHandle = await coordinator.Lock.AcquireAsync(
                resourceId,
                ttl: TimeSpan.FromSeconds(5),
                waitTime: TimeSpan.FromSeconds(10),
                cancellationToken: CancellationToken.None
            );

            if (lockHandle == null)
            {
                Console.WriteLine("❌ Lock acquisition returned null");
                return;
            }

            Console.WriteLine("✅ Lock acquired successfully!");
            Console.WriteLine($"   Lock Handle Type: {lockHandle.GetType().FullName}");

            await Task.Delay(100);

            await lockHandle.ReleaseAsync();
            Console.WriteLine("✅ Lock released successfully!");

            // Test Idempotency API
            Console.WriteLine("\n🔄 Testing Idempotency API...");
            var operationId = $"test-op:{Guid.NewGuid()}";

            await coordinator.Idempotency.RunAsync(
                operationId,
                ttl: TimeSpan.FromMinutes(5),
                action: () =>
                {
                    Console.WriteLine("   ✅ Idempotency action executed");
                    return Task.CompletedTask;
                }
            );

            Console.WriteLine("✅ Idempotency test passed!");

            Console.WriteLine("\n✅ All connection tests passed!");
            Console.WriteLine("   Redis connection is shared and will remain open.");
            Console.WriteLine("   The benchmark should work now.");

            serviceProvider.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Test failed with exception:");
            Console.WriteLine($"   Type: {ex.GetType().Name}");
            Console.WriteLine($"   Message: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
        }
    }
}
