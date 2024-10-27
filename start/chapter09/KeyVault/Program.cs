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

var app = builder.Build();

app.MapGet("/debug/geo/{ip}", async (string ip, IGeolocationService geoService) =>
{
    var countryCode = await geoService.GetCountryCodeAsync(ip);
    return Results.Ok(new { IP = ip, CountryCode = countryCode ?? "Unknown" });
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
