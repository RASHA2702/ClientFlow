using ClientFlow.Infrastructure.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Infrastructure;

[TestClass]
public class IdempotencyStoreTests
{
    private readonly InMemoryIdempotencyStore _store = new();

    [TestMethod]
    public async Task SetAsync_And_GetAsync_ReturnsStoredEntry()
    {
        string key = "req_123456";
        await _store.SetAsync(key, 201, "application/json", "{\"id\":\"inv_1\"}", TimeSpan.FromMinutes(5));

        var entry = await _store.GetAsync(key);

        Assert.IsNotNull(entry);
        Assert.AreEqual(201, entry.StatusCode);
        Assert.AreEqual("application/json", entry.ContentType);
        Assert.AreEqual("{\"id\":\"inv_1\"}", entry.Body);
    }

    [TestMethod]
    public async Task GetAsync_WhenExpired_ReturnsNull()
    {
        string key = "req_expired";
        // Immediate expiration
        await _store.SetAsync(key, 200, "application/json", "{}", TimeSpan.FromMilliseconds(-100));

        var entry = await _store.GetAsync(key);

        Assert.IsNull(entry);
    }

    [TestMethod]
    public async Task TryLockAsync_WhenConcurrentAccess_AllowsOnlyFirstCaller()
    {
        string key = "req_concurrent";

        bool lock1 = await _store.TryLockAsync(key, TimeSpan.FromSeconds(10));
        bool lock2 = await _store.TryLockAsync(key, TimeSpan.FromSeconds(10));

        Assert.IsTrue(lock1);
        Assert.IsFalse(lock2);

        // After release, lock can be re-acquired
        await _store.ReleaseLockAsync(key);
        bool lock3 = await _store.TryLockAsync(key, TimeSpan.FromSeconds(10));
        Assert.IsTrue(lock3);
    }
}
