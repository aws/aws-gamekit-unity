// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
#if UNITY_EDITOR
using UnityEditor;
#endif

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Core
{
    /// <summary>
    /// This class handles the deployment of all GameKit features (i.e. create, re-deploy, delete).
    /// </summary>
    public class FeatureDeploymentOrchestrator : IDisposable
    {
        private Threader _threader = new Threader();
        private ICoreWrapperProvider _coreWrapper;
        private bool _disposedValue = false;
        private IDictionary<Guid, string> _enterPlayModeRestrictions = new Dictionary<Guid, string>();

        public FeatureDeploymentOrchestrator(ICoreWrapperProvider coreWrapper, string baseTemplatesFolder, string instanceFilesFolder)
        {
            _coreWrapper = coreWrapper;
            _coreWrapper.DeploymentOrchestratorInstanceCreate(baseTemplatesFolder, instanceFilesFolder);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += this.OnPlayModeStateChanged;
#endif
        }

        public FeatureDeploymentOrchestrator(string baseTemplatesFolder, string instanceFilesFolder) : this(CoreWrapper.Get(), baseTemplatesFolder, instanceFilesFolder) { }

        ~FeatureDeploymentOrchestrator() => Dispose();

        public void Dispose()
        {
            if (!_disposedValue)
            {
                _coreWrapper.DeploymentOrchestratorInstanceRelease();

                _disposedValue = true;
            }

            GC.SuppressFinalize(this);
        }

        public void Update()
        {
            _threader.Update();
        }

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
        public uint SetCredentials(SetCredentialsDesc setCredentialsDesc)
        {
            return _coreWrapper.DeploymentOrchestratorSetCredentials(setCredentialsDesc);
        }

        /// <summary>
        /// Get the status of a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>FeatureStatus enum related to the status of the requested feature.</returns>
        public FeatureStatus GetFeatureStatus(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorGetFeatureStatus(feature);
        }

        /// <summary>
        /// Get an abridged status of a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>FeatureStatusSummary enum related to the abridged status of the requested feature.</returns>
        public FeatureStatusSummary GetFeatureStatusSummary(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorGetFeatureStatusSummary(feature);
        }

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
        public bool IsFeatureDeploymentInProgress(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorIsFeatureDeploymentInProgress(feature);
        }

        /// <summary>
        /// Check if the requested feature is currently updating (i.e. it's FeatureStatus is not Deployed, Undeployed, Error, or RollbackComplete).
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <returns>true if the requested feature is updating.</returns>
        public bool IsFeatureUpdating(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorIsFeatureUpdating(feature);
        }

        /// <summary>
        /// Check if any GameKit feature is currently updating  (i.e. has a FeatureStatus other than Deployed, Undeployed, Error, or RollbackComplete).
        /// </summary>
        /// <remarks>
        /// This is a fast method. It does not invoke any networked procedures.
        /// </remarks>
        /// <returns>true if any feature is updating.</returns>
        public bool IsAnyFeatureUpdating()
        {
            return _coreWrapper.DeploymentOrchestratorIsAnyFeatureUpdating();
        }

        /// <summary>
        /// Refresh the status of a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <param name="callback">Delegate that is called once this method completes.
        /// It contains the <see cref="FeatureStatus"/> of all features and the result status code for the call.
        /// </param>
        public void RefreshFeatureStatus(FeatureType feature, Action<DeploymentResponseResult> callback)
        {
            _threader.Call<DeploymentResponseResult>(() => _coreWrapper.DeploymentOrchestratorRefreshFeatureStatus(feature), callback);
        }

        /// <summary>
        /// Refresh the status of all features.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </remarks>
        /// <param name="callback">Delegate that is called once this method completes.
        /// It contains the <see cref="FeatureStatus"/> of all features and the result status code for the call.
        /// </param>
        public void RefreshFeatureStatuses(Action<DeploymentResponseResult> callback)
        {
            _threader.Call<DeploymentResponseResult>(() => _coreWrapper.DeploymentOrchestratorRefreshFeatureStatuses(), callback);
        }

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
        public CanExecuteDeploymentActionResult CanCreateFeature(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorCanCreateFeature(feature);
        }

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
        public CanExecuteDeploymentActionResult CanRedeployFeature(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorCanRedeployFeature(feature);
        }

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
        public CanExecuteDeploymentActionResult CanDeleteFeature(FeatureType feature)
        {
            return _coreWrapper.DeploymentOrchestratorCanDeleteFeature(feature);
        }

        /// <summary>
        /// Create a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_INVALID_FEATURE_STATE: Cannot create feature as it or one of its dependencies are in an invalid state for deployment.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <param name="callback">Delegate that is called once this method completes.
        /// It contains the <see cref="FeatureStatus"/> of all features and the result status code for the call.
        /// </param>
        public void CreateFeature(FeatureType feature, Action<DeploymentResponseResult> callback)
        {
            _threader.Call<DeploymentResponseResult>(() =>
            {
                Guid taskGuid = Guid.NewGuid();
                lock (_enterPlayModeRestrictions) _enterPlayModeRestrictions.Add(taskGuid, $"\"{feature.GetDisplayName()}\" feature deployment is in progress.");

                DeploymentResponseResult result = _coreWrapper.DeploymentOrchestratorCreateFeature(feature);

                lock (_enterPlayModeRestrictions)  _enterPlayModeRestrictions.Remove(taskGuid);

                return result;
            },
            callback);
        }

        /// <summary>
        /// Re-deploy a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_INVALID_FEATURE_STATE: Cannot redeploy feature as it or one of its dependencies are in an invalid state for deployment.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <param name="callback">Delegate that is called once this method completes.
        /// It contains the <see cref="FeatureStatus"/> of all features and the result status code for the call.
        /// </param>
        public void RedeployFeature(FeatureType feature, Action<DeploymentResponseResult> callback)
        {
            _threader.Call<DeploymentResponseResult>(() =>
            {
                Guid taskGuid = Guid.NewGuid();
                lock (_enterPlayModeRestrictions) _enterPlayModeRestrictions.Add(taskGuid, $"\"{feature.GetDisplayName()}\" feature redeployment is in progress.");

                DeploymentResponseResult result = _coreWrapper.DeploymentOrchestratorRedeployFeature(feature);
                
                lock (_enterPlayModeRestrictions) _enterPlayModeRestrictions.Remove(taskGuid);
                
                return result;
            },
            callback);
        }

        /// <summary>
        /// Delete a requested feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_CLOUDFORMATION_STACK_DELETE_FAILED: Failed to delete the stack, check output log for exact reason.<br/>
        /// - GAMEKIT_ERROR_ORCHESTRATION_INVALID_FEATURE_STATE: Cannot delete feature as it or one of its downstream dependencies are in an invalid state for deletion.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <param name="callback">Delegate that is called once this method completes.
        /// It contains the <see cref="FeatureStatus"/> of all features and the result status code for the call.</param>
        public void DeleteFeature(FeatureType feature, Action<DeploymentResponseResult> callback)
        {
            _threader.Call<DeploymentResponseResult>(() =>
            {
                Guid taskGuid = Guid.NewGuid();
                lock (_enterPlayModeRestrictions) _enterPlayModeRestrictions.Add(taskGuid, $"\"{feature.GetDisplayName()}\" feature deletion is in progress.");

                DeploymentResponseResult result = _coreWrapper.DeploymentOrchestratorDeleteFeature(feature);

                lock (_enterPlayModeRestrictions) _enterPlayModeRestrictions.Remove(taskGuid);

                return result;
            },
            callback);
        }

#if UNITY_EDITOR
        private void OnPlayModeStateChanged(PlayModeStateChange stateChange)
        {
            // Don't allow transitioning from Edit mode to Play mode when a restrictive operation is running.
            if (stateChange == PlayModeStateChange.ExitingEditMode)
            {
                lock (_enterPlayModeRestrictions)
                {
                    if (_enterPlayModeRestrictions.Count == 0)
                    {
                        return;
                    }

                    EditorUtility.DisplayDialog("Change to Play Mode", "You can't switch to Play mode while AWS GameKit features are deploying or updating. See logs for more details.", "Ok");

                    foreach (var restrictionKvp in _enterPlayModeRestrictions)
                    {
                        Logging.Log(Logging.Level.WARNING, $"Cannot enter Play mode, {restrictionKvp.Value}");
                    }

                    // Revert to Edit mode
                    EditorApplication.isPlaying = false;
                }
            }
        }
#endif
        /// <summary>
        /// Gets the deployment status of each AWS resource within the specified feature.
        /// </summary>
        /// <remarks>
        /// This is a long running operation.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_CLOUDFORMATION_DESCRIBE_RESOURCE_FAILED: If status of the resources could not be determined.
        /// </remarks>
        /// <param name="feature">The GameKit feature to work with.</param>
        /// <param name="callback">Delegate that is called once this method completes.
        /// It contains the resource id, resource type, and resource status of each AWS resource within the specified feature.
        /// Additionally it contains the result status code for the call.</param>
        public void DescribeFeatureResources(FeatureType feature, Action<MultiResourceInfoCallbackResult> callback)
        {
            _threader.Call<MultiResourceInfoCallbackResult>(() => _coreWrapper.DeploymentOrchestratorDescribeFeatureResources(feature), callback);
        }
    }
}
