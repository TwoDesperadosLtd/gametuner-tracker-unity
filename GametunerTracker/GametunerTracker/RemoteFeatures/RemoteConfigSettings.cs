namespace GametunerTracker.RemoteConfig
{
    public class RemoteConfigSettings
    {
        public int CacheTimeSeconds { get; set; } = 3600; // 1 hour default
        public int FetchTimeoutSeconds { get; set; } = 30;
        public int PeriodicFetchIntervalSeconds { get; set; } = 300; // 5 minutes default
        public string ServerUrl { get; set; } = "http://localhost:8081/api/v1/{0}/{1}/features";
        public bool UseDotNetHttpClient { get; set; } = false;
    }
}
