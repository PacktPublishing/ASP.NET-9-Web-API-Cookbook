namespace SignalRServer.Services;

public class UserConnectionManager : IUserConnectionManager
{
    private readonly Dictionary<string, HashSet<string>> _connections = new Dictionary<string, HashSet<string>>();

    public void AddConnection(string username, string connectionId)
    {
        lock (_connections)
        {
            if (!_connections.TryGetValue(username, out HashSet<string> connections))
            {
                connections = new HashSet<string>();
                _connections.Add(username, connections);
            }

            connections.Add(connectionId);
            Console.WriteLine($"Added connection for user {username}: {connectionId}. Total connections: {_connections.Count}");
        }
    }

    public void RemoveConnection(string username, string connectionId)
    {
        lock (_connections)
        {
            if (_connections.TryGetValue(username, out HashSet<string> connections))
            {
                connections.Remove(connectionId);

                if (connections.Count == 0)
                {
                    _connections.Remove(username);
                }
            }
        }
    }

    public string GetConnectionId(string username)
    {
        lock (_connections)
        {
            return _connections.TryGetValue(username, out HashSet<string> connections)
                ? connections.FirstOrDefault()
                : null;
        }
    }
}
