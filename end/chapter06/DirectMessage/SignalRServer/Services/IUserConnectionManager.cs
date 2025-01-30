namespace SignalRServer.Services;

public interface IUserConnectionManager
{
    void AddConnection(string username, string connectionId);
    void RemoveConnection(string username, string connectionId);
    string GetConnectionId(string username);
    IEnumerable<string> GetConnections(string username);
}
