using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace WebJobSDKSample
{
    public static class ServiceCollectionExtensions
    {

        public static void AddHttpClientFactoryService(this IServiceCollection services, IConfiguration configuration)
        {
            AsyncCircuitBreakerPolicy<HttpResponseMessage> breakerPolicy = Policy.HandleResult<HttpResponseMessage>(
               r => !r.IsSuccessStatusCode).AdvancedCircuitBreakerAsync<HttpResponseMessage>(0.5, TimeSpan.FromSeconds(15), 
               7, TimeSpan.FromSeconds(15), OnBreak, OnRest, onHalfOpen: OnHalfOpen);

            services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();

            services.AddSingleton<AsyncCircuitBreakerPolicy<HttpResponseMessage>>(breakerPolicy);

            services.AddHttpClient();

        }



        private static void OnHalfOpen()
        {
            Debug.WriteLine("connection half open");
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