using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net;

public class AzureMapsResponse
{
    [JsonPropertyName("countryRegion")]
    public required CountryRegion CountryRegion { get; set; }

    [JsonPropertyName("ipAddress")]
    private string _ipAddress = string.Empty;
    public string IpAddress 
    { 
        get => _ipAddress;
        set
        {
            if (IPAddress.TryParse(value, out var ip))
            {
                _ipAddress = value;
            }
            else
            {
                throw new JsonException($"Invalid IP address format: {value}");
            }
        }
    }
}

public record CountryRegion
{
    [JsonPropertyName("isoCode")]
    public string IsoCode { get; init; } = string.Empty;
}
