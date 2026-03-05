using Prometheus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chd.LoadTests;

public class PrometheusServer
{
    private IHost? _host;
    private readonly int _port;

    public PrometheusServer(int port = 9091)
    {
        _port = port;
    }

    public void Start()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                // Suppress all Kestrel and ASP.NET Core logs
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.None);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseUrls($"http://0.0.0.0:{_port}")  // Listen on all interfaces (Docker can access)
                    .Configure(app =>
                    {
                        // Prometheus metrics endpoint
                        app.UseRouting();
                        app.UseHttpMetrics(); // Track HTTP metrics
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapMetrics(); // /metrics endpoint
                        });
                    })
                    .SuppressStatusMessages(true); // Suppress startup messages
            })
            .Build();

        _host.Start();
        Console.WriteLine($"[Prometheus] Metrics server started at http://0.0.0.0:{_port}/metrics");
    }

    public void Stop()
    {
        _host?.StopAsync().Wait();
        _host?.Dispose();
        Console.WriteLine("[Prometheus] Metrics server stopped");
    }
}
