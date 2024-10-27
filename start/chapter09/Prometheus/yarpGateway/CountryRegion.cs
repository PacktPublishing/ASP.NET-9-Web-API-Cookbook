using System.Text.Json.Serialization;

public record CountryRegion
{
	[JsonPropertyName("isoCode")]
	public string IsoCode { get; init; } = string.Empty;
}
