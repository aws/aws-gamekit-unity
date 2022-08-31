// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Unity
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.AchievementsAdmin;
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Features.GameKitAchievements;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

// Third Party
using Newtonsoft.Json;

using AchievementListResult = AWS.GameKit.Editor.AchievementsAdmin.AchievementListResult;
using ListAchievementsDesc = AWS.GameKit.Editor.AchievementsAdmin.ListAchievementsDesc;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.Achievements
{
    [Serializable]
    public class AchievementsDataTab : IDrawable
    {
        private const string SINGLE_SPACE = " ";
        private const string TEMPLATE_FILE_NAME = "achievements_template";
        private const string DEFAULT_EXPORT_FILE_NAME = "GameKitAchievementsExport";

        private const string TOOLTIP_MUST_FINISH_SYNCING_WITH_BACKEND = "Must finish syncing data with the cloud backend";
        private const string TOOLTIP_MUST_FINISH_DEPLOYING = "The Achievements feature must finish deploying";
        private const string TOOLTIP_ACHIEVEMENTS_MUST_SUCCESSFULLY_DEPLOY_PREFIX = "The Achievements feature must be successfully deployed";
        private const string TOOLTIP_ACHIEVEMENTS_MUST_SUCCESSFULLY_DEPLOY_SUFFIX = "The feature's last deployment ended in an error.";
        private const string TOOLTIP_ACHIEVEMENTS_MUST_BE_CREATED = "The Achievements feature must be created";

        private const float BUTTON_MINIMUM_SIZE = 50.0f;

        private readonly string TEMPLATE_FILE_ERROR = $"Error opening {TEMPLATE_FILE_NAME} file";
        private readonly string IACHIEVEMENTS_ADMIN_PROVIDER_FILE_PATH = Path.Combine("Assets", "AWS GameKit", "Editor", "Scripts", "AchievementsAdmin", nameof(IAchievementsAdminProvider) + ".cs");

        [SerializeField] private Vector2 _scrollPosition;
        [SerializeField] private Vector2 _achievementsScrollPosition;
        [SerializeField] private SerializablePropertyOrderedDictionary<string, AchievementWidget> _localAchievements;

        [SerializeField] private string _achievementIconsBaseUrl = string.Empty;
        [SerializeField] private bool _isShowingCloudSyncErrorBanner = false;

        [SerializeField] private bool _isDeletingAchievementsFromCloud = false;
        [SerializeField] private bool _isUploadingAchievementsToCloud = false;
        [SerializeField] private bool _isDownloadingAchievementsFromCloud = false;
        private bool _isDataBeingSyncedWithCloudBackend = false;
        private bool _isDeploying = false;
        private bool _isDeployed = false;
        private FeatureStatusSummary _featureStatus;

        private LinkWidget _getTemplateLink;

        // Dependencies
        private IAchievementsAdminProvider _achievementsAdmin;
        private FeatureDeploymentOrchestrator _featureDeploymentOrchestrator;
        private SerializedProperty _serializedProperty;
        private SerializedProperty _localAchievementsSerializedProperty;
        private GameKitEditorManager _gameKitEditorManager;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            // Dependencies
            _achievementsAdmin = dependencies.AchievementsAdmin;
            _featureDeploymentOrchestrator = dependencies.FeatureDeploymentOrchestrator;
            _serializedProperty = serializedProperty;
            _localAchievementsSerializedProperty = _serializedProperty.FindPropertyRelative($"{nameof(_localAchievements)}");
            _gameKitEditorManager = dependencies.GameKitEditorManager;

            // State
            _localAchievements ??= new SerializablePropertyOrderedDictionary<string, AchievementWidget>();
            _localAchievements.Initialize(_localAchievementsSerializedProperty);
            UpdateDeploymentState();

            // Links
            _getTemplateLink = new LinkWidget(L10n.Tr("Get template"), OnClickGetTemplate, new LinkWidget.Options()
            {
                Tooltip = L10n.Tr("Open a JSON template in your text editor.")
            });

            // Register with relevant events
            dependencies.OnEnvironmentOrRegionChange.AddListener(OnEnvironmentOrRegionChange);
        }

        #region GUI
        public void OnGUI()
        {
            // Must be called before DrawAddAchievementButton() and DrawAchievements()
            _localAchievements.FrameDelayedInitializeNewElements();

            UpdateDeploymentState();
            _isDataBeingSyncedWithCloudBackend = _isUploadingAchievementsToCloud || _isDeletingAchievementsFromCloud || _isDownloadingAchievementsFromCloud;

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPosition))
            {
                _scrollPosition = scrollView.scrollPosition;

                EditorGUILayoutElements.Description(L10n.Tr("Manage your game's achievement definitions and sync them to the backend."), indentationLevel: 0);

                EditorGUILayoutElements.SectionDivider();

                using (new EditorGUI.DisabledScope(ShouldDisableGUI()))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        DrawAddAchievementButton();
                        GUILayout.FlexibleSpace();
                        DrawCollapseButton();
                        DrawSortButton();
                    }

                    DrawAchievements();

                    EditorGUILayoutElements.SectionDivider();

                    DrawLocalManagementFooter();

                    EditorGUILayoutElements.SectionDivider();

                    DrawCloudSyncFooter();
                }
            }

            if (ShouldDisableGUI())
            {
                EditorGUILayoutElements.DrawToolTip(L10n.Tr("To enable, please deploy the Achievements\nfeature using the Create button on Deployment tab."));

                if (_localAchievements.Count > 0)
                {
                    // If achievements is no longer deployed (or has not been) then clear any local achievements so that they are not propagated to another stack by mistake.
                    _localAchievements.Clear();
                }
            }
        }

        private void OnEnvironmentOrRegionChange()
        {
            _localAchievements.Clear();
        }

        private void UpdateDeploymentState()
        {
            FeatureStatusSummary previousStatus = _featureStatus;

            _featureStatus = _featureDeploymentOrchestrator.GetFeatureStatusSummary(FeatureType.Achievements);
            _isDeploying = _featureDeploymentOrchestrator.IsFeatureDeploymentInProgress(FeatureType.Achievements);
            _isDeployed = _featureStatus == FeatureStatusSummary.Deployed;

            // If we are changing from an unknown state to knowing that achievements is deployed, then refresh the achievements.
            if (previousStatus == FeatureStatusSummary.Unknown && _isDeployed && !_isDeploying)
            {
                RefreshAchievementIconBaseUrl();

                // Update the status of all achievements, add any missing, and remove any deleted
                SynchronizeAchievementsWithCloud(false);
            }
        }

        private bool ShouldDisableGUI()
        {
            return ((_featureStatus != FeatureStatusSummary.Unknown) && (!_isDeployed || _isDeploying))  || !_gameKitEditorManager.CredentialsSubmitted;
        }

        private void DrawAddAchievementButton()
        {
            bool isEnabled = AreLocalActionButtonsEnabled(
                enabledTooltip: L10n.Tr("Add a new local achievement."),
                actionDescription: L10n.Tr("a new local achievement can be added"),
                out string tooltip);

            // Wrap in single spaces so there's a space between "+" and "Add achievement".
            string text = SINGLE_SPACE + L10n.Tr("Add achievement") + SINGLE_SPACE;

            Texture image = EditorResources.Textures.SettingsWindow.PlusIcon.Get();
            Vector2 imageSize = SettingsGUIStyles.Icons.InsideButtonIconSize;
            Color colorWhenEnabled = SettingsGUIStyles.Buttons.GUIButtonGreen.Get();

            if (EditorGUILayoutElements.Button(text, isEnabled, tooltip, colorWhenEnabled, image, imageSize))
            {
                OnClickAddAchievement();
            }
        }

        private void DrawCollapseButton()
        {
            // Wrap in single spaces so there's a space between the icon and text.
            string text = SINGLE_SPACE + L10n.Tr("Collapse all") + SINGLE_SPACE;
            
            Texture image = EditorGUIUtility.IconContent("d_PlayButton@2x").image;
            Vector2 imageSize = SettingsGUIStyles.Icons.InsideButtonIconSize;

            if (EditorGUILayoutElements.Button(text, true, L10n.Tr("Click to collapse all achievements."), null, image, imageSize, BUTTON_MINIMUM_SIZE))
            {
                foreach(AchievementWidget achievement in _localAchievements.Values)
                {
                    achievement.CollapseWidget();
                }
            }
        }

        private void DrawSortButton()
        {
            // Wrap in single spaces so there's a space between the icon and text.
            string text = SINGLE_SPACE + L10n.Tr("Apply sort") + SINGLE_SPACE;

            Texture image = EditorResources.Textures.FeatureStatusRefresh.Get();
            Vector2 imageSize = SettingsGUIStyles.Icons.InsideButtonIconSize;

            if (EditorGUILayoutElements.Button(text, true, L10n.Tr("Click to sort achievements by their Sort Order followed by Title."), null, image, imageSize, BUTTON_MINIMUM_SIZE))
            {
                // If an achievement's attribute (ex: Title) is selected when "Apply sort" is clicked, then after
                // achievements are sorted the selected attribute still holds the value of the previous achievement
                // in that position. It's value changes to the correct value once focus is dropped from that attribute.
                // We prevent this issue by dropping focus before achievements are sorted.
                GUI.FocusControl(null);

                _localAchievements.Sort((IEnumerable<KeyValuePair<string, AchievementWidget>> achievements) =>
                    {
                        return achievements
                            .OrderBy(
                                achievement => achievement.Value.SortOrder
                            )
                            .ThenBy(
                                achievement => achievement.Value.Title
                            );
                    }
                );
            }
        }

        private void DrawAchievements()
        {
            bool achievementsAreEditable = AreLocalActionButtonsEnabled(string.Empty, string.Empty, out string unusedValue);

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_achievementsScrollPosition))
            {
                _achievementsScrollPosition = scrollView.scrollPosition;
                foreach (AchievementWidget achievement in _localAchievements.Values)
                {
                    achievement.OnGUI(isEditable: achievementsAreEditable);
                }
            }
        }

        private void DrawLocalManagementFooter()
        {
            EditorGUILayoutElements.SectionHeaderWithDescription(
                title: L10n.Tr("Local achievement management"),
                description: L10n.Tr("Quickly configure and manage your Achievements locally using JSON")
            );

            using (new EditorGUILayout.HorizontalScope())
            {
                _getTemplateLink.OnGUI();

                // Right-align the buttons
                GUILayout.FlexibleSpace();

                DrawExportButton();
                DrawImportButton();
            }
        }

        private void DrawExportButton()
        {
            bool isEnabled = AreLocalActionButtonsEnabled(
                enabledTooltip: L10n.Tr("Export your local achievements to a JSON file."),
                actionDescription: L10n.Tr("local achievements can be exported to a JSON file"),
                out string tooltip);

            Color green = SettingsGUIStyles.Buttons.GUIButtonGreen.Get();

            if (EditorGUILayoutElements.Button(L10n.Tr("Export to local file"), isEnabled, tooltip, green))
            {
                OnClickExport();
            }
        }

        private void DrawImportButton()
        {
            bool isEnabled = AreLocalActionButtonsEnabled(
                enabledTooltip: L10n.Tr("Replace your local achievements with new ones imported from a JSON file."),
                actionDescription: L10n.Tr("achievements can be imported from a JSON file"),
                out string tooltip);

            if (EditorGUILayoutElements.Button(L10n.Tr("Import from local file"), isEnabled, tooltip))
            {
                OnClickImport();
            }
        }

        private void DrawCloudSyncFooter()
        {
            EditorGUILayoutElements.SectionHeaderWithDescription(
                title: L10n.Tr("Cloud Sync"),
                description: L10n.Tr("To upload new achievement data to the cloud, choose Save Data. To download saved achievement data to your local machine, choose Get Latest.")
            );

            using (new EditorGUILayout.HorizontalScope())
            {
                // Right-align the buttons
                GUILayout.FlexibleSpace();

                DrawSaveDataButton();
                DrawGetLatestButton();
            }
        }

        private void DrawSaveDataButton()
        {
            bool isEnabled = AreCloudActionButtonsEnabled(
                enabledTooltip: L10n.Tr("Make the cloud match your local achievements."),
                actionDescription: L10n.Tr("achievements can be saved to your cloud backend"),
                out string tooltip);

            string buttonText = _isUploadingAchievementsToCloud || _isDeletingAchievementsFromCloud
                ? L10n.Tr("Saving...")
                : L10n.Tr("Save data");

            Color green = SettingsGUIStyles.Buttons.GUIButtonGreen.Get();

            if (EditorGUILayoutElements.Button(buttonText, isEnabled, tooltip, green))
            {
                OnClickSaveData();
            }
        }

        private void DrawGetLatestButton()
        {
            bool isEnabled = AreCloudActionButtonsEnabled(
                enabledTooltip: L10n.Tr("Replace your local achievements with ones from the cloud, or merge in cloud achievements without overwriting any local ones."),
                actionDescription: L10n.Tr("achievements can be downloaded from your cloud backend"),
                out string tooltip);

            string buttonText = _isDownloadingAchievementsFromCloud
                ? L10n.Tr("Downloading...")
                : L10n.Tr("Get latest");

            if (EditorGUILayoutElements.Button(buttonText, isEnabled, tooltip))
            {
                OnClickGetLatest();
            }
        }

        /// <summary>
        /// Return true if buttons that trigger local operations (i.e. no cloud operations) are enabled, false if disabled.
        /// </summary>
        /// <param name="enabledTooltip">The tooltip the button should display when it is enabled.</param>
        /// <param name="actionDescription">A description of what action the button takes. Is used for the disabled tooltip.</param>
        /// <param name="tooltip">When this method returns, contains the tooltip that should be displayed for this button. This parameter is passed in uninitialized.</param>
        /// <returns>True if the button should be enabled, false if disabled.</returns>
        private bool AreLocalActionButtonsEnabled(string enabledTooltip, string actionDescription, out string tooltip)
        {
            bool isEnabled = !_isDataBeingSyncedWithCloudBackend;

            tooltip = isEnabled
                ? enabledTooltip
                : L10n.Tr($"{TOOLTIP_MUST_FINISH_SYNCING_WITH_BACKEND} before {actionDescription}.");

            return isEnabled;
        }

        /// <summary>
        /// Return true if buttons that trigger cloud operations are enabled, false if disabled.
        /// </summary>
        /// <param name="enabledTooltip">The tooltip the button should display when it is enabled.</param>
        /// <param name="actionDescription">A description of what action the button takes. Is used for the disabled tooltip.</param>
        /// <param name="tooltip">When this method returns, contains the tooltip that should be displayed for this button. This parameter is passed in uninitialized.</param>
        /// <returns>True if the button should be enabled, false if disabled.</returns>
        private bool AreCloudActionButtonsEnabled(string enabledTooltip, string actionDescription, out string tooltip)
        {
            bool isEnabled = _isDeployed && !_isDeploying && !_isDataBeingSyncedWithCloudBackend;

            tooltip = enabledTooltip;
            if (!isEnabled)
            {
                if (_isDataBeingSyncedWithCloudBackend)
                {
                    tooltip = L10n.Tr($"{TOOLTIP_MUST_FINISH_SYNCING_WITH_BACKEND} before {actionDescription}.");
                }
                else if (_isDeploying)
                {
                    tooltip = L10n.Tr($"{TOOLTIP_MUST_FINISH_DEPLOYING} before {actionDescription}.");
                }
                else
                {
                    switch (_featureStatus)
                    {
                        case FeatureStatusSummary.Running:
                            tooltip = L10n.Tr($"{TOOLTIP_MUST_FINISH_DEPLOYING} before {actionDescription}.");
                            break;
                        case FeatureStatusSummary.Error:
                            tooltip = L10n.Tr($"{TOOLTIP_ACHIEVEMENTS_MUST_SUCCESSFULLY_DEPLOY_PREFIX} before {actionDescription}. {TOOLTIP_ACHIEVEMENTS_MUST_SUCCESSFULLY_DEPLOY_SUFFIX}");
                            break;
                        case FeatureStatusSummary.Undeployed:
                        // fall-through
                        case FeatureStatusSummary.Unknown:
                        // fall-through
                        default:
                            tooltip = L10n.Tr($"{TOOLTIP_ACHIEVEMENTS_MUST_BE_CREATED} before {actionDescription}.");
                            break;
                    }
                }
            }

            return isEnabled;
        }
        #endregion

        #region Click Functions
        private void OnClickAddAchievement()
        {
            string guid = System.Guid.NewGuid().ToString().Replace('-','_');
            int sortOrder = AchievementWidget.SORT_MIN;
            if (_localAchievements.Values.Any())
            {
                sortOrder = _localAchievements.Values.Max(existingAchievement => existingAchievement.SortOrder) + 1;
            }
            AchievementWidget achievement = new AchievementWidget()
            {
                Id = guid,
                Title = $"New Achievement {guid}",
                SortOrder = sortOrder
            };

            AddLocalAchievement(achievement);
        }

        private void OnClickGetTemplate()
        {
            string templatePath = ExportListOfAchievements(new List<Achievement>() { new Achievement() }, L10n.Tr("Select or enter the filename to save the template to"), TEMPLATE_FILE_NAME);
            if (string.IsNullOrEmpty(templatePath))
            {
                // this case will happen if the user hits cancel on the SaveFilePanel, this is a normal case
                return;
            }

            if (File.Exists(templatePath))
            {
                try
                {
                    if (Process.Start(templatePath) == null)
                    {
                        Logging.LogError(L10n.Tr(TEMPLATE_FILE_ERROR));
                    }
                }
                catch (Exception e)
                {
                    Logging.LogError(L10n.Tr($"{TEMPLATE_FILE_ERROR}: {e}"));
                }
            }
            else
            {
                Logging.LogError(L10n.Tr($"{templatePath} file not found"));
            }
        }

        private void OnClickExport()
        {
            // convert the list of AchievementWidgets to a list of Achievements while ignoring any that are marked for deletion
            List<Achievement> listOfAchievements =
            (
                from achievementWidget in _localAchievements.Values
                where achievementWidget.IsMarkedForDeletion == false
                select (Achievement)achievementWidget
            ).ToList();

            if (listOfAchievements.Count == 0)
            {
                Logging.LogInfo(L10n.Tr("There are no achievements to export."));
                return;
            }

            ExportListOfAchievements(listOfAchievements, L10n.Tr("Select or enter a filename to export achievements to"), DEFAULT_EXPORT_FILE_NAME);
        }

        private void OnClickImport()
        {
            // Ask for the file to open
            string file = EditorUtility.OpenFilePanel(L10n.Tr("Select a file to import achievements from"), string.Empty, "json");
            if (string.IsNullOrEmpty(file))
            {
                // this case will happen if the user hits cancel on the OpenFilePanel, this is a normal case
                return;
            }

            // read in the file as a string
            string toImport = File.ReadAllText(file);
            if (string.IsNullOrEmpty(toImport))
            {
                Logging.LogWarning(L10n.Tr($"File {file} was empty"));

                return;
            }

            // convert the imported json to a list of achievements
            List<Achievement> listOfAchievements;
            try
            {
                listOfAchievements = JsonConvert.DeserializeObject<List<Achievement>>(toImport);
            }
            catch (Exception e)
            {
                Logging.LogError(L10n.Tr($"Unable to deserialize the JSON file {file} into a list of achievements. Please check the file for JSON syntax errors.: {e}"));

                return;
            }

            Logging.LogInfo(L10n.Tr($"{listOfAchievements.Count} achievement(s) imported from {file}, replacing {_localAchievements.Count} existing achievement(s)."));

            // replace all local achievements with the imported achievements
            _localAchievements.Clear();
            listOfAchievements.ForEach(a => AddLocalAchievement(a));
        }

        /// <summary>
        /// Upload all of the user's local achievements to the cloud, and delete achievements from the cloud which were locally marked for deletion.
        /// </summary>
        private void OnClickSaveData()
        {
            _isShowingCloudSyncErrorBanner = false;

            DeleteAllMarkedAchievements();
            UploadAllLocalAchievements();
        }

        /// <summary>
        /// Delete all the marked achievements from the cloud and locally.
        /// </summary>
        private void DeleteAllMarkedAchievements()
        {
            AchievementWidget[] achievementsToDelete = _localAchievements.Values
                .Where(
                    achievement => !string.IsNullOrEmpty(achievement.Id) && achievement.IsMarkedForDeletion
                )
                .ToArray();

            string[] achievementIdsToDelete = achievementsToDelete
                .Select(
                    achievement => achievement.Id
                )
                .ToArray();

            if (achievementsToDelete.Length == 0)
            {
                Logging.LogInfo($"There are no local achievements marked for deletion. No achievements have been deleted from the cloud.");
                return;
            }

            string achievementTitles = StringHelper.MakeCommaSeparatedList(achievementsToDelete
                .Select(
                    achievement => achievement.Title
                ));
            Logging.LogInfo($"Deleting {achievementsToDelete.Length} achievement(s) from the cloud: {achievementTitles}");

            _isDeletingAchievementsFromCloud = true;

            DeleteAchievementsDesc deleteAchievementsRequest = new DeleteAchievementsDesc()
            {
                AchievementIdentifiers = achievementIdsToDelete,
                BatchSize = (uint)achievementIdsToDelete.Length
            };

            _achievementsAdmin.DeleteAchievementsForGame(deleteAchievementsRequest, (uint resultCode) =>
            {
                if (resultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    foreach (AchievementWidget achievement in achievementsToDelete)
                    {
                        _localAchievements.Remove(achievement.Id);
                    }

                    Logging.LogInfo($"Finished deleting {achievementsToDelete.Length} achievement(s) from the cloud.");
                }
                else
                {
                    string errorMessage = $"Failed to delete the following achievement(s) from your game: {achievementTitles}.";
                    string apiName = nameof(IAchievementsAdminProvider.DeleteAchievementsForGame);

                    HandleFailedCloudApiCall(resultCode, errorMessage, apiName);
                }

                _isDeletingAchievementsFromCloud = false;
            });
        }

        /// <summary>
        /// Upload all local achievements to the cloud which are not marked for deletion.
        /// </summary>
        private void UploadAllLocalAchievements()
        {
            AchievementWidget[] achievementsToAdd = _localAchievements.Values
                .Where(
                    achievement => !string.IsNullOrEmpty(achievement.Id) && !achievement.IsMarkedForDeletion && achievement.SyncStatus != SyncStatus.Synchronized
                )
                .ToArray();

            AdminAchievement[] adminAchievementsToAdd = achievementsToAdd
                .Select(
                    achievement => (AdminAchievement)achievement
                )
                .ToArray();

            if (achievementsToAdd.Length == 0)
            {
                Logging.LogInfo($"There are no local achievements defined. No local achievements have been uploaded to the cloud.");
                return;
            }

            string achievementTitles = StringHelper.MakeCommaSeparatedList(achievementsToAdd
                .Select(
                    achievement => achievement.Title
                ));
            Logging.LogInfo($"Saving {achievementsToAdd.Length} achievements to the cloud: {achievementTitles}");

            _isUploadingAchievementsToCloud = true;

            AddAchievementDesc addAchievementsRequest = new AddAchievementDesc()
            {
                Achievements = adminAchievementsToAdd,
                BatchSize = (uint)achievementsToAdd.Length
            };

            _achievementsAdmin.AddAchievementsForGame(addAchievementsRequest, (uint resultCode) =>
            {
                if (resultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    SynchronizeAchievementsWithCloud(false, true);
                    Logging.LogInfo($"Finished saving {achievementsToAdd.Length} achievements to the cloud.");
                }
                else
                {
                    string errorMessage = $"Failed to upload the following achievements for your game: {achievementTitles}.";
                    string apiName = nameof(IAchievementsAdminProvider.AddAchievementsForGame);

                    HandleFailedCloudApiCall(resultCode, errorMessage, apiName);
                }

                _isUploadingAchievementsToCloud = false;
            });
        }

        private void OnClickGetLatest()
        {
            GetLatestPopupWindow.CreatePopupWindow(
                onClickAddMissing: () =>
                {
                    SynchronizeAchievementsWithCloud(false);
                },
                onClickReplaceAll: () =>
                {
                    SynchronizeAchievementsWithCloud(true);
                }
            );
        }

        private void SynchronizeAchievementsWithCloud(bool shouldReplaceAllLocal, bool replaceIconPaths = false)
        {
            // Clear the error because we are attempting a new cloud API call
            _isShowingCloudSyncErrorBanner = false;

            _isDownloadingAchievementsFromCloud = true;

            ListAchievementsDesc listAchievementsRequest = new ListAchievementsDesc()
            {
                WaitForAllPages = true
            };
            AdminAchievement[] cloudAchievements = new AdminAchievement[]{ };

            Logging.LogInfo($"Downloading achievements from the cloud.");

            _achievementsAdmin.ListAchievementsForGame(listAchievementsRequest,
                callback: (AchievementListResult result) =>
                {
                    cloudAchievements = result.Achievements;
                },
                onCompleteCallback: (uint resultCode) =>
                {
                    if (resultCode == GameKitErrors.GAMEKIT_SUCCESS)
                    {
                        if (shouldReplaceAllLocal)
                        {
                            _localAchievements.Clear();
                            Logging.LogInfo("Deleted all local achievements.");
                        }

                        foreach (AchievementWidget localAchievement in _localAchievements.Values)
                        {
                            // if the achievement has not been explicitly marked as Unsynchronized, meaning it was recently added or edited, then mark as Unknown because we don't know the actual status
                            if (localAchievement.SyncStatus != SyncStatus.Unsynchronized)
                            {
                                localAchievement.SyncStatus = SyncStatus.Unknown;
                            }
                            
                            localAchievement.IsMarkedForDeletion = false;
                        }

                        if (cloudAchievements.Length == 0)
                        {
                            Logging.LogInfo("There are no achievements defined in the cloud for your game.");
                        }
                        else
                        {
                            Logging.LogInfo($"Downloaded {cloudAchievements.Length} achievement(s) from the cloud.");

                            int numAlreadyExistingAchievements = 0;
                            int numCloudOnlyAchievements = 0;
                            for (int i = 0; i < cloudAchievements.Length; ++i)
                            {
                                string lockedIconUrlSuffix;
                                string unlockedIconUrlSuffix;

                                cloudAchievements[i].LockedIcon = UpdateIconPath(cloudAchievements[i].LockedIcon, out lockedIconUrlSuffix);
                                cloudAchievements[i].UnlockedIcon = UpdateIconPath(cloudAchievements[i].UnlockedIcon, out unlockedIconUrlSuffix);

                                if (_localAchievements.ContainsKey(cloudAchievements[i].AchievementId))
                                {
                                    ++numAlreadyExistingAchievements;

                                    // Find and assign a reference to the local achievement.
                                    AchievementWidget localAchievement = _localAchievements.GetValue(cloudAchievements[i].AchievementId);

                                    if (replaceIconPaths)
                                    {
                                        localAchievement.IconPathLocked = cloudAchievements[i].LockedIcon;
                                        localAchievement.IconPathUnlocked = cloudAchievements[i].UnlockedIcon;
                                    }

                                    if (AchievementWidget.AreSame(localAchievement, cloudAchievements[i]))
                                    {
                                        localAchievement.SyncStatus = SyncStatus.Synchronized;

                                        DownloadIcons(lockedIconUrlSuffix, cloudAchievements[i].LockedIcon, localAchievement);
                                        DownloadIcons(unlockedIconUrlSuffix, cloudAchievements[i].UnlockedIcon, localAchievement);
                                    }
                                    else
                                    {
                                        localAchievement.SyncStatus = SyncStatus.Unsynchronized;
                                    }
                                }
                                else
                                {
                                    ++numCloudOnlyAchievements;

                                    // Convert the AdminAchievement to a AchievementWidget and add it to the list of local achievements.
                                    AchievementWidget localAchievement = cloudAchievements[i];
                                    localAchievement.SyncStatus = SyncStatus.Synchronized;

                                    DownloadIcons(lockedIconUrlSuffix, localAchievement.IconPathLocked, localAchievement);
                                    DownloadIcons(unlockedIconUrlSuffix, localAchievement.IconPathUnlocked, localAchievement);

                                    AddLocalAchievement(localAchievement);
                                }
                            }

                            if (!shouldReplaceAllLocal)
                            {
                                Logging.LogInfo($"Updated the synchronization status of {numAlreadyExistingAchievements} existing local achievement(s).");
                            }
                            Logging.LogInfo($"Added {numCloudOnlyAchievements} new local achievement(s) retrieved from the cloud.");
                        }

                        // run through local achievements one last time, any that are still marked as "unknown" status are ones that did not exist on the cloud and are not new/edited, so delete
                        foreach (AchievementWidget localAchievement in _localAchievements.Values)
                        {
                            if (localAchievement.SyncStatus == SyncStatus.Unknown)
                            {
                                localAchievement.IsMarkedForDeletion = true;
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = $"Failed to download achievements from the cloud.";
                        string apiName = nameof(IAchievementsAdminProvider.ListAchievementsForGame);

                        HandleFailedCloudApiCall(resultCode, errorMessage, apiName);
                    }

                    _isDownloadingAchievementsFromCloud = false;
                }
            );
        }

        private string UpdateIconPath(string icon, out string iconUrlSuffix)
        {
            iconUrlSuffix = string.Empty;

            if (!string.IsNullOrEmpty(icon))
            {
                try
                {
                    iconUrlSuffix = icon.Substring(icon.LastIndexOf("icons"));
                }
                catch (ArgumentOutOfRangeException)
                {
                    Logging.LogError($"Icon S3 key not valid: Icon Path = {icon}");

                    return string.Empty;
                }

                return Path.Combine(GameKitPaths.Get().ASSETS_GAMEKIT_ART_PATH, iconUrlSuffix).Replace("\\", "/");
            }

            return string.Empty;
        }

        /// <summary>
        /// Download achievement icons.
        /// </summary>
        /// <param name="iconS3Key">S3 key of the icon.</param>
        /// <param name="downloadPath">local download path of the icon.</param>
        private async void DownloadIcons(string iconS3Key, string downloadPath, AchievementWidget localAchievement)
        {
            if (!string.IsNullOrEmpty(iconS3Key) && !string.IsNullOrEmpty(downloadPath))
            {
                string fullUrl = _achievementIconsBaseUrl + iconS3Key;
                UnityWebRequest request = new UnityWebRequest(fullUrl);
                request.method = UnityWebRequest.kHttpVerbGET;
                DownloadHandlerFile fileHandler = new DownloadHandlerFile(downloadPath);
                fileHandler.removeFileOnAbort = true;
                request.downloadHandler = fileHandler;
                request.timeout = 120; //seconds

                request.SendWebRequest();
                while (!fileHandler.isDone)
                {
                    await System.Threading.Tasks.Task.Yield();
                }
                if (fileHandler.isDone)
                {
                    if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Logging.LogError($"Icon {fullUrl} download failed with {request.error}");
                    }
                    else if (!string.IsNullOrEmpty(fileHandler.error))
                    {
                        Logging.LogError($"Icon {fullUrl} download failed with {fileHandler.error}");
                    }
                    else
                    {
                        Logging.LogInfo($"Icon {fullUrl} download Successful");

                        // Force the textures to load by clearing their cache.
                        localAchievement.ClearTextures();
                    }
                }
            }
        }
        #endregion

        #region Popup Windows
        /// <summary>
        /// The popup window that shows when the "Get latest" button is clicked.
        /// </summary>
        private class GetLatestPopupWindow : EditorWindow
        {
            public static void CreatePopupWindow(Action onClickAddMissing, Action onClickReplaceAll)
            {
                int choice = EditorUtility.DisplayDialogComplex(L10n.Tr("Get latest achievements"),
                    L10n.Tr("Do you want to add missing cloud achievements to your local achievements, " +
                    "or completely replace all local achievements with what is in the cloud?"),
                    L10n.Tr("Add Missing"), L10n.Tr("Cancel"), L10n.Tr("Replace All Local"));
                
                switch (choice)
                {
                    // Add Missing
                    case 0:
                        onClickAddMissing.Invoke();
                        break;
                    // Cancel
                    case 1:
                        break;
                    // Replace All Local
                    case 2:
                        onClickReplaceAll.Invoke();
                        break;
                }
            }
        }
        #endregion

        #region Helpers
        private void AddLocalAchievement(AchievementWidget achievement)
        {
            _localAchievements.Add(achievement.Id, achievement);
        }

        private string ExportListOfAchievements(List<Achievement> listOfAchievements, string saveFilePanelTitle, string defaultFileName)
        {
            // Convert the list of achievements to a human readable JSON file
            string toExport = JsonConvert.SerializeObject(listOfAchievements, Formatting.Indented);

            // Ask for the name of the file and its location
            string file = EditorUtility.SaveFilePanel(saveFilePanelTitle, string.Empty, defaultFileName, "json");
            if (string.IsNullOrEmpty(file))
            {
                // this case will happen if the user hits cancel on the SaveFilePanel, this is a normal case
                return string.Empty;
            }

            // output the JSON object to the requested file
            File.WriteAllText(file, toExport);
            if (!File.Exists(file))
            {
                Logging.LogError(L10n.Tr($"Error creating file {file}"));

                return string.Empty;
            }

            Logging.LogInfo(L10n.Tr($"{listOfAchievements.Count} achievement(s) exported to {file}"));

            return file;
        }

        private void RefreshAchievementIconBaseUrl()
        {
            _achievementsAdmin.GetAchievementIconBaseUrl((StringCallbackResult result) =>
            {
                if (result.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    string baseUrl = result.ResponseValue;
                    string credentialsNotSubmittedUrl = "/";

                    _achievementIconsBaseUrl = baseUrl == credentialsNotSubmittedUrl
                        ? string.Empty
                        : baseUrl;
                }
                else
                {
                    _achievementIconsBaseUrl = string.Empty;

                    string errorMessage = $"Failed to refresh the achievement icon base URL.";
                    string apiName = nameof(IAchievementsAdminProvider.GetAchievementIconBaseUrl);
                    HandleFailedCloudApiCall(result.ResultCode, errorMessage, apiName);
                }
            });
        }

        /// <summary>
        /// Call this method when a cloud-based AWS GameKit API call returns a non-successful result code.<br/><br/>
        ///
        /// This method turns on the "Cloud Sync Error" banner and logs a helpful error message.
        /// </summary>
        /// <param name="resultCode">The result code from the failed API call (<see cref="GameKitErrors"/>).</param>
        /// <param name="errorMessage">An explanation of what went wrong. Should include punctuation but no trailing space at the end of the sentence.
        /// For example, "Failed to delete the following achievements: {achievementTitles}."</param>
        /// <param name="apiName">The method name of the failed API call. Used <c>nameof()</c> to get this value.</param>
        private void HandleFailedCloudApiCall(uint resultCode, string errorMessage, string apiName)
        {
            _isShowingCloudSyncErrorBanner = true;

            string friendlyErrorName = GameKitErrorConverter.GetErrorName(resultCode);
            string hexadecimalErrorCode = GameKitErrors.ToString(resultCode);
            string sourceCodeFilePath = IACHIEVEMENTS_ADMIN_PROVIDER_FILE_PATH;

            Logging.LogError($"{errorMessage} " +
                             $"A {friendlyErrorName} ({hexadecimalErrorCode}) error occurred while calling the {apiName}() API. " +
                             $"This error code is explained in the {apiName}() method's documentation in the file \"{sourceCodeFilePath}\"");
        }
        #endregion
    }
}