using Microsoft.AspNetCore.SignalR;

public class MessagingHub : Hub<IMessagingClient>
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
        await Clients.All.UserConnected(username);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        _userConnectionManager.RemoveConnection(username, Context.ConnectionId);
        await Clients.All.UserDisconnected(username);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(string message)
    {
        var username = Context.UserIdentifier ?? "Anonymous";
        await Clients.All.ReceiveMessage(username, message);
    }
}
