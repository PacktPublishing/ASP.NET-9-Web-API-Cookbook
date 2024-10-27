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
        _apiKey = configuration["AzureMaps:ApiKey"] 
            ?? throw new InvalidOperationException("Azure Maps API key not found");

        _httpClient.BaseAddress = new Uri(
            configuration["AzureMaps:BaseUrl"] 
            ?? "https://atlas.microsoft.com/");
    }

    public async Task<string?> GetCountryCodeAsync(string ipAddress)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<AzureMapsResponse>(
                $"geolocation/ip/json?api-version=1.0&ip={ipAddress}&subscription-key={_apiKey}");
            
            _logger.LogInformation("Retrieved location for IP {IP}: {CountryCode}", 
                ipAddress, 
                response?.CountryRegion.IsoCode);
            
            return response?.CountryRegion.IsoCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get location for IP {IP}", ipAddress);
            return null;
        }
    }
}
