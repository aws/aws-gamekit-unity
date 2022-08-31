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
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Models.FeatureSettings;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.GameStateCloudSaving
{
    [Serializable]
    public class GameStateCloudSavingSettingsTab : FeatureSettingsTab
    {
        public override FeatureType FeatureType => FeatureType.GameStateCloudSaving;

        protected override IEnumerable<IFeatureSetting> FeatureSpecificSettings => new List<IFeatureSetting>()
        {
            _maxSlotsPerPlayer
        };

        protected override IEnumerable<SecretSetting> FeatureSecrets => new List<SecretSetting>()
        {
            // No feature secrets.
        };

        // Feature Settings
        [SerializeField] private FeatureSettingInt _maxSlotsPerPlayer = new FeatureSettingInt("max_save_slots_per_player", defaultValue: 10);

        private static readonly int MINIMUM_SLOTS_PER_PLAYER = 0;

        // This is not a hard limit. You may increase this value up to Int32.MaxValue, although Unity's UI slider overflows after about 2140000000.
        private static readonly int MAXIMUM_SLOTS_PER_PLAYER = 100;

        protected override void DrawSettings()
        {
            SerializedProperty maxSlotsPerPlayerProperty = GetFeatureSettingProperty(nameof(_maxSlotsPerPlayer));
            maxSlotsPerPlayerProperty.intValue = EditorGUILayoutElements.IntSlider(L10n.Tr("Maximum save slots"), maxSlotsPerPlayerProperty.intValue, MINIMUM_SLOTS_PER_PLAYER, MAXIMUM_SLOTS_PER_PLAYER, indentationLevel: 0);
        }
    }
}