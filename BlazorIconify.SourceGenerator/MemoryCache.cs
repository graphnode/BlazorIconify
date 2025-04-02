namespace Graphnode.BlazorIconify.SourceGenerator;

using System;
using System.Collections.Concurrent;

/// <summary>
/// A simple in-memory cache implementation for source generators
/// </summary>
public class MemoryCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public T? Get<T>(string key)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return default;

        if (IsExpired(entry))
        {
            // Remove expired item
            _cache.TryRemove(key, out _);
            return default;
        }

        return (T?)entry.Value;
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        value = default;

        if (!_cache.TryGetValue(key, out var entry))
            return false;

        if (IsExpired(entry))
        {
            // Remove expired item
            _cache.TryRemove(key, out _);
            return false;
        }

        value = (T?)entry.Value;
        return true;
    }

    public void Set<T>(string key, T? value, TimeSpan? absoluteExpiration = null)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        var expirationTime = absoluteExpiration.HasValue
            ? DateTimeOffset.UtcNow.Add(absoluteExpiration.Value)
            : DateTimeOffset.MaxValue;

        var entry = new CacheEntry(value, expirationTime);
        _cache.AddOrUpdate(key, entry, (_, _) => entry);
    }

    public bool Remove(string key)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        return _cache.TryRemove(key, out _);
    }

    public void Clear() => _cache.Clear();

    private static bool IsExpired(CacheEntry entry) => entry.ExpirationTime <= DateTimeOffset.UtcNow;

    private class CacheEntry(object? value, DateTimeOffset expirationTime)
    {
        public object? Value { get; } = value;
        public DateTimeOffset ExpirationTime { get; } = expirationTime;
    }
}
