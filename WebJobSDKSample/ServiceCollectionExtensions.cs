using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace WebJobSDKSample
{
    public static class ServiceCollectionExtensions
    {

        public static void AddHttpClientFactoryService(this IServiceCollection services, IConfiguration configuration)
        {
            var conntionString = configuration["AzureServicBusConnectionString"];

            services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();

          
        }


    }
}