// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

// Unity
using UnityEditor;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Core
{
    public class FeatureResourceManager : IDisposable
    {
        private static int _threadSafeCounter = 0;

        /// <summary>
        /// Provider for all needed GameKit paths.
        /// </summary>
        public IGameKitPathsProvider Paths;

        private AccountInfo _accountInfo = new AccountInfo() { AccountId = string.Empty, CompanyName = string.Empty, Environment = string.Empty, GameName = string.Empty };
        private AccountCredentials _accountCredentials = new AccountCredentials();
        private ICoreWrapperProvider _coreWrapper;
        private Threader _threader;
        private bool _disposedValue = false;

        public FeatureResourceManager(ICoreWrapperProvider coreWrapper, IGameKitPathsProvider gameKitPathsProvider, Threader threader)
        {
            _coreWrapper = coreWrapper;

            Paths = gameKitPathsProvider;

            _accountInfo.Environment = Constants.EnvironmentCodes.DEVELOPMENT;

            _threader = threader;
        }

        ~FeatureResourceManager()
        {
            Dispose();
            _coreWrapper.AccountInstanceRelease();
        }

        public void Dispose()
        {
            _threader.WaitForThreadedWork();

            if (!_disposedValue)
            {
                _coreWrapper.SettingsInstanceRelease();

                _disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets Game name and reinitializes settings.
        /// </summary>
        /// <param name="gameName">Name of the game.</param>
        public void SetGameName(string gameName)
        {
            _accountInfo.GameName = gameName;
            InitializeSettings(true);
        }

        /// <summary>
        /// Sets environment code and reinitializes settings.
        /// </summary>
        /// <param name="environment">Environment code in use.</param>
        public void SetEnvironment(string environment)
        {
            if (!string.IsNullOrEmpty(_accountInfo.GameName))
            {
                _accountInfo.Environment = environment;
                InitializeSettings(true);
            }
        }

        /// <summary>
        /// Get the last used deployment environment or returns dev on failure.
        /// </summary>
        /// <returns>Deployment environment code.</returns>
        public string GetLastUsedEnvironment()
        {
            return _coreWrapper.SettingsGetLastUsedEnvironment();
        }

        /// <summary>
        /// Get the last used deployment region or returns us-west-2 on failure.
        /// </summary>
        /// <returns>Deployment region.</returns>
        public string GetLastUsedRegion()
        {
            return _coreWrapper.SettingsGetLastUsedRegion();
        }

        /// <summary>
        /// Populates accountInfo and accountCredentials.
        /// </summary>
        /// <param name="accountDetails">Account Details.</param>
        /// <returns>Deployment environment.</returns>
        public void SetAccountDetails(AccountDetails accountDetails)
        {
            _accountInfo = accountDetails.CreateAccountInfo();
            _accountCredentials = accountDetails.CreateAccountCredentials();
            InitializeSettings(true);
        }

        /// <summary>
        /// Gets the current account info variable stored in FeatureResourceManager.
        /// </summary>
        /// <returns>Account info that is currently stored in FeatureResourceManager.</returns>
        public AccountInfo GetAccountInfo()
        {
            return _accountInfo;
        }

        /// <summary>
        /// Gets the current account credentials variable stored in FeatureResourceManager.
        /// </summary>
        /// <returns>Account credentials that is currently stored in FeatureResourceManager.</returns>
        public AccountCredentials GetAccountCredentials()
        {
            return _accountCredentials;
        }
        
        /// <summary>
        /// Bootstrap account by creating appropriate S3 buckets.
        /// </summary>
        /// <returns>AWSGameKit result. 
        /// GAMEKIT_SUCCESS : Bootstrap Account was successful.
        /// </returns>
        public uint BootstrapAccount()
        {
            _coreWrapper.AccountInstanceRelease();
            _coreWrapper.AccountInstanceCreateWithRootPaths(_accountInfo, _accountCredentials, Paths.ASSETS_INSTANCE_FILES_FULL_PATH, Paths.PACKAGES_BASE_TEMPLATES_FULL_PATH, Logging.LogCb);
            
            uint result = _coreWrapper.AccountInstanceBootstrap();

            if (result != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError(L10n.Tr($"Error: FeatureResourceManager::BootstrapAccount() Failed to create bucket. Error Code: { result }"));
            }

            return result;
        }

        /// <summary>
        /// Checks if the user's current account info is valid, does not permanently set accountDetails as that should only be done on submit.
        /// </summary>
        /// <param name="accountDetails">Account Details of the account we are checking for validity.</param>
        /// <returns></returns>
        public bool IsAccountInfoValid(AccountDetails accountDetails)
        {
            _coreWrapper.AccountInstanceRelease();
            _coreWrapper.AccountInstanceCreateWithRootPaths(accountDetails.CreateAccountInfo(), accountDetails.CreateAccountCredentials(), Paths.ASSETS_INSTANCE_FILES_FULL_PATH, Paths.PACKAGES_BASE_TEMPLATES_FULL_PATH, Logging.LogCb);

            bool isAccountValid = _coreWrapper.AccountHasValidCredentials();

            return isAccountValid;
        }

        /// <summary>
        /// Creates or Reinitializes settings instance handle.
        /// </summary>
        /// <param name="reinitialize">It determines if the settings instance should be reinitialized or not.</param>
        public void InitializeSettings(bool reinitialize)
        {
            if (reinitialize)
            {
                _coreWrapper.SettingsInstanceRelease();
            }

            _coreWrapper.SettingsInstanceCreate(Paths.ASSETS_INSTANCE_FILES_FULL_PATH, GetPluginVersion(), _accountInfo.GameName, _accountInfo.Environment, Logging.LogCb);
        }

        /// <summary>
        /// Gets AWS GameKit plugin version.
        /// </summary>
        /// <returns>Plugin version.</returns>
        private string GetPluginVersion()
        {
            return "1.1";
        }

        /// <summary>
        /// Add a custom deployment environment to the AWS GameKit settings menu.
        /// </summary>
        /// <remarks>
        /// This custom environment will be available to select from the dropdown menu in the Environment and Credentials section of the settings menu,
        /// alongside the default environments of "Development", "QA", "Staging", and "Production".
        /// </remarks>
        /// <param name="envCode">Two to Three letter code for the environment name. This code will be prefixed on all AWS resources that are
        /// deployed to this environment.Ex: "gam" for "Gamma".</param>
        /// <param name="envDescription">envDescription The environment name that will be displayed in the Environment and Credentials section of the settings menu. Ex: "Gamma".</param>
        public void SaveCustomEnvironment(string envCode, string envDescription)
        {
            _coreWrapper.SettingsAddCustomEnvironment(envCode, envDescription);

            uint result = _coreWrapper.SettingsSave();
            if (result != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError(L10n.Tr($"Error: FeatureResourceManager.SaveCustomEnvironment() Failed to save : { GameKitErrors.ToString(result) }"));
            }
        }

        /// <summary>
        /// Get all of the feature's variables as key-value pairs.
        /// </summary>
        /// <param name="featureType">The feature to get the variables for.</param>
        /// <returns>A result object containing a map of variable key value pairs.</returns>
        public Dictionary<string, string> GetFeatureVariables(FeatureType featureType)
        {
            return _coreWrapper.SettingsGetFeatureVariables(featureType);
        }

        /// <summary>
        /// Get the value of the specified feature variable.
        /// </summary>
        /// <param name="featureType">The feature to get the variable for.</param>
        /// <param name="varName">The key for the variable.</param>
        /// <param name="varValue">When this method returns, contains the value of the feature variable, if the key is found; otherwise, an empty string. This parameter is passed in uninitialized.</param>
        /// <returns><c>true</c> if the feature variable exists; otherwise, <c>false</c>.</returns>
        public bool TryGetFeatureVariable(FeatureType featureType, string varName, out string varValue)
        {
            Dictionary<string, string> featureVars = GetFeatureVariables(featureType);
            return featureVars.TryGetValue(varName, out varValue);
        }

        /// <summary>
        /// Write a variable for a setting to the saveInfo.yml file if that variable is not already present.
        /// </summary>
        /// <remarks>This method will NOT overwrite an existing variable if present.</remarks>
        /// <param name="featureType">The feature that is using the variable pair.</param>
        /// <param name="varName">key for the variable.</param>
        /// <param name="varValue">value for the variable.</param>
        /// <param name="callback">callback that is executed once this method is complete, it is executed even if the new variables are saved.</param>
        public void SetFeatureVariableIfUnset(FeatureType featureType, string varName, string varValue, Action callback)
        {
            Dictionary<string, string> featureVars = GetFeatureVariables(featureType);
            if (!featureVars.ContainsKey(varName))
            {
                SetFeatureVariable(featureType, varName, varValue, callback);
                return;
            }

            // call the callback to signify the completion of this method and return an empty job handle
            callback();
        }

        /// <summary>
        /// Get AWS Account Id using the access and secret key.
        /// </summary>
        /// <param name="accountCredentials">Structure containing both the AWS Access Key and the AWS Secret Key.</param>
        /// <param name="callback">Callback that is executed once this method is complete, returning a string.</param>
        /// <returns>AWSGameKit Account Id.</returns>
        public void GetAccountId(GetAWSAccountIdDescription accountCredentials, Action<StringCallbackResult> callback)
        {
            _threader.Call(_coreWrapper.GetAWSAccountId, accountCredentials, callback);
        }

        /// <summary>
        /// Write a variable for a setting to the saveInfo.yml file.
        /// </summary>
        /// <remarks>This method will overwrite an existing variable if present.</remarks>
        /// <param name="featureType">The feature that is using the variable pair.</param>
        /// <param name="varName">key for the variable.</param>
        /// <param name="varValue">value for the variable.</param>
        /// <param name="callback">callback that is executed once this method is complete.</param>
        public void SetFeatureVariable(FeatureType featureType, string varName, string varValue, Action callback)
        {
            _coreWrapper.SettingsSetFeatureVariables(featureType, new string[] { varName }, new string[] { varValue }, 1);

            // Debounce our writes so we don't thresh on IO
            int currentValue = Interlocked.Increment(ref _threadSafeCounter);

            _threader.Call(() =>
            {   
                Thread.Sleep(3000);

                if (currentValue == Volatile.Read(ref _threadSafeCounter))
                {
                    _coreWrapper.SettingsSave();
                    Interlocked.Exchange(ref _threadSafeCounter, 0);
                }
            }, callback);
        }


        /// <summary>
        /// Write several variable at once to the saveInfo.yml file.
        /// </summary>
        /// <remarks>This method will overwrite an existing variable if present.</remarks>
        /// <param name="variables">IEnumerable of variables in the form Tuple<FeatureType, string, string> where Item1 is a FeatureType, Item2 is the variable name and Item3 is the variable value.</param>
        /// <param name="callback">callback that is executed once this method is complete.</param>
        public void SetFeatureVariables(IEnumerable<Tuple<FeatureType, string, string>> variables, Action callback)
        {
            foreach (var variableTuple in variables)
            {
                _coreWrapper.SettingsSetFeatureVariables(variableTuple.Item1, new string[] { variableTuple.Item2 }, new string[] { variableTuple.Item3 }, 1);
            }

            _coreWrapper.SettingsSave();

            callback();
        }

        /// <summary>
        /// Get all the custom environment key-value pairs (ex: "gam", "Gamma").<br/><br/>
        /// 
        /// The custom environments are returned through the callback and receiver.<br/>
        /// The callback is invoked once for each custom environment.<br/>
        /// The returned keys are 3-letter environment codes(ex: "gam"), and the values are corresponding environment descriptions(ex: "Gamma").
        /// </summary>
        /// <returns>A result object containing a map of environment key value pairs.</returns>
        public Dictionary<string, string> GetCustomEnvironments()
        {
            string settingsFile = Path.Combine(Paths.ASSETS_INSTANCE_FILES_FULL_PATH, _accountInfo.GameName, Paths.SAVE_INFO_FILE_NAME);

            FileInfo fileInfo = new FileInfo(settingsFile);
            if (fileInfo == null || fileInfo.Exists == false)
            {
                return new Dictionary<string, string>();
            }

            return _coreWrapper.SettingsGetCustomEnvironments();
        }

        /// <summary>
        /// Get the game's full name, example: "Stevie goes to the moon".
        /// </summary>
        /// <returns>A string containing the name of the game.</returns>
        public string GetGameName()
        {
            return _accountInfo.GameName.Length > 0 ? _accountInfo.GameName : _coreWrapper.SettingsGetGameName();
        }

        /// <summary>
        /// Set the Game's name, environment, and region, then save settings.
        /// </summary>
        /// <remarks>
        /// Use this method to create the settings file directory, set the game's name, set the games environment, set the games region, and then persist the settings.
        /// </remarks>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult errors.h file for details.</returns>
        public uint SaveSettings()
        {
            uint result = _coreWrapper.SettingsPopulateAndSave(_accountInfo.GameName, _accountInfo.Environment, _accountCredentials.Region);

            if (result != GameKitErrors.GAMEKIT_SUCCESS)
            {
                Logging.LogError(L10n.Tr($"Error: FeatureResourceManager.SaveSettings() Failed to save : { GameKitErrors.ToString(result) }"));
            }

            return result;
        }

        /// <summary>
        /// See ICoreWrapperProvider.ResourcesCreateEmptyConfigFile() for details.
        /// </summary>
        public uint CreateEmptyClientConfigFile()
        {
            _coreWrapper.ResourcesInstanceCreateWithRootPaths(_accountInfo, _accountCredentials, FeatureType.Main, Paths.ASSETS_INSTANCE_FILES_FULL_PATH, Paths.ASSETS_FULL_PATH, Logging.LogCb);
            uint result = _coreWrapper.ResourcesCreateEmptyConfigFile();
            _coreWrapper.ResourcesInstanceRelease();

            return result;
        }
    }
}
