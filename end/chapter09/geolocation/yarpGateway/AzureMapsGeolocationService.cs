using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;

public class AzureMapsGeolocationService : IGeolocationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AzureMapsGeolocationService> _logger;
    private readonly string _apiKey;

    public AzureMapsGeolocationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AzureMapsGeolocationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        _apiKey = configuration["AzureMapsApiKey"] 
            ?? throw new InvalidOperationException("Azure Maps API key not found");

        _httpClient.BaseAddress = new Uri(
            configuration["AzureMaps:BaseUrl"] 
            ?? "https://atlas.microsoft.com/");
    }

    public async Task<string?> GetCountryCodeAsync(string ipAddress)
    {
        try
        {
            _logger.LogInformation("Getting location for IP: {IpAddress}", ipAddress);

            if (!IPAddress.TryParse(ipAddress, out _))
            {
                _logger.LogWarning("Invalid IP address format: {IpAddress}", ipAddress);
                return null;
            }

            var response = await _httpClient.GetFromJsonAsync<AzureMapsResponse>(
                $"geolocation/ip/json?api-version=1.0&ip={ipAddress}&subscription-key={_apiKey}");

            if (response?.CountryRegion.IsoCode == null)
            {
                _logger.LogWarning("No country code returned for IP: {IpAddress}", ipAddress);
                return null;
            }

            _logger.LogInformation("Got country code {CountryCode} for IP: {IpAddress}", 
                response.CountryRegion.IsoCode, ipAddress);

            return response.CountryRegion.IsoCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting location for IP {IpAddress}", ipAddress);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for IP {IpAddress}", ipAddress);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting location for IP {IpAddress}", ipAddress);
            return null;
        }
    }
}
