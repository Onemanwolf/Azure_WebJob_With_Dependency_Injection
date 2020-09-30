using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using WebJobSDKSample;

namespace WebJobsSDKSample
{
    public class Functions
    {
        private  readonly IHttpClientFactoryService _httpClientFactoryService;

        public Functions(IHttpClientFactoryService httpClientFactoryService)
        {
            this._httpClientFactoryService = httpClientFactoryService;
        }
        public void ProcessQueueMessage([ServiceBusTrigger("paychecktimecard", Connection = "AzureServicBusConnectionString")] string message, ILogger logger)
        {
            _httpClientFactoryService.SendTimeCardMessage(message);
            logger.LogInformation(message);
        }
    }
}
