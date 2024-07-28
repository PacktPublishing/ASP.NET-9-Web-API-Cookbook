using Serilog;
using Serilog.Events;

namespace books;

public class Program
{
    public static void Main(string[] args)
    {

    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
                webBuilder.UseStartup<Startup>();
        });
}
