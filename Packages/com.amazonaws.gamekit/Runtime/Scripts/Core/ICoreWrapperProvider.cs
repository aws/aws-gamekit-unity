// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Runtime.Core
{
    /// <summary>
    /// Interface for the AWS GameKit Core APIs.
    /// </summary>
    public interface ICoreWrapperProvider
    {
#if UNITY_EDITOR
        
        #region GameKitAccount

        /// <summary>
        /// Get the AWS Account ID which corresponds to the provided Access Key and Secret Key.
        /// </summary>
        /// <remarks>
        /// For more information about AWS access keys and secret keys, see: https://docs.aws.amazon.com/general/latest/gr/aws-sec-cred-types.html#access-keys-and-secret-access-keys
        /// </remarks>
        /// <param name="accountCredentials">Struct containing an access key and secret key.</param>
        /// <returns>StringCallbackResults which contains a ChrPtr result containing the Account ID and the result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public StringCallbackResult GetAWSAccountId(GetAWSAccountIdDescription accountCredentials);

        /// <summary>
        /// Gets (and creates if necessary) a GameKitAccount instance, which can be used to access the Core Account APIs.
        /// </summary>
        /// <remarks>
        /// Make sure to call AccountInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <returns>Pointer to the new GameKitAccount instance.</returns>
        public IntPtr GetAccountInstance();

        /// <summary>
        /// Create a GameKitAccount instance, which can be used to access the GameKitAccount API.
        /// </summary>
        /// <remarks>
        /// Make sure to call AccountInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <param name="accountInfo">Struct holding account id, game name, and deployment environment.</param>
        /// <param name="credentials">Struct holding account id, region, access key, and secret key.</param>
        /// <param name="logCb">Callback function for logging information and errors.</param>
        /// <returns>Pointer to the new GameKitAccount instance.</returns>
        public IntPtr AccountInstanceCreate(AccountInfo accountInfo, AccountCredentials credentials, FuncLoggingCallback logCb);

        /// <summary>
        /// Create a GameKitAccount instance, which can be used to access the GameKitAccount API. Also sets the plugin and game root paths.
        /// </summary>
        /// <remarks>
        /// Make sure to call AccountInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <param name="accountInfo">Struct holding account id, game name, and deployment environment.</param>
        /// <param name="credentials">Struct holding account id, region, access key, and secret key.</param>
        /// <param name="rootPath">New path for GAMEKIT_ROOT</param>
        /// <param name="pluginRootPath">New path for the plugin root directory.</param>
        /// <param name="logCb">Callback function for logging information and errors.</param>
        /// <returns>Pointer to the new GameKitAccount instance.</returns>
        public IntPtr AccountInstanceCreateWithRootPaths(AccountInfo accountInfo, AccountCredentials credentials, string rootPath, string pluginRootPath, FuncLoggingCallback logCb);

        /// <summary>
        /// Destroy the provided GameKitAccount instance.
        /// </summary>
        public void AccountInstanceRelease();

        /// <summary>
        ///  Get the GAMEKIT_ROOT path where the "instance" templates and settings are going to be stored.
        /// </summary>
        /// <returns>True if the credentials are valid, false otherwise.</returns>
        public bool AccountHasValidCredentials();

        /// <summary>
        /// Create a bootstrap bucket in the AWS account if it doesn't already exist.
        /// </summary>
        /// <remarks>
        /// The bootstrap bucket must be created before deploying any stacks or Lambda functions. There needs to be a unique bootstrap bucket for each combination of Environment, Account ID, and GameName.
        /// </remarks>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint AccountInstanceBootstrap();

        /// <summary>
        /// Create or update a secret in AWS SecretsManager (https://aws.amazon.com/secrets-manager/).
        /// </summary>
        /// <remarks>
        /// The secret name will be "gamekit_<environment>_<gameName>_<secretName>", for example: "gamekit_dev_mygame_amazon_client_secret".
        /// </remarks>
        /// <param name="secretName">Name of the secret. Will be prefixed as described in the details.</param>
        /// <param name="secretValue">Value of the secret.</param>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint GameKitAccountSaveSecret(string secretName, string secretValue);

        /// <summary>
        /// Checks if a secret exists in AWS SecretsManager (https://aws.amazon.com/secrets-manager/).
        /// </summary>
        /// <remarks>
        /// The secret name will be "gamekit_<environment>_<gameName>_<secretName>", for example: "gamekit_dev_mygame_amazon_client_secret".
        /// </remarks>
        /// <param name="secretName">Name of the secret. Will be prefixed as described in the details.</param>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint GameKitAccountCheckSecretExists(string secretName);

        /// <summary>
        /// Upload the dashboard configuration file for every feature to the bootstrap bucket.
        /// </summary>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint GameKitAccountUploadAllDashboards();

        #endregion

        #region GameKitResources

        /// <summary>
        /// Gets (and creates if necessary) a GameKitResources instance, which can be used to access the Core Account APIs.
        /// </summary>
        /// <remarks>
        /// Make sure to call ResourcesInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <returns>Pointer to the new GameKitResources instance.</returns>
        public IntPtr GetResourcesInstance();

        /// <summary>
        /// Create a GameKitFeatureResources instance, which can be used to access the GameKitFeatureResources API.
        /// </summary>
        /// <remarks>
        /// Make sure to call GameKitResourcesInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <param name="accountInfo">Struct holding account id, game name, and deployment environment.</param>
        /// <param name="credentials">Struct holding account id, region, access key, and secret key.</param>
        /// <param name="featureType">The GameKit feature to work with.</param>
        /// <param name="logCb">Callback function for logging information and errors.</param>
        /// <returns>Pointer to the new GameKitFeatureResources instance.</returns>
        public IntPtr ResourcesInstanceCreate(AccountInfo accountInfo, AccountCredentials credentials, FeatureType featureType, FuncLoggingCallback logCb);

        /// <summary>
        /// Create a GameKitFeatureResources instance, which can be used to access the GameKitFeatureResources API. Also sets the root and pluginRoot paths.
        /// </summary>
        /// <remarks>
        /// Make sure to call GameKitResourcesInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <param name="accountInfo">Struct holding account id, game name, and deployment environment.</param>
        /// <param name="credentials">Struct holding account id, region, access key, and secret key.</param>
        /// <param name="featureType">The GameKit feature to work with.</param>
        /// <param name="rootPath">New path for GAMEKIT_ROOT.</param>
        /// <param name="pluginRootPath">New path for the plugin root directory.</param>
        /// <param name="logCb">Callback function for logging information and errors.</param>
        /// <returns>Pointer to the new GameKitFeatureResources instance.</returns>
        public IntPtr ResourcesInstanceCreateWithRootPaths(AccountInfo accountInfo, AccountCredentials credentials, FeatureType featureType, string rootPath, string pluginRootPath, FuncLoggingCallback logCb);

        /// <summary>
        /// Destroy the provided GameKitFeatureResources instance.
        /// </summary>
        public void ResourcesInstanceRelease();

        /// <summary>
        /// Create an empty client configuration file in the current environment's instance files folder.
        /// </summary>
        /// <remarks>
        /// Call this to bootstrap a GameKit config file as soon as an environment has been selected.
        /// </remarks>
        /// <returns>The result status code of the operation (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful. The empty config file was created.<br/>
        /// - GAMEKIT_ERROR_DIRECTORY_CREATE_FAILED: There was an error while creating missing directories in the filepath leading to the config file. Check the logs to see the root cause.<br/>
        /// - GAMEKIT_ERROR_FILE_OPEN_FAILED: There was an error while opening an output stream for the new or existing config file. Check the logs to see the root cause.<br/>
        /// - GAMEKIT_ERROR_FILE_WRITE_FAILED: There was an error while writing text to the config file.
        /// </returns>
        public uint ResourcesCreateEmptyConfigFile();

        #endregion

        #region GameKitSettings

        /// <summary>
        /// Gets a GameKitSettings instance, which can be used to access the Core Settings APIs.
        /// </summary>
        /// <remarks>
        /// Make sure to call Release() to destroy the returned object when finished with it.
        /// </remarks>
        /// <returns>Pointer to the new GameKitSessionManager instance.</returns>
        public IntPtr GetSettingsInstance();

        /// <summary>
        /// Create a GameKitSettings instance and load the settings from the GameKit Settings YAML file.
        /// </summary>
        /// <remarks>
        /// Make sure to call SettingsInstanceRelease() to destroy the returned object when finished with it.
        /// </remarks>
        /// <param name="rootPath">The GAMEKIT_ROOT path where the "instance" templates and settings are stored.</param>
        /// <param name="pluginVersion">The GameKit plugin version.</param>
        /// <param name="currentEnvironment">The current active environment based on what was selected in Environment and Credentials category eg "dev", "qa", custom</param>
        /// <param name="shortGameName">A shortened version of the game name.</param>
        /// <param name="logCb">Callback function for logging information and errors</param>
        /// <returns>Pointer to the new GameKitSettings instance.</returns>
        public IntPtr SettingsInstanceCreate(string rootPath, string pluginVersion, string shortGameName, string currentEnvironment, FuncLoggingCallback logCb);

        /// <summary>
        /// Destroy the provided GameKitSettings instance.
        /// </summary>
        public void SettingsInstanceRelease();

        /// <summary>
        /// Set the game's name.
        /// </summary>
        /// <param name="gameName">The new game name.</param>
        public void SettingsSetGameName(string gameName);

        /// <summary>
        /// Get the game's full name, example: "Stevie goes to the moon".
        /// </summary>
        /// <returns>A string containing the name of the game.</returns>
        public string SettingsGetGameName();

        /// <summary>
        /// Set the last used region.
        /// </summary>
        /// <param name="region">The region last used, example: "us-west-2".</param>
        public void SettingsSetLastUsedRegion(string region);

        /// <summary>
        /// Get the developers last submitted region, example: "us-west-2".
        /// </summary>
        /// <returns>A string containing the name of the last submitted region<./returns>
        public string SettingsGetLastUsedRegion();

        /// <summary>
        /// Set the last used environment.
        /// </summary>
        /// <param name="envcode">The environment code.</param>
        public void SettingsSetLastUsedEnvironment(string envcode);

        /// <summary>
        /// Get the developers last submitted environment code, example: "dev".
        /// </summary>
        /// <returns>A string containing the last submitted environment code.</returns>
        public string SettingsGetLastUsedEnvironment();

        /// <summary>
        /// Get all the custom environment key-value pairs (ex: "gam", "Gamma").<br/><br/>
        /// 
        /// The custom environments are returned through the callback and receiver.<br/>
        /// The callback is invoked once for each custom environment.<br/>
        /// The returned keys are 3-letter environment codes(ex: "gam"), and the values are corresponding environment descriptions(ex: "Gamma").
        /// </summary>
        /// <returns>A result object containing a map of environment key value pairs</returns>
        public Dictionary<string, string> SettingsGetCustomEnvironments();

        /// <summary>
        /// Get all of the feature's variables as key-value pairs.<br/><br/>
        /// 
        /// The variables are returned through the callback and receiver.<br/>
        /// The callback is invoked once for each variable.The variables are returned as key-value pairs of (variableName, variableValue).<br/>
        /// The callback will not be invoked if the feature is missing from the settings file.
        /// </summary>
        /// <param name="featureType">The feature to get the variables for</param>
        /// <returns>A result object containing a map of variable key value pairs</returns>
        public Dictionary<string, string> SettingsGetFeatureVariables(FeatureType featureType);

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
        public void SettingsAddCustomEnvironment(string envCode, string envDescription);

        /// <summary>
        /// Add key-value pairs to the feature's variables map, overwriting existing keys.
        /// </summary>
        /// <remarks>
        /// The parameters "varKeys", "varValues", and "numKeys" represent a map<string, string>, where varKeys[N] maps to varValues[N], and numKeys is the total number of key-value pairs.
        /// </remarks>
        /// <param name="featureType">The feature to set the variables for.</param>
        /// <param name="varKeys">The variable names.</param>
        /// <param name="varValues">The variable values.</param>
        /// <param name="numKeys">The number of key-value pairs. The length of varKeys, varValues, and numKeys should be equal.</param>
        public void SettingsSetFeatureVariables(FeatureType featureType, string[] varKeys, string[] varValues, ulong numKeys);

        /// <summary>
        /// Write the GAMEKIT Settings YAML file to disk.
        /// </summary>
        /// <remarks>
        /// Call this to persist any changes made through the "Set", "Add/Delete", "Activate/Deactivate" methods.
        /// </remarks>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint SettingsSave();

        /// <summary>
        /// Set the Game's name, environment, and resion, then save settings
        /// </summary>
        /// <remarks>
        /// Use this method to create the settings file directory, set the game's name, set the games environment, set the games region, and then persist the settings.
        /// </remarks>
        /// <param name="gameName">The game's correctly formatted name</param>
        /// <param name="envCode">The 3-letter environment code to get the description for</param>
        /// <param name="region">The AWS deployment region</param>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult errors.h file for details.</returns>
        public uint SettingsPopulateAndSave(string gameName, string envCode, string region);

        /// <summary>
        /// Get the path to the "saveInfo.yml" settings file.
        /// </summary>
        /// <remarks>
        /// The path is equal to "GAMEKIT_ROOT/shortGameName/saveInfo.yml".
        /// </remarks>
        /// <returns>The path to "saveInfo.yml" as a string.</returns>
        public string SettingsGetSettingsFilePath();

        /// <summary>
        /// Save a new profile to the AWS credentials file.
        /// </summary>
        /// <param name="profileName">The name of the profile we are saving or updating in the credentials ini file.</param>
        /// <param name="accessKey">The access key of the AWS IAM role we are saving.</param>
        /// <param name="secretKey">The secret key of the AWS IAM role we are saving.</param>
        /// <param name="logCb">Callback function for logging information and errors. </param>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint SaveAWSCredentials(string profileName, string accessKey, string secretKey, FuncLoggingCallback logCb);

        /// <summary>
        /// Sets the AWS access key of an existing profile.
        /// </summary>
        /// <param name="profileName">The name of the profile we are updating in the credentials ini file.</param>
        /// <param name="newAccessKey">The new access key that will be assigned to this profile.</param>
        /// <param name="logCb">Callback function for logging information and errors. </param>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint SetAWSAccessKey(string profileName, string newAccessKey, FuncLoggingCallback logCb);

        /// <summary>
        /// Sets the AWS secret key of an existing profile.
        /// </summary>
        /// <remarks>
        /// If the profile passed in does not exist, will not automatically create the profile and will return an error.
        /// </remarks>
        /// <param name="profileName">The name of the profile we are updating in the credentials ini file.</param>
        /// <param name="newSecretKey">The new secret key that will be assigned to this profile.</param>
        /// <param name="logCb">Callback function for logging information and errors. </param>
        /// <returns>The result code of the operation. GAMEKIT_SUCCESS if successful, else a non-zero value in case of error. Consult GameKitErrors.cs file for details.</returns>
        public uint SetAWSSecretKey(string profileName, string newSecretKey, FuncLoggingCallback logCb);

        /// <summary>
        /// Gets the access key and secret key corresponding to a pre-existing profile in the AWS credentials file.
        /// </summary>
        /// <param name="profileName">The name of the profile we are getting the access key and secret from.</param>
        /// <param name="logCb">Callback function for logging information and errors. </param>
        /// <returns>Callback function for logging information and errors.</returns>
        public KeyValueStringCallbackResult GetAWSProfile(string profileName, FuncLoggingCallback logCb);
        #endregion

        #region Deployment Orchestrator

        /// <summary>
        /// Get a pointer to an already initialized GameKitDeploymentOrchestrator instance.
        /// </summary>
        /// <remarks>This method will throw if DeploymentOrchestratorInstanceCreate was not called prior to calling this method.</remarks>
        /// <returns>Pointer to the existing GameKitDeploymentOrchestrator instance.</returns>
        public IntPtr GetDeploymentOrchestratorInstance();

        /// <summary>
        /// Create a GameKitDeploymentOrchestrator instance.
        /// </summary>
        /// <param name="baseTemplatesFolder">The folder where the "base" templates are stored. The base templates are the cloud formation templates, lambda functions, etc. which are copied to make the instance files (see next parameter).</param>
        /// <param name="instanceFilesFolder">The folder where the "instance" files and settings are going to be stored. The instance files are copied from the base templates, and are intended to be modified by the user.</param>
        /// <returns>Pointer to the new GameKitDeploymentOrchestrator instance.</returns>
        public IntPtr DeploymentOrchestratorInstanceCreate(string baseTemplatesFolder, string instanceFilesFolder);

        /// <summary>
        /// Destroy the provided GameKitDeploymentOrchestrator instance.
        /// </summary>
        public void DeploymentOrchestratorInstanceRelease();

        /// <summary>
        /// Set account credentials.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="setCredentialsDesc">Object holding the AccountInfo and AccountCredentials.</param>
        /// <returns>
        /// The result status code of the operation (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_DEPLOYMENT_IN_PROGRESS: Cannot change credentials while a deployment is in progress.
        /// - GAMEKIT_ERROR_REGION_CODE_CONVERSION_FAILED: Could not retrieve the short region code from the mappings file. See the log for details on how to fix this.
        /// </returns>
        public uint DeploymentOrchestratorSetCredentials(SetCredentialsDesc setCredentialsDesc);

        /// <summary>
        /// Get the status of a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>FeatureStatus enum related to the status of the requested feature.</returns>
        public FeatureStatus DeploymentOrchestratorGetFeatureStatus(FeatureType feature);

        /// <summary>
        /// Get an abridged status of a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>FeatureStatusSummary enum related to the abridged status of the requested feature.</returns>
        public FeatureStatusSummary DeploymentOrchestratorGetFeatureStatusSummary(FeatureType feature);

        /// <summary>
        /// Check if the requested feature is currently being created, redeployed, or deleted.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.<br/><br/>
        ///
        /// Unlike DeploymentOrchestratorIsFeatureUpdating, this method returns true while the Main stack is being deployed (before the feature itself is created or redeployed).
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>true if the requested feature is being created, redeployed, or deleted.</returns>
        public bool DeploymentOrchestratorIsFeatureDeploymentInProgress(FeatureType feature);

        /// <summary>
        /// Check if the requested feature is currently updating (i.e. it's FeatureStatus is not Deployed, Undeployed, Error, or RollbackComplete).
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>true if the requested feature is updating.</returns>
        public bool DeploymentOrchestratorIsFeatureUpdating(FeatureType feature);

        /// <summary>
        /// Check if any GameKit feature is currently updating  (i.e. has a FeatureStatus other than Deployed, Undeployed, Error, or RollbackComplete).
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <returns>true if any feature is updating.</returns>
        public bool DeploymentOrchestratorIsAnyFeatureUpdating();

        /// <summary>
        /// Refresh the status of a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// The <see cref="FeatureStatus"/> of all features and the result status code of the operation.<br/><br/>
        ///
        /// The result status code will be one of (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </returns>
        public DeploymentResponseResult DeploymentOrchestratorRefreshFeatureStatus(FeatureType feature);

        /// <summary>
        /// Refresh the status of all features.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.
        /// </remarks>
        /// <returns>
        /// The <see cref="FeatureStatus"/> of all features and the result status code of the operation.<br/><br/>
        ///
        /// The result status code will be one of (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </returns>
        public DeploymentResponseResult DeploymentOrchestratorRefreshFeatureStatuses();

        /// <summary>
        /// Request if a feature is in a state to be created.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// A <see cref="CanExecuteDeploymentActionResult"/> object describing whether the feature can be created.<br/>
        /// A <see cref="DeploymentActionBlockedReason"/> and list of blocking <see cref="FeatureType"/> are provided in cases where the feature cannot be created.
        /// </returns>
        public CanExecuteDeploymentActionResult DeploymentOrchestratorCanCreateFeature(FeatureType feature);

        /// <summary>
        /// Request if a feature can be re-deployed.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// A <see cref="CanExecuteDeploymentActionResult"/> object describing whether the feature can be redeployed.<br/>
        /// A <see cref="DeploymentActionBlockedReason"/> and list of blocking <see cref="FeatureType"/> are provided in cases where the feature cannot be redeployed.
        /// </returns>
        public CanExecuteDeploymentActionResult DeploymentOrchestratorCanRedeployFeature(FeatureType feature);

        /// <summary>
        /// Request if a feature is in a state where it can be deleted.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// A <see cref="CanExecuteDeploymentActionResult"/> object describing whether the feature can be deleted.<br/>
        /// A <see cref="DeploymentActionBlockedReason"/> and list of blocking <see cref="FeatureType"/> are provided in cases where the feature cannot be deleted.
        /// </returns>
        public CanExecuteDeploymentActionResult DeploymentOrchestratorCanDeleteFeature(FeatureType feature);

        /// <summary>
        /// Create a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// The <see cref="FeatureStatus"/> of all features and the result status code of the operation.<br/><br/>
        ///
        /// The result status code will be one of (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_INVALID_FEATURE_STATE: Cannot create feature as it or one of its dependencies are in an invalid state for deployment.
        /// </returns>
        public DeploymentResponseResult DeploymentOrchestratorCreateFeature(FeatureType feature);

        /// <summary>
        /// Re-deploy a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// The <see cref="FeatureStatus"/> of all features and the result status code of the operation.<br/><br/>
        ///
        /// The result status code will be one of (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_INVALID_FEATURE_STATE: Cannot redeploy feature as it or one of its dependencies are in an invalid state for deployment.
        /// </returns>
        public DeploymentResponseResult DeploymentOrchestratorRedeployFeature(FeatureType feature);

        /// <summary>
        /// Delete a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// The <see cref="FeatureStatus"/> of all features and the result status code of the operation.<br/><br/>
        ///
        /// The result status code will be one of (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_CLOUDFORMATION_STACK_DELETE_FAILED: Failed to delete the stack, check output log for exact reason.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_INVALID_FEATURE_STATE: Cannot delete feature as it or one of its downstream dependencies are in an invalid state for deletion.
        /// </returns>
        public DeploymentResponseResult DeploymentOrchestratorDeleteFeature(FeatureType feature);

        /// <summary>
        /// Gets the deployment status of each AWS resource within the specified feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>
        /// The resource id, resource type, and resource status of each AWS resource within the specified feature.<br/><br/>
        ///
        /// Also returns the result status code of the operation, which will be one of (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_CLOUDFORMATION_DESCRIBE_RESOURCE_FAILED: If status of the resources could not be determined.
        /// </returns>
        public MultiResourceInfoCallbackResult DeploymentOrchestratorDescribeFeatureResources(FeatureType feature);
        
        #endregion
        
#endif
    }
}
