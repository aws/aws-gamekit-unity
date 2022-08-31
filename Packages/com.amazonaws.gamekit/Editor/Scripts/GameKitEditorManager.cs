// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Windows.QuickAccess;
using AWS.GameKit.Editor.Windows.Settings;
using AWS.GameKit.Runtime;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Features.GameKitAchievements;
using AWS.GameKit.Runtime.Features.GameKitGameSaving;
using AWS.GameKit.Runtime.Features.GameKitIdentity;
using AWS.GameKit.Runtime.Features.GameKitUserGameplayData;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor
{
    [InitializeOnLoad]
    public class GameKitEditorManager : Singleton<GameKitEditorManager>
    {
        public SettingsController SettingsWindowController { get; }
        public QuickAccessController QuickAccessWindowController { get; }
        public bool CredentialsSubmitted = false;

        private readonly GameKitManager _gameKitManager;
        private readonly FeatureResourceManager _featureResourceManager;
        private readonly CredentialsManager _credentialsManager;
        private readonly FeatureDeploymentOrchestrator _featureDeploymentOrchestrator;
        private readonly Threader _threader = new Threader();

        private bool _isReady = false;

        /// <summary>
        /// Initialize the GameKitEditorManager singleton and bootstrap the AWS GameKit package.<br/><br/>
        ///
        /// This static constructor is called whenever the scripts are recompiled (also known as a Domain Reload).<br/>
        /// This happens when Unity first loads, when scripts are re-compiled, and when entering Play Mode.<br/><br/>
        ///
        /// For more details, see: https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html
        /// </summary>
        static GameKitEditorManager()
        {
            // Create the singleton instance
            Get();
        }

        /// <summary>
        /// Create a new GameKitEditorManager and bootstrap the AWS GameKit package.
        /// </summary>
        public GameKitEditorManager()
        {
            // validate that the Asset folder file structure is set up correctly
            CreateNeededFolders();
            CopyInitialAssetFiles();

            // This calls the static Update method several times a minute
            EditorApplication.update += Update;

            // Declare and initialize class fields
            _gameKitManager = Singleton<GameKitManager>.Get();
            _featureResourceManager = new FeatureResourceManager(CoreWrapper.Get(), GameKitPaths.Get(), _threader);
            _credentialsManager = new CredentialsManager();
            _featureDeploymentOrchestrator = new FeatureDeploymentOrchestrator(_featureResourceManager.Paths.PACKAGES_BASE_TEMPLATES_FULL_PATH, _featureResourceManager.Paths.ASSETS_INSTANCE_FILES_FULL_PATH);
            
            // Handle initial saveInfo.yml logic
            BootstrapExistingState();

            // Post-bootstrap member initialization
            AchievementsAdmin.AchievementsAdmin achievementsAdmin = GameKitFeature<AchievementsAdmin.AchievementsAdmin>.Get();
            achievementsAdmin.Initialize(_featureResourceManager, _credentialsManager);
            _gameKitManager.AddEditorOnlyFeature(achievementsAdmin);

            SettingsDependencyContainer settingsDependencyContainer = new SettingsDependencyContainer
            {
                // Editor
                GameKitEditorManager = this,
                CredentialsManager = _credentialsManager,

                // Runtime
                GameKitManager = _gameKitManager,
                CoreWrapper = CoreWrapper.Get(),
                FileManager = new FileManager(),

                // Feature management
                FeatureResourceManager = _featureResourceManager,
                FeatureDeploymentOrchestrator = _featureDeploymentOrchestrator,

                // Features
                Achievements = GameKitFeature<Achievements>.Get(),
                AchievementsAdmin = achievementsAdmin,
                GameSaving = GameKitFeature<GameSaving>.Get(),
                Identity = GameKitFeature<Identity>.Get(),
                UserGameplayData = GameKitFeature<UserGameplayData>.Get(),

                // User State
                UserInfo = new UserInfo(),

                // Events
                OnEnvironmentOrRegionChange = new UnityEvent()
            };

            SettingsWindowController = new SettingsController(settingsDependencyContainer);
            QuickAccessWindowController = new QuickAccessController(SettingsWindowController, settingsDependencyContainer);

            // Declare the GameKitEditorContext to be in a ready to use state
            _isReady = true;
        }

        /// <summary>
        /// Execute the callback functions queued by Threader  
        /// </summary>
        public void Update()
        {
            // do not execute while the application is running
            if (!Application.isPlaying)
            {
                // Validate that the GameKitRuntimeManager script is present and active
                GameKitRuntimeManager.KeepGameKitObjectAlive();
            }

            // force update to be called more frequently in editor mode
            EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;

            if (_isReady)
            {
                _threader.Update();
                _featureDeploymentOrchestrator.Update();

                SettingsWindowUpdateController.Update();
            }    
        }

        /// <summary>
        /// Create feature resource manager and bootstrap account  
        /// </summary>
        /// <param name="saveInfoFile">File path location of the saveInfo.yml file</param>
        public void PopulateInformation(string saveInfoFile)
        {
            string gameName = Path.GetFileName(Path.GetDirectoryName(saveInfoFile));
            _featureResourceManager.SetGameName(gameName);

            // Create accountDetails which will be used to populate accountInfo and accountCredentials using SetAccountDetails
            AccountDetails accountDetails = new AccountDetails();
            accountDetails.GameName = gameName;
            accountDetails.Environment = _featureResourceManager.GetLastUsedEnvironment();
            accountDetails.Region = _featureResourceManager.GetLastUsedRegion();
            
            // CredentialsManager retrieves access key and secret key based on the game and environment
            _credentialsManager.SetGameName(accountDetails.GameName);
            _credentialsManager.SetEnv(accountDetails.Environment);

            // Populate accountDetails with the keys generated by the credentialsManager
            // and the accountId retrieved using featureResourceManager

            if (!_credentialsManager.CheckAwsProfileExists(accountDetails.GameName, accountDetails.Environment))
            {
                Debug.LogWarning(L10n.Tr($"The credentials associated with the last used environment, {accountDetails.Environment}, for {accountDetails.GameName} could not be found. It is possible they have been deleted from the ~/.aws/credentials file."));
                return;
            }

            accountDetails.AccessKey = _credentialsManager.GetAccessKey();
            accountDetails.AccessSecret = _credentialsManager.GetSecretAccessKey();

            GetAWSAccountIdDescription accountCredentials;
            accountCredentials.AccessKey = accountDetails.AccessKey;
            accountCredentials.AccessSecret = accountDetails.AccessSecret;

            StringCallbackResult result = CoreWrapper.Get().GetAWSAccountId(accountCredentials);

            if (result.ResultCode != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Debug.LogError("AWS Account ID could not be retrieved, this may be due to the IAM role for your last used environment being changed or deleted.");
                return;
            }

            accountDetails.AccountId = result.ResponseValue;

            _featureResourceManager.SetAccountDetails(accountDetails);
        }

        /// <summary>
        /// Reload all feature settings for all settings tabs that exist in session
        /// </summary>
        public void ReloadAllFeatureSettings()
        {
            foreach (var featureSettingsTab in FeatureSettingsTab.FeatureSettingsTabsInstances)
            {
                featureSettingsTab.ReloadFeatureSettings();
            }
        }

        /// <summary>
        /// Create feature resource manager and bootstrap account  
        /// </summary>
        private void BootstrapExistingState()
        {
            string instanceFilesFolder = _featureResourceManager.Paths.ASSETS_INSTANCE_FILES_RELATIVE_PATH;
            ICollection<string> saveInfoFiles = Directory.GetFiles(instanceFilesFolder, GameKitPaths.Get().SAVE_INFO_FILE_NAME, SearchOption.AllDirectories);
            
            if (saveInfoFiles.Count <= 0)
            {
                return;
            }
            
            IEnumerator<string> itr = saveInfoFiles.GetEnumerator();
            itr.MoveNext();

            // Call PopulateInformation Asynchronously
            _threader.Call(PopulateInformation, itr.Current, () =>
            {
                // The following code needs to be run on Unity's main thread because it calls AssetDatabase.ImportAsset() and uses Application.dataPath.

                // Copy over the config file in case Unity was closed during a feature deployment
                _gameKitManager.CopyAndReloadConfigFile(_featureResourceManager.GetGameName(), _featureResourceManager.GetLastUsedEnvironment());

                Debug.Log("GameKitEditorContext::PopulateInformation completed");
            });
        }

        private static void CreateNeededFolders()
        {
            // Make sure gamekit assets folder(s) exists.
            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(GameKitPaths.Get().ASSETS_DATA_FOLDER_NAME, GameKitPaths.Get().GAME_KIT_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_RESOURCES_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(GameKitPaths.Get().ASSETS_RELATIVE_PATH, GameKitPaths.Get().RESOURCE_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_EDITOR_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(GameKitPaths.Get().ASSETS_RELATIVE_PATH, GameKitPaths.Get().EDITOR_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_EDITOR_RESOURCES_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(Path.Combine(GameKitPaths.Get().ASSETS_EDITOR_RELATIVE_PATH), GameKitPaths.Get().RESOURCE_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_WINDOW_STATE_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(Path.Combine(GameKitPaths.Get().ASSETS_EDITOR_RELATIVE_PATH), GameKitPaths.Get().WINDOWS_STATE_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_CLOUD_RESOURCES_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(GameKitPaths.Get().ASSETS_EDITOR_RELATIVE_PATH, GameKitPaths.Get().CLOUD_RESOURCES_FOLDER_NAME);
            }

            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_INSTANCE_FILES_RELATIVE_PATH))
            {
                AssetDatabase.CreateFolder(GameKitPaths.Get().ASSETS_CLOUD_RESOURCES_RELATIVE_PATH, GameKitPaths.Get().INSTANCE_FILES_FOLDER_NAME);
            }

#if UNITY_ANDROID
            if (!AssetDatabase.IsValidFolder(GameKitPaths.Get().ASSETS_GAMEKIT_RAW_RELATIVE_PATH))
            {
                // Recursively create all the directories up to raw folder
                Directory.CreateDirectory(GameKitPaths.Get().ASSETS_GAMEKIT_RAW_RELATIVE_PATH);
            }
#endif
        }

        private static void CopyInitialAssetFiles()
        {
            // Copy readme files if they don't yet exist
            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_README_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_RESOURCES_README_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_README_RELATIVE_PATH);
            }

            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_EDITOR_RESOURCES_README_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_README_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_EDITOR_RESOURCES_README_RELATIVE_PATH);
            }

            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_GIT_IGNORE_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_GIT_IGNORE_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_GIT_IGNORE_RELATIVE_PATH);
            }

            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_CLOUD_RESOURCES_README_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_CLOUD_RESOURCES_README_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_CLOUD_RESOURCES_README_RELATIVE_PATH);
            }

            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_WINDOW_STATE_README_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_WINDOW_STATE_README_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_WINDOW_STATE_README_RELATIVE_PATH);
            }

            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_INSTANCE_FILES_README_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_INSTANCE_FILES_README_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_INSTANCE_FILES_README_RELATIVE_PATH);
            }

#if UNITY_ANDROID
            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_GAMEKIT_RAW_CERT_RELATIVE_PATH))) 
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_CACERT_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_GAMEKIT_RAW_CERT_RELATIVE_PATH);
            }

            if (!File.Exists(Path.GetFullPath(GameKitPaths.Get().ASSETS_GAMEKIT_ANDROID_MANIFEST_RELATIVE_PATH)))
            {
                AssetDatabase.CopyAsset(GameKitPaths.Get().PACKAGES_ANDROID_MANIFEST_RELATIVE_PATH,
                    GameKitPaths.Get().ASSETS_GAMEKIT_ANDROID_MANIFEST_RELATIVE_PATH);
            }
#endif
        }
    }
}
