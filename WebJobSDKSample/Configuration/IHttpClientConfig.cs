namespace WebJobSDKSample.Configuration
{
    public interface IHttpClientConfig
    {
        string BaseUri { get; set; }
        string CosmosDBConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }

        void ConfigureCosmosDB();
        void ConfigureServiceBus();
        void ConfigureUri();
    }
}