using System;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace DevSubmarine.LukeDictionary.Tools
{
    public static class Logging
    {
        private static bool _initialized = false;
        private static readonly object _lock = new object();

        public static void ConfigureLogging(IConfiguration configuration)
        {
            lock (_lock)
            {
                LoggerConfiguration config = new LoggerConfiguration();

                if (configuration?.GetSection("Logging").Exists() == true)
                    config.ReadFrom.Configuration(configuration, "Logging");
                else
                {
                    config.WriteTo.Console()
                        .WriteTo.File("logs/log.txt", fileSizeLimitBytes: 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 5)
                        .MinimumLevel.Is(Debugger.IsAttached ? LogEventLevel.Verbose : LogEventLevel.Debug)
                            .Enrich.FromLogContext();
                }

                // create the logger
                Log.Logger = config.CreateLogger();

                // enable logging of unhandled exceptions, but only when initializing for the first time
                if (!_initialized)
                    AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

                _initialized = true;
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Fatal((Exception)e.ExceptionObject, "An exception was unhandled");
                Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
