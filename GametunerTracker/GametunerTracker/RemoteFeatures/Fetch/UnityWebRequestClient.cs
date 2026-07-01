using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace GametunerTracker.RemoteConfig
{
    internal class UnityWebRequestClient : IHttpRequestClient
    {
        private readonly int _timeoutSeconds;

        internal UnityWebRequestClient(int timeoutSeconds)
        {
            _timeoutSeconds = timeoutSeconds;
        }

        public async Task<HttpRequestResult> PostJsonAsync(string url, string jsonBody, Dictionary<string, string> headers)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = UnityWebRequest.Post(url, jsonBody))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = _timeoutSeconds;

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }

                var asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    return new HttpRequestResult
                    {
                        IsSuccess = false,
                        Error = request.error
                    };
                }

                return new HttpRequestResult
                {
                    IsSuccess = true,
                    ResponseBody = request.downloadHandler.text
                };
            }
        }
    }
}
