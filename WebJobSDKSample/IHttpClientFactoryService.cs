using System.Threading.Tasks;

namespace WebJobSDKSample
{
    public interface IHttpClientFactoryService
    {

        Task<TimeCard> SendTimeCardMessage(string timeCard);
    }

}