using Serilog;
using Serilog.Context;

namespace ConsoleSample
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}")
                .Enrich.FromScopedLogContext()
                .CreateLogger();

            try
            {
                
                using(var scopedLogContext = new ScopedLogContext())
                {
                    startProcess(scopedLogContext);
                    Log.Information("Carries property trace_id");

                    startActivity(scopedLogContext);
                    Log.Information("Carries property trace_id and span_id");

                    using(scopedLogContext.PushProperty("A", 1))
                    {
                        Log.Information("Carries property trace_id, span_id and A");
                    }
                }

                Log.Information("Carries no properties");

                using (var scopedLogContext = new ScopedLogContext())
                {
                    startProcess(scopedLogContext);
                    Log.Information("Carries property trace_id again");
                    using (var nestedContext = new ScopedLogContext())
                    {
                        using (nestedContext.PushProperty("A", 1))
                        {
                            Log.Information("Carries property trace_id and A");
                        }
                    }
                }
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    
        private static void startProcess(ScopedLogContext logContext)
        {
            logContext.PushProperty("trace_id", Guid.NewGuid().ToString("N"));
        }

        private static void startActivity(ScopedLogContext logContext)
        {
            logContext.PushProperty("span_id", Guid.NewGuid().ToString("N"));
        }

    }
}
