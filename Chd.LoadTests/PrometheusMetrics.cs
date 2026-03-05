using Prometheus;

namespace Chd.LoadTests;

public static class PrometheusMetrics
{
    // Counter'lar - Sürekli artan değerler
    public static readonly Counter LockOperations = Metrics.CreateCounter(
        "chd_lock_operations_total",
        "Total number of lock operations",
        new CounterConfiguration { LabelNames = new[] { "status" } }
    );

    public static readonly Counter IdempotencyOperations = Metrics.CreateCounter(
        "chd_idempotency_operations_total",
        "Total number of idempotency operations",
        new CounterConfiguration { LabelNames = new[] { "status" } }
    );

    public static readonly Counter SagaOperations = Metrics.CreateCounter(
        "chd_saga_operations_total",
        "Total number of saga operations",
        new CounterConfiguration { LabelNames = new[] { "status" } }
    );

    // Histogram'lar - Latency ölçümü için
    public static readonly Histogram LockLatency = Metrics.CreateHistogram(
        "chd_lock_duration_seconds",
        "Lock operation duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) // 1ms'den başla, 10 bucket
        }
    );

    public static readonly Histogram IdempotencyLatency = Metrics.CreateHistogram(
        "chd_idempotency_duration_seconds",
        "Idempotency operation duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        }
    );

    public static readonly Histogram SagaLatency = Metrics.CreateHistogram(
        "chd_saga_duration_seconds",
        "Saga operation duration in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        }
    );

    // Gauge'lar - Anlık değerler
    public static readonly Gauge ActiveTests = Metrics.CreateGauge(
        "chd_active_tests",
        "Number of currently running test scenarios"
    );

    public static readonly Gauge RequestsPerSecond = Metrics.CreateGauge(
        "chd_requests_per_second",
        "Current requests per second",
        new GaugeConfiguration { LabelNames = new[] { "scenario" } }
    );

    // Summary'ler - Percentile hesaplamaları için
    public static readonly Summary OperationLatency = Metrics.CreateSummary(
        "chd_operation_latency_seconds",
        "Operation latency summary",
        new SummaryConfiguration
        {
            LabelNames = new[] { "operation_type" },
            Objectives = new[]
            {
                new QuantileEpsilonPair(0.5, 0.05),   // P50
                new QuantileEpsilonPair(0.9, 0.01),   // P90
                new QuantileEpsilonPair(0.95, 0.01),  // P95
                new QuantileEpsilonPair(0.99, 0.001)  // P99
            }
        }
    );
}
