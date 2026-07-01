// ==================== Main SDK Class ====================
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GametunerTracker.Logging;
using GametunerTracker.Payloads.Contexts;

namespace GametunerTracker.RemoteConfig
{
    public class RemoteConfigClient
    {
        public IStore _store;
        public IFetchClient _fetchClient;
        private Dictionary<string, Feature> _featuresCache;
        private static DeviceContext deviceContext;
        private RemoteConfigSettings _settings;
        private string _userId;
        private readonly object _cacheLock = new object();
        private bool _initialized = false;

        public event Action<Dictionary<string, Feature>> OnConfigUpdated;
        public event Action<Exception> OnError;

        public void Init(
            string appID,
            string userId,
            Dictionary<string, Feature> defaultFeatures,
            RemoteConfigSettings settings,
            bool debugEnabled = false,
            IStore store = null,
            IFetchClient fetchClient = null)
        {
            if (_initialized)
            {
                Log.Debug("Tracker is already initialized");
                return;
                // throw new InvalidOperationException("SDK is already initialized");
            }

            _userId = userId;
            _settings = settings ?? new RemoteConfigSettings();

            // Initialize store
            _store = store ?? new ConfigStore();

            // Load cached features or use defaults
            var cachedFeatures = _store.GetAllFeatures();
            lock (_cacheLock)
            {
                _featuresCache = cachedFeatures.Count > 0 ? cachedFeatures : defaultFeatures;
            }

            // Initialize fetch client
            _fetchClient = fetchClient ?? new HttpFetchClient(
                string.Format(_settings.ServerUrl, appID, debugEnabled ? "dev" : "production"),
                appID,
                userId,
                _settings.FetchTimeoutSeconds,
                _store,
                _settings.UseDotNetHttpClient
                    ? (IHttpRequestClient)new DotNetHttpRequestClient(_settings.FetchTimeoutSeconds)
                    : new UnityWebRequestClient(_settings.FetchTimeoutSeconds)
            );

            _fetchClient.OnDataFetched += (features) =>
            {
                lock (_cacheLock)
                {
                    _featuresCache = features;
                }
                OnConfigUpdated?.Invoke(features);
            };

            _fetchClient.OnFetchError += (ex) =>
            {
                OnError?.Invoke(ex);
            };

            // Start periodic fetch
            _fetchClient.StartPeriodicFetch(_settings.PeriodicFetchIntervalSeconds);

            // Initial fetch
            Task.Run(async () =>
            {
                try
                {
                    await FetchDataAsync();
                }
                catch
                {
                    // Initial fetch failure is non-critical, using cached/default data
                }
            });

            _initialized = true;
        }

        public Dictionary<string, Feature> GetAllData()
        {
            if (!_initialized)
            {
                Log.Debug("SDK not initialized. Call Init() first.");
            }

            lock (_cacheLock)
            {
                return new Dictionary<string, Feature>(_featuresCache);
            }
        }

        public Feature GetFeature(string key)
        {
            if (!_initialized)
            {
                Log.Debug("SDK not initialized. Call Init() first.");
            }

            lock (_cacheLock)
            {
                return _featuresCache.ContainsKey(key) ? _featuresCache[key] : null;
            }
        }

        public async Task FetchDataAsync()
        {
            if (!_initialized)
            {
                Log.Debug("SDK not initialized. Call Init() first.");
            }

            string lastFetchTimeStr = _store.GetMetadata("last_fetch_time");

            if (!string.IsNullOrEmpty(lastFetchTimeStr))
            {
                DateTime lastFetchTime = DateTime.Parse(lastFetchTimeStr);
                TimeSpan timeSinceLastFetch = DateTime.UtcNow - lastFetchTime;

                if (timeSinceLastFetch.TotalSeconds < _settings.CacheTimeSeconds)
                {
                    // Cache is still valid, skip fetch
                    return;
                }
            }

            await _fetchClient.FetchDataAsync();
        }

        public void SetUserAttributes(Dictionary<string, string> attributes)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("SDK not initialized. Call Init() first.");
            }

            _store.SaveUserAttributes(attributes);
        }

        public Dictionary<string, string> GetUserAttributes()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("SDK not initialized. Call Init() first.");
            }

            return _store.GetAllUserAttributes();
        }

        public void Shutdown()
        {
            if (_initialized)
            {
                _fetchClient?.Dispose();
                _initialized = false;
            }
        }
    }
}
