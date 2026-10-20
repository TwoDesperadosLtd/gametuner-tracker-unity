/*
 * Constants.cs
 * SnowplowTracker
 * 
 * Copyright (c) 2015 Snowplow Analytics Ltd. All rights reserved.
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
 * Authors: Joshua Beemster, Paul Boocock
 * Copyright: Copyright (c) 2015-2019 Snowplow Analytics Ltd
 * License: Apache License Version 2.0
 */

/*
 * Modified by AlgebraAI on 2024-01-31
 * - Added event schemas
 * - Changed namespace of schema
 */

using System;

namespace GametunerTracker {
	internal class Constants {

		// Schemas
		public readonly static string SCHEMA_PAYLOAD_DATA    		= "iglu:com.algebraai.gametuner.common/payload_data/jsonschema/1-0-0";
		public readonly static string SCHEMA_CONTEXTS       	 	= "iglu:com.snowplowanalytics.snowplow/contexts/jsonschema/1-0-1";
		public readonly static string SCHEMA_UNSTRUCT_EVENT  		= "iglu:com.snowplowanalytics.snowplow/unstruct_event/jsonschema/1-0-0";
		public readonly static string SCHEMA_DEVICE_CONTEXT  		= "iglu:com.algebraai.gametuner.embedded_context/device_context/jsonschema/1-0-0";
		public readonly static string SCHEMA_SESSION_CONTEXT 		= "iglu:com.algebraai.gametuner.embedded_context/session_context/jsonschema/1-0-0";
		public readonly static string SCHEMA_EVENT_CONTEXT   		= "iglu:com.algebraai.gametuner.context/event_context/jsonschema/1-0-1";
        public readonly static string EVENT_LOGIN_SCHEMA 	 		= "iglu:com.algebraai.gametuner.event/login/jsonschema/1-0-0";
		public readonly static string EVENT_FIRST_OPEN_SCHEMA 		= "iglu:com.algebraai.gametuner.event/first_open/jsonschema/1-0-1";
		public readonly static string EVENT_LOGOUT_SCHEMA 			= "iglu:com.algebraai.gametuner.event/logout/jsonschema/1-0-0";
		public readonly static string EVENT_PURCHASE_SCHEMA 		= "iglu:com.algebraai.gametuner.event/purchase/jsonschema/1-0-1";
		public readonly static string EVENT_AD_STARTED_SCHEMA 		= "iglu:com.algebraai.gametuner.event/ad_started/jsonschema/1-0-0";
		public readonly static string EVENT_AD_WATCHED_SCHEMA 		= "iglu:com.algebraai.gametuner.event/ad_watched/jsonschema/1-0-0";
		public readonly static string EVENT_CURRENCY_CHANGE_SCHEMA 	= "iglu:com.algebraai.gametuner.event/currency_change/jsonschema/1-0-0";
		public readonly static string EVENT_PURCHASE_INITIATED_SCHEMA 	= "iglu:com.algebraai.gametuner.event/purchase_initiated/jsonschema/1-0-1";
		public readonly static string EVENT_GDPR_DELETE_REQUEST 	= "iglu:com.algebraai.gametuner.event/gdpr_delete_request/jsonschema/1-0-0";

		// Event Types
		public readonly static string EVENT_UNSTRUCTURED    = "ue";

		// Emitter
		public readonly static string GET_URI_SUFFIX        = "/i";
		public readonly static string POST_URI_SUFFIX       = "/com.snowplowanalytics.snowplow/tp2";
		public readonly static string POST_CONTENT_TYPE     = "application/json";

		// Session
		public readonly static string SESSION_ID            = "session_id";
		public readonly static string SESSION_INDEX         = "session_index";
		public readonly static string SESSION_TIME      	= "session_time";

		// Device Context
		public readonly static string DEVICE_CATEGORY       = "device_category";
		public readonly static string DEVICE_MANUFACTURER   = "device_manufacturer";
		public readonly static string DEVICE_MODEL    	 	= "model";
		public readonly static string DEVICE_OS_VERSION     = "os_version";
		public readonly static string DEVICE_CPU_TYPE		= "cpu_type";
		public readonly static string DEVICE_GPU	        = "gpu";
		public readonly static string DEVICE_RAM_SIZE       = "ram_size";
		public readonly static string DEVICE_SCREEN_RES     = "screen_resolution";
		public readonly static string DEVICE_LANGUAGE       = "device_language";
		public readonly static string DEVICE_TIMEZONE       = "device_timezone";
		public readonly static string DEVICE_SOURCE         = "source";
		public readonly static string DEVICE_MEDIUM         = "medium";
		public readonly static string DEVICE_CAMPAIGN       = "campaign";
		public readonly static string DEVICE_BUILD_VERSION  = "build_version";
		public readonly static string DEVICE_DEVICE_ID      = "device_id";
		public readonly static string DEVICE_ADVERTISING_ID = "advertising_id";
		public readonly static string DEVICE_IDFA		    = "idfa";
		public readonly static string DEVICE_IDFV		    = "idfv";
		public readonly static string DEVICE_IS_HACKED	    = "is_hacked";
		public readonly static string DEVICE_STORE			= "store";

		//Event Context
		public readonly static string EVENT_INDEX	 	    = "event_index"; 
		public readonly static string EVENT_PREVIOUS_EVENT  = "previous_event"; 
		public readonly static string EVENT_CONSENT_GIVEN   = "consent_given";
        public readonly static string EVENT_BUNDLE_ID 		= "event_bundle_id";
		public readonly static string EVENT_IS_ONLINE  		= "is_online";

		// Commmon Payload Keys
		public readonly static string SCHEMA                = "schema";
		public readonly static string DATA                  = "data";
		public readonly static string EVENT                 = "e";
		public readonly static string EID                   = "eid";
		public readonly static string TIMESTAMP             = "dtm";
		public readonly static string SENT_TIMESTAMP        = "stm";
		public readonly static string TRACKER_VERSION       = "tv";
		public readonly static string APP_ID                = "aid";
		public readonly static string NAMESPACE             = "tna";
		public readonly static string UID                   = "uid";
		public readonly static string INSTALLATION_ID       = "iid";
		public readonly static string SANDBOX_MODE          = "sm";
		public readonly static string CONTEXT               = "co";
		public readonly static string CONTEXT_ENCODED       = "cx";
		public readonly static string UNSTRUCTURED          = "ue_pr";
		public readonly static string UNSTRUCTURED_ENCODED  = "ue_px";
		public readonly static string API_KEY               = "ak";
		
		// Subject Keys
		public readonly static string PLATFORM              = "p";
		public readonly static string RESOLUTION            = "res";
		public readonly static string VIEWPORT              = "vp";
		public readonly static string COLOR_DEPTH           = "cd";
		public readonly static string TIMEZONE              = "tz";
		public readonly static string LANGUAGE              = "lang";
		public readonly static string IP_ADDRESS            = "ip";
		public readonly static string USERAGENT             = "ua";
		public readonly static string DOMAIN_UID            = "duid";
		public readonly static string NETWORK_UID           = "tnuid";

		// Login launch modes

		public readonly static string LOGIN_LAUNCH_MODE_FROM_BACKGROUND     = "FromBackground";
		public readonly static string LOGIN_LAUNCH_MODE_COLD_START     		= "ColdStart";
		public readonly static string LOGIN_LAUNCH_MODE_SESSION_TIMEOUT     = "SessionTimeout";
		public readonly static string LOGIN_LAUNCH_MODE_RECONNECT     		= "Reconnect";
		public readonly static string LOGIN_LAUNCH_MODE_PUSH_NOTIFICATION   = "PushNotification";

    }
}
