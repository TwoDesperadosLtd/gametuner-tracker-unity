namespace GametunerTracker.RemoteConfig
{
    internal class HttpRequestResult
    {
        public bool IsSuccess { get; set; }
        public string ResponseBody { get; set; }
        public string Error { get; set; }
    }
}
