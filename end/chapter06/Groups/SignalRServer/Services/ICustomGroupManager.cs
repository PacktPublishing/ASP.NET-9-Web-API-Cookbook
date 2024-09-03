namespace SignalRServer.Services;

public interface ICustomGroupManager
{
    Task AddUserToGroup(string username, string groupName);
    Task RemoveUserFromGroup(string username, string groupName);
    Task<IEnumerable<string>> GetUserGroups(string username);
}
