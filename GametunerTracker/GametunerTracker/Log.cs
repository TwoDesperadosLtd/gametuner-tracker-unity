/*
 * Log.cs
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
 * - Change namespace from SnowplowTracker.Logging to GametunerTracker.Logging
 */

using System;

namespace GametunerTracker.Logging {
    public interface ILogClient
    {
        void Error(string message);
        void Debug(string message);
        void Verbose(string message);
    }

    internal class UnityLogClient : ILogClient
    {
        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Verbose(string message)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    internal class ConsoleLogClient : ILogClient
    {
        public void Error(string message)
        {
            Console.Error.WriteLine(message);
        }

        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Verbose(string message)
        {
            Console.WriteLine(message);
        }
    }

    public static class Log
    {
        private const string Prefix = "GAMETUNER: ";
        private static readonly object LogLock = new object();

        private static LoggingLevel level = LoggingLevel.Verbose;
        private static bool logging = true;
        private static ILogClient logClient = new UnityLogClient();

        public static void Error(string message)
        {
            if (logging && (int)level >= (int)LoggingLevel.Error)
            {
                logClient.Error(Prefix + message);
            }
        }

        public static void Debug(string message)
        {
            if (logging && (int)level >= (int)LoggingLevel.Debug)
            {
                logClient.Debug(Prefix + message);
            }
        }

        public static void Verbose(string message)
        {
            if (logging && (int)level >= (int)LoggingLevel.Verbose)
            {
                logClient.Verbose(Prefix + message);
            }
        }

        public static void On()
        {
            logging = true;
        }

        public static void Off()
        {
            logging = false;
        }

        public static void SetLogLevel(LoggingLevel newLevel)
        {
            level = newLevel;
        }

        public static void SetLogClient(ILogClient client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            lock (LogLock)
            {
                logClient = client;
            }
        }

        public static void UseUnityLogger()
        {
            SetLogClient(new UnityLogClient());
        }

        public static void UseConsoleLogger()
        {
            SetLogClient(new ConsoleLogClient());
        }
    }
}
