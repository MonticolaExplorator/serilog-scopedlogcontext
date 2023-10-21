using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Context;

namespace Serilog;

/// <summary>
/// Extends <see cref="IHostBuilder"/> with Serilog configuration methods.
/// </summary>
public static class SerilogHostBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="ScopedLogContext"/>.
    /// </summary>
    /// <param name="builder">The host builder to configure.</param>
    /// <returns>The host builder.</returns>
    public static IHostBuilder UseSerilogScopedLogContext(
        this IHostBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        builder.ConfigureServices((_, collection) =>
        {
            collection.AddScoped<ScopedLogContext>();
        });
        return builder;
    }
}
