// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Features.GameKitUserGameplayData
{
    /// <summary>
    /// User Gameplay Data wrapper for GameKit C++ SDK calls
    /// </summary>
    public class UserGameplayDataWrapper : GameKitFeatureWrapperBase
    {
        // Select the correct source path based on the platform
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string IMPORT = "__Internal";
#else
        private const string IMPORT = "aws-gamekit-user-gameplay-data";
#endif

        // Delegates
        protected static UserGameplayData.NetworkChangedDelegate _networkCallbackDelegate;
        protected static UserGameplayData.CacheProcessedDelegate _cacheProcessedDelegate;

        // Network state
        protected static bool _isNetworkHealthy = true;

        // DLL loading   
        [DllImport(IMPORT)] private static extern IntPtr GameKitUserGameplayDataInstanceCreateWithSessionManager(IntPtr sessionManager, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitUserGameplayDataInstanceRelease(IntPtr userGameplayDataInstance);
        [DllImport(IMPORT)] private static extern void GameKitSetUserGameplayDataClientSettings(IntPtr userGameplayDataInstance, UserGameplayDataClientSettings settings);
        [DllImport(IMPORT)] private static extern uint GameKitAddUserGameplayData(IntPtr userGameplayDataInstance, UserGameplayDataBundle userGameplayDataBundle, IntPtr unprocessedItemsReceiver, FuncKeyValueStringCallback unprocessedItemsCallback);
        [DllImport(IMPORT)] private static extern uint GameKitListUserGameplayDataBundles(IntPtr userGameplayDataInstance, IntPtr dispatchReceiver, FuncStringCallback responseCb);
        [DllImport(IMPORT)] private static extern uint GameKitGetUserGameplayDataBundle(IntPtr userGameplayDataInstance, string bundleName, IntPtr dispatchReceiver, FuncKeyValueStringCallback responseCb);
        [DllImport(IMPORT)] private static extern uint GameKitGetUserGameplayDataBundleItem(IntPtr userGameplayDataInstance, UserGameplayDataBundleItem userGameplayDataBundleItem, IntPtr dispatchReceiver, FuncStringCallback responseCb);
        [DllImport(IMPORT)] private static extern uint GameKitUpdateUserGameplayDataBundleItem(IntPtr userGameplayDataInstance, UserGameplayDataBundleItemValue userGameplayDataBundleItemValue);
        [DllImport(IMPORT)] private static extern uint GameKitDeleteAllUserGameplayData(IntPtr userGameplayDataInstance);
        [DllImport(IMPORT)] private static extern uint GameKitDeleteUserGameplayDataBundle(IntPtr userGameplayDataInstance, string bundleName);
        [DllImport(IMPORT)] private static extern uint GameKitDeleteUserGameplayDataBundleItems(IntPtr userGameplayDataInstance, UserGameplayDataDeleteItemsRequest deleteItemsRequest);
        [DllImport(IMPORT)] private static extern void GameKitUserGameplayDataStartRetryBackgroundThread(IntPtr userGameplayDataInstance);
        [DllImport(IMPORT)] private static extern void GameKitUserGameplayDataStopRetryBackgroundThread(IntPtr userGameplayDataInstance);
        [DllImport(IMPORT)] private static extern void GameKitUserGameplayDataSetNetworkChangeCallback(IntPtr userGameplayDataInstance, IntPtr dispatchReceiver, FuncNetworkStatusChangeCallback statusChangeCallback);
        [DllImport(IMPORT)] private static extern void GameKitUserGameplayDataSetCacheProcessedCallback(IntPtr userGameplayDataInstance, IntPtr dispatchReceiver, FuncCacheProcessedCallback cacheProcessedCallback);
        [DllImport(IMPORT)] private static extern void GameKitUserGameplayDataDropAllCachedEvents(IntPtr userGameplayDataInstance);
        [DllImport(IMPORT)] private static extern uint GameKitUserGameplayDataPersistApiCallsToCache(IntPtr userGameplayDataInstance, string offlineCacheFile);
        [DllImport(IMPORT)] private static extern uint GameKitUserGameplayDataLoadApiCallsFromCache(IntPtr userGameplayDataInstance, string offlineCacheFile);

        [AOT.MonoPInvokeCallback(typeof(FuncNetworkStatusChangeCallback))]
        protected static void NetworkStatusChangeCallback(IntPtr dispatchReceiver, bool isConnectionOk, string connectionClient)
        {
            NetworkStatusChangeResults results = new NetworkStatusChangeResults();
            results.IsConnectionOk = isConnectionOk;
            results.ConnectionClient = connectionClient;
            results.ResultCode = GameKitErrors.GAMEKIT_SUCCESS;

            _isNetworkHealthy = isConnectionOk;
            _networkCallbackDelegate(results);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncCacheProcessedCallback))]
        protected static void CacheProcessedCallback(IntPtr dispatchReceiver, bool isCacheProcessed)
        {
            CacheProcessedResults results = new CacheProcessedResults();
            results.IsCacheProcessed = isCacheProcessed;
            results.ResultCode = GameKitErrors.GAMEKIT_SUCCESS;

            _cacheProcessedDelegate(results);
        }

        public void SetUserGameplayDataClientSettings(UserGameplayDataClientSettings settings)
        {
            DllLoader.TryDll(() => GameKitSetUserGameplayDataClientSettings(GetInstance(), settings), nameof(GameKitSetUserGameplayDataClientSettings));
        }

        public AddUserGameplayDataResults AddUserGameplayData(AddUserGameplayDataDesc addUserGameplayDataDesc)
        {
            IntPtr[] bundleItemKeys = Marshaller.ArrayOfStringsToArrayOfIntPtr(addUserGameplayDataDesc.BundleItems.Keys.ToArray());
            IntPtr[] bundleItemValues = Marshaller.ArrayOfStringsToArrayOfIntPtr(addUserGameplayDataDesc.BundleItems.Values.ToArray());

            GCHandle bundleItemKeysPin = GCHandle.Alloc(bundleItemKeys, GCHandleType.Pinned);
            GCHandle bundleItemValuesPin = GCHandle.Alloc(bundleItemValues, GCHandleType.Pinned);

            UserGameplayDataBundle userGameplayDataBundle;
            userGameplayDataBundle.BundleName = addUserGameplayDataDesc.BundleName;
            userGameplayDataBundle.NumKeys = (IntPtr)addUserGameplayDataDesc.BundleItems.Count;
            userGameplayDataBundle.BundleItemKeys = bundleItemKeysPin.AddrOfPinnedObject();
            userGameplayDataBundle.BundleItemValues = bundleItemValuesPin.AddrOfPinnedObject();

            MultiKeyValueStringCallbackResult callbackResults = new MultiKeyValueStringCallbackResult();

            uint status = GameKitErrors.GAMEKIT_ERROR_GENERAL;
            try
            {
                status = DllLoader.TryDll(callbackResults, (IntPtr dispatchReceiver) => GameKitAddUserGameplayData(GetInstance(), userGameplayDataBundle, dispatchReceiver, GameKitCallbacks.MultiKeyValueStringCallback), nameof(GameKitAddUserGameplayData), GameKitErrors.GAMEKIT_ERROR_GENERAL);
            }
            finally
            {
                bundleItemKeysPin.Free();
                bundleItemValuesPin.Free();
                Marshaller.FreeArrayOfIntPtr(bundleItemKeys);
                Marshaller.FreeArrayOfIntPtr(bundleItemValues);
            }

            AddUserGameplayDataResults results = new AddUserGameplayDataResults();
            for (uint i = 0; i < callbackResults.ResponseKeys.Count(); ++i)
            {
                results.BundleItems.Add(callbackResults.ResponseKeys[i], callbackResults.ResponseValues[i]);
            }

            results.ResultCode = status;

            return results;
        }

        public MultiStringCallbackResult ListUserGameplayDataBundles()
        {
            MultiStringCallbackResult results = new MultiStringCallbackResult();

            uint status = DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitListUserGameplayDataBundles(GetInstance(), dispatchReceiver, GameKitCallbacks.MultiStringCallback), nameof(GameKitListUserGameplayDataBundles), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            results.ResultCode = status;

            return results;
        }

        public GetUserGameplayDataBundleResults GetUserGameplayDataBundle(string bundleName)
        {
            MultiKeyValueStringCallbackResult callbackResults = new MultiKeyValueStringCallbackResult();

            uint status = DllLoader.TryDll(callbackResults, (IntPtr dispatchReceiver) => GameKitGetUserGameplayDataBundle(GetInstance(), bundleName, dispatchReceiver, GameKitCallbacks.MultiKeyValueStringCallback), nameof(GameKitGetUserGameplayDataBundle), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            GetUserGameplayDataBundleResults results = new GetUserGameplayDataBundleResults();
            for (uint i = 0; i < callbackResults.ResponseKeys.Count(); ++i)
            {
                results.Bundles.Add(callbackResults.ResponseKeys[i], callbackResults.ResponseValues[i]);
            }

            results.ResultCode = status;

            return results;
        }

        public StringCallbackResult GetUserGameplayDataBundleItem(UserGameplayDataBundleItem userGameplayDataBundleItem)
        {
            StringCallbackResult results = new StringCallbackResult();

            uint status = DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitGetUserGameplayDataBundleItem(GetInstance(), userGameplayDataBundleItem, dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitGetUserGameplayDataBundleItem), GameKitErrors.GAMEKIT_ERROR_GENERAL);
            results.ResultCode = status;

            return results;
        }

        public uint UpdateUserGameplayDataBundleItem(UserGameplayDataBundleItemValue userGameplayDataBundleItemValue)
        {
            return DllLoader.TryDll(() => GameKitUpdateUserGameplayDataBundleItem(GetInstance(), userGameplayDataBundleItemValue), nameof(GameKitUpdateUserGameplayDataBundleItem), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint DeleteAllUserGameplayData()
        {
            return DllLoader.TryDll(() => GameKitDeleteAllUserGameplayData(GetInstance()), nameof(GameKitDeleteAllUserGameplayData), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint DeleteUserGameplayDataBundle(string bundleName)
        {
            return DllLoader.TryDll(() => GameKitDeleteUserGameplayDataBundle(GetInstance(), bundleName), nameof(GameKitDeleteUserGameplayDataBundle), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint DeleteUserGameplayDataBundleItems(DeleteUserGameplayDataBundleItemsDesc deleteUserGameplayDataBundleItemsDesc)
        {
            IntPtr[] bundleItemKeys = Marshaller.ArrayOfStringsToArrayOfIntPtr(deleteUserGameplayDataBundleItemsDesc.BundleItemKeys);
            GCHandle bundleKeysPin = GCHandle.Alloc(bundleItemKeys, GCHandleType.Pinned);

            UserGameplayDataDeleteItemsRequest userGameplayDataDeleteItemsRequest = new UserGameplayDataDeleteItemsRequest();
            userGameplayDataDeleteItemsRequest.BundleName = deleteUserGameplayDataBundleItemsDesc.BundleName;
            userGameplayDataDeleteItemsRequest.NumKeys = (IntPtr)deleteUserGameplayDataBundleItemsDesc.NumKeys;
            userGameplayDataDeleteItemsRequest.BundleItemKeys = bundleKeysPin.AddrOfPinnedObject();

            try
            {
                return DllLoader.TryDll(() => GameKitDeleteUserGameplayDataBundleItems(GetInstance(), userGameplayDataDeleteItemsRequest), nameof(GameKitDeleteUserGameplayDataBundleItems), GameKitErrors.GAMEKIT_ERROR_GENERAL);
            }
            finally
            {
                bundleKeysPin.Free();
                Marshaller.FreeArrayOfIntPtr(bundleItemKeys);
            }
        }

        public void UserGameplayDataStartRetryBackgroundThread()
        {
            DllLoader.TryDll(() => GameKitUserGameplayDataStartRetryBackgroundThread(GetInstance()), nameof(GameKitUserGameplayDataStartRetryBackgroundThread));
        }

        public void UserGameplayDataStopRetryBackgroundThread()
        {
            DllLoader.TryDll(() => GameKitUserGameplayDataStopRetryBackgroundThread(GetInstance()), nameof(GameKitUserGameplayDataStopRetryBackgroundThread));
        }

        public void ForceRetry(Action<bool> callback)
        {
            UserGameplayDataStopRetryBackgroundThread();

            UserGameplayData.NetworkChangedDelegate tempNetworkChangeDelegate = null;
            tempNetworkChangeDelegate = (NetworkStatusChangeResults results) =>
            {
                // This temporary delegate must unsubscribe itself otherwise the captured
                // objects could remain in memory longer than needed.
                _networkCallbackDelegate -= tempNetworkChangeDelegate;

                callback(results.IsConnectionOk);
            };
            _networkCallbackDelegate += tempNetworkChangeDelegate;

            UserGameplayDataStartRetryBackgroundThread();
        }

        public void UserGameplayDataSetNetworkChangeCallback(UserGameplayData.NetworkChangedDelegate networkChangeDelegate)
        {
            _networkCallbackDelegate += networkChangeDelegate;
            DllLoader.TryDll(() => GameKitUserGameplayDataSetNetworkChangeCallback(GetInstance(), IntPtr.Zero, NetworkStatusChangeCallback), nameof(GameKitUserGameplayDataSetNetworkChangeCallback));
        }

        public bool GetLastNetworkHealthState()
        {
            return _isNetworkHealthy;
        }

        public void UserGameplayDataSetCacheProcessedCallback(UserGameplayData.CacheProcessedDelegate cacheProcessedDelegate)
        {
            _cacheProcessedDelegate = cacheProcessedDelegate;

            DllLoader.TryDll(() => GameKitUserGameplayDataSetCacheProcessedCallback(GetInstance(), IntPtr.Zero, CacheProcessedCallback), nameof(GameKitUserGameplayDataSetCacheProcessedCallback));
        }

        public void UserGameplayDataDropAllCachedEvents()
        {
            DllLoader.TryDll(() => GameKitUserGameplayDataDropAllCachedEvents(GetInstance()), nameof(GameKitUserGameplayDataDropAllCachedEvents));
        }

        public uint UserGameplayDataPersistApiCallsToCache(string offlineCacheFile)
        {
            return DllLoader.TryDll(() => GameKitUserGameplayDataPersistApiCallsToCache(GetInstance(), offlineCacheFile), nameof(GameKitUserGameplayDataPersistApiCallsToCache), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint UserGameplayDataLoadApiCallsFromCache(string offlineCacheFile)
        {
            return DllLoader.TryDll(() => GameKitUserGameplayDataLoadApiCallsFromCache(GetInstance(), offlineCacheFile), nameof(GameKitUserGameplayDataLoadApiCallsFromCache), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitUserGameplayDataInstanceCreateWithSessionManager(sessionManager, logCb), nameof(GameKitUserGameplayDataInstanceCreateWithSessionManager), IntPtr.Zero);
        }

        protected override void Release(IntPtr instance)
        {
            DllLoader.TryDll(() => GameKitUserGameplayDataInstanceRelease(instance), nameof(GameKitUserGameplayDataInstanceRelease));
        }
    }
}
