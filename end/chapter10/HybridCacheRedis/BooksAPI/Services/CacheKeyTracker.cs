namespace Books.Services;

public class CacheKeyTracker : ICacheKeyTracker
{
    private readonly HashSet<string> _keys = new();
    private readonly object _lock = new();

    public void TrackKey(string key)
    {
        lock (_lock)
        {
            _keys.Add(key);
        }
    }

    public IReadOnlyCollection<string> GetKeys()
    {
        lock (_lock)
        {
            return _keys.ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _keys.Clear();
        }
    }
}
