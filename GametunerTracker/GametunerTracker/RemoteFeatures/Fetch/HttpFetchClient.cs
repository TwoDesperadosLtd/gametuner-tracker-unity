using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GametunerTracker.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GametunerTracker.RemoteConfig
{
    public class HttpFetchClient : FetchClient, IFetchClient
    {
        private readonly IHttpRequestClient _httpRequestClient;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _periodicFetchTask;
        public new event Action<Dictionary<string, Feature>> OnDataFetched;
        public new event Action<Exception> OnFetchError;

        internal HttpFetchClient(
            string serverUrl,
            string appID,
            string userId,
            int fetchTimeoutSeconds,
            IStore store,
            IHttpRequestClient httpRequestClient = null)
            : base(serverUrl, appID, userId, fetchTimeoutSeconds, store)
        {
            _httpRequestClient = httpRequestClient ?? new UnityWebRequestClient(fetchTimeoutSeconds);
        }

        public void StartPeriodicFetch(int intervalSeconds)
        {
            StopPeriodicFetch();

            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource = cancellationTokenSource;
            var token = cancellationTokenSource.Token;
            _periodicFetchTask = Task.Run(async () => await PeriodicFetchLoop(intervalSeconds, token), token);
        }

        public void StopPeriodicFetch()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }

            if (_periodicFetchTask != null)
            {
                try
                {
                    _periodicFetchTask.Wait(TimeSpan.FromSeconds(5));
                }
                catch (AggregateException ex)
                {
                    foreach (var inner in ex.Flatten().InnerExceptions)
                    {
                        if (inner is TaskCanceledException || inner is OperationCanceledException)
                        {
                            continue;
                        }

                        OnFetchError?.Invoke(inner);
                    }
                }
                _periodicFetchTask = null;
            }
        }

        private async Task PeriodicFetchLoop(int intervalSeconds, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await FetchDataAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    // Expected when stopping
                    break;
                }
                catch (Exception ex)
                {
                    OnFetchError?.Invoke(ex);
                }
            }
        }

        public async Task<Dictionary<string, Feature>> FetchDataAsync()
        {
            try
            {
                var features = new Dictionary<string, Feature>();
                var userAttributes = _store.GetAllUserAttributes();

                var requestBody = new
                {
                    user_id = _userId,
                    user_attributes = userAttributes
                };

                string jsonBody = JsonConvert.SerializeObject(requestBody);
                var response = await _httpRequestClient.PostJsonAsync(
                    _serverUrl,
                    jsonBody,
                    new Dictionary<string, string> { { "Content-Type", "application/json" } });

                if (!response.IsSuccess)
                {
                    Log.Error($"Request error: {response.Error}");
                    return features;
                }

                features = ParseFeaturesResponse(response.ResponseBody);


                // Save to store
                _store.SaveFeatures(features);
                _store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));

                OnDataFetched?.Invoke(features);

                return features;
            }
            catch (Exception ex)
            {
                Log.Error($"FetchDataAsync error: {ex}");
                OnFetchError?.Invoke(ex);
                return new Dictionary<string, Feature>();
            }
        }

        public void Dispose()
        {
            StopPeriodicFetch();

            var disposableRequestClient = _httpRequestClient as IDisposable;
            if (disposableRequestClient != null)
            {
                disposableRequestClient.Dispose();
            }
        }

        private static Dictionary<string, Feature> ParseFeaturesResponse(string responseJson)
        {
            var features = new Dictionary<string, Feature>();

            if (string.IsNullOrEmpty(responseJson))
            {
                return features;
            }

            var root = JObject.Parse(responseJson);
            var featuresResults = root["features_results"] as JObject;
            if (featuresResults == null)
            {
                return features;
            }

            foreach (var property in featuresResults.Properties())
            {
                var featureToken = property.Value as JObject;
                if (featureToken == null)
                {
                    continue;
                }

                var valueToken = featureToken["value"];
                string featureValue = null;
                if (valueToken != null && valueToken.Type != JTokenType.Null)
                {
                    featureValue = valueToken.Type == JTokenType.String
                        ? valueToken.Value<string>()
                        : valueToken.ToString(Formatting.None);
                }

                features[property.Name] = new Feature
                {
                    Key = property.Name,
                    Value = featureValue,
                    IsOn = featureToken.Value<bool?>("is_on") ?? false,
                    Source = featureToken.Value<string>("source")
                };
            }

            return features;
        }
    }
}
