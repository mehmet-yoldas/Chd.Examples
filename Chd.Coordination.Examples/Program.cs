using Chd.Coordination;
using Chd.Coordination.Abstractions;
using Chd.Coordination.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Chd.Coordination.Examples;

/// <summary>
/// Chd.Coordination kütüphanesinin tüm özelliklerini gösteren örnek uygulama
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddCoordination(opt =>
                {
                    opt.RedisConnectionString = "localhost:6379";
                });

                services.AddTransient<DistributedLockExample>();
                services.AddTransient<IdempotencyExample>();
                services.AddTransient<SagaExample>();
                services.AddTransient<RealWorldScenarios>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .Build();

        Console.WriteLine("=== Chd.Coordination Kullanım Örnekleri ===\n");

        while (true)
        {
            Console.WriteLine("\nHangi örneği çalıştırmak istersiniz?");
            Console.WriteLine("1. Distributed Lock Örnekleri");
            Console.WriteLine("2. Idempotency Örnekleri");
            Console.WriteLine("3. Saga Örnekleri");
            Console.WriteLine("4. Gerçek Dünya Senaryoları");
            Console.WriteLine("0. Çıkış");
            Console.Write("\nSeçiminiz: ");

            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await host.Services.GetRequiredService<DistributedLockExample>().RunAllExamples();
                        break;
                    case "2":
                        await host.Services.GetRequiredService<IdempotencyExample>().RunAllExamples();
                        break;
                    case "3":
                        await host.Services.GetRequiredService<SagaExample>().RunAllExamples();
                        break;
                    case "4":
                        await host.Services.GetRequiredService<RealWorldScenarios>().RunAllExamples();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Geçersiz seçim!");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ Hata: {ex.Message}");
            }
        }
    }
}
