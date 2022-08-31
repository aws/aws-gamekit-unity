// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// Third Party
using NUnit.Framework;

// Unity
using UnityEngine;

// Game Kit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class LoggingTests
    {
        private const int MAX_LOG_QUEUE_SIZE_FOR_TEST = 100;

        private const string INFO_LOG = "This is a test of Unity INFO Logging";
        private const string WARNING_LOG = "This is a test of Unity WARNING Logging";
        private const string ERROR_LOG = "This is a test of Unity ERROR Logging";
        private const string EXCEPTION_LOG = "This is a test of Unity EXCEPTION Logging";

        private static int _currentQueueSize = Logging.MaxLogQueueSize;
        private static bool _currentUnityLoggingEnabled = Logging.IsUnityLoggingEnabled;
        private static Logging.Level _currentLoggingLevel = Logging.MinimumUnityLoggingLevel;

        [SetUp]
        public void SetUp()
        {
            Logging.IsUnityLoggingEnabled = true;
            Logging.MinimumUnityLoggingLevel = Logging.Level.INFO;
            Logging.MaxLogQueueSize = MAX_LOG_QUEUE_SIZE_FOR_TEST;
            Logging.ClearLogQueue();
        }

        [TearDown]
        public void TearDown()
        {
            Logging.ClearLogQueue();
            Logging.MaxLogQueueSize = _currentQueueSize;
            Logging.MinimumUnityLoggingLevel = _currentLoggingLevel;
            Logging.IsUnityLoggingEnabled = _currentUnityLoggingEnabled;
        }

        [Test]
        public void LoggingCallback_WhenMinimumUnityLoggingLevelIsSetToLowestLevel_AllLogsAreSeen()
        {
            // act
            Logging.LogInfo(INFO_LOG);
            Logging.LogWarning(WARNING_LOG);
            Logging.LogError(ERROR_LOG);

            // assert
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, $"AWS GameKit: {INFO_LOG}");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, $"AWS GameKit: {WARNING_LOG}");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"AWS GameKit: {ERROR_LOG}");
        }

        [Test]
        public void LoggingCallback_WhenMinimumUnityLoggingLevelIsSetToHighestLevel_OnlyErrorLogsAreSeen()
        {
            // act
            Logging.LogInfo(INFO_LOG);
            Logging.LogWarning(WARNING_LOG);
            Logging.LogError(ERROR_LOG);

            // assert
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"AWS GameKit: {ERROR_LOG}");
        }

        [Test]
        public void LoggingCallback_WhenUnityLoggingIsNotEnabled_OnlyLogToLoggingQueue()
        {
            // arrange
            Logging.IsUnityLoggingEnabled = false;

            // act
            Logging.LogError(ERROR_LOG);

            // assert

            // No Unity logs should be expected, this test will fail if there are any

            Assert.AreEqual(1, Logging.LogQueue.Count);

            LinkedList<string> logs = new LinkedList<string>(Logging.LogQueue);
            Assert.IsTrue(logs.First.Value.Contains(ERROR_LOG));
        }

        [Test]
        public void LoggingCallback_WhenMaxLogQueueSizeIsReached_TheOldestLogIsRemoved()
        {
            // arrange
            const int EXPECT_LOGS = 2;
            Logging.MaxLogQueueSize = EXPECT_LOGS;

            // act 
            Logging.LogInfo(INFO_LOG);
            Logging.LogWarning(WARNING_LOG);
            Logging.LogError(ERROR_LOG);

            // assert
            UnityEngine.TestTools.LogAssert.Expect(LogType.Log, $"AWS GameKit: {INFO_LOG}");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, $"AWS GameKit: {WARNING_LOG}");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"AWS GameKit: {ERROR_LOG}");

            Assert.AreEqual(EXPECT_LOGS, Logging.LogQueue.Count);

            LinkedList<string> logs = new LinkedList<string>(Logging.LogQueue);
            Assert.IsTrue(logs.First.Value.Contains(WARNING_LOG));
            Assert.IsTrue(logs.Last.Value.Contains(ERROR_LOG));
        }

        [Test]
        public void LogException_WhenEverythingIsEnabled_BothALogQueueEntryAndUnityLogIsSeen()
        {
            // arrange
            Exception e = new Exception(EXCEPTION_LOG);

            // act 
            Logging.LogException("Testing the log queue for an exception", e);

            // assert
            Regex regex = new Regex(EXCEPTION_LOG);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, regex);

            LinkedList<string> logs = new LinkedList<string>(Logging.LogQueue);
            Assert.IsTrue(logs.First.Value.Contains(EXCEPTION_LOG));
        }

        [Test]
        public void LogException_WhenUnityLoggingIsDisabled_OnlyALogQueueEntryIsSeen()
        {
            // arrange
            Logging.IsUnityLoggingEnabled = false;

            Exception e = new Exception(EXCEPTION_LOG);

            // act 
            Logging.LogException("Testing the log queue for an exception", e);

            // assert
            LinkedList<string> logs = new LinkedList<string>(Logging.LogQueue);
            Assert.IsTrue(logs.First.Value.Contains(EXCEPTION_LOG));
        }
    }
}
