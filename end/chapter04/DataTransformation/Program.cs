using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Books.Data;
using Books.Services;
using Books.Repositories;
using Books.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi("chapter4", options =>
{
options.AddDocumentTransformer((document, context, cancellationToken) =>
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

builder.Services.AddTransient<XmlFormatterMiddleware>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IBooksRepository, BooksRepository>();
builder.Services.AddScoped<IBooksService, BooksService>();
builder.Services.AddTransient<IDbConnection>(sp => 
    new SqliteConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHealthChecks()
    .AddCheck<DatabasePerformanceHealthCheck>("database_performance", tags: ["database"]);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseMiddleware<XmlFormatterMiddleware>();
app.UseResponseCaching();
app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapOpenApi();
app.MapScalarApiReference();

app.MapHealthChecks("/api/health", new HealthCheckOptions
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

DatabaseSeeder.Initialize(app.Services);

app.Run();
