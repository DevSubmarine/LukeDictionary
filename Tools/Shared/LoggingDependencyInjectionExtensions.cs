using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LoggingDependencyInjectionExtensions
    {
        public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.TryAddSingleton<ILoggerFactory>(new LoggerFactory()
                .AddSerilog(Log.Logger, dispose: true));
            services.AddLogging();

            return services;
        }
    }
}