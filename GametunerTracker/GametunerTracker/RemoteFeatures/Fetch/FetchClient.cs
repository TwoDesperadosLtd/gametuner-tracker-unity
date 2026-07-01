using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GametunerTracker.RemoteConfig
{
    public class FetchClient
    {
        protected readonly string _serverUrl;
        protected readonly string _appID;
        protected readonly string _userId;
        protected readonly int _fetchTimeoutSeconds;
        internal readonly IStore _store;
        public event Action<Dictionary<string, Feature>> OnDataFetched;
        public event Action<Exception> OnFetchError;

        internal FetchClient(string serverUrl, string appID, string userId, int fetchTimeoutSeconds, IStore store)
        {
            _serverUrl = serverUrl;
            _appID = appID;
            _userId = userId;
            _fetchTimeoutSeconds = fetchTimeoutSeconds;
            _store = store;
        }
    }
}