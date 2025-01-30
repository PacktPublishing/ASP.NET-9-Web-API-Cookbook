namespace SignalRServer.Hubs;

public interface IMessagingClient
{
    Task ReceiveMessage(string user, string message);
    Task ReceiveDirectMessage(string user, string message);
    Task ReceiveGroupMessage(string user, string groupName, string message);
    Task UserConnected(string username);
    Task UserDisconnected(string username);
    Task UserLoggedIn(string username);
    Task UserAddedToGroup(string username, string groupName);
    Task UserRemovedFromGroup(string username, string groupName);
}
