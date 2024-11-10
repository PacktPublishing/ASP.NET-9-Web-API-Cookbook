namespace Books.Services;

public class ModificationTracker : IModificationTracker
{
    private DateTime? _lastModified = DateTime.UtcNow;
    
    public DateTime? LastModified => _lastModified;
    
    public void SetModified()
    {
        _lastModified = DateTime.UtcNow;
    }
}
