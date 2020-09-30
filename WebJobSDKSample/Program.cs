using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Polly.CircuitBreaker;
using Polly;
using System.Diagnostics;

namespace WebJobSDKSample
{
    class Program
    {
        static async Task Main()
        {
            var builder = new HostBuilder();
           
            //DI configuration
            var host = builder.ConfigureServices((context, services) =>
            {
                // add CircuitBreaker Policy
                    AsyncCircuitBreakerPolicy<HttpResponseMessage> breakerPolicy = Policy.HandleResult<HttpResponseMessage>(
                    r => !r.IsSuccessStatusCode).AdvancedCircuitBreakerAsync<HttpResponseMessage>(0.5, TimeSpan.FromSeconds(15), 7, TimeSpan.FromSeconds(15), OnBreak, OnRest, onHalfOpen: OnHalfOpen);

                services.AddSingleton<AsyncCircuitBreakerPolicy<HttpResponseMessage>>(breakerPolicy);
                
                //IHttpFactory provider            
                services.AddHttpClient();
                //Our Service we injected into the Functions Class
                services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();
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
            .ConfigureLogging((context, b) =>
            {
                b.AddConsole();
            })
            .Build();
            
            using (host)
            {
                await host.RunAsync();
            }

            //You use a Starup as well if you wanted to 
            //var builder = new HostBuilder()
            //.UseStartup<Startup>()
            //...omitted for brevity
        }

        private static void OnHalfOpen()
        {
            Debug.WriteLine("onnection half open");
        }

        private static void OnRest()
        {
            Debug.WriteLine("Connection reset");
        }

             

        private static void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timespan)
        {
            Debug.WriteLine($"Connection Break: {delegateResult.Result}");
        }
    }
}
