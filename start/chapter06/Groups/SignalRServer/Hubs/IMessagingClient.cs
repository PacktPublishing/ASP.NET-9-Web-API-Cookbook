namespace SignalRServer.Hubs;

public interface IMessagingClient
{
    Task ReceiveMessage(string user, string message);
    Task ReceiveDirectMessage(string user, string message);
    Task UserConnected(string username);
    Task UserDisconnected(string username);
    Task UserLoggedIn(string username);
}
