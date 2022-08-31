// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.UserGameplayData
{
    [Serializable]
    public class UserGameplayDataPage : FeaturePage
    {
        public override FeatureType FeatureType => FeatureType.UserGameplayData;

        [SerializeField] private UserGameplayDataSettingsTab _userGameplayDataSettingsTab;
        [SerializeField] private UserGameplayDataExamplesTab _userGameplayDataExamplesTab;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _userGameplayDataSettingsTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_userGameplayDataSettingsTab)));
            _userGameplayDataExamplesTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_userGameplayDataExamplesTab)));

            base.Initialize(GetDefaultTabs(_userGameplayDataSettingsTab, _userGameplayDataExamplesTab), dependencies);
        }
    }
}