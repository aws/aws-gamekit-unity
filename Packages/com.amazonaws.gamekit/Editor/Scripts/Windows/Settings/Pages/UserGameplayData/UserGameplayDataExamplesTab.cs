
// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Linq;
using AWS.GameKit.Editor.GUILayoutExtensions;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Features.GameKitUserGameplayData;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.UserGameplayData
{
    [Serializable]
    public class UserGameplayDataExamplesTab : FeatureExamplesTab
    {
        private static readonly string RETRY_THREAD_OVERVIEW = L10n.Tr("In order to handle making offline calls, the retry thread must be started. Once the retry thread has been started, all calls made while the network state is unhealthy will be stored in a queue and retried based on the default client settings, " +
                                                                       "unless new settings have been set by the SetClientSettings() method. The retry thread should be stopped before persisting or loading any cached data to the queue.");

        private static readonly string NETWORK_STATUS_OVERVIEW = L10n.Tr("The Network status will be changed to unhealthy after a call has failed and been placed into the offline queue. Once a call has been made successfully or retried successfully the status will be set back to Healthy. " +
                                                                         "If the retry thread has not been started, the network status will always be treated as healthy and all calls will be attempted.");

        private static readonly string CACHE_PROCESSED_OVERVIEW = L10n.Tr("The Cache Processing Status will be set after a Cache file has been loaded into the queue and processed using the LoadFromCache() method.");

        private static readonly string SET_CLIENT_SETTINGS_OVERVIEW = L10n.Tr("Client settings can optionally be set before the retry thread is started. If SetClientSettings() is not called default values will be used. To learn more about the following fields, view their tool tips below. " +
                                                                              "The values of this slider are suggestions, however, the API will take any valid uint.");

        private static readonly string PERSIST_TO_CACHE_OVERVIEW = L10n.Tr("The Persist to Cache method should be used to save the current queue of API calls, that have not been successfully made, to disk when the player exits the program. " +
                                                                    "The optimal file extension for the cache file is '.dat' and should be saved within the 'Application.datapath' directory or any directory a player can access at runtime.");

        private static readonly string LOAD_FROM_CACHE_OVERVIEW = L10n.Tr("The Load from Cache method should be used to load any failed calls that a user may have had during their last session into the queue that will be retried with the retry thread.");

        private static readonly string DROP_CACHED_EVENTS_OVERVIEW = L10n.Tr("Dropping all Cached Events will clear the current queue of unmade calls that were loading into the queue using LoadFromCache(). " +
                                                                             "This will not effect any existing cache files that have been saved to disk with PersistToCache(), only calls that have already been loaded to the queue from a cache.");

        private static readonly IList<string> RETRY_STATEGY_OPTIONS = new List<string> { L10n.Tr("Exponential Backoff"), L10n.Tr("Constant Interval") };

        public override FeatureType FeatureType => FeatureType.UserGameplayData;

        // Dependencies
        private IUserGameplayDataProvider _userGameplayData;

        // Delegates
        private delegate void _networkChangedDelegate(NetworkStatusChangeResults result);

        // User Gameplay Data Examples
        [SerializeField] private AddBundleExampleUI _addBundleExampleUI;
        [SerializeField] private ListUserGameplayDataBundlesExampleUI _listUserGameplayDataExampleUI;
        [SerializeField] private GetBundleExampleUI _getBundleExampleUI;
        [SerializeField] private GetBundleItemExampleUI _getBundleItemExampleUI;
        [SerializeField] private UpdateBundleItemExampleUI _updateBundleItemExampleUI;
        [SerializeField] private DeleteAllUserGameplayDataExampleUI _deleteAllUserGameplayDataExampleUI;
        [SerializeField] private DeleteBundleExampleUI _deleteBundleExampleUI;
        [SerializeField] private DeleteBundleItemsExampleUI _deleteBundleItemsExampleUI;

        // Offline Support Examples
        [SerializeField] private RetryThreadExampleUI _retryThreadExampleUI;
        [SerializeField] private NetworkStatusExampleUI _networkStatusExampleUI;
        [SerializeField] private CacheProcessedExampleUI _cacheProcessedExampleUI;
        [SerializeField] private PersistToCacheExampleUI _persistToCacheExampleUI;
        [SerializeField] private LoadFromCacheExampleUI _loadFromCacheExampleUI;
        [SerializeField] private DropAllCachedEventsExampleUI _dropAllCachedEventsExampleUI;

        // Set Client Settings Examples
        [SerializeField] private SetClientSettingsExampleUI _setClientSettingsExampleUI;

        public override void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _userGameplayData = dependencies.UserGameplayData;

            // User Gameplay Data Examples
            _addBundleExampleUI.Initialize(CallAddBundle, serializedProperty.FindPropertyRelative(nameof(_addBundleExampleUI)));
            _listUserGameplayDataExampleUI.Initialize(CallListBundles, serializedProperty.FindPropertyRelative(nameof(_listUserGameplayDataExampleUI)));
            _getBundleExampleUI.Initialize(CallGetBundle, serializedProperty.FindPropertyRelative(nameof(_getBundleExampleUI)));
            _getBundleItemExampleUI.Initialize(CallGetBundleItem, serializedProperty.FindPropertyRelative(nameof(_getBundleItemExampleUI)));
            _updateBundleItemExampleUI.Initialize(CallUpdateBundleItem, serializedProperty.FindPropertyRelative(nameof(_updateBundleItemExampleUI)));
            _deleteAllUserGameplayDataExampleUI.Initialize(CallDeleteAllUserGameplayData, serializedProperty.FindPropertyRelative(nameof(_deleteAllUserGameplayDataExampleUI)));
            _deleteBundleExampleUI.Initialize(CallDeleteBundle, serializedProperty.FindPropertyRelative(nameof(_deleteBundleExampleUI)));
            _deleteBundleItemsExampleUI.Initialize(CallDeleteBundleItems, serializedProperty.FindPropertyRelative(nameof(_deleteBundleItemsExampleUI)));

            // Set Client Settings Example
            _setClientSettingsExampleUI.Initialize(CallSetClientSettings, serializedProperty.FindPropertyRelative(nameof(_setClientSettingsExampleUI)));

            // Offline Support Examples
            _retryThreadExampleUI.Initialize(CallStartRetryBackgroundThread, CallStopRetryBackgroundThread, serializedProperty.FindPropertyRelative(nameof(_retryThreadExampleUI)));
            CallSetNetworkChangeDelegate();
            CallSetCacheProcessedDelegate();
            _persistToCacheExampleUI.Initialize(CallPersistToCache, serializedProperty.FindPropertyRelative(nameof(_persistToCacheExampleUI)));
            _loadFromCacheExampleUI.Initialize(CallLoadFromCache, serializedProperty.FindPropertyRelative(nameof(_loadFromCacheExampleUI)));
            _dropAllCachedEventsExampleUI.Initialize(CallDropAllCachedEvents, serializedProperty.FindPropertyRelative(nameof(_dropAllCachedEventsExampleUI)));

            base.Initialize(dependencies, serializedProperty);
        }

        protected override void DrawExamples()
        {
            _addBundleExampleUI.OnGUI();
            _listUserGameplayDataExampleUI.OnGUI();
            _getBundleExampleUI.OnGUI();
            _getBundleItemExampleUI.OnGUI();
            _updateBundleItemExampleUI.OnGUI();
            _deleteAllUserGameplayDataExampleUI.OnGUI();
            _deleteBundleExampleUI.OnGUI();
            _deleteBundleItemsExampleUI.OnGUI();
            EditorGUILayoutElements.SectionDivider();

            EditorGUILayout.LabelField("Offline Setup", SettingsGUIStyles.Page.Title);
            EditorGUILayout.Space(5);
            _retryThreadExampleUI.OnGUI();
            _networkStatusExampleUI.OnGUI();
            _cacheProcessedExampleUI.OnGUI();
            _setClientSettingsExampleUI.OnGUI();
            _persistToCacheExampleUI.OnGUI();
            _loadFromCacheExampleUI.OnGUI();
            _dropAllCachedEventsExampleUI.OnGUI();
        }

        public override void OnLogout()
        {
            _retryThreadExampleUI.ForceStopRetryThread();
            _networkStatusExampleUI.NetworkStatusChangeResult = null; // In NetworkStatusExampleUI, null is treated as a user just logged in
            _cacheProcessedExampleUI.CacheProcessedResult = null; // In CacheProcessedExampleUI, null is treated as a user just logged in
        }

        #region User Gameplay Data API Calls 
        private void CallAddBundle()
        {
            if (_addBundleExampleUI.BundleKeys.Distinct().Count() != _addBundleExampleUI.BundleKeys.Count())
            {
                _addBundleExampleUI.ResultCode = GameKitErrors.GAMEKIT_ERROR_GENERAL;
                Debug.LogError("All Bundle Key values in UserGameplayData.AddBundle() must be unique");
                return;
            }
                  
            // In the case where the list of keys and list of values do not need to be serialized and displayed on the screen it is recommended to skip this step and just use a Dictionary.
            Dictionary<string, string> bundleDictionary =
                Enumerable.Range(0, _addBundleExampleUI.BundleKeys.Count)
                    .ToDictionary(i => _addBundleExampleUI.BundleKeys[i], i => _addBundleExampleUI.BundleValues[i]);

            AddUserGameplayDataDesc addUserGameplayData = new AddUserGameplayDataDesc
            {
                BundleName = _addBundleExampleUI.BundleName,
                BundleItems = bundleDictionary
            };

            Debug.Log($"Calling UserGameplayData.AddBundle() for {_addBundleExampleUI.BundleName}");

            _userGameplayData.AddBundle(addUserGameplayData, (AddUserGameplayDataResults result) =>
            {
                Debug.Log($"UserGameplayData.AddBundle() completed with result code {result.ResultCode}");
                _addBundleExampleUI.ResultCode = result.ResultCode;

                if (result.BundleItems.Count > 0)
                {
                    foreach (KeyValuePair<string, string> bundleItem in result.BundleItems)
                    {
                        Debug.LogError($"Failed to process item - [{ bundleItem.Key}, { bundleItem.Value}]");
                    }
                }
            });
        }

        private void CallListBundles()
        {
            Action listBundlesCall = () =>
            {
                _userGameplayData.ListBundles((MultiStringCallbackResult result) =>
                {
                    Debug.Log($"UserGameplayData.ListBundles() completed with result code {result.ResultCode}");
                    _listUserGameplayDataExampleUI.ResultCode = result.ResultCode;

                    _listUserGameplayDataExampleUI.BundleNames = result.ResponseValues.ToList();
                });
            };

            if (_userGameplayData.IsBackgroundThreadRunning())
            {
                // Only do this if the background thread is running as we might have data in the queue that should be synchronized before retrieving the bundles
                _userGameplayData.TryForceSynchronizeAndExecute(listBundlesCall, () => { Debug.Log("Could not synchronize data with the backend."); });
            }
            else
            {
                listBundlesCall();
            }
        }

        private void CallGetBundle()
        {
            Debug.Log($"Calling UserGameplayData.GetBundle() for {_getBundleExampleUI.BundleName}");

            Action getBundleCall = () =>
            {
                _userGameplayData.GetBundle(_getBundleExampleUI.BundleName, (GetUserGameplayDataBundleResults result) =>
                {
                    Debug.Log($"UserGameplayData.GetBundle() completed with result code {result.ResultCode}");
                    _getBundleExampleUI.ResultCode = result.ResultCode;

                    _getBundleExampleUI.BundleKeys = result.Bundles.Keys.ToList();
                    _getBundleExampleUI.BundleValues = result.Bundles.Values.ToList();
                });
            };

            if (_userGameplayData.IsBackgroundThreadRunning())
            {
                // Only do this if the background thread is running as we might have data in the queue that should be synchronized before retrieving the bundle
                _userGameplayData.TryForceSynchronizeAndExecute(getBundleCall, () => { Debug.Log("Could not synchronize data with the backend."); });
            }
            else
            {
                getBundleCall();
            }
        }

        private void CallGetBundleItem()
        {
            Debug.Log($"Calling UserGameplayData.GetBundleItem() with {_getBundleItemExampleUI.BundleName}");

            UserGameplayDataBundleItem userGameplayDayaBundleItem = new UserGameplayDataBundleItem
            {
                BundleName = _getBundleItemExampleUI.BundleName,
                BundleItemKey = _getBundleItemExampleUI.BundleItemKey
            };

            Action getBundleCall = () =>
            {
                _userGameplayData.GetBundleItem(userGameplayDayaBundleItem, (StringCallbackResult result) =>
                {
                    Debug.Log($"UserGameplayData.GetBundle() completed with result code {result.ResultCode}");
                    _getBundleItemExampleUI.ResultCode = result.ResultCode;

                    _getBundleItemExampleUI.BundleItemValueResponse = result.ResponseValue;
                });
            };

            if (_userGameplayData.IsBackgroundThreadRunning())
            {
                // Only do this if the background thread is running as we might have data in the queue that should be synchronized before retrieving the bundle item
                _userGameplayData.TryForceSynchronizeAndExecute(getBundleCall, () => { Debug.Log("Could not synchronize data with the backend."); });
            }
            else
            {
                getBundleCall();
            }
        }

        private void CallUpdateBundleItem()
        {
            Debug.Log($"Calling UserGameplayData.UpdateItem() with {_getBundleItemExampleUI.BundleName}");

            UserGameplayDataBundleItemValue userGameplayDataBundleItemValue = new UserGameplayDataBundleItemValue
            {
                BundleName = _updateBundleItemExampleUI.BundleName,
                BundleItemKey = _updateBundleItemExampleUI.BundleItemKey,
                BundleItemValue = _updateBundleItemExampleUI.NewBundleItemValue
            };

            _userGameplayData.UpdateItem(userGameplayDataBundleItemValue, (uint resultCode) =>
            {
                Debug.Log($"UserGameplayData.UpdateItem() completed with result code {resultCode}");
                _updateBundleItemExampleUI.ResultCode = resultCode;
            });
        }

        private void CallDeleteAllUserGameplayData()
        {
            Debug.Log($"Calling UserGameplayData.DeleteAllData() to delete all User Gameplay data for the currently logged in user");

            _userGameplayData.DeleteAllData((uint resultCode) =>
            {
                Debug.Log($"UserGameplayData.DeleteAllData() completed with result code {resultCode}");
                _deleteAllUserGameplayDataExampleUI.ResultCode = resultCode;
            });
        }

        private void CallDeleteBundle()
        {
            Debug.Log($"Calling UserGameplayData.DeleteBundle() with {_deleteBundleExampleUI.BundleName}");

            _userGameplayData.DeleteBundle(_deleteBundleExampleUI.BundleName, (uint resultCode) =>
            {
                Debug.Log($"UserGameplayData.DeleteBundle() completed with result code {resultCode}");
                _deleteBundleExampleUI.ResultCode = resultCode;
            });
        }

        private void CallDeleteBundleItems()
        {
            DeleteUserGameplayDataBundleItemsDesc deleteUserGameplayDataBundleItemsDesc = new DeleteUserGameplayDataBundleItemsDesc
            {
                BundleName = _deleteBundleItemsExampleUI.BundleName,
                BundleItemKeys = _deleteBundleItemsExampleUI.BundleItemKeys.ToArray(),
                NumKeys = (ulong)_deleteBundleItemsExampleUI.BundleItemKeys.Count
            };

            Debug.Log($"Calling UserGameplayData.DeleteBundleItems() with {deleteUserGameplayDataBundleItemsDesc}");

            _userGameplayData.DeleteBundleItems(deleteUserGameplayDataBundleItemsDesc, (uint resultCode) =>
            {
                Debug.Log($"UserGameplayData.DeleteBundleItems() completed with result code {resultCode}");
                _deleteBundleItemsExampleUI.ResultCode = resultCode;
            });
        }
        #endregion

        #region Example GUI Classes
        [Serializable]
        public class AddBundleExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Add Bundle";
            public string BundleName;

            /*
             * Note: A List of Keys and a List of values are used as a workaround to Dictionaries not being serializable in C#.
             * In the IUserGameplayDataProvider.AddBundle these lists will be formed into a dictionary before being passed in.
             * A dictionary type should be used here as long as the data does not need to be serialized or showed on screen. 
             */

            public List<string> BundleKeys = new List<string>(new string[1]);
            public List<string> BundleValues = new List<string>(new string[1]);

            protected override void DrawInput()
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel("Bundle Name");
                    using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleDictionaryInputAligned))
                    {
                        PropertyField(nameof(BundleName));
                        GUILayout.Space(4);

                        SerializedProperty serializedBundleKeys = _serializedProperty.FindPropertyRelative(nameof(BundleKeys));
                        SerializedProperty serializedBundleValues = _serializedProperty.FindPropertyRelative(nameof(BundleValues));

                        EditorGUILayoutElements.SerializableExamplesDictionary(
                            serializedBundleKeys, 
                            serializedBundleValues, 
                            BundleKeys, 
                            BundleValues, 
                            "Item Keys", 
                            "Item Values");
                    }
                }
            }
        }

        [Serializable]
        public class ListUserGameplayDataBundlesExampleUI : GameKitExampleUI
        {
            public override string ApiName => "List User Gameplay Data Bundles";
            protected override bool _shouldDisplayResponse => true;

            public List<string> BundleNames = new List<string>();

            protected override void DrawOutput()
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleResponseInputAligned))
                {
                    SerializedProperty serializedBundleKeys = _serializedProperty.FindPropertyRelative(nameof(BundleNames));

                    EditorGUILayoutElements.SerializableExamplesList(
                        serializedBundleKeys,
                        BundleNames,
                        "Bundle Names",
                        true);
                }
            }
        }

        [Serializable]
        public class GetBundleExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Get Bundle";
            protected override bool _shouldDisplayResponse => true;

            public string BundleName;

            /*
             * Note: A List of Keys and a List of values is used as a workaround to Dictionaries not being serializable in C#.
             * In the IUserGameplayDataProvider.GetBundle the response will contain a dictionary that is then broken up into two lists for the sake of this example.
             */

            public List<string> BundleKeys = new List<string>();
            public List<string> BundleValues = new List<string>();

            protected override void DrawInput()
            {
                PropertyField(nameof(BundleName), "Bundle Name");
            }

            protected override void DrawOutput()
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleResponseInputAligned))
                {
                    SerializedProperty serializedBundleKeys = _serializedProperty.FindPropertyRelative(nameof(BundleKeys));
                    SerializedProperty serializedBundleValues = _serializedProperty.FindPropertyRelative(nameof(BundleValues));

                    EditorGUILayoutElements.SerializableExamplesDictionary(
                        serializedBundleKeys,
                        serializedBundleValues,
                        BundleKeys,
                        BundleValues,
                        "Bundle Keys",
                        "Bundle Values",
                        isReadonly: true);
                }
            }
        }

        [Serializable]
        public class GetBundleItemExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Get Bundle Item";
            protected override bool _shouldDisplayResponse => true;
         
            public string BundleName;
            public string BundleItemKey;

            public string BundleItemValueResponse;

            protected override void DrawInput()
            {
                PropertyField(nameof(BundleName), "Bundle Name");
                PropertyField(nameof(BundleItemKey), "Bundle Item Key");
            }

            protected override void DrawOutput()
            {
                EditorGUILayoutElements.TextField("Bundle Item ", BundleItemValueResponse, isEnabled: false);
            }
        }

        [Serializable]
        public class UpdateBundleItemExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Update Bundle Item";
            public string BundleName;
            public string BundleItemKey;
            public string NewBundleItemValue;

            protected override void DrawInput()
            {
                PropertyField(nameof(BundleName), "Bundle Name");
                PropertyField(nameof(BundleItemKey), "Bundle Item Key");
                PropertyField(nameof(NewBundleItemValue), "New Bundle Item Value");
            }
        }

        [Serializable]
        public class DeleteAllUserGameplayDataExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Delete All User Gameplay Data";
        }

        [Serializable]
        public class DeleteBundleExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Delete Bundle";
            public string BundleName;

            protected override void DrawInput()
            {
                PropertyField(nameof(BundleName), "Bundle Name");
            }
        }

        [Serializable]
        public class DeleteBundleItemsExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Delete Bundle Items";
            public string BundleName;
            public List<string> BundleItemKeys = new List<string>(new string[1]);

            protected override void DrawInput()
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel("Bundle Name");
                    using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleDictionaryInputAligned))
                    {
                        PropertyField(nameof(BundleName));
                        GUILayout.Space(4);

                        SerializedProperty serializedBundleKeys = _serializedProperty.FindPropertyRelative(nameof(BundleItemKeys));

                        EditorGUILayoutElements.SerializableExamplesList(
                            serializedBundleKeys,
                            BundleItemKeys,
                            "Bundle Item Keys");
                    }
                }
            }
        }
        #endregion

        #region Offline Support Examples API Calls

        private void CallStartRetryBackgroundThread()
        {
            Debug.Log("Calling UserGameplayData.StartRetryBackgroundThread()");

            _userGameplayData.StartRetryBackgroundThread();
        }

        private void CallStopRetryBackgroundThread()
        {
            Debug.Log("Calling UserGameplayData.StopRetryBackgroundThread()");

            _userGameplayData.StopRetryBackgroundThread();
            _cacheProcessedExampleUI.CacheProcessedResult = null; // After the user stops the retry thread they can retry another cache file
        }

        private void CallSetNetworkChangeDelegate()
        {
            Debug.Log("Calling UserGameplayData.SetNetworkChangeDelegate()");

            _userGameplayData.SetNetworkChangeDelegate((NetworkStatusChangeResults result) =>
            {
                Debug.Log($"UserGameplayData Network Change detected. IsConnectionOk: {result.IsConnectionOk}");
                _networkStatusExampleUI.NetworkStatusChangeResult = result;
            });
        }

        private void CallSetCacheProcessedDelegate()
        {
            Debug.Log("Calling UserGameplayData.SetCacheProcessedDelegate()");

            _userGameplayData.SetCacheProcessedDelegate((CacheProcessedResults result) =>
            { 
                Debug.Log($"UserGameplayData Cache processing completed with result code {result.ResultCode}");
                _cacheProcessedExampleUI.CacheProcessedResult = result;
            });
        }

        private void CallSetClientSettings()
        {
            UserGameplayDataClientSettings clientSettings = new UserGameplayDataClientSettings
            {
                ClientTimeoutSeconds = _setClientSettingsExampleUI.ClientTimeoutInSeconds,
                RetryIntervalSeconds = _setClientSettingsExampleUI.RetryIntervalSeconds,
                MaxRetryQueueSize = _setClientSettingsExampleUI.MaxRetryQueueSize,
                MaxRetries = _setClientSettingsExampleUI.MaxRetries,
                RetryStrategy = _setClientSettingsExampleUI.RetryStrategy,
                MaxExponentialRetryThreshold = _setClientSettingsExampleUI.MaxExponentialRetryThreshold,
                PaginationSize = _setClientSettingsExampleUI.PaginationSize
            };

            Debug.Log("Calling UserGameplayData.SetClientSettings()");

            _userGameplayData.SetClientSettings(clientSettings, () =>
            {
                // The result code below is not displayed in the examples UI but setting the result code marks the call as completed
                _setClientSettingsExampleUI.ResultCode = GameKitErrors.GAMEKIT_SUCCESS;
                Debug.Log($"UserGameplayData client settings set successfully.");
            });
        }

        private void CallPersistToCache()
        {
            Debug.Log("Calling UserGameplayData.PersistToCache()");

            // If you need the PersistToCache call to be blocking, which is recommended when saving OnApplicationExit, use ImmediatePersistToCache instead
            _userGameplayData.PersistToCache(_persistToCacheExampleUI.FilePath, (uint result) =>
            {
                Debug.Log($"UserGameplayData.PersistToCache() completed with result code {result}");
                _persistToCacheExampleUI.ResultCode = result;
            });
        }


        private void CallLoadFromCache()
        {
            Debug.Log("Calling UserGameplayData.LoadFromCache()");

            _userGameplayData.LoadFromCache(_loadFromCacheExampleUI.FilePath, (uint result) =>
            {
                Debug.Log($"UserGameplayData.LoadFromCache() completed with result code {result}");
                _loadFromCacheExampleUI.ResultCode = result;
            });
        }

        private void CallDropAllCachedEvents()
        {
            Debug.Log("Calling UserGameplayData.DropAllCachedEvents()");

            _userGameplayData.DropAllCachedEvents(() =>
            {
                // The result code below is not displayed in the examples UI but setting the result code marks the call as completed
                _dropAllCachedEventsExampleUI.ResultCode = GameKitErrors.GAMEKIT_SUCCESS;
                Debug.Log($"All operations that were loading from cache dropped successfully from the retry thread.");
            });
        }


        #endregion

        #region Offline Support Examples GUI Classes

        [Serializable]
        public class RetryThreadExampleUI : IDrawable
        {
            private bool _isRetryThreadStarted;

            private bool _isExampleDisplayed = true;

            private Action _startRetryThreadAction;
            private Action _stopRetryThreadAction;

            protected SerializedProperty _serializedProperty;

            public void Initialize(Action startRetryThreadAction, Action stopRetryThreadAction, SerializedProperty serializedProperty)
            {
                _startRetryThreadAction = startRetryThreadAction;
                _stopRetryThreadAction = stopRetryThreadAction;
                _serializedProperty = serializedProperty;
            }

            public void ForceStopRetryThread()
            {
                // Should be called when the user logs out to ensure the retry thread can be started again by a new user

                if (_isRetryThreadStarted)
                {
                    _isRetryThreadStarted = false;
                    _stopRetryThreadAction();
                }
            }

            public void OnGUI()
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleContainer))
                {
                    _isExampleDisplayed = EditorGUILayout.Foldout(_isExampleDisplayed, "Retry Thread", SettingsGUIStyles.Page.FoldoutTitle);
                    if (_isExampleDisplayed)
                    {
                        using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleFoldoutContainer))
                        {
                            EditorGUILayout.LabelField(RETRY_THREAD_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);
                            EditorGUILayout.Space(5);

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayoutElements.PrefixLabel("Action", 0);

                                if (_isRetryThreadStarted)
                                {
                                    if (GUILayout.Button(L10n.Tr("Stop Retry Thread"),
                                        SettingsGUIStyles.Buttons.GreyButtonNormal))
                                    {
                                        _isRetryThreadStarted = false;
                                        _stopRetryThreadAction();
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button(L10n.Tr("Start Retry Thread"),
                                        SettingsGUIStyles.Buttons.GreyButtonNormal))
                                    {
                                        _isRetryThreadStarted = true;
                                        _startRetryThreadAction();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [Serializable]
        public class NetworkStatusExampleUI : IDrawable
        {
            public NetworkStatusChangeResults NetworkStatusChangeResult;

            private bool _isExampleDisplayed = true;

            public void OnGUI()
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleContainer))
                {
                    _isExampleDisplayed = EditorGUILayout.Foldout(_isExampleDisplayed, "Last Known Network Status", SettingsGUIStyles.Page.FoldoutTitle);
                    if (_isExampleDisplayed)
                    {
                        using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleFoldoutContainer))
                        {
                            EditorGUILayout.LabelField(NETWORK_STATUS_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);

                            EditorGUILayoutElements.CustomField(L10n.Tr("Network status"), () =>
                            {
                                // GameKit will treat the first call as if we are online, status will change based on the result of the first call if the retry thread has been started
                                if (NetworkStatusChangeResult == null || NetworkStatusChangeResult.IsConnectionOk)
                                {
                                    EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Deployed);
                                    EditorGUILayout.LabelField("Online");
                                }
                                else
                                {
                                    EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Error);
                                    EditorGUILayout.LabelField("Offline");
                                }
                            }, indentationLevel: 0);
                        }
                    }
                }
            }
        }

        [Serializable]
        public class CacheProcessedExampleUI : IDrawable
        {
            public CacheProcessedResults CacheProcessedResult;

            private bool _isExampleDisplayed = true;

            public void OnGUI()
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleContainer))
                {
                    _isExampleDisplayed = EditorGUILayout.Foldout(_isExampleDisplayed, "Cache Status", SettingsGUIStyles.Page.FoldoutTitle);
                    if (_isExampleDisplayed)
                    {
                        using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleFoldoutContainer))
                        {
                            EditorGUILayout.LabelField(CACHE_PROCESSED_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);

                            EditorGUILayoutElements.CustomField(L10n.Tr("Cache Processing Status"), () =>
                            {
                                // GameKit will treat the first call as if we are online, status will change based on the result of the first call
                                if (CacheProcessedResult == null)
                                {
                                    EditorGUILayout.LabelField("No Cache Processed");
                                }
                                else if (CacheProcessedResult.IsCacheProcessed)
                                {
                                    EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Deployed);
                                    EditorGUILayout.LabelField("Cache Processed Successfully");
                                }
                                else
                                {
                                    EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Error);
                                    EditorGUILayout.LabelField("Cache Processing Failed");
                                }
                            }, indentationLevel: 0);
                        }
                    }
                }
            }
        }

        [Serializable]
        public class SetClientSettingsExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Set Client Settings";

            protected override bool _shouldDisplayDescription => true;

            protected override bool _shouldDisplayResult => false;

            public uint ClientTimeoutInSeconds = 3;
            public uint RetryIntervalSeconds = 5;
            public uint MaxRetryQueueSize = 256;
            public uint MaxRetries = 32;
            public uint RetryStrategy = 0;
            public uint MaxExponentialRetryThreshold = 32;
            public uint PaginationSize = 100;

            protected override void DrawDescription()
            {
                EditorGUILayout.LabelField(SET_CLIENT_SETTINGS_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);
            }

            protected override void DrawInput()
            {
                SerializedProperty clientTimeoutInSecondsProperty =_serializedProperty.FindPropertyRelative(nameof(ClientTimeoutInSeconds));
                ClientTimeoutInSeconds = (uint)EditorGUILayoutElements.IntSlider(L10n.Tr("Client Timeout"), clientTimeoutInSecondsProperty.intValue, 1, 100, indentationLevel: 0);
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, "Connection timeout in seconds for the internal HTTP client. Default is 3. Uses default if set to 0."));

                SerializedProperty retryIntervalInSecondsProperty = _serializedProperty.FindPropertyRelative(nameof(RetryIntervalSeconds));
                RetryIntervalSeconds = (uint)EditorGUILayoutElements.IntSlider(L10n.Tr("Retry Interval"), retryIntervalInSecondsProperty.intValue, 1, 100, indentationLevel: 0);
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, "Seconds to wait between retries. Default is 5. Uses default value if set to 0."));

                SerializedProperty maxRetryQueueSizeProperty = _serializedProperty.FindPropertyRelative(nameof(MaxRetryQueueSize));
                MaxRetryQueueSize = (uint)EditorGUILayoutElements.IntSlider(L10n.Tr("Max Retry Queue Size"), maxRetryQueueSizeProperty.intValue, 1, 1000, indentationLevel: 0);
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, "Maximum length of custom http client request queue size. Once the queue is full, new requests will be dropped. Default is 256. Uses default if set to 0."));

                SerializedProperty maxRetriesProperty = _serializedProperty.FindPropertyRelative(nameof(MaxRetries));
                MaxRetries = (uint)EditorGUILayoutElements.IntSlider(L10n.Tr("Max Retries"), maxRetriesProperty.intValue, 1, 100, indentationLevel: 0);
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, "Maximum number of times to retry a request before dropping it. Default is 32. Uses default if set to 0."));

                SerializedProperty retryStrategyProperty = _serializedProperty.FindPropertyRelative(nameof(RetryStrategy));
                RetryStrategy = (uint)EditorGUILayout.Popup("Retry Strategy", retryStrategyProperty.intValue, RETRY_STATEGY_OPTIONS.ToArray());

                SerializedProperty maxExponentialRetriesThresholdProperty = _serializedProperty.FindPropertyRelative(nameof(MaxExponentialRetryThreshold));
                MaxExponentialRetryThreshold = (uint)EditorGUILayoutElements.IntSlider(L10n.Tr("Max Exponential Retries"), maxExponentialRetriesThresholdProperty.intValue, 1, 100, indentationLevel: 0);
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, "Maximum retry threshold for Exponential Backoff. Forces a retry even if exponential backoff is set to a greater value. Default is 32. Uses default if set to 0."));

                SerializedProperty paginationSizeProperty = _serializedProperty.FindPropertyRelative(nameof(PaginationSize));
                PaginationSize = (uint)EditorGUILayoutElements.IntSlider(L10n.Tr("Pagination Size"), paginationSizeProperty.intValue, 1, 100, indentationLevel: 0);
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), new GUIContent(string.Empty, "Number of items to retrieve when executing paginated calls such as Get All Data. Default is 100. Uses default if set to 0."));
            }
        }

        [Serializable]
        public class PersistToCacheExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Persist to Cache";

            protected override bool _shouldDisplayDescription => true;

            public string FilePath;

            protected override void DrawDescription()
            {
                EditorGUILayout.LabelField(PERSIST_TO_CACHE_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);
            }

            protected override void DrawInput()
            {
                SerializedProperty filePath = _serializedProperty.FindPropertyRelative(nameof(FilePath));
                filePath.stringValue = EditorGUILayoutElements.FileSelection("File Path", filePath.stringValue, "Select a file", "", 0);
            }
        }

        [Serializable]
        public class LoadFromCacheExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Load from Cache";

            protected override bool _shouldDisplayDescription => true;

            public string FilePath;

            protected override void DrawDescription()
            {
                EditorGUILayout.LabelField(LOAD_FROM_CACHE_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);
            }

            protected override void DrawInput()
            {
                SerializedProperty filePath = _serializedProperty.FindPropertyRelative(nameof(FilePath));
                filePath.stringValue = EditorGUILayoutElements.FileSelection("File Path", filePath.stringValue, "Select a file", "", 0);
            }
        }

        [Serializable]
        public class DropAllCachedEventsExampleUI : GameKitExampleUI
        {
            public override string ApiName => "Drop all Cached Events";

            protected override bool _shouldDisplayDescription => true;

            protected override bool _shouldDisplayResult => false;

            protected override void DrawDescription()
            {
                EditorGUILayout.LabelField(DROP_CACHED_EVENTS_OVERVIEW, SettingsGUIStyles.Page.TextAreaSubtext);
            }
        }

        #endregion
    }
}