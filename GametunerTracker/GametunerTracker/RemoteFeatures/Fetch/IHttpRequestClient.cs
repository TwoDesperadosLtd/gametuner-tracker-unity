using System.Collections.Generic;
using System.Threading.Tasks;

namespace GametunerTracker.RemoteConfig
{
    internal interface IHttpRequestClient
    {
        Task<HttpRequestResult> PostJsonAsync(
            string url,
            string jsonBody,
            Dictionary<string, string> headers);
    }
}
