using System.Collections.Concurrent;
using ClientFlow.Application.Common;

namespace ClientFlow.Infrastructure.Persistence;

public class InMemoryIdempotencyStore : IIdempotencyStore
{
    private record CacheItem(IdempotentEntry Entry, DateTime ExpiresAtUtc);
    private readonly ConcurrentDictionary<string, CacheItem> _entries = new();
    private readonly ConcurrentDictionary<string, DateTime> _inFlightLocks = new();

    public Task<IdempotentEntry?> GetAsync(string key, CancellationToken ct = default)
    {
        CleanExpired();
        if (_entries.TryGetValue(key, out var item))
        {
            if (item.ExpiresAtUtc > DateTime.UtcNow)
            {
                return Task.FromResult<IdempotentEntry?>(item.Entry);
            }
            _entries.TryRemove(key, out _);
        }
        return Task.FromResult<IdempotentEntry?>(null);
    }

    public Task SetAsync(string key, int statusCode, string contentType, string body, TimeSpan ttl, CancellationToken ct = default)
    {
        var entry = new IdempotentEntry(statusCode, contentType, body, DateTime.UtcNow);
        var expiresAt = DateTime.UtcNow.Add(ttl);
        _entries[key] = new CacheItem(entry, expiresAt);
        _inFlightLocks.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> TryLockAsync(string key, TimeSpan lockDuration, CancellationToken ct = default)
    {
        CleanExpired();
        var lockExpiry = DateTime.UtcNow.Add(lockDuration);

        while (true)
        {
            if (_inFlightLocks.TryGetValue(key, out var currentExpiry))
            {
                if (currentExpiry > DateTime.UtcNow)
                {
                    return Task.FromResult(false); // Resource locked by concurrent operation
                }
                if (_inFlightLocks.TryUpdate(key, lockExpiry, currentExpiry))
                {
                    return Task.FromResult(true);
                }
            }
            else
            {
                if (_inFlightLocks.TryAdd(key, lockExpiry))
                {
                    return Task.FromResult(true);
                }
            }
        }
    }

    public Task ReleaseLockAsync(string key, CancellationToken ct = default)
    {
        _inFlightLocks.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private void CleanExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _entries)
        {
            if (kvp.Value.ExpiresAtUtc <= now)
            {
                _entries.TryRemove(kvp.Key, out _);
            }
        }
        foreach (var kvp in _inFlightLocks)
        {
            if (kvp.Value <= now)
            {
                _inFlightLocks.TryRemove(kvp.Key, out _);
            }
        }
    }
}
