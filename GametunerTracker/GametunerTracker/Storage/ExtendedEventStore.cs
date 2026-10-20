/*
 * ExtendedEventStore.cs
 * GametunerTracker.Storage
 * 
 * Copyright (c) 2024 AlgebraAI. All rights reserved.
 *
 * This program is licensed to you under the Apache License Version 2.0,
 * and you may not use this file except in compliance with the Apache License Version 2.0.
 * You may obtain a copy of the Apache License Version 2.0 at http://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the Apache License Version 2.0 is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the Apache License Version 2.0 for the specific language governing permissions and limitations there under.
 * 
 * Author: Djordje Smiljanic
 * Copyright: Copyright (c) 2022-2024 AlgebraAI
 * License: Apache License Version 2.0
 */

using System;
using GametunerTracker.Logging;

namespace GametunerTracker.Storage
{
    /// <summary>
    /// Extension of the EventStore to store additional data localy.
    /// </summary>
    internal class ExtendedEventStore : EventStore
    {

        public class EventsMetaData { 
            public string Id { get; set; }
            public string ValueString { get; set; }
            public int ValueInt { get; set; }
        }

        private const string COLLECTION_METADATA                        = "eventsData";
        private const string COLLECTION_METADATA_LAST_ADDED_EVENT       = "lastAddedEventName";
        private const string COLLECTION_METADATA_LAST_BUNDLE_ID         = "lastTransactionId";
        private const string COLLECTION_METADATA_EVENT_INDEX            = "eventIndex";
        private const string COLLECTION_METADATA_USER_ID                = "userId";
        private const string COLLECTION_METADATA_INSTALLATION_ID        = "installationId";
        private const string COLLECTION_METADATA_REGISTRATION_TIME      = "registrationTime";
        private const string COLLECTION_METADATA_OPT_OUT                = "optOut";
        private const string COLLECTION_METADATA_CONSENT                = "consent";

        public ExtendedEventStore(string filename = "snowplow_events_lite.db") : base(filename) { 
            try
            {
                _dbLock.EnterWriteLock();

                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);
                colData.EnsureIndex("Key");
            }
            catch (Exception e)
            {
                Log.Error("Event Store: Failed to create metadata table");
                Log.Error(e.ToString());
                throw;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the last event name.
        /// </summary>
        /// <returns>Event name</returns>
        public string GetLastAddedEvent() { 
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_LAST_ADDED_EVENT);
                if (result == null) {
                    return string.Empty;
                }

                return result.ValueString;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get last added event failed");
                Log.Error(e.ToString());
                return string.Empty;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Update name of last added event.
        /// </summary>
        /// <param name="eventName">Event name to update</param>
        /// <returns>Is event name updated</returns>
        public bool UpdateLastAddedEvent(string eventName)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);
                EventsMetaData metaDataEventName = new EventsMetaData { Id = COLLECTION_METADATA_LAST_ADDED_EVENT, ValueString = eventName };

                if (!colData.Update(metaDataEventName)) {
                    colData.Insert(metaDataEventName);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: Last event failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets and update the event index
        /// </summary>
        /// <returns>Event index</returns>
        public int GetAndUpdateEventIndex() { 
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);
                int lastEventIndex = 0;
                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_EVENT_INDEX);
                if (result != null) {
                    lastEventIndex = result.ValueInt;
                }
                
                EventsMetaData metaDataIndex = new EventsMetaData { Id = COLLECTION_METADATA_EVENT_INDEX, ValueInt = (lastEventIndex + 1)};

                if (!colData.Update(metaDataIndex)) {
                    colData.Insert(metaDataIndex);
                }

                return lastEventIndex;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get event index failed");
                Log.Error(e.ToString());
                return 0;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets last bundle id.
        /// </summary>
        /// <returns>ID of last bundle</returns>
        public int GetLastBundleId() { 
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_LAST_BUNDLE_ID);
                if (result == null) {
                    return 0;
                }

                return result.ValueInt;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get last bundel event failed");
                Log.Error(e.ToString());
                return 0;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Update last bundel id.
        /// </summary>
        /// <returns>Is last bundle updated</returns>
        public bool UpdateLastBundleId()
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_LAST_BUNDLE_ID);
                EventsMetaData metadata = new EventsMetaData { Id = COLLECTION_METADATA_LAST_BUNDLE_ID, ValueInt = 0 };

                if (result == null) { 
                    colData.Insert(metadata);
                } else {
                    int bundleID = result.ValueInt + 1;
                    metadata.ValueInt = bundleID;
                    colData.Update(metadata);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: Last bundle failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets user id from cache.
        /// </summary>
        /// <returns>Cached userID</returns>
        public string GetCacheUserId() { 
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_USER_ID);
                if (result == null) {
                    return string.Empty;
                }

                return result.ValueString;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get user id failed");
                Log.Error(e.ToString());
                return string.Empty;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets installation id from cache.
        /// </summary>
        /// <returns>Cached installation id</returns>
        public string GetInstallationId() { 
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_INSTALLATION_ID);
                if (result == null) {
                    return string.Empty;
                }

                return result.ValueString;
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get installation id failed");
                Log.Error(e.ToString());
                return string.Empty;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }        

