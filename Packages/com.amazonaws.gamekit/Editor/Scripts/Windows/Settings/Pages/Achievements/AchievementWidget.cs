// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.AchievementsAdmin;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Runtime.Features.GameKitAchievements;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.Achievements
{
    /// <summary>
    /// Indicates whether an achievement's local and cloud definitions are identical.<br/><br/>
    ///
    /// The method <see cref="AchievementWidget.AreSame"/> determines whether two achievements are identical.<br/><br/>
    ///
    /// Local and cloud achievements are compared to each other if they have the same <see cref="AchievementWidget.Id"/>, which is the "primary key" for achievement definitions.<br/><br/>
    ///
    /// The SyncStatus is refreshed whenever the buttons "Save data" or "Get latest" are clicked.
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// It is unknown whether this local achievement is synchronized with the cloud.
        /// </summary>
        Unknown,

        /// <summary>
        /// This local achievement is synchronized with the cloud. The cloud and local definitions are identical, as compared by <see cref="AchievementWidget.AreSame"/>.
        /// </summary>
        Synchronized,

        /// <summary>
        /// This local achievement is unsynchronized with the cloud. The cloud and local definitions are different, or the achievement doesn't exist in the cloud.
        /// </summary>
        Unsynchronized
    }

    /// <summary>
    /// A GUI element which renders a single, editable Achievement.
    /// </summary>
    [Serializable]
    public class AchievementWidget : IHasSerializedPropertyOfSelf
    {
        public SerializedProperty SerializedPropertyOfSelf { get; set; }

        // UI Slider Min/Max Values
        // These are not hard limits for your game. You can increase these values if you'd like. These limits only exist to make the UI sliders look nice with a reasonable range of values.
        internal const int POINTS_MAX = 200;
        internal const int POINTS_MIN = 1;
        internal const int STEPS_MAX = 1000;
        internal const int STEPS_MIN = 1;
        internal const int SORT_MAX = 200;
        internal const int SORT_MIN = 1;

        // UI Fields
        public string Id = string.Empty;
        public string Title = string.Empty;
        public int Points = POINTS_MIN;
        public string DescriptionLocked = string.Empty;
        public string DescriptionUnlocked = string.Empty;
        public string IconPathLocked = string.Empty;
        public string IconPathUnlocked = string.Empty;
        public int NumberOfStepsToEarn = STEPS_MIN;
        public bool IsInvisibleToPlayers = false;
        public bool CanBeAchieved = true;
        public int SortOrder = SORT_MIN;
        public bool IsMarkedForDeletion = false;

        // When we first create a new achievement, we know its not on the cloud, thus we implicitly know its Unsynchronized
        public SyncStatus SyncStatus = SyncStatus.Unsynchronized;
        
        [SerializeField] private Vector2 _scrollPosition;
        [SerializeField] private bool _isExpanded = false;

        // These should explicitly never be serialized, we what the texture to only buffer while Unity is running, but refresh on each start.
        [NonSerialized] private Texture _lockedTexture = null;
        [NonSerialized] private Texture _unlockedTexture = null;

        public static implicit operator AdminAchievement(AchievementWidget achievementWidget)
        {
            return new AdminAchievement()
            {
                AchievementId = achievementWidget.Id,
                Title = achievementWidget.Title,
                LockedDescription = achievementWidget.DescriptionLocked,
                UnlockedDescription = achievementWidget.DescriptionUnlocked,
                LockedIcon = achievementWidget.IconPathLocked,
                UnlockedIcon = achievementWidget.IconPathUnlocked,
                RequiredAmount = (uint)achievementWidget.NumberOfStepsToEarn,
                Points = (uint)achievementWidget.Points,
                OrderNumber = (uint)achievementWidget.SortOrder,
                IsStateful = achievementWidget.NumberOfStepsToEarn > 1,
                IsSecret = achievementWidget.IsInvisibleToPlayers,
                IsHidden = !achievementWidget.CanBeAchieved,
            };
        }

        public static implicit operator AchievementWidget(AdminAchievement adminAchievement)
        {
            return new AchievementWidget()
            {
                Id = adminAchievement.AchievementId,
                Title = adminAchievement.Title,
                DescriptionLocked = adminAchievement.LockedDescription,
                DescriptionUnlocked = adminAchievement.UnlockedDescription,
                IconPathLocked = adminAchievement.LockedIcon,
                IconPathUnlocked = adminAchievement.UnlockedIcon,
                NumberOfStepsToEarn = (int)adminAchievement.RequiredAmount,
                Points = (int)adminAchievement.Points,
                SortOrder = (int)adminAchievement.OrderNumber,
                IsInvisibleToPlayers = adminAchievement.IsSecret,
                CanBeAchieved = !adminAchievement.IsHidden
            };
        }

        public static implicit operator Achievement(AchievementWidget achievementWidget)
        {
            return new Achievement()
            {
                AchievementId = achievementWidget.Id,
                Title = achievementWidget.Title,
                LockedDescription = achievementWidget.DescriptionLocked,
                UnlockedDescription = achievementWidget.DescriptionUnlocked,
                LockedIcon = achievementWidget.IconPathLocked,
                UnlockedIcon = achievementWidget.IconPathUnlocked,
                CurrentValue = 0,
                RequiredAmount = achievementWidget.NumberOfStepsToEarn,
                Points = achievementWidget.Points,
                OrderNumber = achievementWidget.SortOrder,
                IsSecret = achievementWidget.IsInvisibleToPlayers,
                IsHidden = !achievementWidget.CanBeAchieved,
                IsEarned = false,
                IsNewlyEarned = false,
                EarnedAt = string.Empty,
                UpdatedAt = string.Empty
            };
        }

        public static implicit operator AchievementWidget(Achievement achievement)
        {
            return new AchievementWidget()
            {
                Id = achievement.AchievementId,
                Title = achievement.Title,
                DescriptionLocked = achievement.LockedDescription,
                DescriptionUnlocked = achievement.UnlockedDescription,
                IconPathLocked = achievement.LockedIcon,
                IconPathUnlocked = achievement.UnlockedIcon,
                NumberOfStepsToEarn = achievement.RequiredAmount,
                Points = achievement.Points,
                SortOrder = achievement.OrderNumber,
                IsInvisibleToPlayers = achievement.IsSecret,
                CanBeAchieved = !achievement.IsHidden
            };
        }

        /// <summary>
        /// Return true if the local achievement and cloud achievement are exactly the same.
        /// </summary>
        public static bool AreSame(AchievementWidget localAchievement, AchievementWidget cloudAchievement)
        {
            return localAchievement.Id == cloudAchievement.Id
                   && localAchievement.Title == cloudAchievement.Title
                   && localAchievement.Points == cloudAchievement.Points
                   && localAchievement.DescriptionLocked == cloudAchievement.DescriptionLocked
                   && localAchievement.DescriptionUnlocked == cloudAchievement.DescriptionUnlocked
                   && localAchievement.IconPathLocked.Equals(cloudAchievement.IconPathLocked)
                   && localAchievement.IconPathUnlocked.Equals(cloudAchievement.IconPathUnlocked)
                   && localAchievement.NumberOfStepsToEarn == cloudAchievement.NumberOfStepsToEarn
                   && localAchievement.IsInvisibleToPlayers == cloudAchievement.IsInvisibleToPlayers
                   && localAchievement.CanBeAchieved == cloudAchievement.CanBeAchieved
                   && localAchievement.SortOrder == cloudAchievement.SortOrder;
        }

        /// <summary>
        /// Public method for collapsing an expanded widget.
        /// </summary>
        public void CollapseWidget()
        {
            _isExpanded = false;
        }

        /// <summary>
        /// Public method for clearing the current icon textures and forcing a reload.
        /// </summary>
        public void ClearTextures()
        {
            _lockedTexture = null;
            _unlockedTexture = null;
        }

        /// <summary>
        /// Draw the achievement on the screen.
        /// </summary>
        /// <param name="isEditable">True if the achievement's attributes can be edited, false if not.</param>
        public void OnGUI(bool isEditable)
        {
            if (IsMarkedForDeletion || SerializedPropertyOfSelf == null)
            {
                // Don't display this achievement
                return;
            }
            
            using (new EditorGUILayout.VerticalScope(_isExpanded ? SettingsGUIStyles.Achievements.BodyExpanded : SettingsGUIStyles.Achievements.BodyCollapsed))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    _isExpanded = EditorGUILayout.Foldout(_isExpanded, string.Empty, true, EditorStyles.foldout);

                    GUILayout.Label(Title, SettingsGUIStyles.Achievements.Header);

                    Rect headerTitleRect = GUILayoutUtility.GetLastRect();

                    GUI.Label(new Rect(headerTitleRect.x - SettingsGUIStyles.Achievements.HEADER_ICON_HORIZONTAL_PADDING, 
                            headerTitleRect.y + SettingsGUIStyles.Achievements.HEADER_ICON_VERTICAL_PADDING,
                            SettingsGUIStyles.Achievements.HEADER_ICON_WIDTH,
                            SettingsGUIStyles.Achievements.HEADER_ICON_HEIGHT), 
                        GetAchievementHeader(), 
                        EditorStyles.label);

                    GUILayout.FlexibleSpace();

                    Texture deleteTexture = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
                    using (new EditorGUI.DisabledScope(!isEditable))
                    {
                        if (GUILayout.Button(new GUIContent(deleteTexture, L10n.Tr("Delete this achievement locally. To delete from the cloud, click \"Save data\" after clicking this button.")), SettingsGUIStyles.Achievements.DeleteButton))
                        {
                            IsMarkedForDeletion = true;
                        }
                    }
                }
                if (!_isExpanded)
                {
                    return;
                }


                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.Achievements.Expanded))
                {
                    UpdateSyncStatus(Id, PropertyField(nameof(Id), L10n.Tr("ID (primary key)"), string.Empty, isEnabled: false));
                    UpdateSyncStatus(Title, PropertyField(nameof(Title), L10n.Tr("Title"), string.Empty, isEditable));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        Points = UpdateSyncStatus(Points, EditorGUILayoutElements.OverrideSlider(L10n.Tr("Points"), Points, POINTS_MIN, POINTS_MAX, isEnabled: isEditable));
                    }

                    GUILayout.Space(SettingsGUIStyles.Achievements.SPACING);

                    DescriptionLocked = UpdateSyncStatus(DescriptionLocked, DescriptionField(L10n.Tr("Description\n(when locked)"), L10n.Tr("Enter a description for this achievement while it is still locked"), DescriptionLocked, isEditable));
                    DescriptionUnlocked = UpdateSyncStatus(DescriptionUnlocked, DescriptionField(L10n.Tr("Description\n(when achieved)"), L10n.Tr("Enter a description for this achievement for after it is achieved"), DescriptionUnlocked, isEditable));

                    _lockedTexture = ImageField(L10n.Tr("Image/icon\n(when locked)"), L10n.Tr("Select image/icon for locked achievement"), _lockedTexture, nameof(IconPathLocked), ref IconPathLocked, isEditable);
                    _unlockedTexture = ImageField(L10n.Tr("Image/icon\n(when achieved)"), L10n.Tr("Select image/icon for achieved achievement"), _unlockedTexture, nameof(IconPathUnlocked), ref IconPathUnlocked, isEditable);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        NumberOfStepsToEarn = UpdateSyncStatus(NumberOfStepsToEarn, EditorGUILayoutElements.OverrideSlider(L10n.Tr("No. steps to earn"), NumberOfStepsToEarn, STEPS_MIN, STEPS_MAX, isEnabled: isEditable));
                    }

                    GUILayout.Space(SettingsGUIStyles.Achievements.SPACING);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PrefixLabel(L10n.Tr("Visibility"), SettingsGUIStyles.Achievements.VisibilityLabel);
                        EditorGUILayoutElements.KeepPreviousPrefixLabelEnabled();

                        IsInvisibleToPlayers = UpdateSyncStatus(IsInvisibleToPlayers, EditorGUILayoutElements.ToggleLeft(L10n.Tr("Invisible to players"), IsInvisibleToPlayers, isEnabled: isEditable));
                        CanBeAchieved = UpdateSyncStatus(CanBeAchieved, EditorGUILayoutElements.ToggleLeft(L10n.Tr("Can be achieved"), CanBeAchieved, isEnabled: isEditable));
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(SettingsGUIStyles.Achievements.SPACING);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        SortOrder = UpdateSyncStatus(SortOrder, EditorGUILayoutElements.OverrideSlider(L10n.Tr("Sort order"), SortOrder, SORT_MIN, SORT_MAX, isEnabled: isEditable));
                    }
                }
            }
        }

        private string PropertyField(string propertyPath, string label, string placeholder = "", bool isEnabled = true, params GUILayoutOption[] options)
        {
            SerializedProperty property = SerializedPropertyOfSelf.FindPropertyRelative(propertyPath);
            if (property == null)
            {
                // when adding items quickly, there is a race condition where the property may not exist yet, skip this element for this render loop, it will render on the next loop once the property is available.
                return string.Empty;
            }

            EditorGUILayoutElements.PropertyField(label, property, 1, placeholder, isEnabled, options);
            GUILayout.Space(SettingsGUIStyles.Achievements.SPACING);

            return property.stringValue;
        }

        private string DescriptionField(string title, string placeholder, string description, bool isEnabled)
        {
            GUIContent content = new GUIContent(description);

            float minWidth, maxWidth;
            SettingsGUIStyles.Achievements.Description.CalcMinMaxWidth(content, out minWidth, out maxWidth);
            float calculatedHeight = SettingsGUIStyles.Achievements.Description.CalcHeight(content, maxWidth);
            float height = Math.Max(calculatedHeight, SettingsGUIStyles.Achievements.DESCRIPTION_MIN_HEIGHT);

            string value = EditorGUILayoutElements.DescriptionField(
                title,
                description,
                1,
                placeholder,
                isEnabled,
                SettingsGUIStyles.Achievements.DescriptionLabel,
                SettingsGUIStyles.Achievements.Description,
                GUILayout.MinHeight(height)
            );

            GUILayout.Space(SettingsGUIStyles.Achievements.SPACING);

            return value;
        }

        private Texture ImageField(string label, string windowText, Texture currentImage, string nameOfImagePath, ref string imagePath, bool isEnabled)
        {
            Texture defaultIcon = EditorGUIUtility.IconContent(L10n.Tr("d_Texture Icon")).image;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.HorizontalScope(SettingsGUIStyles.Achievements.ImageBody))
                {
                    EditorGUILayout.PrefixLabel(label, SettingsGUIStyles.Achievements.ImageLabel, SettingsGUIStyles.Achievements.ImageLabel);
                    EditorGUILayoutElements.KeepPreviousPrefixLabelEnabled();

                    EditorGUILayout.LabelField(new GUIContent(currentImage ?? defaultIcon), SettingsGUIStyles.Achievements.Image);
                    if (EditorGUILayoutElements.Button(L10n.Tr("Browse"), isEnabled))
                    {
                        string file = EditorUtility.OpenFilePanel(windowText, string.Empty, "png,PNG,jpeg,JPEG,jpg,JPG");
                        if (file.Length > 0)
                        {
                            SerializedPropertyOfSelf.FindPropertyRelative(nameOfImagePath).stringValue = file;

                            SetToUnsynchronized();

                            currentImage = null;
                        }
                    }
                }

                string oldPath = SerializedPropertyOfSelf.FindPropertyRelative(nameOfImagePath).stringValue;
                imagePath = UpdateSyncStatus(oldPath, PropertyField(nameOfImagePath, string.Empty, L10n.Tr("or enter your icon's path"), isEnabled));

                if (!oldPath.Equals(imagePath))
                {
                    currentImage = null;
                }
            }

            GUILayout.Space(SettingsGUIStyles.Achievements.SPACING);

            if (currentImage == null)
            {
                if (File.Exists(imagePath))
                {
                    if (!IsFileAvailable(imagePath))
                    {
                        // This case occurs if the file is still open from download while we are trying to read it. Keep as null and try again on the next frame.
                        return null;
                    }

                    byte[] imageBytes = File.ReadAllBytes(imagePath);

                    Texture2D newImage = new Texture2D(1, 1);
                    if (!ImageConversion.LoadImage(newImage, imageBytes, false))
                    {
                        Logging.LogInfo(L10n.Tr($"Could not load image at {imagePath}"));

                        return defaultIcon;
                    }

                    return newImage;
                }
                else if (!string.IsNullOrEmpty(imagePath))
                {
                    Logging.LogInfo(L10n.Tr($"File not found at {imagePath}"));

                    return defaultIcon;
                }
                else
                {
                    // this is the case where the achievement has just been created and has no image paths
                    return defaultIcon;
                }
            }
            else
            {
                return currentImage;
            }
        }

        private bool IsFileAvailable(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            { 
                // File is not available.
                return false;
            }

            // File is free.
            return true;
        }

        private GUIContent GetAchievementHeader()
        {
            if (SyncStatus == SyncStatus.Unknown)
            {
                return new GUIContent(EditorResources.Textures.Colors.Transparent.Get());
            }
            else if (SyncStatus == SyncStatus.Synchronized)
            {
                return new GUIContent(EditorResources.Textures.FeatureStatusSuccess.Get(), L10n.Tr("Synced with cloud"));
            }
            else // Unsynchronized
            {
                return new GUIContent(EditorResources.Textures.Unsynchronized.Get(), L10n.Tr("Not in sync with cloud"));
            }
        }

        private void SetToUnsynchronized()
        {
            // SyncStatus is serialized, so we need to change its serialized property, if we just assign it a value instead then it will be overridden when Unity updates its serialized values
            SerializedPropertyOfSelf.FindPropertyRelative(nameof(SyncStatus)).enumValueIndex = (int)SyncStatus.Unsynchronized;
        }

        private T UpdateSyncStatus<T>(T oldValue, T newValue)
        {
            if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            {
                SetToUnsynchronized();
            }

            return newValue;
        }
    }
}
