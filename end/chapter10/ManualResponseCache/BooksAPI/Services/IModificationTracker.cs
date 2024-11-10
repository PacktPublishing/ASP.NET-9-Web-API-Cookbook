namespace Books.Services;

public interface IModificationTracker
{
    DateTime? LastModified { get; }
    void SetModified();
}
