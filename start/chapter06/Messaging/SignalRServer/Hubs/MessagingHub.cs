using Microsoft.AspNetCore.SignalR; 
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims;
using SignalRServer.Services;

[Authorize]
public class MessagingHub : Hub
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly ICustomGroupManager _groupManager;

    public MessagingHub(IUserConnectionManager userConnectionManager, ICustomGroupManager groupManager) { 
	    _userConnectionManager = userConnectionManager; 
	    _groupManager = groupManager; 
    }


    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value 
            ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (!string.IsNullOrEmpty(username))
        {
            _userConnectionManager.AddConnection(username, Context.ConnectionId);
            await Clients.All.SendAsync("UserConnected", username);
            Console.WriteLine($"User connected: {username}");
        }
        else
        {
            Console.WriteLine("Username not found in claims");
        }
        await base.OnConnectedAsync();
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var username = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value 
            ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        _userConnectionManager.RemoveConnection(username!, Context.ConnectionId);
        await Clients.All.SendAsync("UserDisconnected", username);
        await base.OnDisconnectedAsync(exception);
    }

     public async Task SendToAll(string message)
    {
        var username = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value 
            ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }

    public async Task SendToIndividual(string targetUsername, string message)
    {
        var senderUsername = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value 
            ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        Console.WriteLine($"Attempting to send message from {senderUsername} to {targetUsername}");

        var targetConnectionIds = _userConnectionManager.GetConnections(targetUsername).ToList();
        Console.WriteLine($"Found {targetConnectionIds.Count} connection(s) for {targetUsername}");

        if (targetConnectionIds.Any())
        {
            foreach (var connectionId in targetConnectionIds)
            {
                await Clients.Client(connectionId).SendAsync("ReceiveDirectMessage", senderUsername, message);
                Console.WriteLine($"Sent message to {targetUsername} on connection {connectionId}");
            }
        }
        else
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"User {targetUsername} is not connected.");
            Console.WriteLine($"User {targetUsername} is not connected");
        }
    }

    public async Task SendToGroup(string groupName, string message)
    {
        var senderUsername = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value 
            ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", senderUsername, groupName, message);
    }

    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }

    [Authorize(Roles = "Admin")]
    public async Task AddUserToGroup(string username, string groupName)
    {
        await _groupManager.AddUserToGroup(username, groupName);
        var connectionIds = _userConnectionManager.GetConnections(username);
        foreach (var connectionId in connectionIds)
        {
            await Groups.AddToGroupAsync(connectionId, groupName);
        }
        await Clients.All.SendAsync("UserAddedToGroup", username, groupName);
    }

    [Authorize(Roles = "Admin")]
    public async Task RemoveUserFromGroup(string username, string groupName)
    {
        await _groupManager.RemoveUserFromGroup(username, groupName);
        var connectionIds = _userConnectionManager.GetConnections(username);
        foreach (var connectionId in connectionIds)
        {
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
        }
        await Clients.All.SendAsync("UserRemovedFromGroup", username, groupName);
    }



}
