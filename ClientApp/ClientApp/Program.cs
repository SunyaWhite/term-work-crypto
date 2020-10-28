using ClientApp.Services;
using ClientApp.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClientApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.AddEnvironmentVariables("DOCKER_");
                })
                .ConfigureServices((hostContext, services) =>
                {

                    var dockerHost = hostContext.Configuration[ConfigurationOptions.DOCKER_HOST];

                    var host = string.IsNullOrEmpty(dockerHost)
                        ? hostContext.Configuration[ConfigurationOptions.DEFAULT_HOST]
                        : dockerHost;

                    services.AddHostedService<Worker>();
                    // Workaround. Passing host url
                    services.AddSingleton<VerificationService>(provider =>
                    {
                        var logger = provider.GetRequiredService<ILogger<VerificationService>>();
                        return new VerificationService(host, logger);
                    });
                });
    }
}