using System.Collections.Generic;

namespace GametunerTracker.RemoteConfig
{
    public interface IStore
    {
        void SaveFeature(Feature feature);
        Feature GetFeature(string key);
        Dictionary<string, Feature> GetAllFeatures();
        void SaveFeatures(Dictionary<string, Feature> features);
        void SaveMetadata(string key, string value);
        string GetMetadata(string key);
        void SaveUserAttribute(string key, string value);
        string GetUserAttribute(string key);
        void SaveUserAttributes(Dictionary<string, string> attributes);
        Dictionary<string, string> GetAllUserAttributes();
    }
}