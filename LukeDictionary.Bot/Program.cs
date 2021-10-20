using System.Threading.Tasks;
using DevSubmarine.LukeDictionary.Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevSubmarine.LukeDictionary
{
    class Program
    {
        static Task Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // load secrets files if present
                    config.AddJsonFile("appsecrets.json", optional: true);
                    config.AddJsonFile($"appsecrets.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // load config
                    services.Configure<DiscordOptions>(context.Configuration);

                    // add services
                    services.AddDiscord();
                })
                .Build();
            return host.RunAsync();
        }
    }
}
