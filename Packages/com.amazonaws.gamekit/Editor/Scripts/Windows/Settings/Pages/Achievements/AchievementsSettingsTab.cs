// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Models;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.Achievements
{
    [Serializable]
    public class AchievementsSettingsTab : FeatureSettingsTab
    {
        public override FeatureType FeatureType => FeatureType.Achievements;

        protected override IEnumerable<IFeatureSetting> FeatureSpecificSettings => new List<IFeatureSetting>()
        {
            // No feature specific settings.
        };

        protected override IEnumerable<SecretSetting> FeatureSecrets => new List<SecretSetting>()
        {
            // No feature secrets.
        };

        protected override bool ShouldReloadFeatureSettings() => false;

        protected override bool ShouldDrawSettingsSection() => false;

        protected override void DrawSettings()
        {
            // No settings.
        }
    }
}