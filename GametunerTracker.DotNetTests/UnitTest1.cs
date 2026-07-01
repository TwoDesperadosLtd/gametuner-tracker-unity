using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GametunerTracker.RemoteConfig;

namespace GametunerTracker.DotNetTests;

public class RemoteConfigClientTests
{
    // [Test]
    // public async Task FetchDataAsync_WithDotNetHttpClient_PullsFeaturesFromServer()
    // {
    //     Logging.Log.UseConsoleLogger();
    //     var store = new InMemoryStore();
    //     store.SaveMetadata("last_fetch_time", DateTime.UtcNow.AddHours(-2).ToString("o"));
    //     var settings = new RemoteConfigSettings
    //     {
    //         UseDotNetHttpClient = true,
    //         CacheTimeSeconds = 0,
    //         PeriodicFetchIntervalSeconds = 86400
    //     };

    //     var client = new RemoteConfigClient();
    //     client.Init(
    //         "nono",
    //         "user_123",
    //         new Dictionary<string, Feature>(),
    //         settings,
    //         false,
    //         store);

    //     await client.FetchDataAsync();

    //     var feature = client.GetFeature("feature1");
    //     Assert.That(feature, Is.Not.Null);
    //     Assert.That(feature!.IsOn, Is.True);
    //     Assert.That(feature.Source, Is.EqualTo("experiment"));
    //     Assert.That(feature.Value, Is.EqualTo("{\"level\":\"g2\"}"));

    //     client.Shutdown();
    // }

    [Test]
    public void Init_UsesDefaultFeatures_WhenStoreIsEmpty()
    {
        var store = new InMemoryStore();
        store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
        var defaults = new Dictionary<string, Feature>
        {
            { "feature_1", new Feature { Key = "feature_1", Value = "default" } }
        };

        var client = new RemoteConfigClient();
        client.Init("app-id", "user-id", defaults, new RemoteConfigSettings(), false, store, new FakeFetchClient());

        Assert.That(client.GetFeature("feature_1")?.Value, Is.EqualTo("default"));
        client.Shutdown();
    }

    [Test]
    public void Init_UsesCachedFeatures_WhenStoreHasData()
    {
        var store = new InMemoryStore();
        store.SaveFeature(new Feature { Key = "feature_1", Value = "cached" });
        store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));

        var client = new RemoteConfigClient();
        client.Init("app-id", "user-id", new Dictionary<string, Feature>(), new RemoteConfigSettings(), false, store, new FakeFetchClient());

        Assert.That(client.GetFeature("feature_1")?.Value, Is.EqualTo("cached"));
        client.Shutdown();
    }

    [Test]
    public async Task FetchDataAsync_Skips_WhenCacheIsFresh()
    {
        var store = new InMemoryStore();
        store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
        var fetchClient = new FakeFetchClient();
        var client = BuildClient(store, fetchClient);

        await client.FetchDataAsync();

        Assert.That(fetchClient.FetchCallCount, Is.EqualTo(0));
        client.Shutdown();
    }

    [Test]
    public async Task FetchDataAsync_FetchesAndUpdatesCache_WhenCacheIsStale()
    {
        var store = new InMemoryStore();
        store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
        var fetchClient = new FakeFetchClient
        {
            NextResult = new Dictionary<string, Feature>
            {
                { "feature_2", new Feature { Key = "feature_2", Value = "{\"level\":21}", IsOn = true, Source = "defaultValue" } }
            }
        };
        var client = BuildClient(store, fetchClient);
        store.SaveMetadata("last_fetch_time", DateTime.UtcNow.AddHours(-2).ToString("o"));

        await client.FetchDataAsync();

        Assert.That(fetchClient.FetchCallCount, Is.EqualTo(1));
        Assert.That(client.GetFeature("feature_2")?.Value, Is.EqualTo("{\"level\":21}"));
        Assert.That(client.GetFeature("feature_2")?.IsOn, Is.True);
        client.Shutdown();
    }

    [Test]
    public void UserAttributes_RoundTrip_AfterInit()
    {
        var store = new InMemoryStore();
        store.SaveMetadata("last_fetch_time", DateTime.UtcNow.ToString("o"));
        var client = BuildClient(store, new FakeFetchClient());
        var attributes = new Dictionary<string, string>
        {
            { "country", "US" },
            { "tier", "A" }
        };

        client.SetUserAttributes(attributes);
        var actual = client.GetUserAttributes();

        Assert.That(actual["country"], Is.EqualTo("US"));
        Assert.That(actual["tier"], Is.EqualTo("A"));
        client.Shutdown();
    }

    private static RemoteConfigClient BuildClient(InMemoryStore store, FakeFetchClient fetchClient)
    {
        var client = new RemoteConfigClient();
        client.Init(
            "app-id",
            "user-id",
            new Dictionary<string, Feature>(),
            new RemoteConfigSettings { CacheTimeSeconds = 3600, PeriodicFetchIntervalSeconds = 3600 },
            false,
            store,
            fetchClient);
        return client;
    }

    private class InMemoryStore : IStore
    {
        private readonly Dictionary<string, Feature> _features = new();
        private readonly Dictionary<string, string> _metadata = new();
        private readonly Dictionary<string, string> _userAttributes = new();

        public void SaveFeature(Feature feature)
        {
            if (feature != null)
            {
                _features[feature.Key] = feature;
            }
        }

        public Feature GetFeature(string key)
        {
            return _features.TryGetValue(key, out var feature) ? feature : null;
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
            return _metadata.TryGetValue(key, out var value) ? value : null;
        }

        public void SaveUserAttribute(string key, string value)
        {
            _userAttributes[key] = value;
        }

        public string GetUserAttribute(string key)
        {
            return _userAttributes.TryGetValue(key, out var value) ? value : null;
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
        public Dictionary<string, Feature> NextResult { get; set; } = new();

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

    private sealed class LocalFeatureServer : IDisposable
    {
        private readonly HttpListener _listener;
        private readonly string _responseJson;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _serveTask;

        public string BaseUrl { get; }
        public int RequestCount { get; private set; }

        public LocalFeatureServer(string responseJson)
        {
            _responseJson = responseJson;
            var port = FindFreePort();
            BaseUrl = $"http://127.0.0.1:{port}/";

            _listener = new HttpListener();
            _listener.Prefixes.Add(BaseUrl);
            _listener.Start();

            _serveTask = Task.Run(() => ServeLoopAsync(_cts.Token));
        }

        public void Dispose()
        {
            _cts.Cancel();
            _listener.Close();
            try
            {
                _serveTask.Wait(TimeSpan.FromSeconds(2));
            }
            catch
            {
            }
            _cts.Dispose();
        }

        private async Task ServeLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await _listener.GetContextAsync();
                }
                catch
                {
                    break;
                }

                RequestCount++;

                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                var bytes = Encoding.UTF8.GetBytes(_responseJson);
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken);
                context.Response.OutputStream.Close();

                if (context.Request.InputStream != null)
                {
                    using var reader = new StreamReader(context.Request.InputStream);
                    _ = await reader.ReadToEndAsync();
                }
            }
        }

        private static int FindFreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
