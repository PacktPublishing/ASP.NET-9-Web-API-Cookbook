using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

public class LocaleUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var locale = connection.User?.FindFirst("locale")?.Value ?? "en-US";

        if (string.IsNullOrEmpty(userId))
            return null;

        return $"{userId} - {locale}";
    }
}
