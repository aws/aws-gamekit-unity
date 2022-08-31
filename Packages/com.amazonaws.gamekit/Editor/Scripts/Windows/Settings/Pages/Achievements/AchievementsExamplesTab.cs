// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.Features.GameKitAchievements;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.Achievements
{
    [Serializable]
    public class AchievementsExamplesTab : FeatureExamplesTab
    {
        public override FeatureType FeatureType => FeatureType.Achievements;

        private IAchievementsProvider _achievements;

        [SerializeField] private ListAchievementsExampleUI _listAchievementsExampleUI;
        [SerializeField] private GetAchievementExampleUI _getAchievementExampleUI;
        [SerializeField] private UpdateAchievementExampleUI _updateAchievementExampleUI;

        public override void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _achievements = dependencies.Achievements;

            _listAchievementsExampleUI.Initialize(CallListAchievements, serializedProperty.FindPropertyRelative(nameof(_listAchievementsExampleUI)));
            _getAchievementExampleUI.Initialize(CallGetAchievement, serializedProperty.FindPropertyRelative(nameof(_getAchievementExampleUI)));
            _updateAchievementExampleUI.Initialize(CallUpdateAchievement, serializedProperty.FindPropertyRelative(nameof(_updateAchievementExampleUI)));

            base.Initialize(dependencies, serializedProperty);
        }

        #region Helper Functions
        public static void DrawAchievement(Achievement achievement, int indentationLevel = 1)
        {
            EditorGUILayoutElements.TextField("Achievement Id", achievement.AchievementId, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Title", achievement.Title, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Current Value", achievement.CurrentValue.ToString(), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Points", achievement.Points.ToString(), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Locked Description", achievement.LockedDescription, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Unlocked Description", achievement.UnlockedDescription, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Locked Icon (path)", achievement.LockedIcon, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Unlocked Icon (path)", achievement.UnlockedIcon, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Required Amount", achievement.RequiredAmount.ToString(), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.CustomField("Visibility", () =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayoutElements.ToggleLeft("Invisible to players", achievement.IsSecret, false);
                    EditorGUILayoutElements.ToggleLeft("Cannot be achieved", achievement.IsHidden, false);
                }
            }, indentationLevel);
            EditorGUILayoutElements.ToggleField("Earned", achievement.IsEarned, indentationLevel, false);
            EditorGUILayoutElements.ToggleField("Newly Earned", achievement.IsNewlyEarned, indentationLevel, false);

            // Convert ISO-8601 formatted string to local time for improved readability
            string earnedAt = TimeUtils.ISO8601StringToLocalFormattedString(achievement.EarnedAt);
            string updatedAt = TimeUtils.ISO8601StringToLocalFormattedString(achievement.UpdatedAt);
            EditorGUILayoutElements.TextField("Earned At", earnedAt, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Updated At", updatedAt, indentationLevel, isEnabled: false);
        }

        public static void DrawAchievements(List<Achievement> achievements, int indentationLevel = 1)
        {
            for (int i = 0; i < achievements.Count; i++)
            {
                DrawAchievement(achievements[i], indentationLevel);
                if (i != achievements.Count - 1)
                {
                    EditorGUILayoutElements.SectionDivider();
                }
            }
        }
        #endregion

        #region Achievements API Calls
        protected override void DrawExamples()
        {
            _listAchievementsExampleUI.OnGUI();
            _getAchievementExampleUI.OnGUI();
            _updateAchievementExampleUI.OnGUI();
        }

        private void CallListAchievements()
        {
            // ListAchievements is instrumented to work with pagination. For the examples, we've chosen to wait for the entire achievements list to load in one API call.
            ListAchievementsDesc listAchievementsDesc = new ListAchievementsDesc
            {
                // A large page size will increase the time between callbacks, as more data has to be read from the database. However, it will also reduce the total number of
                // networked round-trips to fetch all of the user's data. A small page size will increase the number of networked round-trips, but reduce the latency before any results are returned.
                // To reduce the overall query latency, pick a larger page size. To reduce the latency for receiving a subset of the results, pick a smaller page size.
                // PageSize is capped at 100 on the cloud backend.
                PageSize = 100,

                // When WaitForAllPages is false, the Action<AchievementListResult> callback you pass into ListAchievementsForPlayer() will be continually invoked
                // each time a page of data is returned from the cloud backend. When set to true, the GameKit SDK will aggregate all of a user's achievement data
                // before invoking the callback a single time. In both scenarios, the final Action<uint> onCompleteCallback will be invoked once all achievements have been listed.
                WaitForAllPages = true
            };

            Debug.Log($"Calling Achievements.ListAchievementsForPlayer() with {listAchievementsDesc}");

            _listAchievementsExampleUI.Achievements.Clear();

            _achievements.ListAchievementsForPlayer(listAchievementsDesc, (AchievementListResult result) =>
            {
                // This may be called multiple times; once per page. As we've opted to use WaitForAllPages = true, this should only be called once.
                Debug.Log($"Achievements.ListAchievementsForPlayer() returned a page of results with {result.Achievements.Length} achievements");

                _listAchievementsExampleUI.Achievements.AddRange(result.Achievements);
            },
            (uint result) =>
            {
                // This is executed when an error has occurred, or once all pages have been fetched.
                Debug.Log($"Achievements.ListAchievementsForPlayer() completed with result code {GameKitErrorConverter.GetErrorName(result)}");
                _listAchievementsExampleUI.ResultCode = result;
            });
        }

        private void CallGetAchievement()
        {
            Debug.Log($"Calling Achievements.GetAchievementForPlayer() for achievement \"{_getAchievementExampleUI.AchievementId}\"");

            _getAchievementExampleUI.Achievement = new Achievement();

            _achievements.GetAchievementForPlayer(_getAchievementExampleUI.AchievementId, (AchievementResult result) =>
            {
                Debug.Log($"Achievements.GetAchievementForPlayer() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _getAchievementExampleUI.ResultCode = result.ResultCode;
                _getAchievementExampleUI.Achievement = result.Achievement;
            });
        }

        private void CallUpdateAchievement()
        {
            UpdateAchievementDesc updateAchievementDesc = new UpdateAchievementDesc
            {
                AchievementId = _updateAchievementExampleUI.AchievementId,
                IncrementBy = (uint) _updateAchievementExampleUI.IncrementBy
            };

            Debug.Log($"Calling Achievements.UpdateAchievementForPlayer() with {updateAchievementDesc}");

            _updateAchievementExampleUI.Achievement = new Achievement();

            _achievements.UpdateAchievementForPlayer(updateAchievementDesc, (AchievementResult result) =>
            {
                Debug.Log($"Achievements.UpdateAchievementForPlayer() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _updateAchievementExampleUI.ResultCode = result.ResultCode;
                _updateAchievementExampleUI.Achievement = result.Achievement;
            });
        }
        #endregion
    }

    #region Achievements Examples 
    [Serializable]
    public class ListAchievementsExampleUI : GameKitExampleUI
    {
        public override string ApiName => "List Achievements";
        protected override bool _shouldDisplayResponse => true;

        public List<Achievement> Achievements = new List<Achievement>();
        
        protected override void DrawOutput()
        {
            AchievementsExamplesTab.DrawAchievements(Achievements);
        }
    }

    [Serializable]
    public class GetAchievementExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Get Achievement";
        protected override bool _shouldDisplayResponse => true;

        public string AchievementId;

        public Achievement Achievement;

        protected override void DrawInput()
        {
            PropertyField(nameof(AchievementId), "Achievement Id");
        }

        protected override void DrawOutput()
        {
            AchievementsExamplesTab.DrawAchievement(Achievement);
        }
    }

    [Serializable]
    public class UpdateAchievementExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Update Achievement";
        protected override bool _shouldDisplayResponse => true;

        public string AchievementId;
        public int IncrementBy = 1;

        public Achievement Achievement;

        protected override void DrawInput()
        {
            PropertyField(nameof(AchievementId), "Achievement Id");
            PropertyField(nameof(IncrementBy), "Increment By");
        }

        protected override void DrawOutput()
        {
            AchievementsExamplesTab.DrawAchievement(Achievement);
        }
    }
    #endregion
}