using Chd.Coordination.Abstractions;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Chd.Coordination.Benchmark;

internal static class BenchmarkHelper
{
    private static readonly Lazy<Task<ConnectionMultiplexer>> _redisConnection = new(
        async () => await ConnectionMultiplexer.ConnectAsync("localhost:6379")
    );

    private static ConnectionMultiplexer? _redis;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets or creates a shared Redis connection (singleton pattern)
    /// </summary>
    public static async Task<ConnectionMultiplexer> GetSharedRedisAsync()
    {
        if (_redis != null && _redis.IsConnected)
            return _redis;

        lock (_lock)
        {
            if (_redis == null || !_redis.IsConnected)
            {
                _redis = _redisConnection.Value.GetAwaiter().GetResult();
            }
        }

        return _redis;
    }

    /// <summary>
    /// Creates a new ServiceProvider and Coordinator using the shared Redis connection
    /// </summary>
    public static async Task<(ServiceProvider ServiceProvider, ICoordinator Coordinator)> SetupAsync()
    {
        var redis = await GetSharedRedisAsync();
        var database = redis.GetDatabase();

        var services = new ServiceCollection();

        // Register shared Redis dependencies
        services.AddSingleton<IDatabase>(database);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        // Register fencing token provider (required by Chd.Coordination)
        services.AddSingleton<IFencingTokenProvider, NoOpFencingTokenProvider>();

        // Add Coordination services
        services.AddCoordination(opt =>
        {
            opt.RedisConnectionString = "localhost:6379";
        });

        var serviceProvider = services.BuildServiceProvider();
        var coordinator = serviceProvider.GetRequiredService<ICoordinator>();

        return (serviceProvider, coordinator);
    }

    /// <summary>
    /// Closes the shared Redis connection (call this at application shutdown)
    /// </summary>
    public static async Task CloseSharedRedisAsync()
    {
        if (_redis != null)
        {
            await _redis.CloseAsync();
            _redis.Dispose();
            _redis = null;
        }
    }
}

// Simple implementation of IFencingTokenProvider for benchmarking
internal class NoOpFencingTokenProvider : IFencingTokenProvider
{
    public Task<long> NextAsync(string resourceId)
    {
        // Return a simple timestamp-based token
        return Task.FromResult(DateTimeOffset.UtcNow.Ticks);
    }
}
