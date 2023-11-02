using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Enrichers.ScopedLogContext.Test.Support;
using Serilog.Events;

namespace Serilog.Enrichers.ScopedLogContext.Test.Context;

public class ScopedLogContextTests
{
    [Fact]
    public void PushedPropertiesAreAvailableToLoggers()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();
        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            using (scopedLogContext.PushProperty("A", 1))
            using (scopedLogContext.Push(new PropertyEnricher("B", 2)))
            using (scopedLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4))) // Different overload
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
                Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
                Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());
            }
        }
    }

    [Fact]
    public void PushedPropertiesAreNotAvailableToLoggersWhenDisposed()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();
        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            using (scopedLogContext.PushProperty("A", 1))
            using (scopedLogContext.Push(new PropertyEnricher("B", 2)))
            using (scopedLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4))) // Different overload
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
                Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
                Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());
            }
            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.False(lastEvent!.Properties.ContainsKey("A"));
            Assert.False(lastEvent.Properties.ContainsKey("B"));
            Assert.False(lastEvent.Properties.ContainsKey("C"));
            Assert.False(lastEvent.Properties.ContainsKey("D"));
        }
    }

    [Fact]
    public void PushedPropertiesAreNotAvailableToLoggersWhenScopeDisposed()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();
        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            scopedLogContext.PushProperty("A", 1);
            scopedLogContext.Push(new PropertyEnricher("B", 2));
            scopedLogContext.Push(new PropertyEnricher("C", 3), new PropertyEnricher("D", 4)); // Different overload

            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());
            Assert.Equal(2, lastEvent.Properties["B"].LiteralValue());
            Assert.Equal(3, lastEvent.Properties["C"].LiteralValue());
            Assert.Equal(4, lastEvent.Properties["D"].LiteralValue());
        }

        log.Write(Some.InformationEvent());
        Assert.NotNull(lastEvent);
        Assert.False(lastEvent!.Properties.ContainsKey("A"));
        Assert.False(lastEvent.Properties.ContainsKey("B"));
        Assert.False(lastEvent.Properties.ContainsKey("C"));
        Assert.False(lastEvent.Properties.ContainsKey("D"));

    }


    [Fact]
    public void MoreNestedPropertiesOverrideLessNestedOnes()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();
        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            scopedLogContext.PushProperty("A", 1);

            log.Write(Some.InformationEvent());
            Assert.NotNull(lastEvent);
            Assert.Equal(1, lastEvent!.Properties["A"].LiteralValue());

            using (scopedLogContext.PushProperty("A", 2))
            {
                log.Write(Some.InformationEvent());
                Assert.Equal(2, lastEvent.Properties["A"].LiteralValue());
            }

            log.Write(Some.InformationEvent());
            Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());
        }

        log.Write(Some.InformationEvent());
        Assert.False(lastEvent.Properties.ContainsKey("A"));
    }

    [Fact]
    public void MultipleNestedPropertiesOverrideLessNestedOnes()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();

        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            using (scopedLogContext.Push(new PropertyEnricher("A1", 1), new PropertyEnricher("A2", 2)))
            {
                log.Write(Some.InformationEvent());
                Assert.NotNull(lastEvent);
                Assert.Equal(1, lastEvent!.Properties["A1"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["A2"].LiteralValue());

                using (scopedLogContext.Push(new PropertyEnricher("A1", 10), new PropertyEnricher("A2", 20)))
                {
                    log.Write(Some.InformationEvent());
                    Assert.Equal(10, lastEvent.Properties["A1"].LiteralValue());
                    Assert.Equal(20, lastEvent.Properties["A2"].LiteralValue());
                }

                log.Write(Some.InformationEvent());
                Assert.Equal(1, lastEvent.Properties["A1"].LiteralValue());
                Assert.Equal(2, lastEvent.Properties["A2"].LiteralValue());
            }

            log.Write(Some.InformationEvent());
            Assert.False(lastEvent.Properties.ContainsKey("A1"));
            Assert.False(lastEvent.Properties.ContainsKey("A2"));
        }
    }

    [Fact]
    public async Task ContextPropertiesCrossAsyncCalls()
    {
        await TestWithSyncContext(async () =>
        {
            LogEvent? lastEvent = null;

            var log = new LoggerConfiguration()
                .Enrich.FromScopedLogContext()
                .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
                .CreateLogger();
            using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
            {
                using (scopedLogContext.PushProperty("A", 1))
                {
                    var pre = Thread.CurrentThread.ManagedThreadId;

                    await Task.Yield();

                    var post = Thread.CurrentThread.ManagedThreadId;

                    log.Write(Some.InformationEvent());
                    Assert.NotNull(lastEvent);
                    Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());

                    Assert.False(Thread.CurrentThread.IsThreadPoolThread);
                    Assert.True(Thread.CurrentThread.IsBackground);
                    Assert.NotEqual(pre, post);
                }
            }
        },
            new ForceNewThreadSyncContext());
    }

    [Fact]
    public async Task ContextEnrichersInAsyncScopeCanBeCleared()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();
        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            using (scopedLogContext.Push(new PropertyEnricher("A", 1)))
            {
                await Task.Run(() =>
                {
                    scopedLogContext.Reset();
                    log.Write(Some.InformationEvent());
                });

                Assert.NotNull(lastEvent);
                Assert.Empty(lastEvent!.Properties);

                // Reset should only work for current async scope, outside of it previous Context
                // instance should be available again.
                log.Write(Some.InformationEvent());
                Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());
            }
        }
    }

    [Fact]
    public async Task ContextEnrichersCanBeTemporarilyCleared()
    {
        LogEvent? lastEvent = null;

        var log = new LoggerConfiguration()
            .Enrich.FromScopedLogContext()
            .WriteTo.Sink(new DelegatingSink(e => lastEvent = e))
            .CreateLogger();

        using (var scopedLogContext = new Serilog.Context.ScopedLogContext())
        {
            using (scopedLogContext.Push(new PropertyEnricher("A", 1)))
            {
                using (scopedLogContext.Suspend())
                {
                    await Task.Run(() =>
                    {
                        log.Write(Some.InformationEvent());
                    });

                    Assert.NotNull(lastEvent);
                    Assert.Empty(lastEvent!.Properties);
                }

                // Suspend should only work for scope of using. After calling Dispose all enrichers
                // should be restored.
                log.Write(Some.InformationEvent());
                Assert.Equal(1, lastEvent.Properties["A"].LiteralValue());
            }
        }
    }


    static async Task TestWithSyncContext(Func<Task> testAction, SynchronizationContext syncContext)
    {
        var prevCtx = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(syncContext);

        Task t;
        try
        {
            t = testAction();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(prevCtx);
        }

        await t;
    }
}

class ForceNewThreadSyncContext : SynchronizationContext
{
    public override void Post(SendOrPostCallback d, object? state) => new Thread(x => d(x)) { IsBackground = true }.Start(state);
}
