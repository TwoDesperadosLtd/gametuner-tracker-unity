using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using GametunerTracker.RemoteConfig;

namespace SnowplowTrackerTests
{
    [TestFixture]
    public class TestRemoteConfigClient
    {
        [Test]
        public void InitUsesDefaultFeaturesWhenStoreIsEmpty()
        {
            var store = new InMemoryStore();
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));

            var client = new RemoteConfigClient();
            var defaults = new Dictionary<string, Feature>
            {
                { "billing_mode", new Feature { Key = "billing_mode", Value = "default" } }
            };

            client.Init("sdk-key", "user-1", defaults, new RemoteConfigSettings(), false, store, new FakeFetchClient());

            var feature = client.GetFeature("billing_mode");
            Assert.NotNull(feature);
            Assert.AreEqual("default", feature.Value);

            client.Shutdown();
        }

        [Test]
        public void InitPrefersCachedFeaturesOverDefaults()
        {
            var store = new InMemoryStore();
            store.SaveFeature(new Feature { Key = "billing_mode", Value = "cached" });
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));

            var client = new RemoteConfigClient();
            var defaults = new Dictionary<string, Feature>
            {
                { "billing_mode", new Feature { Key = "billing_mode", Value = "default" } }
            };

            client.Init("sdk-key", "user-1", defaults, new RemoteConfigSettings(), false, store, new FakeFetchClient());

            var feature = client.GetFeature("billing_mode");
            Assert.NotNull(feature);
            Assert.AreEqual("cached", feature.Value);

            client.Shutdown();
        }

        [Test]
        public async Task FetchDataAsyncSkipsNetworkWhenCacheIsFresh()
        {
            var store = new InMemoryStore();
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
            var fetchClient = new FakeFetchClient();
            var client = BuildClient(store, fetchClient);

            await client.FetchDataAsync();

            Assert.AreEqual(0, fetchClient.FetchCallCount);
            client.Shutdown();
        }

        [Test]
        public async Task FetchDataAsyncCallsFetchClientWhenCacheIsStale()
        {
            var store = new InMemoryStore();
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
            var fetchClient = new FakeFetchClient();
            var client = BuildClient(store, fetchClient);

            await Task.Delay(50);
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.AddHours(-2).ToString("o"));

            await client.FetchDataAsync();

            Assert.AreEqual(1, fetchClient.FetchCallCount);
            client.Shutdown();
        }

        [Test]
        public async Task FetchDataAsyncUpdatesCacheWhenFetchClientReturnsData()
        {
            var store = new InMemoryStore();
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
            var fetchClient = new FakeFetchClient();
            fetchClient.NextResult = new Dictionary<string, Feature>
            {
                { "difficulty", new Feature { Key = "difficulty", Value = "hard", IsOn = true } }
            };

            var client = BuildClient(store, fetchClient);
            await Task.Delay(50);
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.AddHours(-2).ToString("o"));

            await client.FetchDataAsync();

            var feature = client.GetFeature("difficulty");
            Assert.NotNull(feature);
            Assert.AreEqual("hard", feature.Value);
            Assert.True(feature.IsOn);
            client.Shutdown();
        }

        [Test]
        public void UserAttributesRoundTripAfterInit()
        {
            var store = new InMemoryStore();
            store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));

            var client = BuildClient(store, new FakeFetchClient());
            var attributes = new Dictionary<string, string>
            {
                { "country", "US" },
                { "segment", "testers" }
            };

            client.SetUserAttributes(attributes);
            var stored = client.GetUserAttributes();

            Assert.AreEqual("US", stored["country"]);
            Assert.AreEqual("testers", stored["segment"]);
            client.Shutdown();
        }

        [Test]
        public void SetUserAttributesThrowsBeforeInit()
        {
            var client = new RemoteConfigClient();
            var attributes = new Dictionary<string, string> { { "country", "US" } };

            Assert.Throws<InvalidOperationException>(() => client.SetUserAttributes(attributes));
        }

        private static RemoteConfigClient BuildClient(InMemoryStore store, FakeFetchClient fetchClient)
        {
            var client = new RemoteConfigClient();
            client.Init(
                "sdk-key",
                "user-1",
                new Dictionary<string, Feature>(),
                new RemoteConfigSettings { CacheTimeSeconds = 3600, PeriodicFetchIntervalSeconds = 3600 },
                false,
                store,
                fetchClient);
            return client;
        }

        private class InMemoryStore : IStore
        {
            private readonly Dictionary<string, Feature> _features = new Dictionary<string, Feature>();
            private readonly Dictionary<string, string> _metadata = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _userAttributes = new Dictionary<string, string>();

            public void SaveFeature(Feature feature)
            {
                if (feature != null)
                {
                    _features[feature.Key] = feature;
                }
            }

            public Feature GetFeature(string key)
            {
                return _features.ContainsKey(key) ? _features[key] : null;
            }

            public Dictionary<string, Feature> GetAllFeatures()
            {
                return new Dictionary<string, Feature>(_features);
            }

            public void SaveFeatures(Dictionary<string, Feature> features)
            {
                foreach (var kvp in features)
                {
                    _features[kvp.Key] = kvp.Value;
                }
            }

            public void SaveMetadata(string key, string value)
            {
                _metadata[key] = value;
            }

            public string GetMetadata(string key)
            {
                return _metadata.ContainsKey(key) ? _metadata[key] : null;
            }

            public void SaveUserAttribute(string key, string value)
            {
                _userAttributes[key] = value;
            }

            public string GetUserAttribute(string key)
            {
                return _userAttributes.ContainsKey(key) ? _userAttributes[key] : null;
            }

            public void SaveUserAttributes(Dictionary<string, string> attributes)
            {
                foreach (var kvp in attributes)
                {
                    _userAttributes[kvp.Key] = kvp.Value;
                }
            }

            public Dictionary<string, string> GetAllUserAttributes()
            {
                return new Dictionary<string, string>(_userAttributes);
            }
        }

        private class FakeFetchClient : IFetchClient
        {
            public event Action<Dictionary<string, Feature>> OnDataFetched;
            public event Action<Exception> OnFetchError;

            public int FetchCallCount { get; private set; }
            public Dictionary<string, Feature> NextResult { get; set; } = new Dictionary<string, Feature>();

            public void StartPeriodicFetch(int intervalSeconds)
            {
            }

            public void StopPeriodicFetch()
            {
            }

            public Task<Dictionary<string, Feature>> FetchDataAsync()
            {
                FetchCallCount++;
                OnDataFetched?.Invoke(NextResult);
                return Task.FromResult(NextResult);
            }

            public void Dispose()
            {
            }
        }
    }
}
