using Microsoft.AspNetCore.SignalR; 
using Microsoft.AspNetCore.Authorization; 
using SignalRServer.Services;
using System.Security.Claims;

namespace SignalRServer.Hubs;

[Authorize]
public class MessagingHub(IUserConnectionManager userConnectionManager) : Hub<IMessagingClient>
{
    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? "Unknown User";

        if (!string.IsNullOrEmpty(username))
        {
            userConnectionManager.AddConnection(username, Context.ConnectionId);
            await Clients.All.UserConnected(username);
            Console.WriteLine($"User connected: {username}");
        }
        else
        {
            Console.WriteLine("Username not found in claims");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        userConnectionManager.RemoveConnection(username!, Context.ConnectionId);
        await Clients.All.UserDisconnected(username);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendToAll(string message)
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? "Unknown User";

        await Clients.All.ReceiveMessage(username, message);
    }

    public async Task SendToIndividual(string targetUsername, string message)
    {
        var senderUsername = Context.User?.FindFirst(ClaimTypes.Name)?.Value      ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? "Unknown User";

        Console.WriteLine($"Attempting to send message from {senderUsername} to {targetUsername}");

        var targetConnectionIds = userConnectionManager.GetConnections(targetUsername).ToList();
        Console.WriteLine($"Found {targetConnectionIds.Count} connection(s) for {targetUsername}");

        if (targetConnectionIds.Any())
        {
            foreach (var connectionId in targetConnectionIds)
            {
                await Clients.Client(connectionId).ReceiveDirectMessage(senderUsername, message);
                Console.WriteLine($"Sent message to {targetUsername} on connection {connectionId}");
            }
        }
        else
        {
            await Clients.Caller.ReceiveMessage("System", $"User {targetUsername} is not connected.");
            Console.WriteLine($"User {targetUsername} is not connected");
        }
    }

    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }
}
