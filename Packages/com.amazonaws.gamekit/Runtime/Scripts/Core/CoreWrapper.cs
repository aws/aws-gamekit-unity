// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Exceptions;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Core
{
    /// <summary>
    /// Implements ICoreWrapper and ICoreWrapperProvider
    /// </summary>
    public class CoreWrapper : Singleton<CoreWrapper>, ICoreWrapperProvider
    {
#if UNITY_EDITOR
        
        // Select the correct source path based on the platform
        private const string IMPORT = "aws-gamekit-core";
        
        // Saved session manager instance
        private IntPtr _accountInstance = IntPtr.Zero;
        private IntPtr _featureResourcesInstance = IntPtr.Zero;
        private IntPtr _settingsInstance = IntPtr.Zero;
        private IntPtr _featureStatusInstance = IntPtr.Zero;
        private IntPtr _deploymentOrchestrator = IntPtr.Zero;

        [DllImport(IMPORT)] private static extern uint GameKitGetAwsAccountId(IntPtr dispatchReceiver, FuncStringCallback responseCb, string accessKey, string secretKey, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern IntPtr GameKitAccountInstanceCreate(AccountInfo accountInfo, AccountCredentials credentials, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern IntPtr GameKitAccountInstanceCreateWithRootPaths(AccountInfo accountInfo, AccountCredentials credentials, string rootPath, string pluginRootPath, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitAccountInstanceRelease(IntPtr accountInstance);
        [DllImport(IMPORT)] private static extern bool GameKitAccountHasValidCredentials(IntPtr accountInstance);
        [DllImport(IMPORT)] private static extern uint GameKitAccountInstanceBootstrap(IntPtr accountInstance);
        [DllImport(IMPORT)] private static extern uint GameKitAccountSaveSecret(IntPtr accountInstance, string secretName, string secretValue);
        [DllImport(IMPORT)] private static extern uint GameKitAccountCheckSecretExists(IntPtr accountInstance, string secretName);
        [DllImport(IMPORT)] private static extern uint GameKitAccountUploadAllDashboards(IntPtr accountInstance);
        [DllImport(IMPORT)] private static extern IntPtr GameKitResourcesInstanceCreate(AccountInfo accountInfo, AccountCredentials credentials, FeatureType featureType, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern IntPtr GameKitResourcesInstanceCreateWithRootPaths(AccountInfo accountInfo, AccountCredentials credentials, FeatureType featureType, string rootPath, string pluginRootPath, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitResourcesInstanceRelease(IntPtr resourcesInstance);
        [DllImport(IMPORT)] private static extern uint GameKitResourcesCreateEmptyConfigFile(IntPtr resourcesInstance);
        [DllImport(IMPORT)] private static extern IntPtr GameKitSettingsInstanceCreate(string rootPath, string pluginVersion, string shortGameName, string currentEnvironment, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitSettingsInstanceRelease(IntPtr settingsInstance);
        [DllImport(IMPORT)] private static extern void GameKitSettingsSetGameName(IntPtr settingsInstance, string gameName);
        [DllImport(IMPORT)] private static extern void GameKitSettingsSetLastUsedRegion(IntPtr settingsInstance, string region);
        [DllImport(IMPORT)] private static extern void GameKitSettingsSetLastUsedEnvironment(IntPtr settingsInstance, string envCode);
        [DllImport(IMPORT)] private static extern void GameKitSettingsAddCustomEnvironment(IntPtr settingsInstance, string envCode, string envDescription);
        [DllImport(IMPORT)] private static extern void GameKitSettingsSetFeatureVariables(IntPtr settingsInstance, FeatureType featureType, string[] varKeys, string[] varValues, ulong numKeys);
        [DllImport(IMPORT)] private static extern uint GameKitSettingsSave(IntPtr settingsInstance);
        [DllImport(IMPORT)] private static extern uint GameKitSettingsPopulateAndSave(IntPtr settingsInstance, string gameName, string envCode, string region);
        [DllImport(IMPORT)] private static extern void GameKitSettingsGetGameName(IntPtr settingsInstance, IntPtr dispatchReceiver, FuncStringCallback responseCb);
        [DllImport(IMPORT)] private static extern void GameKitSettingsGetLastUsedRegion(IntPtr settingsInstance, IntPtr dispatchReceiver, FuncStringCallback responseCb);
        [DllImport(IMPORT)] private static extern void GameKitSettingsGetLastUsedEnvironment(IntPtr settingsInstance, IntPtr dispatchReceiver, FuncStringCallback responseCb);
        [DllImport(IMPORT)] private static extern void GameKitSettingsGetCustomEnvironments(IntPtr settingsInstance, IntPtr dispatchReceiver, FuncKeyValueStringCallback responseCb);
        [DllImport(IMPORT)] private static extern void GameKitSettingsGetFeatureVariables(IntPtr settingsInstance, IntPtr dispatchReceiver, FeatureType featureType, FuncKeyValueStringCallback responseCb);
        [DllImport(IMPORT)] private static extern void GameKitSettingsGetSettingsFilePath(IntPtr settingsInstance, IntPtr dispatchReceiver, FuncStringCallback responseCb);
        [DllImport(IMPORT)] private static extern uint GameKitSaveAwsCredentials(string profileName, string accessKey, string secretKey, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern bool GameKitAwsProfileExists(string profileName);
        [DllImport(IMPORT)] private static extern uint GameKitSetAwsAccessKey(string profileName, string newAccessKey, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern uint GameKitSetAwsSecretKey(string profileName, string newSecretKey, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern uint GameKitGetAwsProfile(string profileName, IntPtr dispatchReceiver, FuncKeyValueStringCallback responseCb, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern IntPtr GameKitDeploymentOrchestratorCreate(string baseTemplatesFolder, string instanceFilesFolder, string sourceEngine, string pluginVersion, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitDeploymentOrchestratorInstanceRelease(IntPtr deploymentOrchestratorInstance);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorSetCredentials(IntPtr deploymentOrchestratorInstance, AccountInfo accountInfo, AccountCredentials accountCredentials);
        [DllImport(IMPORT)] private static extern FeatureStatus GameKitDeploymentOrchestratorGetFeatureStatus(IntPtr deploymentOrchestratorInstance, FeatureType feature);
        [DllImport(IMPORT)] private static extern FeatureStatusSummary GameKitDeploymentOrchestratorGetFeatureStatusSummary(IntPtr deploymentOrchestratorInstance, FeatureType feature);
        [DllImport(IMPORT)] private static extern bool GameKitDeploymentOrchestratorIsFeatureDeploymentInProgress(IntPtr deploymentOrchestratorInstance, FeatureType feature);
        [DllImport(IMPORT)] private static extern bool GameKitDeploymentOrchestratorIsFeatureUpdating(IntPtr deploymentOrchestratorInstance, FeatureType feature);
        [DllImport(IMPORT)] private static extern bool GameKitDeploymentOrchestratorIsAnyFeatureUpdating(IntPtr deploymentOrchestratorInstance);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorRefreshFeatureStatus(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncDeploymentResponseCallback resultCb);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorRefreshFeatureStatuses(IntPtr deploymentOrchestratorInstance, IntPtr receiver, FuncDeploymentResponseCallback resultCb);
        [DllImport(IMPORT)] private static extern bool GameKitDeploymentOrchestratorCanCreateFeature(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncCanExecuteDeploymentActionCallback resultCb);
        [DllImport(IMPORT)] private static extern bool GameKitDeploymentOrchestratorCanRedeployFeature(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncCanExecuteDeploymentActionCallback resultCb);
        [DllImport(IMPORT)] private static extern bool GameKitDeploymentOrchestratorCanDeleteFeature(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncCanExecuteDeploymentActionCallback resultCb);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorCreateFeature(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncDeploymentResponseCallback resultCb);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorRedeployFeature(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncDeploymentResponseCallback resultCb);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorDeleteFeature(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncDeploymentResponseCallback resultCb);
        [DllImport(IMPORT)] private static extern uint GameKitDeploymentOrchestratorDescribeFeatureResources(IntPtr deploymentOrchestratorInstance, FeatureType feature, IntPtr receiver, FuncResourceInfoCallback resultCb);
        
        #region GameKitAccount

        public StringCallbackResult GetAWSAccountId(GetAWSAccountIdDescription iamAccountCredentials)
        {
            StringCallbackResult results = new StringCallbackResult();

            uint status  = DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitGetAwsAccountId(dispatchReceiver, GameKitCallbacks.StringCallback, iamAccountCredentials.AccessKey, iamAccountCredentials.AccessSecret, Logging.LogCb), nameof(GameKitGetAwsAccountId), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            results.ResultCode = status;

            return results;
        }

        public IntPtr GetAccountInstance()
        {
            if (_accountInstance == IntPtr.Zero)
            {
                throw new GameKitInstanceNotFound("CoreWrapper::AccountInstanceCreate() must be called with proper account and credential details before any account methods can be used");
            }

            return _accountInstance;
        }

        public IntPtr AccountInstanceCreate(AccountInfo accountInfo, AccountCredentials credentials, FuncLoggingCallback logCb)
        {
            _accountInstance = DllLoader.TryDll(() => GameKitAccountInstanceCreate(accountInfo, credentials, logCb), nameof(GameKitAccountInstanceCreate), IntPtr.Zero);
            return _accountInstance;
        }

        public IntPtr AccountInstanceCreateWithRootPaths(AccountInfo accountInfo, AccountCredentials credentials, string rootPath, string pluginRootPath, FuncLoggingCallback logCb)
        {
            _accountInstance = DllLoader.TryDll(() => GameKitAccountInstanceCreateWithRootPaths(accountInfo, credentials, rootPath, pluginRootPath, logCb), nameof(GameKitAccountInstanceCreateWithRootPaths), IntPtr.Zero);
            return _accountInstance;
        }

        public void AccountInstanceRelease()
        {
            if (_accountInstance != IntPtr.Zero)
            {
                DllLoader.TryDll(() => GameKitAccountInstanceRelease(GetAccountInstance()),
                    nameof(GameKitAccountInstanceRelease));
                _accountInstance = IntPtr.Zero;
            }
        }

        public bool AccountHasValidCredentials()
        {
            return DllLoader.TryDll(() => GameKitAccountHasValidCredentials(GetAccountInstance()), nameof(GameKitAccountHasValidCredentials), false);
        }

        public uint AccountInstanceBootstrap()
        {
            return DllLoader.TryDll(() => GameKitAccountInstanceBootstrap(GetAccountInstance()), nameof(GameKitAccountInstanceBootstrap), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint GameKitAccountSaveSecret(string secretName, string secretValue)
        {
            return DllLoader.TryDll(() => GameKitAccountSaveSecret(GetAccountInstance(), secretName, secretValue), nameof(GameKitAccountSaveSecret), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint GameKitAccountCheckSecretExists(string secretName)
        {
            if (_accountInstance != IntPtr.Zero)
            {
                return DllLoader.TryDll(() => GameKitAccountCheckSecretExists(GetAccountInstance(), secretName), nameof(GameKitAccountCheckSecretExists), GameKitErrors.GAMEKIT_ERROR_GENERAL);
            }

            return GameKitErrors.GAMEKIT_WARNING_SECRETSMANAGER_SECRET_NOT_FOUND;
        }

        public uint GameKitAccountUploadAllDashboards()
        {
            return DllLoader.TryDll(() => GameKitAccountUploadAllDashboards(GetAccountInstance()), nameof(GameKitAccountUploadAllDashboards), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        #endregion

        #region GameKitResources

        public IntPtr GetResourcesInstance()
        {
            if ((_featureResourcesInstance) == IntPtr.Zero)
            {
                throw new GameKitInstanceNotFound("CoreWrapper::ResourcesInstanceCreate() must be called with account and credentials information before any account methods can be used");
            }

            return _featureResourcesInstance;
        }

        public IntPtr ResourcesInstanceCreate(AccountInfo accountInfo, AccountCredentials credentials, FeatureType featureType, FuncLoggingCallback logCb)
        {
            _featureResourcesInstance = DllLoader.TryDll(() => GameKitResourcesInstanceCreate(accountInfo, credentials, featureType, logCb), nameof(GameKitResourcesInstanceCreate), IntPtr.Zero);
            return _featureResourcesInstance;
        }

        public IntPtr ResourcesInstanceCreateWithRootPaths(AccountInfo accountInfo, AccountCredentials credentials, FeatureType featureType, string rootPath, string pluginRootPath, FuncLoggingCallback logCb)
        {
            _featureResourcesInstance = DllLoader.TryDll(() => GameKitResourcesInstanceCreateWithRootPaths(accountInfo, credentials, featureType, rootPath, pluginRootPath, logCb), nameof(GameKitResourcesInstanceCreateWithRootPaths), IntPtr.Zero);
            return _featureResourcesInstance;
        }

        public void ResourcesInstanceRelease()
        {
            if (_featureResourcesInstance != IntPtr.Zero)
            {
                DllLoader.TryDll(() => GameKitResourcesInstanceRelease(GetResourcesInstance()),
                    nameof(GameKitResourcesInstanceRelease));
                _featureResourcesInstance = IntPtr.Zero;
            }
        }

        public uint ResourcesCreateEmptyConfigFile()
        {
            return DllLoader.TryDll(() => GameKitResourcesCreateEmptyConfigFile(GetResourcesInstance()), nameof(GameKitResourcesCreateEmptyConfigFile), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        #endregion

        #region GameKitSettings

        public IntPtr GetSettingsInstance()
        {
            if (_settingsInstance == IntPtr.Zero)
            {
                throw new GameKitInstanceNotFound("CoreWrapper::SettingsInstanceCreate() must be called with proper paths before any settings methods can be used");
            }

            return _settingsInstance;
        }

        public IntPtr SettingsInstanceCreate(string rootPath, string pluginVersion, string shortGameName, string currentEnvironment, FuncLoggingCallback logCb)
        {
            if (_settingsInstance == IntPtr.Zero)
            {
                _settingsInstance = DllLoader.TryDll<IntPtr>(() => GameKitSettingsInstanceCreate(rootPath, pluginVersion, shortGameName, currentEnvironment, logCb), nameof(GameKitSettingsInstanceCreate), IntPtr.Zero);
            }

            return _settingsInstance;
        }

        public void SettingsInstanceRelease()
        {
            if (_settingsInstance != IntPtr.Zero)
            {
                DllLoader.TryDll(() => GameKitSettingsInstanceRelease(GetSettingsInstance()), nameof(GameKitSettingsInstanceRelease));
                _settingsInstance = IntPtr.Zero;
            }
        }

        public void SettingsSetGameName(string gameName)
        {
            DllLoader.TryDll(() => GameKitSettingsSetGameName(GetSettingsInstance(), gameName), nameof(GameKitSettingsSetGameName));
        }

        public string SettingsGetGameName()
        {
            StringCallbackResult results = new StringCallbackResult();

            DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitSettingsGetGameName(GetSettingsInstance(), dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitSettingsGetGameName));

            return results.ResponseValue;
        }

        public void SettingsSetLastUsedRegion(string region)
        {
            DllLoader.TryDll(() => GameKitSettingsSetLastUsedRegion(GetSettingsInstance(), region), nameof(GameKitSettingsSetLastUsedRegion));
        }

        public string SettingsGetLastUsedRegion()
        {
            StringCallbackResult results = new StringCallbackResult();

            DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitSettingsGetLastUsedRegion(GetSettingsInstance(), dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitSettingsGetLastUsedRegion));
            
            return results.ResponseValue;
        }

        public void SettingsSetLastUsedEnvironment(string envcode)
        {
            DllLoader.TryDll(() => GameKitSettingsSetLastUsedEnvironment(GetSettingsInstance(), envcode), nameof(GameKitSettingsSetLastUsedEnvironment));
        }

        public string SettingsGetLastUsedEnvironment()
        {
            StringCallbackResult results = new StringCallbackResult();
            
            DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitSettingsGetLastUsedEnvironment(GetSettingsInstance(), dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitSettingsGetLastUsedEnvironment));
            
            return results.ResponseValue;
        }

        public Dictionary<string, string> SettingsGetCustomEnvironments()
        {
            MultiKeyValueStringCallbackResult callbackResults = new MultiKeyValueStringCallbackResult();

            DllLoader.TryDll(callbackResults, (IntPtr dispatchReceiver) => GameKitSettingsGetCustomEnvironments(GetSettingsInstance(), dispatchReceiver, GameKitCallbacks.MultiKeyValueStringCallback), nameof(GameKitSettingsGetCustomEnvironments));

            Dictionary<string, string> results = new Dictionary<string, string>();
            for (uint i = 0; i < callbackResults.ResponseKeys.Length; ++i)
            {
                results[callbackResults.ResponseKeys[i]] = callbackResults.ResponseValues[i];
            }

            return results;
        }

        public Dictionary<string, string> SettingsGetFeatureVariables(FeatureType featureType)
        {
            MultiKeyValueStringCallbackResult callbackResults = new MultiKeyValueStringCallbackResult();

            DllLoader.TryDll(callbackResults, (IntPtr dispatchReceiver) => GameKitSettingsGetFeatureVariables(GetSettingsInstance(), dispatchReceiver, featureType, GameKitCallbacks.MultiKeyValueStringCallback), nameof(GameKitSettingsGetFeatureVariables));

            Dictionary<string, string> results = new Dictionary<string, string>();
            for (uint i = 0; i < callbackResults.ResponseKeys.Length; ++i)
            {
                results[callbackResults.ResponseKeys[i]] = callbackResults.ResponseValues[i];
            }

            return results;
        }

        public void SettingsAddCustomEnvironment(string envCode, string envDescription)
        {
            DllLoader.TryDll(() => GameKitSettingsAddCustomEnvironment(GetSettingsInstance(), envCode, envDescription), nameof(GameKitSettingsAddCustomEnvironment));
        }

        public void SettingsSetFeatureVariables(FeatureType featureType, string[] varKeys, string[] varValues, ulong numKeys)
        {
            DllLoader.TryDll(() => GameKitSettingsSetFeatureVariables(GetSettingsInstance(), featureType, varKeys, varValues, numKeys), nameof(GameKitSettingsAddCustomEnvironment));
        }

        public uint SettingsSave()
        {
            return DllLoader.TryDll(() => GameKitSettingsSave(GetSettingsInstance()), nameof(GameKitSettingsSave), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint SettingsPopulateAndSave(string gameName, string envCode, string region)
        {
            return DllLoader.TryDll(() => GameKitSettingsPopulateAndSave(GetSettingsInstance(), gameName, envCode, region), nameof(GameKitSettingsPopulateAndSave), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public string SettingsGetSettingsFilePath()
        {
            StringCallbackResult results = new StringCallbackResult();

            DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitSettingsGetSettingsFilePath(GetSettingsInstance(), dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitSettingsGetSettingsFilePath));
            
            return results.ResponseValue;
        }

        public uint SaveAWSCredentials(string profileName, string accessKey, string secretKey, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitSaveAwsCredentials(profileName, accessKey, secretKey, logCb), nameof(GameKitSaveAwsCredentials), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }
        public bool AwsProfileExists(string profileName)
        {
            return DllLoader.TryDll(() => GameKitAwsProfileExists(profileName), nameof(GameKitAwsProfileExists), false);
        }

        public uint SetAWSAccessKey(string profileName, string newAccessKey, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitSetAwsAccessKey(profileName, newAccessKey, logCb), nameof(GameKitSetAwsAccessKey), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint SetAWSSecretKey(string profileName, string newSecretKey, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitSetAwsSecretKey(profileName, newSecretKey, logCb), nameof(GameKitSetAwsSecretKey), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public KeyValueStringCallbackResult GetAWSProfile(string profileName, FuncLoggingCallback logCb)
        {
            KeyValueStringCallbackResult results = new KeyValueStringCallbackResult();

            uint status = DllLoader.TryDll(results, (IntPtr dispatchReceiver) => GameKitGetAwsProfile(profileName, dispatchReceiver, GameKitCallbacks.KeyValueStringCallback, logCb), nameof(GameKitGetAwsProfile), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            results.ResultCode = status;

            return results;
        }
        #endregion

        #region Deployment Orchestrator

        public IntPtr GetDeploymentOrchestratorInstance()
        {
            if (_deploymentOrchestrator == IntPtr.Zero)
            {
                throw new GameKitInstanceNotFound("CoreWrapper::DeploymentOrchestratorInstanceCreate() must be called with proper paths before any settings methods can be used");
            }

            return _deploymentOrchestrator;
        }

        public IntPtr DeploymentOrchestratorInstanceCreate(string baseTemplatesFolder, string instanceFilesFolder)
        {
            if (_deploymentOrchestrator == IntPtr.Zero)
            {
                string sourceEngine = "UNITY";
                string version = "UNKNOWN";
                UnityEditor.PackageManager.Requests.ListRequest req = UnityEditor.PackageManager.Client.List();

                while (!req.IsCompleted)
                {
                    // Keep sleeping until all package info is read from disk, takes about 0.5 - 1.5 seconds for request to read as complete
                    System.Threading.Thread.Sleep(250);
                }

                foreach (UnityEditor.PackageManager.PackageInfo item in req.Result)
                {
                    if (item.name == GameKitPaths.Get().GAME_KIT_FOLDER_NAME)
                    {
                        version = item.version;
                    }
                }

                _deploymentOrchestrator = DllLoader.TryDll<IntPtr>(() => GameKitDeploymentOrchestratorCreate(
                        baseTemplatesFolder,
                        instanceFilesFolder,
                        sourceEngine,
                        version,
                        Logging.LogCb),
                    nameof(GameKitDeploymentOrchestratorCreate),
                    IntPtr.Zero);
            }

            return _deploymentOrchestrator;
        }

        public void DeploymentOrchestratorInstanceRelease()
        {
            if (_deploymentOrchestrator != IntPtr.Zero)
            {
                DllLoader.TryDll(() => GameKitDeploymentOrchestratorInstanceRelease(GetDeploymentOrchestratorInstance()), nameof(GameKitDeploymentOrchestratorInstanceRelease));
                _deploymentOrchestrator = IntPtr.Zero;
            }
        }

        public uint DeploymentOrchestratorSetCredentials(SetCredentialsDesc setCredentialsDesc)
        {
            return DllLoader.TryDll(() => GameKitDeploymentOrchestratorSetCredentials(GetDeploymentOrchestratorInstance(), setCredentialsDesc.AccountInfo, setCredentialsDesc.AccountCredentials), nameof(GameKitDeploymentOrchestratorSetCredentials), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public FeatureStatus DeploymentOrchestratorGetFeatureStatus(FeatureType feature)
        {
            return DllLoader.TryDll(() => GameKitDeploymentOrchestratorGetFeatureStatus(GetDeploymentOrchestratorInstance(), feature), nameof(GameKitDeploymentOrchestratorGetFeatureStatus), FeatureStatus.Unknown);
        }

        public FeatureStatusSummary DeploymentOrchestratorGetFeatureStatusSummary(FeatureType feature)
        {
            return DllLoader.TryDll(() => GameKitDeploymentOrchestratorGetFeatureStatusSummary(GetDeploymentOrchestratorInstance(), feature), nameof(GameKitDeploymentOrchestratorGetFeatureStatusSummary), FeatureStatusSummary.Unknown);
        }

        public bool DeploymentOrchestratorIsFeatureDeploymentInProgress(FeatureType feature)
        {
            return DllLoader.TryDll(() => GameKitDeploymentOrchestratorIsFeatureDeploymentInProgress(GetDeploymentOrchestratorInstance(), feature), nameof(GameKitDeploymentOrchestratorIsFeatureDeploymentInProgress), false);
        }

        public bool DeploymentOrchestratorIsFeatureUpdating(FeatureType feature)
        {
            return DllLoader.TryDll(() => GameKitDeploymentOrchestratorIsFeatureUpdating(GetDeploymentOrchestratorInstance(), feature), nameof(GameKitDeploymentOrchestratorIsFeatureUpdating), false);
        }

        public bool DeploymentOrchestratorIsAnyFeatureUpdating()
        {
            return DllLoader.TryDll(() => GameKitDeploymentOrchestratorIsAnyFeatureUpdating(GetDeploymentOrchestratorInstance()), nameof(GameKitDeploymentOrchestratorIsAnyFeatureUpdating), false);
        }

        public DeploymentResponseResult DeploymentOrchestratorRefreshFeatureStatus(FeatureType feature)
        {
            DeploymentResponseResult result = new DeploymentResponseResult();

            DeploymentResponseCallbackResult callbackResult = new DeploymentResponseCallbackResult();

            result.ResultCode = DllLoader.TryDll(callbackResult, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorRefreshFeatureStatus(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.DeploymentResponseCallback), nameof(GameKitDeploymentOrchestratorRefreshFeatureStatus), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            for (int i = 0; i < callbackResult.Features.Length; ++i)
            {
                result.FeatureStatuses[callbackResult.Features[i]] = callbackResult.FeatureStatuses[i];
            }

            return result;
        }

        public DeploymentResponseResult DeploymentOrchestratorRefreshFeatureStatuses()
        {
            DeploymentResponseResult result = new DeploymentResponseResult();

            DeploymentResponseCallbackResult callbackResult = new DeploymentResponseCallbackResult();

            result.ResultCode = DllLoader.TryDll(callbackResult, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorRefreshFeatureStatuses(GetDeploymentOrchestratorInstance(), dispatchReceiver, GameKitCallbacks.DeploymentResponseCallback), nameof(GameKitDeploymentOrchestratorRefreshFeatureStatuses), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            for (int i = 0; i < callbackResult.Features.Length; ++i)
            {
                result.FeatureStatuses[callbackResult.Features[i]] = callbackResult.FeatureStatuses[i];
            }

            return result;
        }

        public CanExecuteDeploymentActionResult DeploymentOrchestratorCanCreateFeature(FeatureType feature)
        {
            CanExecuteDeploymentActionResult result = new CanExecuteDeploymentActionResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorCanCreateFeature(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.CanExecuteDeploymentActionCallback), nameof(GameKitDeploymentOrchestratorCanCreateFeature), false);

            return result;
        }

        public CanExecuteDeploymentActionResult DeploymentOrchestratorCanRedeployFeature(FeatureType feature)
        {
            CanExecuteDeploymentActionResult result = new CanExecuteDeploymentActionResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorCanRedeployFeature(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.CanExecuteDeploymentActionCallback), nameof(GameKitDeploymentOrchestratorCanRedeployFeature), false);

            return result;
        }

        public CanExecuteDeploymentActionResult DeploymentOrchestratorCanDeleteFeature(FeatureType feature)
        {
            CanExecuteDeploymentActionResult result = new CanExecuteDeploymentActionResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorCanDeleteFeature(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.CanExecuteDeploymentActionCallback), nameof(GameKitDeploymentOrchestratorCanDeleteFeature), false);

            return result;
        }

        public DeploymentResponseResult DeploymentOrchestratorCreateFeature(FeatureType feature)
        {
            DeploymentResponseResult result = new DeploymentResponseResult();

            DeploymentResponseCallbackResult callbackResult = new DeploymentResponseCallbackResult();

            result.ResultCode = DllLoader.TryDll(callbackResult, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorCreateFeature(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.DeploymentResponseCallback), nameof(GameKitDeploymentOrchestratorCreateFeature), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            for (int i = 0; i < callbackResult.Features.Length; ++i)
            {
                result.FeatureStatuses[callbackResult.Features[i]] = callbackResult.FeatureStatuses[i];
            }

            return result;
        }

        public DeploymentResponseResult DeploymentOrchestratorRedeployFeature(FeatureType feature)
        {
            DeploymentResponseResult result = new DeploymentResponseResult();

            DeploymentResponseCallbackResult callbackResult = new DeploymentResponseCallbackResult();

            result.ResultCode = DllLoader.TryDll(callbackResult, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorRedeployFeature(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.DeploymentResponseCallback), nameof(GameKitDeploymentOrchestratorRedeployFeature), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            for (int i = 0; i < callbackResult.Features.Length; ++i)
            {
                result.FeatureStatuses[callbackResult.Features[i]] = callbackResult.FeatureStatuses[i];
            }

            return result;
        }

        public DeploymentResponseResult DeploymentOrchestratorDeleteFeature(FeatureType feature)
        {
            DeploymentResponseResult result = new DeploymentResponseResult();

            DeploymentResponseCallbackResult callbackResult = new DeploymentResponseCallbackResult();

            result.ResultCode = DllLoader.TryDll(callbackResult, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorDeleteFeature(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.DeploymentResponseCallback), nameof(GameKitDeploymentOrchestratorDeleteFeature), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            for (int i = 0; i < callbackResult.Features.Length; ++i)
            {
                result.FeatureStatuses[callbackResult.Features[i]] = callbackResult.FeatureStatuses[i];
            }

            return result;
        }

        public MultiResourceInfoCallbackResult DeploymentOrchestratorDescribeFeatureResources(FeatureType feature)
        {
            MultiResourceInfoCallbackResult result = new MultiResourceInfoCallbackResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitDeploymentOrchestratorDescribeFeatureResources(GetDeploymentOrchestratorInstance(), feature, dispatchReceiver, GameKitCallbacks.MultiResourceInfoCallback), nameof(GameKitDeploymentOrchestratorDescribeFeatureResources), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            return result;
        }
        
        #endregion
        
#endif
    }
}
