namespace SignalRServer.Services;

public class CustomGroupManager : ICustomGroupManager
{
    private readonly Dictionary<string, HashSet<string>> _groupUserMap = new Dictionary<string, HashSet<string>>();

    public Task AddUserToGroup(string username, string groupName)
    {
        lock (_groupUserMap)
        {
            if (!_groupUserMap.ContainsKey(groupName))
            {
                _groupUserMap[groupName] = new HashSet<string>();
            }
            _groupUserMap[groupName].Add(username);
        }
        return Task.CompletedTask;
    }

    public Task RemoveUserFromGroup(string username, string groupName)
    {
        lock (_groupUserMap)
        {
            if (_groupUserMap.ContainsKey(groupName))
            {
                _groupUserMap[groupName].Remove(username);
                if (_groupUserMap[groupName].Count == 0)
                {
                    _groupUserMap.Remove(groupName);
                }
            }
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetUserGroups(string username)
    {
        lock (_groupUserMap)
        {
            return Task.FromResult(_groupUserMap
                .Where(g => g.Value.Contains(username))
                .Select(g => g.Key));
        }
    }
}
