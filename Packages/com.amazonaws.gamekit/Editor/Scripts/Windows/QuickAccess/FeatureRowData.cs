// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Windows.Settings;

namespace AWS.GameKit.Editor.Windows.QuickAccess
{
    /// <summary>
    /// All of the data needed to display a feature's row in the Quick Access window.
    /// </summary>
    public class FeatureRowData
    {
        public string Name;
        public string Description;
        public Texture Icon;
        public PageType PageType;

        public FeatureRowData(FeatureType featureType)
        {
            Name = featureType.GetDisplayName();
            Description = featureType.GetDescription();
            Icon = featureType.GetIcon();
            PageType = featureType.GetPageType();
        }
    }
}
