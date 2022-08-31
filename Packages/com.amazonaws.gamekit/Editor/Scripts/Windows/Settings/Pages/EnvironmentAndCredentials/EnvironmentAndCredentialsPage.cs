// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// Unity
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

// GameKit
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.EnvironmentAndCredentials
{
    [Serializable]
    public class EnvironmentAndCredentialsPage : Page
    {
        // All keywords cannot be used in the games title or environment code
        private static readonly string[] RESERVED_KEYWORDS = { "aws", "amazon", "cognito" };

        // UI Strings
        private static readonly string GAMEKIT_INTRODUCTION = L10n.Tr("If you want to get the full experience of what GameKit offers, go to AWS to create an account, then provide your " +
                                                              "credentials in the GameKit plugin. Your new AWS account comes with a slate of free usage benefits, including " +
                                                              "all of the AWS services that GameKit game features use. ");

        private static readonly string AWS_INTRODUCTION = L10n.Tr("With an AWS account, you can get in-depth, hands-on experience with each GameKit game feature, all for free. You can " +
                                                          "work with the full GameKit plugin, customize each GameKit feature and add it to your game, create the necessary AWS " +
                                                          "cloud resources, and then test to see your new GameKit game features in action. Without an AWS account, you can view " +
                                                          "some areas of the GameKit plugin and explore the GameKit sample materials.");

        private static readonly string CHANGE_ENVIRONMENT_WARNING = L10n.Tr("You can switch to another environment, change the AWS Region for deployments, or enter new AWS credentials. " +
                                                                    "After changing settings, you must choose Submit. Are you sure that you want to change environment settings?" +
                                                                    "\n\nWARNING: This action will result in any local Achievement configuration data being lost, " + 
                                                                    "please upload any local data to the cloud by clicking on \"Save data\" on the Achievements Configure Data tab." +
                                                                    "\n\nNOTE: After submitting new environment settings, you must restart Unity Editor.");

        private static readonly string EXISTING_CREDENTIALS_INFO = L10n.Tr("Set credentials for this new environment. Use existing values (carried over from the previous environment) or enter new ones.");

        private static readonly string LOCATE_EXISTING_CONFIG = L10n.Tr("Locate existing");

        private static readonly string CHANGE_ENVIRONMENT_TOOLTIP = L10n.Tr("You can't switch environments while AWS resources are deploying or updating");

        private static readonly string CHANGE_ENVIRONMENT_AND_CREDS = L10n.Tr("Change environment + credentials");

        private static readonly string CANCEL_ENVIRONMENT_AND_CREDS_CHANGE = L10n.Tr("Cancel environment + credentials change");

        // Key that is used for the option that allows for creation of a new environment 
        private const string NEW_CUSTOM_ENV_KEY = ":::";

        // This string is shown in place of the user's AWS Account ID when there is an error determining the Account ID, when either the access or secret keys are not syntactically correct, or when there is trouble communicating with AWS.
        private const string AWS_ACCOUNT_ID_EMPTY = "...";
        private const string AWS_ACCOUNT_ID_LOADING = "Loading...";
        private static readonly string AWS_ACCOUNT_INVALID_PAIR = L10n.Tr("The AWS credentials entered are not a valid pair.");
        private static readonly string AWS_ACCOUNT_NOT_VALIDATED = L10n.Tr("The user credentials you provided cannot be validated.\nPlease enter a valid access key pair or create a new one using AWS IAM.");
        private static readonly string INTERNET_CONNECTIVITY_ISSUE = L10n.Tr("Internet is not reachable. Restore internet and reopen the Project Settings window to work with AWS GameKit features.");

        public override string DisplayName => "Environment & Credentials";

        // Aws Environment and region
        private readonly IDictionary<string, string> _environmentMapping = new Dictionary<string, string>();
        private readonly IDictionary<string, string> _regionMapping = new Dictionary<string, string>();
        private readonly IList<string> _environmentOptions = new List<string>();
        private readonly IList<string> _regionOptions = new List<string>();

        // Dependencies
        private GameKitManager _gameKitManager;
        private GameKitEditorManager _gameKitEditorManager;
        private FeatureResourceManager _featureResourceManager;
        private FeatureDeploymentOrchestrator _featureDeploymentOrchestrator;
        private CredentialsManager _credentialsManager;
        private SerializedProperty _serializedProperty;
        private UnityEvent _onEnvironmentOrRegionChange;

        // State
        private bool _isNewProject = true;
        private bool _showNewEnvironmentNotification = false;
        [SerializeField] private bool _showNewToAws = false;
        [SerializeField] private Vector2 _scrollPosition;

        // UI Fields
        private string _projectAlias = string.Empty;
        private string _accessKeyId = string.Empty;
        private string _secretAccessKey = string.Empty;
        private string _awsAccountId = AWS_ACCOUNT_ID_EMPTY;

        private int _currentEnvironment = 0;
        private int _currentRegion = 3; // Set to "us-west-2" by design

        private string _customEnvironmentName = string.Empty;
        private string _customEnvironmentCode = string.Empty;

        // Error message values
        private string _projectAliasErrorMessage = string.Empty;
        private string _customEnvironmentNameErrorMessage = string.Empty;
        private string _customEnvironmentCodeErrorMessage = string.Empty;
        private string _accessKeyIdErrorMessage = string.Empty;
        private string _secretKeyErrorMessage = string.Empty;
        private string _credentialPairErrorMessage = string.Empty;

        private const string validLowerCaseAndNumericalPattern = @"^[a-z0-9]+$";
        private readonly Regex _validLowerCaseAndNumericalRx = new Regex(validLowerCaseAndNumericalPattern);
        private const string validAlphaNumericPattern = @"^[A-Za-z0-9]+$";
        private readonly Regex _validAlphanumericRx = new Regex(validAlphaNumericPattern);
        private const string validUpperCaseAndNumericalPattern = @"^[A-Z0-9]+$";
        private readonly Regex _validUpperCaseAndNumericalRx = new Regex(validUpperCaseAndNumericalPattern);

        private LinkWidget _learnAboutFreeTierLinkWidget;
        private LinkWidget _beyondFreeTierLinkWidget;
        private LinkWidget _forgotCredentialsLinkWidget;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            // Dependencies
            _gameKitManager = dependencies.GameKitManager;
            _gameKitEditorManager = dependencies.GameKitEditorManager;
            _featureResourceManager = dependencies.FeatureResourceManager;
            _featureDeploymentOrchestrator = dependencies.FeatureDeploymentOrchestrator;
            _credentialsManager = dependencies.CredentialsManager;
            _serializedProperty = serializedProperty;
            _onEnvironmentOrRegionChange = dependencies.OnEnvironmentOrRegionChange;

            // Populate controls
            PopulateDefaultEnvironments();
            PopulateRegions();

            // Try to load and set initial state
            SetInitialState();
        }

        protected override IList<string> GetTitle()
        {
            return new List<string>() { DisplayName };
        }

        protected override void DrawContent()
        {
            CreateLinkWidgets();

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;

                DrawNewToAwsSection();

                EditorGUILayoutElements.SectionSpacer();

                DrawProjectAliasSection();

                EditorGUILayoutElements.SectionSpacer();

                DrawEnvironmentAndRegionSection();

                EditorGUILayoutElements.SectionSpacer();

                DrawAwsAccountCredentialsSection();

                EditorGUILayoutElements.SectionSpacer(extraPixels: -5);
            }

            EditorGUILayoutElements.HorizontalDivider();

            EditorGUILayoutElements.SectionSpacer(extraPixels: -5);

            DrawSubmitCredentialsSection();
        }

        /// <summary>
        /// Avoid a null-reference exception by creating the links during OnGUI instead of the constructor.
        /// The exception happens because the EditorStyles used in SettingsGUIStyles (ex: EditorStyles.foldout) are null until the first frame of the GUI.
        /// </summary>
        private void CreateLinkWidgets()
        {
            LinkWidget.Options options = new LinkWidget.Options()
            {
                OverallStyle = SettingsGUIStyles.Page.Paragraph,
                ContentOffset = new Vector2(
                    x: -3, // Match the left alignment of the surrounding paragraph text.
                    y: -3  // Vertically center the links.
                ),
            };

            _learnAboutFreeTierLinkWidget = new LinkWidget(L10n.Tr("Learn more about the AWS free tier."), DocumentationURLs.FREE_TIER_INTRO, options);
            _beyondFreeTierLinkWidget = new LinkWidget(L10n.Tr("Ready to move beyond the free tier?"), L10n.Tr("Learn more about controlling costs with AWS."), DocumentationURLs.FREE_TIER_REFERENCE, options);
            _forgotCredentialsLinkWidget = new LinkWidget(L10n.Tr("Help me get my user credentials."), DocumentationURLs.SETTING_UP_CREDENTIALS, options);
        }

        /// <summary>
        /// Draws the section containing the New to Aws foldout and all contents within the foldout.
        /// </summary>
        private void DrawNewToAwsSection()
        {
            Color originalBackgroundColor = GUI.backgroundColor;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _showNewToAws = EditorGUILayout.Foldout(_showNewToAws, L10n.Tr("  New to AWS?"), SettingsGUIStyles.Page.FoldoutTitle);

                if (_showNewToAws)
                {
                    using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.Page.FoldoutBox))
                    {
                        EditorGUILayout.LabelField(GAMEKIT_INTRODUCTION, SettingsGUIStyles.Page.Paragraph);
                        EditorGUILayout.LabelField(AWS_INTRODUCTION, SettingsGUIStyles.Page.Paragraph);

                        GUI.backgroundColor = SettingsGUIStyles.Buttons.GUIButtonGreen.Get();

                        if (GUILayout.Button(L10n.Tr("Create an account"), SettingsGUIStyles.Buttons.CreateAccountButton))
                        {
                            Application.OpenURL(DocumentationURLs.CREATE_ACCOUNT);
                        }

                        GUI.backgroundColor = originalBackgroundColor;

                        _learnAboutFreeTierLinkWidget.OnGUI();
                        _beyondFreeTierLinkWidget.OnGUI();
                    }
                }
            }
        }

        /// <summary>
        /// Draws the section containing the Aws project alias.
        /// </summary>
        private void DrawProjectAliasSection()
        {
            EditorGUILayoutElements.SectionHeader(L10n.Tr("Create your project in the cloud"));

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.Page.VerticalLayout))
                {
                    EditorGUILayoutElements.PrefixLabel(L10n.Tr("AWS project alias"));
                    if (_isNewProject)
                    {
                        EditorGUILayout.LabelField(L10n.Tr("Cannot be changed later"),
                            SettingsGUIStyles.Page.PrefixLabelSubtext);
                    }
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (_isNewProject)
                        {
                            using (EditorGUI.ChangeCheckScope projectAliasChangeCheck = new EditorGUI.ChangeCheckScope())
                            {
                                _projectAlias = EditorGUILayout.TextArea(_projectAlias);

                                if (projectAliasChangeCheck.changed)
                                {
                                    OnProjectAliasChanged();
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(_projectAlias);
                        }
                    }

                    if (_isNewProject)
                    {
                        EditorGUILayout.LabelField(
                            L10n.Tr("The project alias must have 1-12 characters. Valid characters: a-z, 0-9."),
                            SettingsGUIStyles.Page.TextAreaSubtext);
                    }
                    else
                    {
                        if (_gameKitEditorManager.CredentialsSubmitted)
                        {
                            string current_tooltip = _featureDeploymentOrchestrator.IsAnyFeatureUpdating()
                                ? CHANGE_ENVIRONMENT_TOOLTIP
                                : string.Empty;

                            if (EditorGUILayoutElements.Button(CHANGE_ENVIRONMENT_AND_CREDS, tooltip: current_tooltip, isEnabled: !_featureDeploymentOrchestrator.IsAnyFeatureUpdating()))
                            {
                                OnChangeEnvironmentAndCredentials();
                            }
                        }
                        else
                        {
                            if (EditorGUILayoutElements.Button(CANCEL_ENVIRONMENT_AND_CREDS_CHANGE, isEnabled: !_featureDeploymentOrchestrator.IsAnyFeatureUpdating()))
                            {
                                OnCancelEnvironmentAndCredentialsChange();
                            }
                        }
                    }

                    if (!IsGameNameValid())
                    {
                        EditorGUILayout.HelpBox(_projectAliasErrorMessage, MessageType.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the section containing the environment and regions dropdown.
        /// </summary>
        private void DrawEnvironmentAndRegionSection()
        {
            EditorGUILayoutElements.SectionHeader(L10n.Tr("Select an environment"));

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel(L10n.Tr("Environment"));
                EditorGUI.BeginChangeCheck();

                using (EditorGUI.ChangeCheckScope environmentChangeCheck = new EditorGUI.ChangeCheckScope())
                {
                    using (new EditorGUI.DisabledScope(!AreAwsCredentialInputsEnabled()))
                    {
                        _currentEnvironment = EditorGUILayout.Popup(_currentEnvironment, _environmentOptions.ToArray());
                    }

                    if (environmentChangeCheck.changed)
                    {
                        OnEnvironmentSelectionChanged();
                    }
                }
            }

            if (GetSelectedEnvironmentKey() == NEW_CUSTOM_ENV_KEY)
            {
                DrawNewEnvironmentSection();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel("Region");
                using (new EditorGUI.DisabledScope(!AreAwsCredentialInputsEnabled()))
                {
                    _currentRegion = EditorGUILayout.Popup(_currentRegion, _regionOptions.ToArray());
                }
            }
        }

        /// <summary>
        /// Draws the section containing the forms to fill out for a new environment. 
        /// </summary>
        private void DrawNewEnvironmentSection()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.CustomEnvironmentVerticalLayout))
            {
                EditorGUILayout.LabelField(L10n.Tr("Environment name"));
                _customEnvironmentName = EditorGUILayout.TextArea(_customEnvironmentName);
                EditorGUILayout.LabelField(L10n.Tr("The environment name must have 1-16 characters. Valid characters: A-Z, a-z, 0-9, space."), SettingsGUIStyles.Page.TextAreaSubtext);
            }

            if (_customEnvironmentName.Length != 0 && !IsEnvironmentNameValid())
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.CustomEnvironmentErrorVerticalLayout))
                {
                    EditorGUILayout.HelpBox(_customEnvironmentNameErrorMessage, MessageType.Error);
                }
            }

            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.CustomEnvironmentVerticalLayout))
            {
                EditorGUILayout.LabelField(L10n.Tr("Environment code"));
                GUI.SetNextControlName("EnvironmentCodeTextField");
                _customEnvironmentCode = EditorGUILayout.TextArea(_customEnvironmentCode);
                EditorGUILayout.LabelField(L10n.Tr("The environment code must have 2-3 characters. Valid characters: a-z, 0-9."), SettingsGUIStyles.Page.TextAreaSubtext);
            }

            if (_customEnvironmentCode.Length != 0 && !IsEnvironmentCodeValid())
            {
                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.CustomEnvironmentErrorVerticalLayout))
                {
                    EditorGUILayout.HelpBox(_customEnvironmentCodeErrorMessage, MessageType.Error);
                }
            }
        }

        /// <summary>
        /// Draws the section containing the forms to fill out for a new environment.
        /// </summary>
        private void DrawAwsAccountCredentialsSection()
        {
            EditorGUILayoutElements.SectionHeader(L10n.Tr("AWS account credentials"));

            if (_showNewEnvironmentNotification)
            {
                using (new EditorGUILayout.HorizontalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.AccountCredentialsHelpBoxesVerticalLayout))
                {
                    EditorGUILayout.HelpBox(EXISTING_CREDENTIALS_INFO, MessageType.Info);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel(L10n.Tr("Access Key ID"));
                using (new EditorGUILayout.VerticalScope())
                {
                    using (EditorGUI.ChangeCheckScope accessKeyChangeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        using (new EditorGUI.DisabledScope(!AreAwsCredentialInputsEnabled()))
                        {
                            _accessKeyId = EditorGUILayout.TextField(_accessKeyId);
                        }

                        if (accessKeyChangeCheck.changed)
                        {
                            OnAwsCredentialsChanged(IsAccessKeyValid());
                        }
                    }

                    if (!string.IsNullOrEmpty(_accessKeyIdErrorMessage))
                    {
                        using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.AccountCredentialsHelpBoxesVerticalLayout))
                        {
                            EditorGUILayout.HelpBox(_accessKeyIdErrorMessage, MessageType.Error);
                            GUILayout.Space(SettingsGUIStyles.EnvironmentAndCredentialsPage.SpaceAfterAccountIdHelpBox);
                        }
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel(L10n.Tr("Secret Access Key"));
                using (new EditorGUILayout.VerticalScope())
                {
                    using (EditorGUI.ChangeCheckScope secretKeyChangeCheck = new EditorGUI.ChangeCheckScope())
                    {
                        using (new EditorGUI.DisabledScope(!AreAwsCredentialInputsEnabled()))
                        {
                            _secretAccessKey = EditorGUILayout.PasswordField(_secretAccessKey);
                        }

                        if (secretKeyChangeCheck.changed)
                        {
                            OnAwsCredentialsChanged(IsSecretKeyValid());
                        }
                    }

                    if (!string.IsNullOrEmpty(_secretKeyErrorMessage))
                    {
                        using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.AccountCredentialsHelpBoxesVerticalLayout))
                        {
                            EditorGUILayout.HelpBox(_secretKeyErrorMessage, MessageType.Error);
                            GUILayout.Space(SettingsGUIStyles.EnvironmentAndCredentialsPage.SpaceAfterAccountIdHelpBox);
                        }
                    }
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.PrefixLabel(L10n.Tr("AWS Account ID"));
                using (new EditorGUILayout.VerticalScope())
                {
                    using (new EditorGUI.DisabledScope(!AreAwsCredentialInputsEnabled()))
                    {
                        EditorGUILayout.LabelField(_awsAccountId);
                    }

                    if (!string.IsNullOrEmpty(_credentialPairErrorMessage))
                    {
                        using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.AccountCredentialsHelpBoxesVerticalLayout))
                        {
                            EditorGUILayout.HelpBox(_credentialPairErrorMessage, MessageType.Error);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the section containing the options and information regarding credential submission.
        /// </summary>
        private void DrawSubmitCredentialsSection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!IsSubmitButtonEnabled()))
                {
                    if (GUILayout.Button(L10n.Tr("Submit"), SettingsGUIStyles.Buttons.SubmitCredentialsButton))
                    {
                        OnSubmit();
                    }
                }
                GUILayout.FlexibleSpace();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.HorizontalScope(SettingsGUIStyles.EnvironmentAndCredentialsPage.GetUserCredentialsLinkLayout))
                {
                    _forgotCredentialsLinkWidget.OnGUI();
                }
            }
        }

        /// <summary>
        /// Checks if any user input options under the 'Select an environment' section should be enabled.
        /// </summary>
        /// <returns>A boolean value determining if the Environment inputs in the Environment and Credentials settings should be enabled.</returns>
        private bool AreEnvironmentInputsEnabled()
        {
            return !_gameKitEditorManager.CredentialsSubmitted && !string.IsNullOrEmpty(_projectAlias);
        }

        /// <summary>
        /// Checks if any user input options under the 'AWS account credentials' section should be enabled.
        /// </summary>
        /// <returns>A boolean value determining if the Aws credentials inputs in the Environment and Credentials settings should be enabled.</returns>
        private bool AreAwsCredentialInputsEnabled()
        {
            return !_gameKitEditorManager.CredentialsSubmitted && !string.IsNullOrEmpty(_projectAlias);
        }

        /// <summary>
        /// Checks if the submit button should be enabled or disabled.
        /// </summary>
        /// <returns>A boolean value determining if the submit button in the Environment and Credentials settings should be enabled.</returns>
        private bool IsSubmitButtonEnabled()
        {
            bool isCustomEnvironment = _environmentMapping.Keys.ElementAt(_currentEnvironment) == NEW_CUSTOM_ENV_KEY;

            if (isCustomEnvironment && (string.IsNullOrEmpty(_customEnvironmentCode) || _customEnvironmentCode.Length < 2 || string.IsNullOrEmpty(_customEnvironmentName) || !IsEnvironmentCodeValid() || !IsEnvironmentNameValid()))
            {
                return false;
            }

            return !_gameKitEditorManager.CredentialsSubmitted && !string.IsNullOrEmpty(_projectAlias) && IsGameNameValid() && !_awsAccountId.Equals(AWS_ACCOUNT_ID_LOADING) && !_awsAccountId.Equals(AWS_ACCOUNT_ID_EMPTY) && string.IsNullOrEmpty(_credentialPairErrorMessage);
        }

        /// <summary>
        /// Reads from the project name in _featureResourceManager or in the UI to gather and set the rest of the information needed in the Environment and Credentials settings.
        /// </summary>
        private void SetInitialState()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                EditorUtility.DisplayDialog(L10n.Tr("No Internet Connection"), "Must have an internet connection to configure AWS GameKit Settings, restart settings window after connection is re-established.", "Ok");
                Debug.LogWarning("No internet connection, won't be able to customize AWS GameKit Settings until connection is re-established, and settings window is reopened.");
                return;
            }

            if (!string.IsNullOrEmpty(_featureResourceManager.GetAccountInfo().GameName))
            {
                _isNewProject = false;
                _projectAlias = _featureResourceManager.GetAccountInfo().GameName;
                PopulateCustomEnvironments();
                LoadLastUsedEnvironment();
                LoadLastUsedRegion();

                if (LoadAndSetAwsCredentials())
                {
                    SetValidCredentialsSubmitted(IsAccessKeyValid() && IsSecretKeyValid());
                }
            }

        }

        /// <summary>
        /// Set whether valid credentials have been submitted.<br/><br/>
        ///
        /// If they have, inform the GameKitEditorManager and call OnValidCredentialsSubmitted().
        /// </summary>
        /// <param name="areSubmitted">True if valid credentials have been submitted, false if not.</param>
        private void SetValidCredentialsSubmitted(bool areSubmitted)
        {
            _gameKitEditorManager.CredentialsSubmitted = areSubmitted;

            if (areSubmitted)
            {
                OnValidCredentialsSubmitted();
            }
        }

        /// <summary>
        /// Attempt to set credentials on the FeatureDeploymentOrchestrator and refresh feature statuses.
        /// </summary>
        private void OnValidCredentialsSubmitted()
        {
            SetCredentialsDesc desc = new SetCredentialsDesc()
            {
                AccountCredentials = _featureResourceManager.GetAccountCredentials(),
                AccountInfo = _featureResourceManager.GetAccountInfo()
            };

            if (_featureDeploymentOrchestrator.SetCredentials(desc) != GameKitErrors.GAMEKIT_SUCCESS)
            {
                // The error is already logged by SetCredentials.
                return;
            }

            _featureDeploymentOrchestrator.RefreshFeatureStatuses(response =>
            {
                // Take no action, the response.ResultCode is always GAMEKIT_SUCCESS.
            });

            _gameKitEditorManager.ReloadAllFeatureSettings();
        }

        /// <summary>
        /// Attempts to load and set Aws credentials from the user's .aws/credentials file.
        /// </summary>
        /// <returns>True if the Aws credentials have successfully been reloaded into the environment and credentials page.</returns>
        private bool LoadAndSetAwsCredentials()
        {
            if (TryLoadAwsCredentialsFromFile())
            {
                OnAwsCredentialsChanged(true);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Opens a file explorer for users to select a pre-existing saveInfo.yml file.
        /// 
        /// When a user selects a existing saveInfo.yml file their custom environments, last used environments, credentials and project alias will be set automatically.
        /// This change will be persisted after Unity is restarted.
        /// </summary>
        private void OnLoadCustomGameConfigFile()
        {
            string path = EditorUtility.OpenFilePanel("Locate existing saveInfo.yml file", "", "yml");

            if (string.IsNullOrEmpty(path))
            {
                Logging.LogInfo("Configuration file wasn't selected");
                return;
            }

            string fileDirectory = Path.GetDirectoryName(path);
            Logging.LogInfo(L10n.Tr($"Parsed game name from config: {fileDirectory}"));

            _projectAlias = new DirectoryInfo(Path.GetDirectoryName(path)).Name;
        }

        /// <summary>
        /// Checks if the game name that has been input is valid.
        /// </summary>
        /// <returns>True if the game name is valid and false if the game name is not valid.</returns>
        private bool IsGameNameValid()
        {
            if (_projectAlias.Length > 12)
            {
                _projectAliasErrorMessage = L10n.Tr("The game title must have 1 - 12 characters");
                return false;
            }

            if (InputContainsReservedKeyword(_projectAlias, out string reservedKeywordOutput))
            {
                _projectAliasErrorMessage = L10n.Tr($"The game title cannot contain the substring '{reservedKeywordOutput}'.");
                return false;
            }

            return !InputContainsInvalidCharacters(_projectAlias, _validLowerCaseAndNumericalRx, ref _projectAliasErrorMessage);
        }

        /// <summary>
        /// Checks if the custom environment name that has been input is valid.
        /// </summary>
        /// <returns>True if the environment name is valid and false if the environment name is not valid.</returns>
        private bool IsEnvironmentNameValid()
        {
            if (_customEnvironmentName.Length < 1 || _customEnvironmentName.Length > 16)
            {
                _customEnvironmentNameErrorMessage = L10n.Tr("The environment name must have 1-16 characters");
                return false;
            }

            return !InputContainsInvalidCharacters(_customEnvironmentName, _validAlphanumericRx, ref _customEnvironmentNameErrorMessage);
        }

        /// <summary>
        /// Checks if the custom environment code that has been input is valid.
        /// </summary>
        /// <returns>True if the environment code is valid and false if the environment code is not valid.</returns>
        private bool IsEnvironmentCodeValid()
        {
            if ((_customEnvironmentCode.Length < 2 && GUI.GetNameOfFocusedControl() != "EnvironmentCodeTextField") || _customEnvironmentCode.Length > 3)
            {
                _customEnvironmentCodeErrorMessage = L10n.Tr("The environment code must have 2-3 characters"); 
                return false;
            }

            if (InputContainsReservedKeyword(_customEnvironmentCode, out string reservedKeywordOutput))
            {
                _customEnvironmentCodeErrorMessage = L10n.Tr($"The environment code cannot contain the substring '{reservedKeywordOutput}'.");
                return false;
            }

            return !InputContainsInvalidCharacters(_customEnvironmentCode, _validLowerCaseAndNumericalRx, ref _customEnvironmentCodeErrorMessage);
        }

        /// <summary>
        /// Checks if the access key that has been input is valid.
        /// </summary>
        /// <returns>True if the access key is valid and false if the access key is not valid.</returns>
        private bool IsAccessKeyValid()
        {
            if (_accessKeyId.Length > 128)
            {
                _accessKeyIdErrorMessage = L10n.Tr("The access key ID must be less than 128 characters.");
                return false;
            }

            return !InputContainsInvalidCharacters(_accessKeyId, _validUpperCaseAndNumericalRx, ref _accessKeyIdErrorMessage);
        }

        /// <summary>
        /// Checks if the secret key that has been input is valid.
        /// </summary>
        /// <returns>True if the secret key is valid and false if the secret key is not valid.</returns>
        private bool IsSecretKeyValid()
        {
            bool isValid = string.IsNullOrEmpty(_secretAccessKey) || _secretAccessKey.Length <= 40;

            if (!isValid)
            {
                _secretKeyErrorMessage = "Enter a valid secret access key.";
            }
            else
            {
                _secretKeyErrorMessage = string.Empty;
            }

            return isValid;
        }

        /// <summary>
        /// Checks if the input matches a regex. If the input does not match will display all invalid characters as part of the error message.
        /// </summary>
        /// <param name="input">The string that will be checked for invalid characters.</param>
        /// <param name="validCharactersRegex">The regex that will be running against the input to see if there are invalid characters.</param>
        /// <param name="errorMessage">A reference to the error message that should be altered in the case there is an invalid character.</param>
        /// <returns></returns>
        private bool InputContainsInvalidCharacters(string input, Regex validCharactersRegex, ref string errorMessage)
        {
            char[] invalidCharArray = input.Where(c => !validCharactersRegex.IsMatch(c.ToString())).ToArray();

            if (invalidCharArray.Length != 0)
            {
                errorMessage = L10n.Tr($"Invalid characters: {string.Join(", ", invalidCharArray)}.");
                return true;
            }

            errorMessage = string.Empty;

            return false;
        }

        /// <summary>
        /// Checks if a string contains any reserved keywords that can cause issues with deployment.
        /// </summary>
        /// <param name="input">The string that is being checked for reserved keywords.</param>
        /// <param name="reservedKeywordOutput">If the input string does contain a reserved keyword, reservedKeywordOutput will be set to the first reserved keyword that was found. Otherwise reservedKeywordOutput will be empty.</param>
        /// <returns>True if the input contains reserved keywords, false if the input does not contain any reserved keywords.</returns>
        private bool InputContainsReservedKeyword(string input, out string reservedKeywordOutput)
        {
            reservedKeywordOutput = "";
            foreach (string reservedKeyword in RESERVED_KEYWORDS)
            {
                if (input.Contains(reservedKeyword))
                {
                    reservedKeywordOutput = reservedKeyword;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the custom user created environments and adds the environments to the maps and options.
        /// </summary>
        private void PopulateCustomEnvironments()
        {
            IDictionary<string, string> environments = _featureResourceManager.GetCustomEnvironments();

            foreach (KeyValuePair<string, string> kvp in environments)
            {
                if (!_environmentMapping.ContainsKey(kvp.Key))
                {
                    _environmentMapping.Add(kvp.Key, kvp.Value);
                    _environmentOptions.Add(kvp.Value);
                }

                if (!_environmentMapping.Values.Contains(kvp.Value))
                {
                    _environmentOptions.Remove(_environmentMapping[kvp.Key]);
                    _environmentMapping.Remove(kvp.Key);

                    _environmentMapping.Add(kvp.Key, kvp.Value);
                    _environmentOptions.Add(kvp.Value);
                }
            }
        }

        /// <summary>
        /// Loads the user's last used environment from their saveInfo.yml file.
        /// </summary>
        private void LoadLastUsedEnvironment()
        {
            string lastUsedEnvironmentCode = _featureResourceManager.GetLastUsedEnvironment();

            string lastUsedEnvironmentValue = _environmentMapping[lastUsedEnvironmentCode];

            _currentEnvironment = _environmentOptions.IndexOf(lastUsedEnvironmentValue);

            _featureResourceManager.SetEnvironment(GetSelectedEnvironmentKey());
        }

        /// <summary>
        /// Loads the user's last used region from their saveInfo.yml file.
        /// </summary>
        private void LoadLastUsedRegion()
        {
            string lastUsedRegionCode = _featureResourceManager.GetLastUsedRegion();

            string lastUsedRegionValue = _regionMapping[lastUsedRegionCode];

            _currentRegion = _regionOptions.IndexOf(lastUsedRegionValue);
        }

        /// <summary>
        /// Attempts to find a user's Aws credentials in their .aws/credentials file based on their project alias and environment key.
        /// </summary>
        /// <returns>Returns true if the Aws credentials have been found and set properly in the corresponding input boxes.</returns>
        private bool TryLoadAwsCredentialsFromFile()
        {
            string envKey = GetSelectedEnvironmentKey();
            if (envKey == NEW_CUSTOM_ENV_KEY)
            {
                envKey = _customEnvironmentCode;
            }

            _credentialsManager.SetGameName(_projectAlias);
            _credentialsManager.SetEnv(envKey);

            string accessKey = _credentialsManager.GetAccessKey();
            string secretKey = _credentialsManager.GetSecretAccessKey();

            if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
            {
                _accessKeyId = accessKey;
                _secretAccessKey = secretKey;

                return true;
            }

            if (!_isNewProject && !string.IsNullOrEmpty(_accessKeyId) && !string.IsNullOrEmpty(_secretAccessKey))
            {
                _showNewEnvironmentNotification = true;
            }

            return false;
        }

        /// <summary>
        /// Retrieves the user's Aws account id based on their input access key and secret key.
        /// </summary>
        private void RetrieveAccountId()
        {
            _awsAccountId = AWS_ACCOUNT_ID_LOADING;
            _credentialPairErrorMessage = string.Empty;

            GetAWSAccountIdDescription accountCredentials;
            accountCredentials.AccessKey = _accessKeyId;
            accountCredentials.AccessSecret = _secretAccessKey;

            void AccountIdCallback(StringCallbackResult accountId)
            {
                if (!string.IsNullOrEmpty(accountId.ResponseValue))
                {
                    _awsAccountId = accountId.ResponseValue;
                }
            }

            _featureResourceManager.GetAccountId(accountCredentials, AccountIdCallback);
        }

        /// <summary>
        /// Populates the environment map with default regions.
        /// </summary>
        private void PopulateDefaultEnvironments()
        {
            _environmentMapping["dev"] = "Development";
            _environmentMapping["qa"] = "QA";
            _environmentMapping["stg"] = "Staging";
            _environmentMapping["prd"] = "Production";
            _environmentMapping[NEW_CUSTOM_ENV_KEY] = "Add new environment";

            _environmentOptions.Clear();
            foreach (KeyValuePair<string, string> environmentKeyValuePair in _environmentMapping)
            {
                _environmentOptions.Add(environmentKeyValuePair.Value);
            }
        }

        /// <summary>
        /// Populates the region map and options with all supported Aws regions.
        /// </summary>
        private void PopulateRegions()
        {
            // List of all AWS regions (supported and unsupported)
            // All regions added here to also keep track of currently unsupported ones
            _regionOptions.Clear();
            foreach (AwsRegion awsRegion in Enum.GetValues(typeof(AwsRegion)))
            {
                if (awsRegion.IsRegionSupported())
                {
                    _regionMapping[awsRegion.GetRegionKey()] = awsRegion.GetRegionDescription();
                    _regionOptions.Add(awsRegion.GetRegionDescription());
                }
            }
        }

        /// <summary>
        /// When project alias is changed, check if the input is valid and if credentials already exist
        /// </summary>
        private void OnProjectAliasChanged()
        {
            if (IsGameNameValid())
            {
                if (_credentialsManager.CheckAwsProfileExists(_projectAlias, GetSelectedEnvironmentKey()))
                {
                    PopulateCustomEnvironments();
                    LoadAndSetAwsCredentials();
                }
            }
        }

        /// <summary>
        /// Checks if the new environment selected has corresponding AWS credentials and sets the new environment in the feature resource manager 
        /// </summary>
        private void OnEnvironmentSelectionChanged()
        {
            if (GetSelectedEnvironmentKey() != NEW_CUSTOM_ENV_KEY)
            {
                _showNewEnvironmentNotification = false;
                string previousAwsAccessKey = _accessKeyId;
                string previousAwsSecretKey = _secretAccessKey;

                if (_credentialsManager.CheckAwsProfileExists(_projectAlias, GetSelectedEnvironmentKey()))
                {
                    TryLoadAwsCredentialsFromFile();

                    if (!previousAwsAccessKey.Equals(_accessKeyId) || !previousAwsSecretKey.Equals(_secretAccessKey))
                    {
                        OnAwsCredentialsChanged(IsAccessKeyValid());
                    }
                }
            }
            else
            {
                _showNewEnvironmentNotification = true;
            }

            _featureResourceManager.SetEnvironment(GetSelectedEnvironmentKey());
        }

        /// <summary>
        /// Determines if the user's access and secret keys are valid and if they are calls the Aws backend to retrieve their account Id.
        /// </summary>
        /// <param name="areFieldsValid">Boolean value stating if a changed field is valid, if not there is no need to attempt to continue getting the account id.</param>
        private void OnAwsCredentialsChanged(bool areFieldsValid)
        {
            _showNewEnvironmentNotification = false;

            if (!areFieldsValid)
            {
                _credentialPairErrorMessage = string.Empty;
                _awsAccountId = AWS_ACCOUNT_ID_EMPTY;
                return;
            }

            if (_accessKeyId.Length != 20 || _secretAccessKey.Length != 40)
            {
                if (!string.IsNullOrEmpty(_accessKeyId) && !string.IsNullOrEmpty(_secretAccessKey))
                {
                    _credentialPairErrorMessage = AWS_ACCOUNT_INVALID_PAIR;
                    _awsAccountId = AWS_ACCOUNT_ID_EMPTY;
                }

                SetValidCredentialsSubmitted(false);
                return;
            }

            AccountDetails accountDetails = GetAccountDetails();
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                if (_featureResourceManager.IsAccountInfoValid(accountDetails))
                {
                    RetrieveAccountId();
                    return;
                }

                _credentialPairErrorMessage = AWS_ACCOUNT_INVALID_PAIR;
                _awsAccountId = AWS_ACCOUNT_ID_EMPTY;
            }
            else
            {
                _credentialPairErrorMessage = INTERNET_CONNECTIVITY_ISSUE;
                _awsAccountId = AWS_ACCOUNT_ID_EMPTY;
            }
        }

        /// <summary>
        /// Displays dialog about changing environment before setting the credentials submitted value to false
        /// </summary>
        private void OnChangeEnvironmentAndCredentials()
        {
            if (EditorUtility.DisplayDialog(L10n.Tr("Change Environment"), CHANGE_ENVIRONMENT_WARNING, "Ok", "Cancel"))
            {
                SetValidCredentialsSubmitted(false);

                _onEnvironmentOrRegionChange.Invoke();
            }
        }

        /// <summary>
        /// Revert credentials back to a user's last used environments, reload the AWS credentials and mark credentials submitted as true.
        /// </summary>
        private void OnCancelEnvironmentAndCredentialsChange()
        {
            LoadLastUsedEnvironment();
            LoadLastUsedRegion();
            if (TryLoadAwsCredentialsFromFile())
            {
                bool areCredentialsValid = IsAccessKeyValid() && IsSecretKeyValid();

                OnAwsCredentialsChanged(areCredentialsValid);
                SetValidCredentialsSubmitted(areCredentialsValid);

                if (areCredentialsValid)
                {
                    // Text Fields only update after they loose focus, in order to force the valid access key or secret key to replace the invalid one displayed, we need to force them to lose focus.
                    GUI.FocusControl(null);
                }
            }

            _featureResourceManager.SetEnvironment(GetSelectedEnvironmentKey());
        }

        /// <summary>
        /// Stores the user's current credentials and bootstraps their current environment
        /// </summary>
        private void OnSubmit()
        {
            _showNewEnvironmentNotification = false;

            bool isCustomEnvironment = GetSelectedEnvironmentKey() == NEW_CUSTOM_ENV_KEY;

            AccountDetails accountDetails = GetAccountDetails();
            _featureResourceManager.SetAccountDetails(accountDetails);

            uint result = _featureResourceManager.BootstrapAccount();

            if (result == GameKitErrors.GAMEKIT_SUCCESS)
            {
                // Save credentials 
                _credentialsManager.SetGameName(accountDetails.GameName);
                _credentialsManager.SetEnv(accountDetails.Environment);
                _credentialsManager.SaveCredentials(accountDetails.AccessKey, accountDetails.AccessSecret);

                _featureResourceManager.SaveSettings();

                // Save custom environment
                if (isCustomEnvironment)
                {
                    _featureResourceManager.SaveCustomEnvironment(_customEnvironmentCode, _customEnvironmentName);
                    PopulateCustomEnvironments();

                    _currentEnvironment = GetEnvironmentKeyIndex(_customEnvironmentCode);

                    _customEnvironmentName = string.Empty;
                    _customEnvironmentCode = string.Empty;
                }

                // Update client config file
                string gameAlias = _featureResourceManager.GetGameName();
                string environmentCode = _featureResourceManager.GetLastUsedEnvironment();

                if (!_gameKitManager.DoesConfigFileExist(gameAlias, environmentCode))
                {
                    _featureResourceManager.CreateEmptyClientConfigFile();
                }

                _gameKitManager.CopyAndReloadConfigFile(gameAlias, environmentCode);

                SetValidCredentialsSubmitted(true);
                _isNewProject = false;
            }
            else
            {
                _credentialPairErrorMessage = AWS_ACCOUNT_NOT_VALIDATED;
                Logging.LogError(L10n.Tr($"The user credentials you provided cannot be validated: error {result}"));
                _awsAccountId = AWS_ACCOUNT_ID_EMPTY;
            }
        }

        /// <summary>
        /// Creates and returns an account details structure based on the user's current inputs.
        /// </summary>
        /// <returns>Account details struct based on the user's current input.</returns>
        private AccountDetails GetAccountDetails()
        {
            string envCode = GetSelectedEnvironmentKey();
            if (envCode == NEW_CUSTOM_ENV_KEY)
            {
                envCode = _customEnvironmentCode;
            }

            AccountDetails accountDetails = new AccountDetails
            {
                Environment = envCode,
                AccountId = _awsAccountId,
                GameName = _projectAlias,
                Region = GetCurrentRegionKey(),
                AccessKey = _accessKeyId,
                AccessSecret = _secretAccessKey
            };

            return accountDetails;
        }

        /// <summary>
        /// Gets the index of a specified environment key within the environment map 
        /// </summary>
        /// <param name="environmentKey">The environment key of which the index should be retrieved</param>
        /// <returns>The index of the passed in environment key</returns>
        private int GetEnvironmentKeyIndex(string environmentKey)
        {
            return _environmentMapping.Keys.ToList().IndexOf(environmentKey);
        }

        /// <summary>
        /// Gets the currently selected environment key value based on the current environment index.
        /// </summary>
        /// <returns>Environment key based on the currently selected environment.</returns>
        private string GetSelectedEnvironmentKey()
        {
            return _environmentMapping.Keys.ElementAt(_currentEnvironment);
        }

        /// <summary>
        /// Gets the currently selected region key value based on the current region index.
        /// </summary>
        /// <returns>Region key based on the currently selected region.</returns>
        private string GetCurrentRegionKey()
        {
            return _regionMapping.Keys.ElementAt(_currentRegion);
        }
    }
}
