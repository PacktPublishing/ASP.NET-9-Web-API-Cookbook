using Serilog;
using Serilog.Events;

namespace books;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
		    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
		    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
		    .MinimumLevel.Override("Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware", LogEventLevel.Information)
		    .Enrich.FromLogContext()
		    .WriteTo.Console()
		    .WriteTo.Seq("http://localhost:5341")
		    .CreateLogger();
        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureWebHostDefaults(webBuilder =>
        {
                webBuilder.UseStartup<Startup>();
        });
}
