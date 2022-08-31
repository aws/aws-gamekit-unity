// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Features.GameKitUserGameplayData;
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class UserGameplayDataWrapperTests : GameKitTestBase
    {
        const string CONNECTION_CLIENT = "connection client";

        [Test]
        public void NetworkStatusChangeCallback_StandardCase_CallsSuccessfully()
        {
            UserGameplayDataWrapperTarget.SetNetworkChangedDelegate((NetworkStatusChangeResults result) =>
            {
                Assert.True(result.IsConnectionOk);
                Assert.AreEqual(result.ConnectionClient, CONNECTION_CLIENT);
            });

            // act
            UserGameplayDataWrapperTarget.NetworkStatusChangeCallback(IntPtr.Zero, true, CONNECTION_CLIENT);

            // assertion handled in callback above
        }

        [Test]
        public void CacheProcessedCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            UserGameplayDataWrapperTarget.SetCacheProcessedDelegate((CacheProcessedResults result) =>
            {
                Assert.True(result.IsCacheProcessed);
            });

            // act
            UserGameplayDataWrapperTarget.CacheProcessedCallback(IntPtr.Zero, true);

            // assertion handled in callback above
        }
    }

    public class UserGameplayDataWrapperTarget : UserGameplayDataWrapper
    {
        public static void SetCacheProcessedDelegate(UserGameplayData.CacheProcessedDelegate cacheProcessedDelegate) => UserGameplayDataWrapper._cacheProcessedDelegate = cacheProcessedDelegate;

        public static void SetNetworkChangedDelegate(UserGameplayData.NetworkChangedDelegate networkChangedDelegate) => UserGameplayDataWrapper._networkCallbackDelegate = networkChangedDelegate;

        public new static void NetworkStatusChangeCallback(IntPtr dispatchReceiver, bool isConnectionOk, string connectionClient) => UserGameplayDataWrapper.NetworkStatusChangeCallback(
            dispatchReceiver,
            isConnectionOk,
            connectionClient);

        public new static void CacheProcessedCallback(IntPtr dispatchReceiver, bool isCacheProcessed) => UserGameplayDataWrapper.CacheProcessedCallback(dispatchReceiver, isCacheProcessed);
    }
}
