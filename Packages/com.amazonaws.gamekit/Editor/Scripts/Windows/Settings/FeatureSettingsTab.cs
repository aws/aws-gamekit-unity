// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Models.FeatureSettings;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// Base class for all feature settings tabs.<br/><br/>
    ///
    /// This tab lets users configure the feature's settings and create/redeploy/delete the feature and it's dashboard.
    /// </summary>
    [Serializable]
    public abstract class FeatureSettingsTab : IDrawable
    {
        private const string LEARN_MORE_TEXT = "Learn more about dashboards";
        private const string CURRENT_VALUE = "CurrentValue";
        private List<string> _resourceList;

        protected bool _isFeatureBeingProcessed = false;
        protected bool _isFeatureMainStackBeingRedeployed = false;

        /// <summary>
        /// A list of all features settings tabs that are initialized, used to create the short versions in All Features
        /// </summary>
        public static List<FeatureSettingsTab> FeatureSettingsTabsInstances = new List<FeatureSettingsTab>();

        /// <summary>
        /// The feature which these settings are for.
        /// </summary>
        public abstract FeatureType FeatureType { get; }

        /// <summary>
        /// This feature's specific settings, excluding common settings such as dashboards.
        /// </summary>
        protected abstract IEnumerable<IFeatureSetting> FeatureSpecificSettings { get; }

        /// <summary>
        /// Any secrets that a feature may need to store such as password fields and secret keys. Secrets will be stored in AWS Secrets Manager.
        /// </summary>
        protected abstract IEnumerable<SecretSetting> FeatureSecrets { get; }

        /// <summary>
        /// Whether the <c>DrawSettings()</c> method should be called. If false, then a message saying "No feature settings" will be displayed in the settings section.
        /// </summary>
        protected virtual bool ShouldDrawSettingsSection() => true;

        /// <summary>
        /// Whether the feature allows reload of settings.
        /// </summary>
        protected virtual bool ShouldReloadFeatureSettings() => true;

        /// <summary>
        /// All of this feature's settings, including both feature-specific settings and common settings (such as dashboards).
        /// </summary>
        private IEnumerable<IFeatureSetting> AllFeatureSettings => CommonSettings.Concat(FeatureSpecificSettings);

        // Common Feature Settings
        private IEnumerable<IFeatureSetting> CommonSettings => new List<IFeatureSetting>()
        {
            _isDashboardEnabled
        };

        [SerializeField] private FeatureSettingBool _isDashboardEnabled = new FeatureSettingBool("cloudwatch_dashboard_enabled", defaultValue: true);

        // State
        [SerializeField] private Vector2 _scrollPosition;
        private bool _isRefreshing;

        // Dependencies
        protected ICoreWrapperProvider _coreWrapper;
        private GameKitManager _gameKitManager;
        private GameKitEditorManager _gameKitEditorManager;
        private FeatureResourceManager _featureResourceManager;
        private FeatureDeploymentOrchestrator _featureDeploymentOrchestrator;
        protected SerializedProperty _serializedProperty;

        // Links
        private readonly LinkWidget _learnMoreAboutDashboardsDeployedLink = new LinkWidget(L10n.Tr("Active"), L10n.Tr(LEARN_MORE_TEXT), DocumentationURLs.CLOUDWATCH_DASHBOARDS_REFERENCE);
        private readonly LinkWidget _learnMoreAboutDashboardsUndeployedLink = new LinkWidget(L10n.Tr("Inactive"), L10n.Tr(LEARN_MORE_TEXT), DocumentationURLs.CLOUDWATCH_DASHBOARDS_REFERENCE, spaceAfterPrependedText: 10f);
        private readonly LinkWidget _learnMoreAboutDashboardsNoCredentialsLink = new LinkWidget(L10n.Tr("Enter valid environment and credentials to see dashboard status."), L10n.Tr(LEARN_MORE_TEXT), DocumentationURLs.CLOUDWATCH_DASHBOARDS_REFERENCE, spaceAfterPrependedText: 10f);

        public virtual void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            // Dependencies
            _coreWrapper = dependencies.CoreWrapper;
            _gameKitManager = dependencies.GameKitManager;
            _gameKitEditorManager = dependencies.GameKitEditorManager;
            _featureResourceManager = dependencies.FeatureResourceManager;
            _featureDeploymentOrchestrator = dependencies.FeatureDeploymentOrchestrator;
            _serializedProperty = serializedProperty;

            foreach (SecretSetting featureSecret in FeatureSecrets)
            {
                if (_coreWrapper.GameKitAccountCheckSecretExists(featureSecret.SecretIdentifier) == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    featureSecret.IsStoredInCloud = true;
                }
            }

            LoadFeatureSettings(false);

            if (!FeatureSettingsTabsInstances.Contains(this))
            {
                FeatureSettingsTabsInstances.Add(this);   
            }
        }

        public void ReloadFeatureSettings()
        {
            if (ShouldReloadFeatureSettings())
            {
                LoadFeatureSettings(true);
            }
        }

        public void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;

                EditorGUILayoutElements.Description(L10n.Tr("Configure how you want this feature to function and create or redeploy the game backend."), indentationLevel: 0);

                EditorGUILayoutElements.SectionDivider();

                if (ShouldDrawSettingsSection())
                {
                    EditorGUILayoutElements.SectionHeader(L10n.Tr("Configure"));
                    DrawSettings();
                }
                else
                {
                    DrawNoSettingsToDisplay();
                }
            }

            GUILayout.FlexibleSpace();

            DrawFooter();
        }

        /// <summary>
        /// Draw the settings for this feature.
        /// </summary>
        protected abstract void DrawSettings();

        /// <summary>
        /// Display a GUI element to let the user know there are no settings to display for the current feature.
        /// </summary>
        private void DrawNoSettingsToDisplay()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayoutElements.SectionHeader(L10n.Tr("No feature settings"), 0, TextAnchor.MiddleCenter);

            GUIStyle centeredTextStyle = new GUIStyle("label");
            centeredTextStyle.alignment = TextAnchor.MiddleCenter;
            EditorGUILayout.LabelField(L10n.Tr("This feature has no settings to configure."), centeredTextStyle);
        }

        private void DrawFooter()
        {
            EditorGUILayoutElements.SectionDivider();

            EditorGUILayoutElements.SectionHeader(L10n.Tr("Deploy"));

            DrawDashboardDeployment();
            EditorGUILayout.Space(15f);
            DrawFeatureDeployment();
            EditorGUILayout.Space(15f);
            DrawFeatureDescription();
        }

        private void DrawDashboardDeployment()
        {
            const string CLOUD_DEPLOY_STATE = "cloudwatch_dashboard_enabled";

            bool isDashboardDeployed;

            Dictionary<string, string> settings = new Dictionary<string, string>();
            if (_gameKitEditorManager.CredentialsSubmitted)
            {
                settings = _coreWrapper.SettingsGetFeatureVariables(FeatureType);
            }

            if (settings.ContainsKey(CLOUD_DEPLOY_STATE))
            {
                string dashboardDeployed;
                settings.TryGetValue(CLOUD_DEPLOY_STATE, out dashboardDeployed);

                isDashboardDeployed = Boolean.Parse(dashboardDeployed);
            }
            else
            {
                isDashboardDeployed = false;
            }

            bool isDeployButtonEnabled = _featureDeploymentOrchestrator.CanCreateFeature(FeatureType).CanExecuteAction || _featureDeploymentOrchestrator.CanRedeployFeature(FeatureType).CanExecuteAction;

            // Dashboard status
            if (_gameKitEditorManager.CredentialsSubmitted)
            {
                if (isDashboardDeployed)
                {
                    EditorGUILayoutElements.CustomField(L10n.Tr("Dashboard status"), () =>
                    {
                        if (isDeployButtonEnabled)
                        {
                            EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Deployed);
                            GetOpenDashboardLink().OnGUI();
                            EditorGUILayout.Space(20f);
                        }
                        _learnMoreAboutDashboardsDeployedLink.OnGUI();
                    }, indentationLevel: 0);
                }
                else
                {
                    EditorGUILayoutElements.CustomField(L10n.Tr("Dashboard status"), () =>
                    {
                        EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Undeployed);
                        _learnMoreAboutDashboardsUndeployedLink.OnGUI();
                    }, indentationLevel: 0);
                }
            }
            else
            {
                EditorGUILayoutElements.CustomField(L10n.Tr("Dashboard status"), () =>
                {
                    EditorGUILayoutElements.DeploymentStatusIcon(FeatureStatus.Unknown);
                    _learnMoreAboutDashboardsNoCredentialsLink.OnGUI();
                }, indentationLevel: 0);
            }

            // Dashboard action
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel(L10n.Tr("Dashboard action"), indentationLevel: 0);
                EditorGUILayoutElements.KeepPreviousPrefixLabelEnabled();
                DrawDashboardDeploymentButton(isDeployButtonEnabled, isDashboardDeployed);
            }
        }

        private IDrawable GetOpenDashboardLink()
        {
            // This LinkWidget needs to be created during OnGUI to make sure the dashboard URL matches the current environment.
            return new LinkWidget(L10n.Tr("Open Dashboard"), GetDashboardUrl(), new LinkWidget.Options()
            {
                // Remove the gap between this link and the "Learn more" link that comes after.
                // If either of these below options were removed, then there would be a large gap before the "Learn more" link.
                ShouldWordWrapLinkLabel = false,
                Alignment = LinkWidget.Alignment.None
            });
        }

        private string GetDashboardUrl()
        {
            string gameName = _featureResourceManager.GetGameName();
            string environmentCode = _featureResourceManager.GetLastUsedEnvironment();
            string region = _featureResourceManager.GetLastUsedRegion();

            return FeatureType.GetDashboardUrl(gameName, environmentCode, region);
        }

        private void DrawDashboardDeploymentButton(bool isButtonEnabled, bool isDashboardDeployed)
        {
            string buttonText = isDashboardDeployed
                ? L10n.Tr("Deactivate")
                : L10n.Tr("Activate");
            Color buttonColor = isDashboardDeployed
                ? SettingsGUIStyles.Buttons.GUIButtonRed.Get()
                : SettingsGUIStyles.Buttons.GUIButtonGreen.Get();
            Action clickFunction = isDashboardDeployed
                ? (Action)OnClickDashboardDeactivate
                : (Action)OnClickDashboardActivate;

            if (EditorGUILayoutElements.Button(buttonText, isButtonEnabled, colorWhenEnabled: buttonColor))
            {
                clickFunction();
            }
        }

        public FeatureStatus GetFeatureStatus()
        {
            FeatureStatus deploymentStatus = _featureDeploymentOrchestrator.GetFeatureStatus(FeatureType);

            // This ensures that we are not displaying status of Main Stack for another feature that is redeployed in parallel
            if( deploymentStatus == FeatureStatus.GeneratingTemplates || 
                deploymentStatus == FeatureStatus.UploadingDashboards || 
                deploymentStatus == FeatureStatus.UploadingLayers || 
                deploymentStatus == FeatureStatus.UploadingFunctions ||
                deploymentStatus == FeatureStatus.DeployingResources)
            {
                _isFeatureMainStackBeingRedeployed = false;
            }

            // Display deployment status for main stack
            if((_isFeatureBeingProcessed && deploymentStatus == FeatureStatus.Undeployed) || _isFeatureMainStackBeingRedeployed)
            {
                FeatureStatus mainStackStatus = _featureDeploymentOrchestrator.GetFeatureStatus(FeatureType.Main);
                if(mainStackStatus != FeatureStatus.Deployed)
                {
                    deploymentStatus = mainStackStatus;
                }
            }

            return deploymentStatus;
        }

        private string AppendBlockingFeatures(string originalMessage, CanExecuteDeploymentActionResult result)
        {
            string features = string.Join(", ", result.BlockingFeatures.Select(f => f.GetDisplayName()).ToList());
            return originalMessage + features;
        }

        private string DisabledDeploymentReasons(CanExecuteDeploymentActionResult result)
        {
            if (result.CanExecuteAction)
            {
                return string.Empty;
            }

            switch (result.Reason)
            {
                case DeploymentActionBlockedReason.CredentialsInvalid:
                    return "Must submit valid credentials before this action is enabled";

                case DeploymentActionBlockedReason.DependenciesMustBeCreated:
                    return AppendBlockingFeatures("Must deploy these features first for this action to be enabled: ", result);

                case DeploymentActionBlockedReason.DependenciesMustBeDeleted:
                    return AppendBlockingFeatures("Must delete these features first for this action to be enabled: ", result);

                case DeploymentActionBlockedReason.DependenciesStatusIsInvalid:
                    return AppendBlockingFeatures("A feature this action depends on has an invalid status: ", result);

                case DeploymentActionBlockedReason.FeatureMustBeCreated:
                    return "This feature must be created first for this action to be enabled.";

                case DeploymentActionBlockedReason.FeatureMustBeDeleted:
                    return "This feature must be deleted first for this action to be enabled.";

                case DeploymentActionBlockedReason.FeatureStatusIsUnknown:
                    return "The status of this feature cannot be determined, this action will be disabled until status is retrievable.";

                case DeploymentActionBlockedReason.OngoingDeployments:
                    if (!result.BlockingFeatures.Contains(this.FeatureType))
                    {
                        return AppendBlockingFeatures("This action is disabled while the following features are updating: ", result);
                    }

                    return "This action is disabled while any update is in progress for this feature.";

                case DeploymentActionBlockedReason.MainStackNotReady:
                    return "This action is disabled while the Main stack is updating and not in a ready state";

                default:
                    return "Action is disabled for an unknown reason.";
            }
        }

        private void DrawFeatureDeployment()
        {
            // State
            FeatureStatus deploymentStatus = GetFeatureStatus();
            CanExecuteDeploymentActionResult canCreate = _featureDeploymentOrchestrator.CanCreateFeature(FeatureType);
            CanExecuteDeploymentActionResult canRedeploy = _featureDeploymentOrchestrator.CanRedeployFeature(FeatureType);
            CanExecuteDeploymentActionResult canDelete = _featureDeploymentOrchestrator.CanDeleteFeature(FeatureType);

            // Deployment status
            if (_gameKitEditorManager.CredentialsSubmitted)
            {
                EditorGUILayoutElements.CustomField(L10n.Tr("Deployment status"), () =>
                    {
                        EditorGUILayoutElements.DeploymentStatusIcon(deploymentStatus);
                        GUILayout.Label(deploymentStatus.GetDisplayName(), CommonGUIStyles.DeploymentStatusText);
                        if(EditorGUILayoutElements.DeploymentRefreshIconButton(_isRefreshing))
                        {
                            OnClickFeatureRefresh();
                        } 
                        GUILayout.FlexibleSpace();
                    },
                    guiStyle: CommonGUIStyles.DeploymentStatus,
                    indentationLevel: 0);
            }
            else
            {
                EditorGUILayoutElements.CustomField(L10n.Tr("Deployment status"), () =>
                    {
                        EditorGUILayout.LabelField(L10n.Tr("No environment selected."));
                    },
                    indentationLevel: 0);
            }

            // AWS resource actions
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel(L10n.Tr("AWS resource actions"), indentationLevel: 0);
                EditorGUILayoutElements.KeepPreviousPrefixLabelEnabled();
                if (EditorGUILayoutElements.Button(L10n.Tr("Create"), isEnabled: canCreate.CanExecuteAction && _gameKitEditorManager.CredentialsSubmitted, tooltip: DisabledDeploymentReasons(canCreate), colorWhenEnabled: SettingsGUIStyles.Buttons.GUIButtonGreen.Get()))
                {
                    _isFeatureBeingProcessed = true;
                    OnClickFeatureCreate();
                }

                if (EditorGUILayoutElements.Button(L10n.Tr("Redeploy"), isEnabled: canRedeploy.CanExecuteAction && _gameKitEditorManager.CredentialsSubmitted, tooltip: DisabledDeploymentReasons(canRedeploy)))
                {
                    _isFeatureMainStackBeingRedeployed = true;
                    OnClickFeatureRedeploy();
                }

                if (EditorGUILayoutElements.Button(L10n.Tr("Delete"), isEnabled: canDelete.CanExecuteAction && _gameKitEditorManager.CredentialsSubmitted, tooltip: DisabledDeploymentReasons(canDelete), colorWhenEnabled: SettingsGUIStyles.Buttons.GUIButtonRed.Get()))
                {
                    _isFeatureBeingProcessed = true;
                    OnClickFeatureDelete();
                }
            }
        }

        private void DrawFeatureDescription()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayoutElements.HelpBoxWithReadMore($"{FeatureType.GetVerboseDescription()} {L10n.Tr("Uses AWS services")}: {FeatureType.GetResourcesUIString()}", FeatureType.GetDocumentationUrl());
            }
        }
        
        #region All Features summary

        public void DrawFeatureSummary()
        {
            // State
            FeatureStatus deploymentStatus = GetFeatureStatus();
            CanExecuteDeploymentActionResult canCreate = _featureDeploymentOrchestrator.CanCreateFeature(FeatureType);
            CanExecuteDeploymentActionResult canRedeploy = _featureDeploymentOrchestrator.CanRedeployFeature(FeatureType);
            CanExecuteDeploymentActionResult canDelete = _featureDeploymentOrchestrator.CanDeleteFeature(FeatureType);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                // Deployment status
                if (_gameKitEditorManager.CredentialsSubmitted)
                {
                    EditorGUILayoutElements.CustomField(L10n.Tr(FeatureType.GetDisplayName()), () =>
                        {
                            EditorGUILayoutElements.DeploymentStatusIcon(deploymentStatus);
                            GUILayout.Label(deploymentStatus.GetDisplayName());
                            GUILayout.FlexibleSpace();
                        },
                        indentationLevel: 0);
                }
                else
                {
                    EditorGUILayoutElements.CustomField(L10n.Tr(FeatureType.GetDisplayName()),
                        () => { EditorGUILayout.LabelField(L10n.Tr("No environment selected.")); },
                        indentationLevel: 0);
                }

                // AWS resource actions
                if (EditorGUILayoutElements.SmallButton(L10n.Tr("Create"),
                        isEnabled: canCreate.CanExecuteAction && _gameKitEditorManager.CredentialsSubmitted,
                        colorWhenEnabled: SettingsGUIStyles.Buttons.GUIButtonGreen.Get(),
                        tooltip: DisabledDeploymentReasons(canCreate)))
                {
                    _isFeatureBeingProcessed = true;
                    OnClickFeatureCreate();
                }

                if (EditorGUILayoutElements.SmallButton(L10n.Tr("Redeploy"),
                        isEnabled: canRedeploy.CanExecuteAction && _gameKitEditorManager.CredentialsSubmitted,
                        tooltip: DisabledDeploymentReasons(canRedeploy)))
                {
                    _isFeatureMainStackBeingRedeployed = true;
                    OnClickFeatureRedeploy();
                }

                if (EditorGUILayoutElements.SmallButton(L10n.Tr("Delete"),
                        isEnabled: canDelete.CanExecuteAction && _gameKitEditorManager.CredentialsSubmitted,
                        colorWhenEnabled: SettingsGUIStyles.Buttons.GUIButtonRed.Get(),
                        tooltip: DisabledDeploymentReasons(canDelete)))
                {
                    _isFeatureBeingProcessed = true;
                    OnClickFeatureDelete();
                }
            }
        }
        #endregion

        #region Click Functions
        private void OnClickDashboardActivate()
        {
            _isDashboardEnabled.CurrentValue = true;
            OnToggleDashboard();
        }

        private void OnClickDashboardDeactivate()
        {
            _isDashboardEnabled.CurrentValue = false;
            OnToggleDashboard();
        }

        private void OnToggleDashboard()
        {
            if (_featureDeploymentOrchestrator.CanCreateFeature(FeatureType).CanExecuteAction)
            {
                OnClickFeatureCreate();
                return;
            }

            if (_featureDeploymentOrchestrator.CanRedeployFeature(FeatureType).CanExecuteAction)
            {
                OnClickFeatureRedeploy();
                return;
            }
        }

        /// <summary>
        /// Upon deployment or redeployment will save any enabled and non empty secret values, such as secret IDs, to AWS Secrets Manager
        /// </summary>
        protected void SaveSecretSettingsToCloud()
        {
            GUI.FocusControl(null); // Used to ensure that no secret ID inputs are selected. Clearing out the value of a selected field will not refresh it in the GUI if selected.

            foreach (SecretSetting featureSecret in FeatureSecrets)
            {
                if (!string.IsNullOrEmpty(featureSecret.SecretValue))
                {
                    if (_coreWrapper.GameKitAccountSaveSecret(featureSecret.SecretIdentifier, featureSecret.SecretValue) == GameKitErrors.GAMEKIT_SUCCESS)
                    {
                        featureSecret.IsStoredInCloud = true;
                        featureSecret.SecretValue = string.Empty;
                    }
                    else
                    {
                        featureSecret.IsStoredInCloud = false;
                    }
                }
            }
        }

        private void OnClickFeatureCreate()
        {
            SaveFeatureSettings();

            SaveSecretSettingsToCloud();

            _featureDeploymentOrchestrator.CreateFeature(FeatureType, (DeploymentResponseResult response) =>
            {
                if (response.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    _gameKitManager.CopyAndReloadConfigFile(_featureResourceManager.GetGameName(), _featureResourceManager.GetLastUsedEnvironment());
                }
                else
                {
                    // Any errors are already logged by CreateFeature().
                }
                _isFeatureBeingProcessed = false;
            });
        }

        private void OnClickFeatureRedeploy()
        {
            SaveFeatureSettings();

            SaveSecretSettingsToCloud();

            _featureDeploymentOrchestrator.RedeployFeature(FeatureType, (DeploymentResponseResult response) =>
            {
                if (response.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    _gameKitManager.CopyAndReloadConfigFile(_featureResourceManager.GetGameName(), _featureResourceManager.GetLastUsedEnvironment());
                }
                else
                {
                    // Any errors are already logged by RedeployFeature().
                }
                // Reset the value when the feature stack deployment completes in either success or failure
                _isFeatureMainStackBeingRedeployed = false;
            });
        }
        
        private void OnClickFeatureRefresh()
        {
            _isRefreshing = true;

            _featureDeploymentOrchestrator.RefreshFeatureStatuses((result =>
            {
                // This API always returns GAMEKIT_SUCCESS
                _isRefreshing = false;
                _featureResourceManager.InitializeSettings(true);
            }));
        }

        private void OnClickFeatureDelete()
        {
            _resourceList = new List<string>();

            _featureDeploymentOrchestrator.DescribeFeatureResources(FeatureType, (MultiResourceInfoCallbackResult result) =>
            {
                for (int i = 0; i < result.LogicalResourceId.Length; i++)
                {
                    _resourceList.Add(result.ResourceType[i] + " resource with id " + result.LogicalResourceId[i] + " in " + result.ResourceStatus[i] + " status.\n");
                }

                // open UI to list the resources, confirm deletion with all the interactions there.
                DeleteFeatureWindow.ShowWindow(FeatureTypeConverter.GetDisplayName(FeatureType), _resourceList, ()=> 
                {
                    _featureDeploymentOrchestrator.DeleteFeature(FeatureType, (DeploymentResponseResult response) =>
                    {
                        if (response.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                        {
                            _isDashboardEnabled.CurrentValue = false;
                            SaveFeatureSettings();
                            _gameKitManager.CopyAndReloadConfigFile(_featureResourceManager.GetGameName(), _featureResourceManager.GetLastUsedEnvironment());
                        }
                        _isFeatureBeingProcessed = false;
                    });

                    _isRefreshing = true;
                    _featureDeploymentOrchestrator.RefreshFeatureStatuses((result =>
                    {
                        // This API always returns GAMEKIT_SUCCESS
                        _isRefreshing = false;
                        _featureResourceManager.InitializeSettings(true);
                    }));
                });
            });
        }
        #endregion

        /// <summary>
        /// Load all of this feature's settings from the saveInfo.yml file.<br/><br/>
        ///
        /// Set all of the feature's settings in memory to their value saved in the saveInfo.yml file (if the persisted values exist), otherwise leave them with their default values.
        /// </summary>
        private void LoadFeatureSettings(bool resetDefaultIfNotPersisted)
        {
            if (!_gameKitEditorManager.CredentialsSubmitted)
            {
                // There's no saveInfo.yml file to load.
                // Use the default settings for this feature.
                return;
            }

            foreach (IFeatureSetting featureSetting in AllFeatureSettings)
            {
                if (_featureResourceManager.TryGetFeatureVariable(FeatureType, featureSetting.VariableName, out string persistedValue))
                {
                    featureSetting.SetCurrentValueFromString(persistedValue);
                }
                else
                {
                    // Use the default settings.
                    // The featureSetting.CurrentValue was already set to the default value during the FeatureSetting's class constructor.
                    // Reset explicitly to default if flag is set
                    if (resetDefaultIfNotPersisted)
                    {
                        featureSetting.SetCurrentValueFromString(featureSetting.DefaultValueString);
                    }
                }
            }
        }

        /// <summary>
        /// Save this feature's settings to the saveInfo.yml file.
        /// </summary>
        private void SaveFeatureSettings()
        {
            _featureResourceManager.SetFeatureVariables(
                AllFeatureSettings.Select(featureSetting => Tuple.Create(FeatureType, featureSetting.VariableName, featureSetting.CurrentValueString)),
                () => { });
        }

        protected SerializedProperty GetFeatureSettingProperty(string settingName)
        {
            SerializedProperty settingProperty = _serializedProperty.FindPropertyRelative(settingName);
            return settingProperty.FindPropertyRelative(CURRENT_VALUE);
        }
    }
}