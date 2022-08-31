// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class GameKitTestBase
    {
        public List<string> Log;

        [SetUp]
        public void LoggingSetUp()
        {
            Log = new List<string>();
            Logging.LogCb = (level, message, size) => { Log.Add(message); };
            Logging.IsUnityLoggingEnabled = false;
            Logging.ClearLogQueue();
        }

        [TearDown]
        public void LoggingTearDown()
        {
            Logging.ClearLogQueue();
            Logging.IsUnityLoggingEnabled = true;
            Logging.LogCb = Logging.DefaultLogCb;
        }
    }
}
