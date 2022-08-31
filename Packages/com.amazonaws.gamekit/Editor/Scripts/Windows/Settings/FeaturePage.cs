// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Windows.Settings.Pages.AllFeatures;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// The base class for all Feature pages displayed in the AWS GameKit Settings window.
    /// </summary>
    [Serializable]
    public abstract class FeaturePage : AllFeaturesPage
    {
        public static string DeploymentTabName => L10n.Tr("Deployment");
        public static string TestingTabName => L10n.Tr("Testing");

        public override string DisplayName => FeatureType.GetDisplayName();

        /// <summary>
        /// The feature this page is for.
        /// </summary>
        public abstract FeatureType FeatureType { get; }

        [SerializeField] private TabWidget _tabViewWidget;

        /// <summary>
        /// Change the currently selected tab to the named tab.
        /// </summary>
        /// <remarks>
        /// If the named tab does not exist, then an Error will be logged and the page's currently selected tab will not change.<br/><br/>
        /// </remarks>
        /// <param name="tabName">The name of the tab to select. All <see cref="FeaturePage"/>'s should have tabs named <see cref="DeploymentTabName"/> and <see cref="TestingTabName"/> at minimum.</param>
        public override void SelectTab(string tabName)
        {
            _tabViewWidget.SelectTab(tabName);
        }

        /// <summary>
        /// Create a new FeaturePage with an arbitrary set of tabs.
        /// </summary>
        protected void Initialize(Tab[] tabs, SettingsDependencyContainer dependencies)
        {
            GUI.ToolbarButtonSize tabSelectorButtonSize = GUI.ToolbarButtonSize.FitToContents;
            _tabViewWidget.Initialize(tabs, tabSelectorButtonSize);
            base.Initialize(dependencies);
        }

        /// <summary>
        /// Get the default set of tabs that every <see cref="FeaturePage"/> should have.
        /// </summary>
        protected static Tab[] GetDefaultTabs(FeatureSettingsTab settingsTab, FeatureExamplesTab examplesTab)
        {
            return new Tab[]
            {
                CreateSettingsTab(settingsTab),
                CreateExamplesTab(examplesTab)
            };
        }

        /// <summary>
        /// Create a <see cref="FeatureSettingsTab"/>. This should only be used during the constructor to build a <c>Tab[]</c> when not using <see cref="GetDefaultTabs"/>.
        /// </summary>
        protected static Tab CreateSettingsTab(FeatureSettingsTab settingsTab)
        {
            return new Tab(DeploymentTabName, settingsTab);
        }

        /// <summary>
        /// Create a <see cref="FeatureExamplesTab"/>. This should only be used during the constructor to build a <c>Tab[]</c> when not using <see cref="GetDefaultTabs"/>.
        /// </summary>
        protected static Tab CreateExamplesTab(FeatureExamplesTab examplesTab)
        {
            return new Tab(TestingTabName, examplesTab);
        }

        protected override void DrawContent()
        {
            // Feature Description
            EditorGUILayout.LabelField(FeatureType.GetDescription(withEndingPeriod: true), SettingsGUIStyles.FeaturePage.Description);

            // Tab View
            _tabViewWidget.OnGUI(SettingsGUIStyles.FeaturePage.TabSelector);
        }
    }
}
