using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebJobSDKSample.Configuration;

namespace WebJobSDKSample
{
    public class HttpClientFactoryService : IHttpClientFactoryService 
    {
        private readonly AsyncRetryPolicy _retryPolicy; //you can init here if you like
        private readonly IHttpClientFactory _httpClientFactory;
        private const int MaxRetries = 3;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreaker;
        private readonly IConfiguration _configuration;
        private readonly ILogger<HttpClientFactoryService> _logger;
        private readonly IHttpClientConfig _httpClientConfig;

        public HttpClientFactoryService(IHttpClientFactory httpClientFactory, AsyncCircuitBreakerPolicy<HttpResponseMessage> 
            circuitBreaker, IConfiguration configuration, ILogger<HttpClientFactoryService> logger)
        {

            //add in Service Configuration 
            _httpClientFactory = httpClientFactory;
            _circuitBreaker = circuitBreaker;

            // injection configuration 
            _configuration = configuration;
            _logger = logger;
            _logger.LogDebug(1, "NLog injected into HttpClientService");

            //init policy
            _retryPolicy = Policy.Handle<HttpRequestException>().RetryAsync(MaxRetries);

        }

        public HttpClientFactoryService()
        {

        }


        public async Task<TimeCard> SendTimeCardMessage(string timeCard)
        {
            var stringContent = new StringContent(timeCard, UnicodeEncoding.UTF8, "application/json");

           

      
            HttpClient client = _httpClientFactory.CreateClient();

            //Add Configuration
            var uri = _configuration["BaseAddress"] + "/api/TimeCard";

            _logger.LogInformation($"Base Uri: {uri}");
            HttpResponseMessage response = await _retryPolicy.ExecuteAsync(
                () => _circuitBreaker.ExecuteAsync( () => client.PostAsync(requestUri: uri, stringContent)
                    ));

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return null;
            }

            var resultString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TimeCard>(resultString);




            ////Excute Retry Policy Only
            //return await _retryPolicy.ExecuteAsync(async () =>
            //{
            //    var uri = "https://YourUriGoesHere";

            //    //Post to api with client
            //    HttpResponseMessage result = await client.PostAsync(requestUri: uri, stringContent);

            //    if (result.StatusCode != System.Net.HttpStatusCode.OK)
            //    {
            //        return null;
            //    }

            //    var resultString = await result.Content.ReadAsStringAsync();
            //    return JsonSerializer.Deserialize<TimeCard>(resultString);

            // });
        }


    }

}
