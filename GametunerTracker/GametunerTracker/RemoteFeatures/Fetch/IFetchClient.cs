using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GametunerTracker.RemoteConfig
{
    public interface IFetchClient
    {
        event Action<Dictionary<string, Feature>> OnDataFetched;
        event Action<Exception> OnFetchError;
        void StartPeriodicFetch(int intervalSeconds);
        void StopPeriodicFetch();
        Task<Dictionary<string, Feature>> FetchDataAsync();
        void Dispose();
    }
}