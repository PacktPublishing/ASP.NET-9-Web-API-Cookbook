using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using books.Data;
using books.Services;
using books.Repositories;
using books.Middleware;


namespace books;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            });
        });
        
    services.AddOpenApi("chapter4", options =>
    {
        options.UseTransformer((document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "Chapter 4 API",
                Version = "v1",
                Description = "API for demonstrating health checks"
            };

            document.Paths["/api/health"] = new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Summary = "Health Check",
                        Description = "Performs a health check on the application",
                        Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Health" } },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "OK" },
                            ["503"] = new OpenApiResponse { Description = "Service Unavailable" }
                        }
                    }
                }
            };

            return Task.CompletedTask;
        });
    });

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBooksRepository, BooksRepository>();
        services.AddScoped<IBooksService, BooksService>();

        services.AddTransient<IDbConnection>(sp => 
            new SqliteConnection(Configuration.GetConnectionString("DefaultConnection")));

        services.AddHealthChecks()
            .AddCheck<DatabasePerformanceHealthCheck>("database_performance", tags: new[] {"database"});

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
        app.UseForwardedHeaders();

        app.UseResponseCaching();


        app.UseCors();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapOpenApi();
            endpoints.MapScalarApiReference();

            endpoints.MapHealthChecks("/api/health", new HealthCheckOptions
            {
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                var response = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration
                    }),
                    totalDuration = report.TotalDuration
                };
                await context.Response.WriteAsJsonAsync(response);
                }
            });

        });

        DatabaseSeeder.Initialize(serviceProvider);
    }
}
