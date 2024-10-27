using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Yarp.ReverseProxy.LoadBalancing;
using YarpGateway.LoadBalancing;

var builder = WebApplication.CreateBuilder(args);

// Add logging early
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
});
var logger = loggerFactory.CreateLogger<Program>();

// Common services for both dev and prod
builder.AddServiceDefaults();
builder.Services.AddSingleton<ILoadBalancingPolicy, GeoLoadBalancingPolicy>();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors();
builder.Services.AddHttpClient<IGeolocationService, AzureMapsGeolocationService>();
builder.Services.AddOpenApi();

// Configure Azure Maps settings based on environment
try
{
    if (builder.Environment.IsDevelopment())
    {
        logger.LogInformation("Using development configuration with user secrets");
        builder.Services.Configure<AzureMapsSettings>(options =>
        {
            options.ApiKey = builder.Configuration["AzureMapsApiKey"] 
                ?? throw new InvalidOperationException("Azure Maps API key not found in user secrets");
            options.BaseUrl = builder.Configuration["AzureMaps:BaseUrl"] 
                ?? "https://atlas.microsoft.com/";
        });
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
        
        var mapsApiKey = builder.Configuration["AzureMapsApiKey"];
        if (!string.IsNullOrEmpty(mapsApiKey))
        {
            logger.LogInformation("Successfully retrieved Azure Maps API key from Key Vault");
        }

        builder.Services.Configure<AzureMapsSettings>(options =>
        {
            options.ApiKey = mapsApiKey ?? 
                throw new InvalidOperationException("Azure Maps API key not found in Key Vault");
            options.BaseUrl = builder.Configuration["AzureMaps:BaseUrl"] 
                ?? "https://atlas.microsoft.com/";
        });

        // Add health checks for production
        builder.Services.AddHealthChecks()
            .AddAzureKeyVault(
                keyVaultUri,
                credential,
                options => options.AddSecret("AzureMapsApiKey"));
    }
}
catch (Azure.Identity.CredentialUnavailableException ex)
{
    logger.LogError(ex, "Failed to authenticate with Azure. Please run 'az login' or check your credentials.");
    throw;
}
catch (Exception ex)
{
    logger.LogError(ex, "Unexpected error during startup configuration");
    throw;
}

var app = builder.Build();

app.UseHttpsRedirection();

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
        return Results.Ok(new { IP = ip, CountryCode = countryCode ?? "Unknown" });
    });

    app.MapGet("/debug/route/{ip}", async (string ip, IGeolocationService geoService) =>
    {
        var countryCode = await geoService.GetCountryCodeAsync(ip);
        return Results.Ok(new 
        {
            Ip = ip,
            CountryCode = countryCode,
            Region = countryCode switch 
            {
                "US" or "CA" or "MX" => "NorthAmerica",
                "GB" or "DE" or "FR" or "IT" or "ES" => "Europe",
                "JP" or "CN" or "KR" or "AU" or "NZ" => "AsiaPacific",
                _ => "Default"
            }
        });
    });

     app.MapGet("/debug/routing", async (
        HttpContext context,
        IConfiguration config,
        IGeolocationService geoService) =>
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault() 
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
            
        var countryCode = await geoService.GetCountryCodeAsync(ip);
        var region = countryCode switch
        {
            "US" or "CA" or "MX" => "NorthAmerica",
            "GB" or "DE" or "FR" or "IT" or "ES" => "Europe",
            "JP" or "CN" or "KR" or "AU" or "NZ" or "HK" => "AsiaPacific",
            _ => "Default"
        };

        return Results.Ok(new
        {
            TestIP = ip,
            CountryCode = countryCode,
            Region = region,
            ExpectedPort = region switch
            {
                "NorthAmerica" => 5010,
                "Europe" => 5020,
                "AsiaPacific" => 5030,
                _ => 5010 // default to NA
            }
        });
    });

    app.MapOpenApi();
}

app.UseCors();
app.MapReverseProxy();

app.Run();
