using System;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Database;
using DevSubmarine.LukeDictionary.Discord;
using DevSubmarine.LukeDictionary.PasteMyst;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DevSubmarine.LukeDictionary
{
    class Program
    {
        static Task Main(string[] args)
        {
            // add default logger for errors that happen before host runs
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/unhandled.log",
                fileSizeLimitBytes: 1048576,        // 1MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 5,
                rollingInterval: RollingInterval.Day)
                .CreateLogger();
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // create a generic host
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // load secrets files if present
                    config.AddJsonFile("appsecrets.json", optional: true);
                    config.AddJsonFile($"appsecrets.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .UseSerilog((context, config) 
                    => config.ReadFrom.Configuration(context.Configuration, "Logging"), 
                    preserveStaticLogger: true)
                .ConfigureServices((context, services) =>
                {
                    // load config
                    services.Configure<DiscordOptions>(context.Configuration);
                    services.Configure<MongoOptions>(context.Configuration.GetSection("Database"));
                    services.Configure<DevSubmarineOptions>(context.Configuration);
                    services.Configure<PasteMystOptions>(context.Configuration.GetSection("PasteMyst"));

                    // add services
                    services.AddDiscord();
                    services.AddMongoDB();
                    services.AddPasteMyst();
                    services.AddMemoryCache();
                })
                .Build();
            return host.RunAsync();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Logger.Error((Exception)e.ExceptionObject, "An exception was unhandled");
                Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
