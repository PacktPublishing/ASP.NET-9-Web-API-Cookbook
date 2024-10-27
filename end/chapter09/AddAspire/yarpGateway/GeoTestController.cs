using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class GeoTestController : ControllerBase
{
    private readonly IGeolocationService _geoService;
    private readonly ILogger<GeoTestController> _logger;

    public GeoTestController(
        IGeolocationService geoService,
        ILogger<GeoTestController> logger)
    {
        _geoService = geoService;
        _logger = logger;
    }

    [HttpGet("test/{ip}")]
    public async Task<IActionResult> TestGeolocation(string ip)
    {
        var countryCode = await _geoService.GetCountryCodeAsync(ip);
        return Ok(new { IP = ip, CountryCode = countryCode ?? "Unknown" });
    }
}
