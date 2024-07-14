using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using books.Data;
using books.Services;
using books.Repositories;


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

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddOpenApi("chapter4");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IBooksRepository, BooksRepository>();
        services.AddScoped<IBooksService, BooksService>();

        services.AddHealthChecks()
            .AddCheck("Database", () =>
            {
                using var connection = new SqliteConnection(Configuration.GetConnectionString("DefaultConnection"));
                try
                {
                    connection.Open();
                    return HealthCheckResult.Healthy();
                }
                catch (SqliteException)
                {
                    return HealthCheckResult.Unhealthy();
                }
            }, tags: new[] { "database" });
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
