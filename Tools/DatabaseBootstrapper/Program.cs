using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Database;
using DevSubmarine.LukeDictionary.Database.Services;
using DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper.CollectionCreators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DevSubmarine.LukeDictionary.Tools.DatabaseBootstrapper
{
    class Program
    {
        private static IServiceProvider _services;

        static async Task Main(string[] args)
        {
            // config
            IConfiguration config = ConfigurationLoader.Load(args);

            // logging
            Logging.ConfigureLogging(config);

            try
            {
                // prepare DI container
                IServiceCollection serviceCollection = ConfigureServices(config);
                _services = serviceCollection.BuildServiceProvider();

                // run
                ApplicationRunner runner = _services.GetRequiredService<ApplicationRunner>();
                await runner.RunAsync().ConfigureAwait(false);

                // wait for enter on done
                if (Debugger.IsAttached)
                    Console.ReadLine();
            }
            finally
            {
                OnExit();
            }
        }

        private static IServiceCollection ConfigureServices(IConfiguration configuration)
        {
            IServiceCollection services = new ServiceCollection();

            services.Configure<MongoOptions>(configuration);

            services.AddSerilogLogging();
            services.AddTransient<ApplicationRunner>();
            services.AddSingleton<IMongoDatabaseClient, MongoDatabaseClient>();

            // COLLECTION CREATORS
            services.AddCollectionCreator<WordsCollectionCreator>();

            return services;
        }

        private static void OnExit()
        {
            try { Log.CloseAndFlush(); } catch { }
            try
            {
                if (_services is IDisposable disposableServices)
                    disposableServices.Dispose();
            }
            catch { }
        }
    }
}
