using Microsoft.AspNetCore.SignalR;
using SignalRServer.Services;

namespace SignalRServer.Hubs;

public class MessagingHub(IUserConnectionManager userConnectionManager) : Hub 
{
                                                                          public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        userConnectionManager.AddConnection(username, Context.ConnectionId);
        await Clients.All.SendAsync("UserConnected", username);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        userConnectionManager.RemoveConnection(username, Context.ConnectionId);
        await Clients.All.SendAsync("UserDisconnected", username);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }
}
