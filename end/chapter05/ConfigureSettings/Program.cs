using Serilog;
using Serilog.Events;

namespace books;

public class Program
{
    public static void Main(string[] args)
    {
        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "logs"));

        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Seq(
                        serverUrl: GetSeqUrl(context.Configuration),
                        apiKey: GetSeqApiKey(context.Configuration)))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static string GetSeqUrl(IConfiguration configuration)
        {
            var seqUrl = configuration["Seq:Url"];
            if (string.IsNullOrEmpty(seqUrl))
            {
                seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
            }

            return string.IsNullOrEmpty(seqUrl) ? "http://localhost:5341" : seqUrl;
        }

        private static string GetSeqApiKey(IConfiguration configuration)
        {
            return configuration["Seq:ApiKey"] ?? 
                   Environment.GetEnvironmentVariable("SEQ_API_KEY") ?? 
                   throw new InvalidOperationException("Seq API key not found.");
        }
}
