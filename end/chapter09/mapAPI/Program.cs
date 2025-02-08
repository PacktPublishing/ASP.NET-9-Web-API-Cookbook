var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IGeolocationService, AzureMapsGeolocationService>();

var app = builder.Build();

app.MapGet("/debug/geo/{ip}", async (string ip, IGeolocationService geoService) =>
{
    var countryCode = await geoService.GetCountryCodeAsync(ip);
    return Results.Ok(new { IP = ip, CountryCode = countryCode ?? "Unknown" });
});


app.UseHttpsRedirection();

app.Run();
