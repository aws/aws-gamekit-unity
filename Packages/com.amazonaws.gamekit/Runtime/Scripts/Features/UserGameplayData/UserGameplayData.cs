// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Features.GameKitUserGameplayData
{
    /// <summary>
    /// User Gameplay Data feature.
    /// </summary>
    public class UserGameplayData : GameKitFeatureBase<UserGameplayDataWrapper>, IUserGameplayDataProvider
    {
        public override FeatureType FeatureType => FeatureType.UserGameplayData;

        private static FileManager _fileManager;
        
        // Automatic offline state
        private bool _isOfflineCacheRunning = false;
        private string _offlineCacheFile = string.Empty;
        
#if UNITY_IOS
        private bool _shouldAutoRestartCache = false;
#endif

        // Delegates 
        public delegate void NetworkChangedDelegate(NetworkStatusChangeResults results);
        public delegate void CacheProcessedDelegate(CacheProcessedResults results);

        /// <summary>
        /// Call to get an instance of the GameKit UserGameplayData feature.
        /// </summary>
        /// <returns>An instance of the UserGameplayData feature that can be used to call UserGameplayData related methods.</returns>
        public static UserGameplayData Get()
        {
            _fileManager = new FileManager();
            return GameKitFeature<UserGameplayData>.Get();
        }
        
        /// <summary>
        /// Used to trigger any functionality that should happen when the applications pause status is changed.
        /// </summary>
        /// <param name="isPaused">True if the application is paused, else False.</param>
        protected override void NotifyPause(bool isPaused)
        {
#if UNITY_IOS
            // On iOS shutdown code can sometimes be unreliable since each application is paused before being shut down.
            // In order to ensure the users cache is being saved properly we can save when the application is suspended and reload when its opened again
            
            if (isPaused && _isOfflineCacheRunning)
            {
                OfflineSupportCleanup();
                _shouldAutoRestartCache = true;
            }
            else if (!isPaused && _shouldAutoRestartCache)
            {
                if (!string.IsNullOrEmpty(_offlineCacheFile) && _fileManager.FileExists(_offlineCacheFile))
                {
                    LoadFromCache(_offlineCacheFile, result =>
                    {
                        Logging.LogInfo($"User Gameplay Data LoadFromCache completed with result: {result}");

                        if (result == 0)
                        {
                            StartRetryBackgroundThread();
                        }
                        else
                        {
                            Logging.LogInfo($"Since LoadFromCache was not successful, the retry thread will not be restarted to ensure calls are not overwritten.");
                        }
                    });
                }
                else
                {
                    StartRetryBackgroundThread();
                }
                
                _shouldAutoRestartCache = false;
            }
#endif
        }

        /// <summary>
        /// Used to cleanup User Gameplay Data functionality when the application is closing.
        /// </summary>
        protected override void DestroyFeature()
        {
#if !UNITY_IOS  // For iOS this is handled NotifyPause
            OfflineSupportCleanup();
#endif
        }

        /// <summary>
        /// Handles stopping the retry thread, if it is running, when the User Gameplay Data feature is being destroyed.  
        /// If automatic caching has been set up will also persist the current queue to the cache file.
        /// </summary>
        private void OfflineSupportCleanup()
        {
            if (_isOfflineCacheRunning)
            {
                StopRetryBackgroundThread();

                if (!string.IsNullOrEmpty(_offlineCacheFile))
                {
                    uint result = ImmediatePersistToCache(_offlineCacheFile);
                    Logging.LogInfo($"User Gameplay Data PersistToCache completed with result: {result}");
                }
            }
        }

        /// <summary>
        /// Creates a new bundle or updates BundleItems within a specific bundle for the calling user.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_NAME: The bundle name of userGameplayDataBundle is malformed. If this error is received, Check the output log for more details on requirements.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_ITEM_KEY: At least one of the bundle keys of userGameplayData are malformed. If this error is received, Check the output log for more details on which item keys are not valid.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason. <br/>
        /// </summary>
        /// <param name="addUserGameplayDataDesc">Object containing a map of game play data to add</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void AddBundle(AddUserGameplayDataDesc addUserGameplayDataDesc, Action<AddUserGameplayDataResults> callback)
        {
            Call(Feature.AddUserGameplayData, addUserGameplayDataDesc, callback);
        }

        /// <summary>
        /// Applies the settings to the User Gameplay Data Client. Should be called immediately after the instance has been created and before any other API calls.
        /// </summary>
        /// <param name="userGameplayDataClientSettings">Object containing client settings</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void SetClientSettings(UserGameplayDataClientSettings userGameplayDataClientSettings, Action callback)
        {
            Call(Feature.SetUserGameplayDataClientSettings, userGameplayDataClientSettings, callback);
        }

        /// <summary>
        /// Lists the bundle name of every bundle that the calling user owns.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The response body from the backend could not be parsed successfully<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason.<br/>
        /// </summary>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void ListBundles(Action<MultiStringCallbackResult> callback)
        {
            Call(Feature.ListUserGameplayDataBundles, callback);
        }

        /// <summary>
        /// Gets all items that are associated with a certain bundle for the calling user.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_NAME: The bundle name of UserGameplayDataBundleName is malformed. If this error is received, Check the output log for more details on requirements.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The response body from the backend could not be parsed successfully<br/>
        /// - GAMEKIT_ERROR_GENERAL: The bundle name does not exist or the request has failed for unknown reason.<br/>
        /// </summary>
        /// <param name="bundleName">Name of the data bundle items to fetch</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void GetBundle(string bundleName, Action<GetUserGameplayDataBundleResults> callback)
        {
            // Callback wrapper for zero bundles returned
            Call(Feature.GetUserGameplayDataBundle, bundleName, (GetUserGameplayDataBundleResults result) =>
            {
                if (result.ResultCode == GameKitErrors.GAMEKIT_SUCCESS && result.Bundles.Count == 0)
                {
                    Logging.LogError($"UserGameplayData.GetBundle() failed with result code {GameKitErrors.GAMEKIT_ERROR_GENERAL}. No bundles found with name {bundleName}");
                    result.ResultCode = GameKitErrors.GAMEKIT_ERROR_GENERAL;
                }
                callback.Invoke(result);
            });
        }

        /// <summary>
        /// Gets a single item that is associated with a certain bundle for a user.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_NAME: The bundle name in userGameplayDataBundleItem is malformed. If this error is received, Check the output log for more details on requirements.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_ITEM_KEY: The bundle key in userGameplayDataBundleItem is malformed. If this error is received, Check the output log for more details on which item keys are not valid.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason.<br/>
        /// </summary>
        /// <param name="userGameplayDataBundleItem">Object containing needed inforamtion for fetching the bundle item</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void GetBundleItem(UserGameplayDataBundleItem userGameplayDataBundleItem, Action<StringCallbackResult> callback)
        {
            Call(Feature.GetUserGameplayDataBundleItem, userGameplayDataBundleItem, callback);
        }

        /// <summary>
        /// Updates the value of an existing item inside a bundle with new item data.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_NAME: The bundle name in userGameplayDataBundleItem is malformed. If this error is received, Check the output log for more details on requirements.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_ITEM_KEY: The bundle key in userGameplayDataBundleItem is malformed. If this error is received, Check the output log for more details on which item keys are not valid.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason.<br/>
        /// </summary>
        /// <param name="userGameplayDataBundleItemValue">Object containing information for the bundle item update and what to update it with</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void UpdateItem(UserGameplayDataBundleItemValue userGameplayDataBundleItemValue, Action<uint> callback)
        {
            Call(Feature.UpdateUserGameplayDataBundleItem, userGameplayDataBundleItemValue, callback);
        }

        /// <summary>
        /// Permanently deletes all bundles associated with a user.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason.<br/>
        /// </summary>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void DeleteAllData(Action<uint> callback)
        {
            Call(Feature.DeleteAllUserGameplayData, callback);
        }

        /// <summary>
        /// Permanently deletes an entire bundle, along with all corresponding items, associated with a user.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_NAME: The bundle name in UserGameplayDataBundleName is malformed. If this error is received, Check the output log for more details on requirements.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason.<br/>
        /// </summary>
        /// <param name="bundleName">name of the bundle to delete</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        /// <returns>Unity JobHandle object for the call</returns>
        public void DeleteBundle(string bundleName, Action<uint> callback)
        {
            Call(Feature.DeleteUserGameplayDataBundle, bundleName, callback);
        }

        /// <summary>
        /// Permanently deletes a list of items inside of a bundle associated with a user.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_SETTINGS_FILE_READ_FAILED: The session manager does not have settings loaded in for the User Gameplay data feature.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_NAME: The bundle name in userGameplayDataBundleItemsDeleteRequest is malformed. If this error is received, Check the output log for more details on requirements.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_BUNDLE_ITEM_KEY: The bundle key in userGameplayDataBundleItemsDeleteRequest is malformed. If this error is received, Check the output log for more details on which item keys are not valid.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_FAILED: The call made to the backend service has failed.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_API_CALL_DROPPED: The call made to the backend service has been dropped.<br/>
        /// - GAMEKIT_WARNING_USER_GAMEPLAY_DATA_API_CALL_ENQUEUED: The call made to the backend service has been enqueued as connection may be unhealthy and will automatically be retried.<br/>
        /// - GAMEKIT_ERROR_GENERAL: The request has failed unknown reason.<br/>
        /// </summary>
        /// <param name="deleteUserGameplayDataBundleItemsDesc">Object containing the bundle and list of items to delete</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        /// <returns>Unity JobHandle object for the call</returns>
        public void DeleteBundleItems(DeleteUserGameplayDataBundleItemsDesc deleteUserGameplayDataBundleItemsDesc, Action<uint> callback)
        {
            Call(Feature.DeleteUserGameplayDataBundleItems, deleteUserGameplayDataBundleItemsDesc, callback);
        }

        /// <summary>
        /// Enables automatic offline mode, including persisting all failed calls in the retry thread to a cache file at the end of program.<br/><br/>
        /// If the cache needs to be persisted before the application is closed it is recommended to use StopRetryBackgroundThread() to stop the thread and PersistToCache to save the cache manually, then EnableAutomaticOfflineModeWithCaching can be restarted. <br/><br/>
        /// 
        /// Note: Methods such as DropAllCachedEvents, SetNetworkChangeDelegate, SetCacheProcessedDelegate and SetClientSettings are not handled by this method and should be called independently.
        /// </summary>
        /// <param name="offlineCacheFile">The location of the offline cache file. All persisted calls in the cache will automatically be loaded into the queue. Any failed calls in the queue at the end of the program will be cached in this file.</param>
        public void EnableAutomaticOfflineModeWithCaching(string offlineCacheFile)
        {
            if (_isOfflineCacheRunning)
            {
                Logging.LogWarning("EnableAutomaticOfflineModeWithCaching: Background thread already running, ensure that the thread is stopped before attempting to start the retry thread again.");
                return;
            }

            _offlineCacheFile = offlineCacheFile;
            if (System.IO.File.Exists(_offlineCacheFile))
            {
                LoadFromCache(offlineCacheFile, result =>
                {
                    Logging.LogInfo($"User Gameplay Data LoadFromCache completed with result: {result}");

                    StartRetryBackgroundThread();
                });
            }
            else
            {
                StartRetryBackgroundThread();
            }
        }

        /// <summary>
        /// Start the Retry background thread.<br/><br/>
        ///
        /// The DestroyFeature() method, which is automatically called when the application is closing, will handle stopping the thread.<br/>
        /// However, if you need to persist the files during play it is recommended that you call StopRetryBackgroundThread() and Save/Load to Cache manually.<br/><br/>
        ///
        /// Note: If you are Caching your offline calls, StartRetryBackgroundThread() should be called after loading from cache.
        /// </summary>
        public void StartRetryBackgroundThread()
        {
            if (!_isOfflineCacheRunning)
            {
                _isOfflineCacheRunning = true;
                Feature.UserGameplayDataStartRetryBackgroundThread();
            }
        }

        /// <summary>
        /// Stop the Retry background thread.<br/><br/>
        ///
        /// Note: If you are caching your offline calls, StopRetryBackgroundThread() should be called before saving to cache.
        /// </summary>
        public void StopRetryBackgroundThread()
        {
            if (_isOfflineCacheRunning)
            {
                Feature.UserGameplayDataStopRetryBackgroundThread();
                _isOfflineCacheRunning = false;
            }
        }

        /// <summary>
        /// Return information about the running state of the background thread.
        /// </summary>
        /// <returns>True if it is running, false otherwise</returns>
        public bool IsBackgroundThreadRunning()
        {
            return _isOfflineCacheRunning;
        }

        /// <summary>
        /// Clear all pending events from the user's cache.
        /// </summary>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        /// <returns>Unity JobHandle object for the call</returns>
        public void DropAllCachedEvents(Action callback)
        {
            Call(Feature.UserGameplayDataDropAllCachedEvents, callback);
        }

        /// <summary>
        /// Set the callback to invoke when the network state changes.
        /// </summary>
        /// <param name="callback">Delegate that should be called whenever there is a network change.</param>
        public void SetNetworkChangeDelegate(NetworkChangedDelegate callback)
        {
            Feature.UserGameplayDataSetNetworkChangeCallback(callback);
        }

        /// <summary>
        /// Get the last known state of network connectivity. 
        /// </summary>
        /// <returns>The last known state of connectivity. True means healthy, False means unhealthy</returns>
        public bool GetLastNetworkHealthState()
        {
            return Feature.GetLastNetworkHealthState();
        }

        /// <summary>
        /// Set the callback to invoke when the offline cache finishes processing.
        /// </summary>
        /// <param name="callback">Delegate that should be called whenever there the cache has finished processing.</param>
        public void SetCacheProcessedDelegate(CacheProcessedDelegate callback)
        {
            Feature.UserGameplayDataSetCacheProcessedCallback(callback);
        }

        /// <summary>
        /// Write the pending API calls to cache.<br/><br/>
        /// 
        /// Pending API calls are requests that could not be sent due to network being offline or other failures.<br/>
        /// The internal queue of pending calls is cleared. It is recommended to stop the background thread before calling this method.<br/><br/>
        ///
        /// This is the non-blocking version persist to cache call and should be used when saving during play, if you need to persist to cache on application close the blocking version, ImmediatePersistToCache() is recommended.<br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_CACHE_WRITE_FAILED: There was an issue writing the queue to the offline cache file.<br/>
        /// </summary>
        /// <param name="offlineCacheFile">Path to the offline cache file.</param>
        /// <param name="callback">Delegate that is called once the function has finished executing.</param>
        /// <returns>Unity JobHandle object for the call</returns>
        public void PersistToCache(string offlineCacheFile, Action<uint> callback)
        {
            Call(Feature.UserGameplayDataPersistApiCallsToCache, offlineCacheFile, callback);
        }

        /// <summary>
        /// Write the pending API calls to cache.<br/><br/>
        /// 
        /// Pending API calls are requests that could not be sent due to network being offline or other failures.<br/>
        /// The internal queue of pending calls is cleared. It is recommended to stop the background thread before calling this method.<br/><br/>
        ///
        /// This is the blocking version of the call and should be used on application close. PersistToCache() is recommended for saving to cache during play.<br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_CACHE_WRITE_FAILED: There was an issue writing the queue to the offline cache file.<br/>
        /// </summary>
        /// <param name="offlineCacheFile">Path to the offline cache file.</param>
        /// <returns>The result code of persisting the queue to cache.</returns>
        public uint ImmediatePersistToCache(string offlineCacheFile)
        {
            return Feature.UserGameplayDataPersistApiCallsToCache(offlineCacheFile);
        }

        /// <summary>
        /// Read the pending API calls from cache.<br/><br/>
        /// 
        /// The calls will be enqueued and retried as soon as the Retry background thread is started and network connectivity is up.<br/>
        /// The contents of the cache are deleted.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_USER_GAMEPLAY_DATA_CACHE_READ_FAILED: There was an issue loading the offline cache file to the queue.<br/>
        /// </summary>
        /// <param name="offlineCacheFile">Path to the offline cache file.</param>
        /// <param name="callback">Delegate that is called once the function has finished executing.</param>
        /// <returns>Unity JobHandle object for the call</returns>
        public void LoadFromCache(string offlineCacheFile, Action<uint> callback)
        {
            Call(Feature.UserGameplayDataLoadApiCallsFromCache, offlineCacheFile, callback);
        }

        /// <summary>
        /// Forces an immediate retry of the calls that are in the queue.
        /// </summary>
        /// <remarks>The Network state can transiton from Unhealthy to Healthy as a side effect of this.</remarks>
        /// <param name="resultCallback">Callback invoked then the retry finishes. Returns success or failure.</param>
        public void ForceRetry(Action<bool> resultCallback)
        {
            Logging.LogInfo("Forcing a Retry to synchronize the data with the backend.");
            Feature.ForceRetry(resultCallback);
        }

        /// <summary>
        /// Attempts to synchronize data in the queue with the backend and, if successful, executes another User Gameplay Data API.
        /// Use this when you want to be sure that the data in the backend is updated before making subsequent calls.
        /// </summary>
        /// <param name="gameplayDataApiCall">The User Gameplay Data API to call (for example GetBundle(), GetBundleItem(), ListBundles(), or other APIs)</param>
        /// <param name="onErrorAction">Action to execute in case of error. Error can happen if the device is still offline or if the backend is experiencing problems.</param>
        public void TryForceSynchronizeAndExecute(Action gameplayDataApiCall, Action onSynchronizeErrorAction)
        {
            if (GetLastNetworkHealthState())
            {
                gameplayDataApiCall();
            }
            else
            {
                ForceRetry((bool success) => 
                {
                    if (success)
                    {
                        gameplayDataApiCall();
                    }
                    else
                    {
                        onSynchronizeErrorAction();
                    }
                });
            }
        }
    }
}

