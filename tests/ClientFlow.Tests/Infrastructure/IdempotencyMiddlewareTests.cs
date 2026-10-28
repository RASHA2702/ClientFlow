using System.Text;
using ClientFlow.Api.Middleware;
using ClientFlow.Application.Common;
using ClientFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientFlow.Tests.Infrastructure;

[TestClass]
public class IdempotencyMiddlewareTests
{
    [TestMethod]
    public async Task InvokeAsync_NonMutatingGetRequest_BypassesIdempotency()
    {
        var store = new InMemoryIdempotencyStore();
        bool nextCalled = false;
        var middleware = new IdempotencyMiddleware((ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Headers["Idempotency-Key"] = "test-key";

        await middleware.InvokeAsync(context, store);

        Assert.IsTrue(nextCalled);
        Assert.IsFalse(context.Response.Headers.ContainsKey("X-Idempotency-Replay"));
    }

    [TestMethod]
    public async Task InvokeAsync_KeyExceedsLength_Returns400BadRequest()
    {
        var store = new InMemoryIdempotencyStore();
        var middleware = new IdempotencyMiddleware((ctx) => Task.CompletedTask);

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Headers["Idempotency-Key"] = new string('a', 150); // > 128 chars
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, store);

        Assert.AreEqual(StatusCodes.Status400BadRequest, context.Response.StatusCode);
    }

    [TestMethod]
    public async Task InvokeAsync_CachedKey_ReplaysCachedResponseWithoutCallingNext()
    {
        var store = new InMemoryIdempotencyStore();
        await store.SetAsync("inv_key_replay", 201, "application/json", "{\"invoiceId\":\"replayed\"}", TimeSpan.FromHours(1));

        bool nextCalled = false;
        var middleware = new IdempotencyMiddleware((ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Headers["Idempotency-Key"] = "inv_key_replay";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, store);

        Assert.IsFalse(nextCalled);
        Assert.AreEqual(201, context.Response.StatusCode);
        Assert.AreEqual("true", context.Response.Headers["X-Idempotency-Replay"].ToString());

        context.Response.Body.Position = 0;
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.AreEqual("{\"invoiceId\":\"replayed\"}", responseBody);
    }

    [TestMethod]
    public async Task InvokeAsync_NewKey_ExecutesNextAndCachesResponse()
    {
        var store = new InMemoryIdempotencyStore();
        var middleware = new IdempotencyMiddleware(async (ctx) =>
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            var bytes = Encoding.UTF8.GetBytes("{\"status\":\"created\"}");
            await ctx.Response.Body.WriteAsync(bytes);
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Headers["Idempotency-Key"] = "new_unique_key";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context, store);

        Assert.AreEqual(200, context.Response.StatusCode);
        Assert.AreEqual("false", context.Response.Headers["X-Idempotency-Replay"].ToString());

        // Verify stored in idempotency store
        var stored = await store.GetAsync("new_unique_key");
        Assert.IsNotNull(stored);
        Assert.AreEqual(200, stored.StatusCode);
        Assert.AreEqual("{\"status\":\"created\"}", stored.Body);
    }
}
