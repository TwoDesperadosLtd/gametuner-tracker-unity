using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using LiteDB;
using GametunerTracker.Logging;

namespace GametunerTracker.RemoteConfig
{
    internal class ConfigStore: IStore
    {
        private readonly string dbPath;

        protected readonly LiteDatabase _db;
        protected readonly ReaderWriterLockSlim _dbLock = new ReaderWriterLockSlim();
        protected const string FEATURES_COLLECTION_NAME = "features";
        protected const string METADATA_COLLECTION_NAME = "metadata";
        protected const string USER_ATTRIBUTES_COLLECTION_NAME = "user_attributes";


        public ConfigStore(string filename = "gametuner_features_lite.db", bool saveUnityInPersistentDataPath = true)
        {
            var filePath = filename;
            if (saveUnityInPersistentDataPath)
            {
                dbPath = $"{Application.persistentDataPath}/{filename}";
            }

            try
            {
                _dbLock.EnterWriteLock();
                //Exclusive mode required for iOS
                FileMode fileMode = Application.platform == RuntimePlatform.IPhonePlayer ||
                    Application.platform == RuntimePlatform.OSXPlayer
                    ? FileMode.Exclusive
                    : FileMode.Shared;


                _db = new LiteDatabase(
                        new ConnectionString(dbPath)
                        {
                            Mode = fileMode
                        });

                var col = _db.GetCollection<Feature>(FEATURES_COLLECTION_NAME);
                col.EnsureIndex("Key");
            }
            catch (Exception e)
            {
                Log.Error("Feature Store: Failed to create");
                Log.Error(e.ToString());
                throw;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        public void SaveFeature(Feature feature)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var col = _db.GetCollection<Feature>(FEATURES_COLLECTION_NAME);

                if (feature != null)
                {
                    col.Upsert(feature);
                    Log.Verbose("ConfigStore: Feature " + feature.Key + " added");
                }
                else
                {
                    Log.Error("ConfigStore: Feature is null, cannot add");
                }
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: Feature add failed");
                Log.Error(e.ToString());
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        public Feature GetFeature(string key)
        {
            try
            {
                _dbLock.EnterReadLock();
                var col = _db.GetCollection<Feature>(FEATURES_COLLECTION_NAME);
                return col.FindOne(f => f.Key == key);
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: GetFeature failed");
                Log.Error(e.ToString());
                return null;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        public Dictionary<string, Feature> GetAllFeatures()
        {
            try
            {
                _dbLock.EnterReadLock();
                var col = _db.GetCollection<Feature>(FEATURES_COLLECTION_NAME);
                var features = new Dictionary<string, Feature>();
                foreach (var feature in col.FindAll())
                {
                    features[feature.Key] = feature;
                }
                return features;
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: GetAllFeatures failed");
                Log.Error(e.ToString());
                return new Dictionary<string, Feature>();
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        public void SaveFeatures(Dictionary<string, Feature> features)
        {
            try
            {
                _dbLock.EnterWriteLock();
                var col = _db.GetCollection<Feature>(FEATURES_COLLECTION_NAME);
                foreach (var feature in features.Values)
                {
                    if (feature != null)
                    {
                        col.Upsert(feature);
                    }
                }
                Log.Verbose("ConfigStore: Features saved");
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: SaveFeatures failed");
                Log.Error(e.ToString());
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        public void SaveMetadata(string key, string value)
        {
            try
            {
                _dbLock.EnterWriteLock();
                var col = _db.GetCollection<FeaturesMetaData>(METADATA_COLLECTION_NAME);
                var doc = new FeaturesMetaData { Key = key, Value = value };
                col.Upsert(doc);
                Log.Verbose($"ConfigStore: Metadata {key} saved");
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: SaveMetadata failed");
                Log.Error(e.ToString());
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        public string GetMetadata(string key)
        {
            try
            {
                _dbLock.EnterReadLock();
                var col = _db.GetCollection<FeaturesMetaData>(METADATA_COLLECTION_NAME);
                var doc = col.FindOne(x => x.Key == key);
                return doc != null ? doc.Value : null;
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: GetMetadata failed");
                Log.Error(e.ToString());
                return null;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        public void SaveUserAttribute(string key, string value)
        {
            try
            {
                _dbLock.EnterWriteLock();
                var col = _db.GetCollection<UserAttribute>(USER_ATTRIBUTES_COLLECTION_NAME);
                var doc = new UserAttribute { Key = key, Value = value };
                col.Upsert(doc);
                Log.Verbose($"ConfigStore: UserAttribute {key} saved");
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: SaveUserAttribute failed");
                Log.Error(e.ToString());
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        public string GetUserAttribute(string key)
        {
            try
            {
                _dbLock.EnterReadLock();
                var col = _db.GetCollection<UserAttribute>(USER_ATTRIBUTES_COLLECTION_NAME);
                var doc = col.FindOne(x => x.Key == key);
                return doc != null ? doc.Value : null;
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: GetUserAttribute failed");
                Log.Error(e.ToString());
                return null;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        public void SaveUserAttributes(Dictionary<string, string> attributes)
        {
            try
            {
                _dbLock.EnterWriteLock();
                var col = _db.GetCollection<UserAttribute>(USER_ATTRIBUTES_COLLECTION_NAME);
                foreach (var kvp in attributes)
                {
                    var doc = new UserAttribute { Key = kvp.Key, Value = kvp.Value };
                    col.Upsert(doc);
                }
                Log.Verbose("ConfigStore: UserAttributes saved");
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: SaveUserAttributes failed");
                Log.Error(e.ToString());
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        public Dictionary<string, string> GetAllUserAttributes()
        {
            try
            {
                _dbLock.EnterReadLock();
                var col = _db.GetCollection<UserAttribute>(USER_ATTRIBUTES_COLLECTION_NAME);
                var dict = new Dictionary<string, string>();
                foreach (var doc in col.FindAll())
                {
                    dict[doc.Key] = doc.Value;
                }
                return dict;
            }
            catch (Exception e)
            {
                Log.Error("ConfigStore: GetAllUserAttributes failed");
                Log.Error(e.ToString());
                return new Dictionary<string, string>();
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }
    }
}