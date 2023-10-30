using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Enrichers.ScopedLogContext.Test.Support;

internal static class LogEventPropertyValueExtensions
{
    public static object? LiteralValue(this LogEventPropertyValue @this)
    {
        return ((ScalarValue)@this).Value;
    }
}
