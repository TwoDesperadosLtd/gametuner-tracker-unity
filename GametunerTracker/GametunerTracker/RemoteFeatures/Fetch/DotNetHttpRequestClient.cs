using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GametunerTracker.RemoteConfig
{
    internal class DotNetHttpRequestClient : IHttpRequestClient, IDisposable
    {
        private readonly HttpClient _httpClient;

        internal DotNetHttpRequestClient(int timeoutSeconds)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(timeoutSeconds)
            };
        }

        public async Task<HttpRequestResult> PostJsonAsync(string url, string jsonBody, Dictionary<string, string> headers)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return new HttpRequestResult
                    {
                        IsSuccess = response.IsSuccessStatusCode,
                        ResponseBody = responseBody,
                        Error = response.IsSuccessStatusCode
                            ? null
                            : string.Format("HTTP {0}: {1}", (int)response.StatusCode, response.ReasonPhrase)
                    };
                }
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
