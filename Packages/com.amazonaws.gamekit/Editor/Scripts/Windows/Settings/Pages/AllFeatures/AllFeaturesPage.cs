// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;

// GameKit
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.Core;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.AllFeatures
{
    [Serializable]
    public class AllFeaturesPage : Page
    {
        public override string DisplayName => "All Features";
        private FeatureResourceManager _featureResourceManager;
        private GameKitEditorManager _gamekitEditorManager;
        
        public void Initialize(SettingsDependencyContainer dependencies)
        {
            _featureResourceManager = dependencies.FeatureResourceManager;
            _gamekitEditorManager = dependencies.GameKitEditorManager;
        }

        protected override void DrawContent()
        {
            // Headers
            DrawFeatureHeaders();
            
            EditorGUILayoutElements.SectionDivider();
            
            // Features
            foreach (var featureSettingsTab in FeatureSettingsTab.FeatureSettingsTabsInstances)
            {
                featureSettingsTab.DrawFeatureSummary();
                EditorGUILayoutElements.SectionSpacer();
            }
        }

        private void DrawFeatureHeaders()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.CustomField(L10n.Tr("Feature: "),
                    () => { EditorGUILayout.LabelField(L10n.Tr("Deployment Status:")); },
                    indentationLevel: 0);
            }
        }

        protected override void DrawTitle()
        {
            bool creds = _gamekitEditorManager.CredentialsSubmitted;
            string env = creds ? _featureResourceManager.GetLastUsedEnvironment() : "";
            string region = creds ? _featureResourceManager.GetLastUsedRegion() : "";

            UnityEngine.GUIStyle titleStyle = SettingsGUIStyles.Page.Title;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(DisplayName, titleStyle);
            if (!_gamekitEditorManager.CredentialsSubmitted)
            {
                EditorGUILayout.EndHorizontal();
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Environment: " + env, SettingsGUIStyles.Page.EnvDetails);
            EditorGUILayout.LabelField("Region: " + region, SettingsGUIStyles.Page.EnvDetails);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
