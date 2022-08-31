// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Features.GameKitUserGameplayData;
using AWS.GameKit.Runtime.Features.GameKitGameSaving;
using AWS.GameKit.Runtime.Features.GameKitAchievements;
using AWS.GameKit.Runtime.Features.GameKitIdentity;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Core
{
    /// <summary>
    /// Manager for all GameKit features.
    /// </summary>
    public class GameKitManager : IDisposable
    {
        /// <summary>
        /// The count of how many features are currently being managed by the GameKitManager.
        /// </summary>
        public int FeatureCount => _features.Count;

        protected List<GameKitFeatureBase> _features = new List<GameKitFeatureBase>();
        
        private SessionManager _sessionManager = SessionManager.Get();

        private bool _disposedValue = false;
        private bool _isInitialized = false;

        /// <summary>
        /// Constructs a GameKitManager object.
        /// </summary>
        /// <remarks>
        /// This should only be called from within Unity's main game thread.
        /// </remarks>
        public GameKitManager()
        {
            AddFeatures();
        }

        ~GameKitManager() => Dispose();

        /// <summary>
        /// Public initializer used to manually initialize feature where required.
        /// </summary>
        /// <param name="reinitialize">When true, forces reinitialization of all features even if they are already initialized.</param>
        public void EnsureFeaturesAreInitialized(bool reinitialize = false)
        {
            if (!_isInitialized || reinitialize)
            {
                Logging.LogInfo("GameKitManager: initializing features.");

                ReloadConfigFile();

                // initialize all of the features in list order
                _features.ForEach(feature => feature.OnInitialize());
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Public implementation of Dispose pattern callable by consumers. Used to manually cleanup the GameKitManager.
        /// </summary>
        public void Dispose()
        {
            if (!_disposedValue)
            {
                Logging.LogInfo("GameKitManager: cleaning up features.");
                
                // cleanup each of the features in reverse list order
                try
                {
                    for (int i = _features.Count - 1; i >= 0; --i)
                    {
                        _features[i].OnDispose();
                    }
                }
                finally
                {
                    _isInitialized = false;
                }

                // release the session manager's instance
                _sessionManager.Release();
                
                _disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Public OnApplicationPause method for the GameKitManager, this is intended to be called from a Monbehavior OnApplicationPauseMethod.
        /// This can be used to notify features that a game instance has been paused or unpaused.
        /// <param name="isPaused">True if the application is paused, else False.</param>
        /// </summary>
        public void OnApplicationPause(bool isPaused)
        {
            // Notify all features that the application pause status has been changed
            _features.ForEach(feature => feature.OnApplicationPause(isPaused));
        }

        /// <summary>
        /// Public update method for the GameKitManager, this is intended to only be called during a Monobehavior update or editor update.
        /// </summary>
        public void Update()
        {
            // update each feature
            _features.ForEach(feature => feature.Update());
        }

        /// <summary>
        /// See SessionManager.ReloadConfig().
        /// </summary>
        /// <remarks>
        /// This method must be called before using any GameKit feature APIs.
        /// </remarks>
        /// <returns>True if every feature's settings are loaded in memory from the "awsGameKitClientConfig.yml" file, false otherwise.</returns>
        public bool ReloadConfigFile()
        {
#if (UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS) && !UNITY_EDITOR
            _sessionManager.ReloadConfigMobile();
#else
            _sessionManager.ReloadConfig();
#endif
            
            return AreAllFeatureSettingsLoaded();
        }

        /// <summary>
        /// Return true if every feature's settings are loaded in memory from the config file, false otherwise.
        /// </summary>
        public bool AreAllFeatureSettingsLoaded()
        {
            bool result = false;

            _features.ForEach(feature => { result &= AreFeatureSettingsLoaded(feature.FeatureType); });

            return result;
        }

        /// <summary>
        /// See SessionManager.AreSettingsLoaded().
        /// </summary>
        /// <remarks>
        /// These settings are found in file "awsGameKitClientConfig.yml" which is generated by GameKit each time you deploy or re-deploy a feature.
        /// The file is loaded by calling either SessionManagerWrapper.Create() or SessionManagerWrapper.ReloadConfigFile().
        /// </remarks>
        /// <param name="type">FeatureType enum that is being checked</param>
        /// <returns>True if the settings for the feature are loaded, false otherwise.</returns>
        public bool AreFeatureSettingsLoaded(FeatureType type)
        {
            return _sessionManager.AreSettingsLoaded(type);
        }

#if UNITY_EDITOR
        /// <summary>
        /// See SessionManager.CopyAndReloadConfig().
        /// </summary>
        /// <param name="gameAlias">The game's alias, ex: "mygame".</param>
        /// <param name="environmentCode">The environment to copy the config from, ex: "dev".</param>
        /// <returns>True if the "awsGameKitClientConfig.yml" was reloaded successfully, false otherwise.</returns>
        public void CopyAndReloadConfigFile(string gameAlias, string environmentCode)
        {
            _sessionManager.CopyAndReloadConfig(gameAlias, environmentCode);

#if UNITY_ANDROID
            _sessionManager.CopyConfigToMobileAssets(gameAlias, environmentCode);
#endif
        }

        /// <summary>
        /// See SessionManager.DoesConfigFileExist().
        /// </summary>
        /// <param name="gameAlias">The game's alias, ex: "mygame".</param>
        /// <param name="environmentCode">The environment to copy the config from, ex: "dev".</param>
        public bool DoesConfigFileExist(string gameAlias, string environmentCode)
        {
            return _sessionManager.DoesConfigFileExist(gameAlias, environmentCode);
        }
#endif

#if UNITY_EDITOR
        /// <summary>
        /// Add a feature which is only used inside the Unity editor. This method should only be called by AWS GameKit. Game developers don't need to call this method.
        /// </summary>
        public void AddEditorOnlyFeature(GameKitFeatureBase feature)
        {
            _features.Add(feature);

            EnsureFeaturesAreInitialized(reinitialize: true);
        }
#endif

        protected virtual void AddFeatures()
        {
            // add features that need to be initialized first
            _features.Add(Identity.Get());

            // add all features, if a feature is not wanted and the DLLs are removed then it should be commented out here as well
            _features.Add(Achievements.Get());
            _features.Add(UserGameplayData.Get());
            _features.Add(GameSaving.Get());
        }

        protected void SetSessionManager(SessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }
    }
}

