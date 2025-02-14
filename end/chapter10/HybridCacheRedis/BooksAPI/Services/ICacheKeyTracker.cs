namespace Books.Services;

public interface ICacheKeyTracker
{
    void TrackKey(string key);
    IReadOnlyCollection<string> GetKeys();
    void Clear();
}
