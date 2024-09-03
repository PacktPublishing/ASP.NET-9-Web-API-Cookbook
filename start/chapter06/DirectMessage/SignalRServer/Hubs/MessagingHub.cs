using Microsoft.AspNetCore.SignalR; 
using Microsoft.AspNetCore.Authorization; 
using System.Security.Claims;

[Authorize]
public class MessagingHub : Hub<IMessagingClient>
{
    private readonly IUserConnectionManager _userConnectionManager;

    public MessagingHub(IUserConnectionManager userConnectionManager) { 
	    _userConnectionManager = userConnectionManager; 
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(username))
        {
            _userConnectionManager.AddConnection(username, Context.ConnectionId);
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

        _userConnectionManager.RemoveConnection(username!, Context.ConnectionId);
        await Clients.All.UserDisconnected(username);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendToAll(string message)
    {
        var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        await Clients.All.ReceiveMessage(username, message);
    }

}
