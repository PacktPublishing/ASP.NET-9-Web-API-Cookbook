using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Yarp.ReverseProxy.LoadBalancing;
using Yarp.ReverseProxy.Model;

namespace YarpGateway.LoadBalancing;

public class GeoLoadBalancingPolicy : ILoadBalancingPolicy
{
    private readonly IGeolocationService _geoService;
    private readonly ILogger<GeoLoadBalancingPolicy> _logger;

    public GeoLoadBalancingPolicy(
        IGeolocationService geoService,
        ILogger<GeoLoadBalancingPolicy> logger)
    {
        _geoService = geoService;
        _logger = logger;
    }

    public string Name => "GeoLoadBalancing";

    public DestinationState? PickDestination(
        HttpContext context, 
        ClusterState cluster, 
        IReadOnlyList<DestinationState> availableDestinations)
    {
        var clientIp = GetOriginalClientIp(context);
        
        _logger.LogInformation(
            "Processing request from IP {IP} (Remote: {Remote}, Forwarded: {Forwarded})", 
            clientIp,
            context.Connection.RemoteIpAddress,
            context.Request.Headers["X-Forwarded-For"].ToString());

        var countryCode = Task.Run(async () => 
            await _geoService.GetCountryCodeAsync(clientIp)).GetAwaiter().GetResult();

        _logger.LogInformation("IP {IP} resolved to country code {CountryCode}", 
            clientIp, countryCode);

        var region = DetermineRegion(countryCode);
        
        // Find destination for this region using Config.Metadata
        var destination = availableDestinations.FirstOrDefault(d => 
            d.Model.Config.Metadata?.GetValueOrDefault("Region") == region);

        if (destination == null)
        {
            _logger.LogWarning("No destination found for region {Region}, using first available", region);
            return availableDestinations.FirstOrDefault();
        }

        _logger.LogInformation("Routing request to {Destination} for region {Region}", 
            destination.Model.Config.Address, region);

        return destination;
    }

    private string GetOriginalClientIp(HttpContext context)
    {
        // Check forwarded headers in order of trust
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Get the leftmost IP (original client)
            var ips = forwardedFor.Split(',');
            // Validate IP before using it
            if (IPAddress.TryParse(ips[0].Trim(), out var ip))
            {
                // Don't trust private IPs in X-Forwarded-For
                if (!IsPrivateIpAddress(ip))
                {
                    return ip.ToString();
                }
            }
        }

        // Fallback to direct connection
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private bool IsPrivateIpAddress(IPAddress ip)
    {
        // Check if IP is in private ranges
        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        byte[] bytes = ip.GetAddressBytes();
        // Check if it's IPv4 by checking address family
    return ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && (
        bytes[0] == 10 || // 10.0.0.0/8
        (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) || // 172.16.0.0/12
        (bytes[0] == 192 && bytes[1] == 168) // 192.168.0.0/16
    );

    }

    private string DetermineRegion(string? countryCode) => countryCode switch
    {
        "US" or "CA" or "MX" => "NorthAmerica",
        "GB" or "DE" or "FR" or "IT" or "ES" => "Europe",
        "JP" or "CN" or "KR" or "AU" or "NZ" => "AsiaPacific",
        _ => "Default"
    };
}
