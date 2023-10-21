using Serilog.Context;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers.ScopedLogContext;
sealed class ScopedLogContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var currentInstance = Context.ScopedLogContext._current.Value;
        if (currentInstance != null)
            currentInstance.Enrich(logEvent, propertyFactory);
    }
}

