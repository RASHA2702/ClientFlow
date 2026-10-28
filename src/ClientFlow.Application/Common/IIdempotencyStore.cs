namespace ClientFlow.Application.Common;

public record IdempotentEntry(int StatusCode, string ContentType, string Body, DateTime StoredAtUtc);

public interface IIdempotencyStore
{
    Task<IdempotentEntry?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, int statusCode, string contentType, string body, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> TryLockAsync(string key, TimeSpan lockDuration, CancellationToken ct = default);
    Task ReleaseLockAsync(string key, CancellationToken ct = default);
}
