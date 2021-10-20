using Microsoft.Extensions.Configuration;

namespace DevSubmarine.LukeDictionary.Tools
{
    public static class ConfigurationLoader
    {
        public static IConfiguration Load(string[] args)
        {
            return new ConfigurationBuilder()
                .AddEnvironmentVariables()
                //.AddJsonFile("appsettings.json", optional: true)
                //.AddJsonFile("appsecrets.json", optional: true)
                .AddCommandLine(args)
                .Build();
        }
    }
}
