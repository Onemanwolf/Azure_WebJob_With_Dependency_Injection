using Microsoft.Extensions.Configuration;

namespace WebJobSDKSample.Configuration
{
    public class HttpClientConfig : IHttpClientConfig
    {
        public string ServiceBusConnectionString { get; set; }
        public string CosmosDBConnectionString { get; set; }

        public string BaseUri { get; set; }

        private IConfiguration _configuration;

        






        public HttpClientConfig(IConfiguration configuration)
        {
            _configuration = configuration;


        }


        public void ConfigureCosmosDB()
        {
            var cosmosDBConnectionString = _configuration["PayChexCosmosDbConnectionString"];
            CosmosDBConnectionString = cosmosDBConnectionString;
        }

        public void ConfigureServiceBus()
        {
            var serviceBusConnectionString = _configuration["PaychexServiceBusConnectionsString"];
            ServiceBusConnectionString = serviceBusConnectionString;
        }

        public void ConfigureUri()
        {
            var uri = _configuration["BaseAddress"] + "/api/TimeCard";
            BaseUri = uri;

        }


    }
}
