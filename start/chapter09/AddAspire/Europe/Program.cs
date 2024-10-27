using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog;
using Serilog.Events;
using Books.Data;
using Books.Services;
using Books.Repositories;

namespace Europe.Api;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting Web API");
            
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IBooksRepository, BooksRepository>();
            builder.Services.AddScoped<IBooksService, BooksService>();

            var app = builder.Build();

            // Configure middleware (from Startup.Configure)
            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    var actionDescriptor = httpContext.GetEndpoint()?.Metadata
                        .GetMetadata<ControllerActionDescriptor>();
                    
                    if (actionDescriptor != null)
                    {
                        diagnosticContext.Set("ActionName", actionDescriptor.ActionName);
                        diagnosticContext.Set("ControllerName", actionDescriptor.ControllerName);
                    }
                };
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms. Controller: {ControllerName}, Action: {ActionName}";
            });

            app.UseResponseCaching();
            app.UseCors();
            app.UseRouting();
            app.UseWebSockets();
            app.MapControllers();

            // Initialize database
            using (var scope = app.Services.CreateScope())
            {
                DatabaseSeeder.Initialize(scope.ServiceProvider);
            }

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
