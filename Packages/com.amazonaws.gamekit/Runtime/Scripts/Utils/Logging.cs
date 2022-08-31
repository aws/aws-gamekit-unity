// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Runtime.Utils
{
    public static class Logging
    {
        private const int DEFAULT_MAX_LOG_QUEUE_SIZE = 1000;
        private const bool DEFAULT_IS_UNITY_LOGGING_ENABLED = true;
        private const Level DEFAULT_MINIMUM_UNITY_LOGGING_LEVEL = Level.INFO;

        public enum Level : int
        {
            VERBOSE = 1,
            INFO = 2,
            WARNING = 3,
            ERROR = 4,

            // This Level only applies on the Unity plugin side, the AWS GameKit C++ SDK code does not differentiate between ERRORs and EXCEPTIONs
            EXCEPTION = 5
        }

        public static uint InfoLogCount => _infoLogCount;
        public static uint WarningLogCount => _warningLogCount;
        public static uint ErrorLogCount => _errorLogCount;
        public static IReadOnlyCollection<string> LogQueue => _logQueue;

        public static bool IsUnityLoggingEnabled
        {
            get => _isUnityLoggingEnabled;
            set 
            {
                _isUnityLoggingEnabled = value;
                AwsGameKitPersistentSettings.SaveBool(nameof(IsUnityLoggingEnabled), value);
            }
        }
        
        public static Level MinimumUnityLoggingLevel
        {
            get => _minimumUnityLoggingLevel;
            set
            {
                _minimumUnityLoggingLevel = value;
                AwsGameKitPersistentSettings.SaveInt(nameof(MinimumUnityLoggingLevel), (int)value);
            }
        }

        public static int MaxLogQueueSize
        {
            get => _maxLogQueueSize;
            set
            {
                _maxLogQueueSize = value;
                AwsGameKitPersistentSettings.SaveInt(nameof(MaxLogQueueSize), value);
            }
        }

        public static FuncLoggingCallback DefaultLogCb => LoggingCallback;

        public static FuncLoggingCallback LogCb 
        {
            get => _logCb;
            set => _logCb = value; 
        }

        private static bool _isUnityLoggingEnabled;
        private static Level _minimumUnityLoggingLevel;
        private static int _maxLogQueueSize;
        private static FuncLoggingCallback _logCb = DefaultLogCb;
        private static uint _infoLogCount = 0;
        private static uint _warningLogCount = 0;
        private static uint _errorLogCount = 0;

        private static ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();

        static Logging()
        {
            // Default logging level to error when outside editor, override by changing log level within game.
#if !UNITY_EDITOR
            MinimumUnityLoggingLevel = Level.ERROR;
#endif
#if DEBUG
            MinimumUnityLoggingLevel = Level.INFO;
#endif
            IsUnityLoggingEnabled = AwsGameKitPersistentSettings.LoadBool(nameof(IsUnityLoggingEnabled), DEFAULT_IS_UNITY_LOGGING_ENABLED);
            MinimumUnityLoggingLevel = (Level)AwsGameKitPersistentSettings.LoadInt(nameof(MinimumUnityLoggingLevel), (int)DEFAULT_MINIMUM_UNITY_LOGGING_LEVEL);
            MaxLogQueueSize = AwsGameKitPersistentSettings.LoadInt(nameof(MaxLogQueueSize), DEFAULT_MAX_LOG_QUEUE_SIZE);
        }

        public static void ClearLogQueue()
        {
            _logQueue = new ConcurrentQueue<string>();

            _infoLogCount = 0;
            _warningLogCount = 0;
            _errorLogCount = 0;
        }
        
        public static void Log(Level level, object message)
        {
            string messageAsString = message.ToString();

            _logCb((uint)level, messageAsString, messageAsString.Length);
        }

        public static void LogInfo(object message) => Log(Level.INFO, message);
        public static void LogWarning(object message) => Log(Level.WARNING, message);
        public static void LogError(object message) => Log(Level.ERROR, message);

        public static void LogException(object message, Exception e)
        {
            ++_errorLogCount;

            LogInUnity(Level.EXCEPTION, () =>
            {
                Debug.LogException(e);
            });

            AddToLogQueue((uint)Level.EXCEPTION, $"{message.ToString()}, {e}");
        }

        [AOT.MonoPInvokeCallback(typeof(FuncLoggingCallback))]
        private static void LoggingCallback(uint level, string message, int size)
        {
            switch (level)
            {
                case ((uint)Level.VERBOSE):
                    goto default;
                case ((uint)Level.INFO):
                    goto default;
                case ((uint)Level.WARNING):
                    ++_warningLogCount;

                    LogInUnity(Level.WARNING, () =>
                    {
                        Debug.LogWarning($"AWS GameKit: {message}");
                    });

                    break;
                case ((uint)Level.ERROR):
                    ++_errorLogCount;

                    LogInUnity(Level.ERROR, () =>
                    {
                        Debug.LogError($"AWS GameKit: {message}");
                    });

                    break;
                default:
                    ++_infoLogCount;

                    LogInUnity(Level.INFO, () =>
                    {
                        Debug.Log($"AWS GameKit: {message}");
                    });

                    break;
            }

            AddToLogQueue(level, message);
        }

        private static void AddToLogQueue(uint level, string message)
        {
#if UNITY_EDITOR
            // treat VERBOSE level the same as INFO
            level = level == (uint)Level.VERBOSE ? (uint)Level.INFO : level;

            _logQueue.Enqueue($"{DateTime.Now} {((Level)level).ToString()}: {message}");

            if (_logQueue.Count > MaxLogQueueSize)
            {
                string removedLog;
                _logQueue.TryDequeue(out removedLog);

                // subtract from the correct log count
                if (removedLog.Contains(Level.INFO.ToString()))
                {
                    --_infoLogCount;
                }
                else if(removedLog.Contains(Level.WARNING.ToString()))
                {
                    --_warningLogCount;
                }
                else
                {
                    --_errorLogCount;
                }
            }

            // request an update of the editor window anytime we add a new log to our queue
            SettingsWindowUpdateController.RequestUpdate();
#endif
        }

        private static void LogInUnity(Level minimumLevelRequired, Action lambdaForLogging)
        {
            if (IsUnityLoggingEnabled && MinimumUnityLoggingLevel <= minimumLevelRequired)
            {
                lambdaForLogging();
            }
        }
    }
}
