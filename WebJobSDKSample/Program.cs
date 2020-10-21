using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace WebJobSDKSample
{
    class Program
    {
        //Add Property for Configuration
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
          .SetBasePath(System.IO.Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
          .AddEnvironmentVariables()

          .Build();




        static async Task Main()
        {
            var builder = new HostBuilder();

            //DI configuration
            var host = builder.ConfigureServices((context, services) =>
            {
                
                ServiceCollectionExtensions.AddHttpClientFactoryService(services, Configuration);


                //Move all Service creation to extension class

                // add CircuitBreaker Policy
                //AsyncCircuitBreakerPolicy<HttpResponseMessage> breakerPolicy = Policy.HandleResult<HttpResponseMessage>(
                //r => !r.IsSuccessStatusCode).AdvancedCircuitBreakerAsync<HttpResponseMessage>(0.5, TimeSpan.FromSeconds(15), 7, TimeSpan.FromSeconds(15), OnBreak, OnRest, onHalfOpen: OnHalfOpen);


                //services.AddSingleton<AsyncCircuitBreakerPolicy<HttpResponseMessage>>(breakerPolicy);

                //services.AddHttpClient();
                //Our Service we injected into the Functions Class
                //services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();
            })


            .ConfigureWebJobs(b =>
            {
                b.AddAzureStorageCoreServices();
                b.AddAzureStorage();
                b.AddServiceBus(sbOptions =>
                {
                    sbOptions.MessageHandlerOptions.AutoComplete = true;
                    sbOptions.MessageHandlerOptions.MaxConcurrentCalls = 16;
                });
            })


            // setup configuration of certs 
            .ConfigureAppConfiguration((hostingContext, config) =>
            {

                var builtConfig = config.Build();

                using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certs = store.Certificates
                        .Find(X509FindType.FindByThumbprint,
                            builtConfig["AzureADCertThumbprint"], false);

                    config.AddAzureKeyVault(
                        $"https://{builtConfig["KeyVaultName"]}.vault.azure.net/",
                        builtConfig["AzureADApplicationId"],
                        certs.OfType<X509Certificate2>().Single());

                    store.Close();

                }
                //Update Configuration of Nlog to use Key Vault for Connections Strings
                UpdateNLogConfig(Configuration, hostingContext.HostingEnvironment);


            }

            )
            .ConfigureLogging((context, b) =>
            {
                //Configure Log Providers 
                b.ClearProviders();
                b.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                b.AddConsole();
                b.AddNLog("Nlog.config");
            })

            .UseNLog()
            .Build();

            using (host)
            {

                await host.RunAsync();
            }


        }


        private static void UpdateNLogConfig(IConfiguration configuration, IHostEnvironment env)
        {
            var azureEventHubConnectionString = configuration["AzureEventHubConnectionString"];
            GlobalDiagnosticsContext.Set("AzureEventHubConnectionString", azureEventHubConnectionString);
            var configFile = env.IsDevelopment() ? $"nlog.{env.EnvironmentName}.config" : "nlog.config";
            LogManager.Configuration = LogManager.LoadConfiguration(configFile).Configuration;
        }
    }
}
