// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.GUILayoutExtensions;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.Achievements
{
    [Serializable]
    public class AchievementsPage : FeaturePage
    {
        public static string AchievementsDataTabName = L10n.Tr("Configure Data");

        public override FeatureType FeatureType => FeatureType.Achievements;

        [SerializeField] private AchievementsSettingsTab _achievementSettingsTab;
        [SerializeField] private AchievementsExamplesTab _achievementExamplesTab;
        [SerializeField] private AchievementsDataTab _achievementDataTab;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _achievementSettingsTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_achievementSettingsTab)));
            _achievementExamplesTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_achievementExamplesTab)));
            _achievementDataTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_achievementDataTab)));

            Tab[] tabs = new Tab[]
            {
                CreateSettingsTab(_achievementSettingsTab),
                new Tab(AchievementsDataTabName, _achievementDataTab),
                CreateExamplesTab(_achievementExamplesTab)
            };
            base.Initialize(tabs, dependencies);
        }
    }
}