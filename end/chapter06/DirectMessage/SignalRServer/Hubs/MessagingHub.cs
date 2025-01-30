using Microsoft.AspNetCore.SignalR;
using SignalRServer.Services;
using System.Security.Claims;

namespace SignalRServer.Hubs;

public class MessagingHub(IUserConnectionManager userConnectionManager) : Hub<IMessagingClient> 
{
                                                                          public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        Console.WriteLine($"OnConnectedAsync: User {username} connecting with ID {Context.ConnectionId}");
        userConnectionManager.AddConnection(username, Context.ConnectionId);
        await Clients.All.UserConnected(username);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        userConnectionManager.RemoveConnection(username, Context.ConnectionId);
        await Clients.All.UserDisconnected(username);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        await Clients.All.ReceiveMessage(username, message);
    }

    public async Task SendToIndividual(string targetUsername, string message)
    {
        System.Diagnostics.Debugger.Break();
        var senderUsername = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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
}
