public interface IGeolocationService
{
    Task<string?> GetCountryCodeAsync(string ipAddress);
}
