using Microsoft.AspNetCore.SignalR;

public static class MessagingHubExtensions
{
    public static async Task AnnounceUserLogin(this IHubContext<MessagingHub, IMessagingClient> hubContext, string username)
    {
        var message = $"{username} has logged in.";
        await hubContext.Clients.All.ReceiveMessage("System", message);
    }
}
