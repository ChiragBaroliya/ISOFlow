namespace ISOFlow.Application.Interfaces;

public interface ICacheService
{
    T? Get<T>(string key);
    void Set<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);
    void Remove(string key);
}