        /// <summary>
        /// Update user id.
        /// </summary>
        /// <returns>Is user id updated</returns>
        public bool UpdateUserId(string userID)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_USER_ID);
                EventsMetaData metadata = new EventsMetaData { Id = COLLECTION_METADATA_USER_ID, ValueString = userID };

                if (result == null) { 
                    colData.Insert(metadata);
                } else {
                    colData.Update(metadata);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: UserID failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Update installation id.
        /// </summary>
        /// <returns>Is installation id updated</returns>
        public bool UpdateInstallationId(string installationId)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_INSTALLATION_ID);
                EventsMetaData metadata = new EventsMetaData { Id = COLLECTION_METADATA_INSTALLATION_ID, ValueString = installationId };

                if (result == null) { 
                    colData.Insert(metadata);
                } else {
                    colData.Update(metadata);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: Installation id failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }        

        public bool UpdateEvent(EventRow eventRow)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<Event>(COLLECTION_NAME);

                var result = colData.FindOne(x => x.Id == eventRow.GetRowId());

                if (result != null) { 
                    result.Payload = eventRow.GetPayload().ToString();
                    colData.Update(result);
                } 

                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: UserID failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        //COLLECTION_METADATA_REGISTRATION_TIME
        /// <summary>
        /// Gets registration time.
        /// </summary>
        /// <returns>Registration time</returns>
        public long GetFirstOpenTime() { 
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_REGISTRATION_TIME);
                if (result == null) {
                    return 0L;
                }

                return Convert.ToInt64(result.ValueString);
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get first open time failed");
                Log.Error(e.ToString());
                return 0L;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Sets registration time.
        /// </summary>
        /// <returns>Is registration time updated</returns>
        public bool SetFirstOpenTime(long firstOpenTime)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_REGISTRATION_TIME);
                EventsMetaData metadata = new EventsMetaData { Id = COLLECTION_METADATA_REGISTRATION_TIME, ValueString = firstOpenTime.ToString() };

                if (result == null) { 
                    colData.Insert(metadata);
                } else {
                    colData.Update(metadata);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: FirstOpenTime failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets optout option.
        /// </summary>
        /// <returns>Optout option</returns>
        public bool GetOptOut() { 
            try
            {
                _dbLock.EnterReadLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_OPT_OUT);
                if (result == null) {
                    return false;
                }

                return Convert.ToBoolean(result.ValueInt);
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get user id failed");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Update optout option.
        /// </summary>
        /// <returns>Is optout updated</returns>
        public bool SetOptOut(bool isUserOptOut)
        {
            try
            {
                _dbLock.EnterWriteLock();
                // Get event collection
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_OPT_OUT);
                EventsMetaData metadata = new EventsMetaData { Id = COLLECTION_METADATA_OPT_OUT, ValueInt = Convert.ToInt32(isUserOptOut) };

                if (result == null) { 
                    colData.Insert(metadata);
                } else {
                    colData.Update(metadata);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: UserID failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets terms and conditions consent.
        /// </summary>
        /// <returns>Consent, null if it was never set</returns>
        public bool? GetConsent() { 
            try
            {
                _dbLock.EnterReadLock();
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_CONSENT);
                if (result == null) {
                    return null;
                }

                return Convert.ToBoolean(result.ValueInt);
            }
            catch (Exception e)
            {
                Log.Error($"EventStore: Get consent failed");
                Log.Error(e.ToString());
                return null;
            }
            finally
            {
                _dbLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Update terms and conditions consent.
        /// </summary>
        /// <returns>Is consent updated</returns>
        public bool SetConsent(bool consentGiven)
        {
            try
            {
                _dbLock.EnterWriteLock();
                var colData = _db.GetCollection<EventsMetaData>(COLLECTION_METADATA);

                var result = colData.FindOne(x => x.Id == COLLECTION_METADATA_CONSENT);
                EventsMetaData metadata = new EventsMetaData { Id = COLLECTION_METADATA_CONSENT, ValueInt = Convert.ToInt32(consentGiven) };

                if (result == null) { 
                    colData.Insert(metadata);
                } else {
                    colData.Update(metadata);
                }
               
                return true;
            }
            catch (Exception e)
            {
                Log.Error("EventStore: Consent failed to save");
                Log.Error(e.ToString());
                return false;
            }
            finally
            {
                _dbLock.ExitWriteLock();
            }
        }
    }
}
