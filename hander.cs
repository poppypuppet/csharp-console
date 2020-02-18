


using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PEngineModule.Logs
{
    public class Handler : HttpMessageHandler
    {
        public Handler()
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
        }
    }
}