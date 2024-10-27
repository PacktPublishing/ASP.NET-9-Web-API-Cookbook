using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;  // For HealthCheckOptions
using Microsoft.AspNetCore.Http;  // For context access in health check
using System.Text.Json;  // For JSON serialization

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
var logger = loggerFactory.CreateLogger<Program>();

builder.Services.AddOpenApi();

builder.Services.AddHttpClient<IGeolocationService, AzureMapsGeolocationService>();

builder.Services.AddHealthChecks();

try
{
    if (builder.Environment.IsDevelopment())
    {
        logger.LogInformation("Using development configuration with user secrets");
    }
    else
    {
        // Production Key Vault configuration
        string keyVaultName = builder.Configuration["KeyVault:VaultName"]
            ?? throw new InvalidOperationException("KeyVault:VaultName not found in configuration");
        
        logger.LogInformation("Attempting to connect to Key Vault: {KeyVaultName}", keyVaultName);
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        var credential = new DefaultAzureCredential();
        
        builder.Configuration.AddAzureKeyVault(keyVaultUri, credential);

        builder.Services.AddHealthChecks()
            .AddAzureKeyVault(
                keyVaultUri,
                credential,
                options => options.AddSecret("AzureMapsApiKey"));
    }
}
catch (Exception ex)
{
    logger.LogError(ex, "Configuration error");
    throw;
}

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/config", (IConfiguration configuration) =>
    {
        return Results.Ok(new 
        { 
            KeyVaultConnected = !string.IsNullOrEmpty(configuration["AzureMapsApiKey"]),
            KeyVaultName = configuration["KeyVault:VaultName"]
        });
    });
    
    app.MapGet("/debug/geo/{ip}", async (string ip, IGeolocationService geoService) =>
    {
        var countryCode = await geoService.GetCountryCodeAsync(ip);
        return Results.Ok(new { IP = ip, CountryCode = countryCode ?? "Unknown" 
        });
    });

        app.MapOpenApi();
    }

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            var result = new
            {
                Status = report.Status.ToString(),
                Components = report.Entries.Select(e => new
                {
                    Component = e.Key,
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description
                })
            };
            
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(result);
        }
    });

app.UseHttpsRedirection();

app.Run();
