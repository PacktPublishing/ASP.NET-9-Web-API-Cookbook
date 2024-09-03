using Microsoft.AspNetCore.SignalR;

public class MessagingHub : Hub
{
    private readonly IUserConnectionManager _userConnectionManager;

    public MessagingHub(IUserConnectionManager userConnectionManager)
    {
        _userConnectionManager = userConnectionManager;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        _userConnectionManager.AddConnection(username, Context.ConnectionId);
        await Clients.All.SendAsync("UserConnected", username);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        _userConnectionManager.RemoveConnection(username, Context.ConnectionId);
        await Clients.All.SendAsync("UserDisconnected", username);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        await Clients.All.SendAsync("ReceiveMessage", username, message);
    }
}
