// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Editor.Windows.Settings;

namespace AWS.GameKit.Editor
{
    public class DeleteFeatureWindow : EditorWindow
    {
        public Action DeleteCallback;
        public string FeatureName;
        public List<string> ResourceDescriptions;
        private Vector2 _scrollPosition = Vector2.zero;
        private string _confirmationText = "";

        public static void ShowWindow(string featureName, List<string> resourceDescriptions, Action deleteCallback)
        {
            DeleteFeatureWindow window = GetWindow<DeleteFeatureWindow>("Delete " + featureName);
            window.DeleteCallback = deleteCallback;
            window.FeatureName = featureName;
            window.ResourceDescriptions = resourceDescriptions;
            window.minSize = new Vector2(SettingsGUIStyles.DeleteWindow.MIN_SIZE_X, SettingsGUIStyles.DeleteWindow.MIN_SIZE_Y);
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("The following resources will be deleted for the " + FeatureName + " feature.", SettingsGUIStyles.DeleteWindow.GeneralText);
            }

            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.DeleteWindow.ResourceList))
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                foreach (string line in ResourceDescriptions)
                {
                    EditorGUILayout.LabelField(line, SettingsGUIStyles.DeleteWindow.ResourceDescriptionLine);
                }
                EditorGUILayout.EndScrollView();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                LinkWidget.Options backupLinkOptions = new LinkWidget.Options()
                {
                    Alignment = LinkWidget.Alignment.Right,
                    ContentOffset = new Vector2(
                    x: -25,
                    y: 10
                ),
                };
                LinkWidget backupLink = new LinkWidget(L10n.Tr("How can I backup a DynamoDB table to Amazon S3 ?"), DocumentationURLs.BACKUP_DYNAMO_REFERENCE, backupLinkOptions);
                backupLink.OnGUI();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("To confirm deletion type 'Yes' below.", SettingsGUIStyles.DeleteWindow.GeneralText);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _confirmationText = EditorGUILayout.TextField(_confirmationText, SettingsGUIStyles.DeleteWindow.GeneralTextField);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("This action retains your locally stored GameKit code and configuration files for this feature, which will be used in future deployments. " +
                    "To return this feature to the default GameKit experience, you must delete these files local files manually.", SettingsGUIStyles.DeleteWindow.GeneralText);
            }

            LinkWidget.Options learnMoreLinkOptions = new LinkWidget.Options()
            {
                ContentOffset = new Vector2(
                    x: 25,
                    y: 0
                )
            };
            LinkWidget learnMoreLink = new LinkWidget(L10n.Tr("Learn More"), DocumentationURLs.DELETE_INSTANCE_REFERENCE, learnMoreLinkOptions);
            learnMoreLink.OnGUI();

            using (new EditorGUILayout.HorizontalScope(SettingsGUIStyles.DeleteWindow.ButtonMargins))
            {
                GUILayout.FlexibleSpace();
                if (EditorGUILayoutElements.Button("Ok", _confirmationText.ToLower() == "yes"))
                {
                    DeleteCallback();
                    this.Close();
                }
                if (EditorGUILayoutElements.Button("Cancel"))
                {
                    // close window
                    this.Close();
                }
            }
        }
    }
}
