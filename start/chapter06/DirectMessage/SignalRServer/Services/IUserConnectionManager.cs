public interface IUserConnectionManager
{
    void AddConnection(string username, string connectionId);
    void RemoveConnection(string username, string connectionId);
}
