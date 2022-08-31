// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.GameStateCloudSaving
{
    [Serializable]
    public class GameStateCloudSavingPage : FeaturePage
    {
        public override FeatureType FeatureType => FeatureType.GameStateCloudSaving;

        [SerializeField] private GameStateCloudSavingSettingsTab _gameStateCloudSavingSettingsTab;
        [SerializeField] private GameStateCloudSavingExamplesTab _gameStateCloudSavingExamplesTab;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _gameStateCloudSavingSettingsTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_gameStateCloudSavingSettingsTab)));
            _gameStateCloudSavingExamplesTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_gameStateCloudSavingExamplesTab)));

            base.Initialize(GetDefaultTabs(_gameStateCloudSavingSettingsTab, _gameStateCloudSavingExamplesTab), dependencies);
        }
    }
}