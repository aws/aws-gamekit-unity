// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// GameKit
using AWS.GameKit.Runtime.Core;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class SessionManagerTests : GameKitTestBase
    {
        [Test]
        public void DoesConfigFileExist_PartOfPathMissing_DoesNotRaiseException()
        {
#if UNITY_EDITOR
            // arrange
            SessionManager sessionManager = SessionManager.Get();

            // act / assert
            Assert.DoesNotThrow(() =>
            {
                sessionManager.DoesConfigFileExist("nonExistentGameAlias", "dev");
            });
#endif
        }

        [Test]
        public void DoesConfigFileExist_PartOfPathMissing_DoesNotLogError()
        {
#if UNITY_EDITOR
            // arrange
            SessionManager sessionManager = SessionManager.Get();
            int expectedLogCount = 0;

            // act
            sessionManager.DoesConfigFileExist("nonExistentGameAlias", "dev");

            // assert
            Assert.AreEqual(expectedLogCount, Log.Count);
#endif
        }

        [Test]
        public void DoesConfigFileExist_PartOfPathMissing_ReturnsFalse()
        {
#if UNITY_EDITOR
            // arrange
            SessionManager sessionManager = SessionManager.Get();

            // act
            bool result = sessionManager.DoesConfigFileExist("nonExistentGameAlias", "dev");

            // assert
            Assert.False(result);
#endif
        }
    }
}
