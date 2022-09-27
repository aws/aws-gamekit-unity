// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Editor.Windows.Settings;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Windows.QuickAccess
{
    /// <summary>
    /// Displays the Quick Access window and receives its UI events.
    /// </summary>
    public class QuickAccessWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "AWS GameKit";

        private enum InitializationLevel
        {
            // Initialize() has not been called. The GUI is empty.
            Uninitialized,

            // Initialize() has been called. The GUI is drawn.
            Initialized
        }

        // Controllers
        private SettingsController _settingsWindowController;
        private SettingsDependencyContainer _dependencies;

        // State
        private InitializationLevel _initLevel = InitializationLevel.Uninitialized;

        // Fields marked with [SerializeField] are persisted & restored whenever Unity is restarted, Play Mode is entered, or the assembly is rebuilt.
        // However, they are cleared when the window is closed. Unlike the SettingsWindow, we don't want to persist these when the window is closed.
        [SerializeField] private Vector2 _scrollbarPositions;

        // Events
        public static event OnWindowEnabled Enabled;

        public delegate void OnWindowEnabled(QuickAccessWindow enabledQuickAccessWindow);

        private void OnEnable()
        {
            // Set title
            string windowTitle = WINDOW_TITLE;
            Texture windowIcon = EditorResources.Textures.WindowIcon.Get();
            titleContent = new GUIContent(windowTitle, windowIcon);

            // Set minimum size
            minSize = new Vector2(QuickAccessGUIStyles.Window.MinWidth, QuickAccessGUIStyles.Window.MinHeight);

            // Publish event
            Enabled?.Invoke(this);
        }

        /// <summary>
        /// Initialize this window so it can start drawing the GUI. This is effectively the window's constructor.
        /// </summary>
        public void Initialize(SettingsController settingsWindowController, SettingsDependencyContainer dependencies)
        {
            _settingsWindowController = settingsWindowController;
            _dependencies = dependencies;

            _initLevel = InitializationLevel.Initialized;
        }

        private void OnGUI()
        {
            if (_initLevel == InitializationLevel.Uninitialized)
            {
                GUILayout.Space(0f);
                return;
            }

            EditorWindowHelper.SetBackgroundColor(this, QuickAccessGUIStyles.Window.BackgroundColor);

            _scrollbarPositions = EditorGUILayout.BeginScrollView(_scrollbarPositions);
            DrawEnvironmentAndCredentials();
            DrawFeatureRows();
            EditorGUILayout.EndScrollView();

            RefreshButtonColors();
        }

        private void DrawEnvironmentAndCredentials()
        {
            
        }

        private void DrawFeatureRows()
        {
            Dictionary<FeatureType, FeatureStatus> featuresStatus = new Dictionary<FeatureType, FeatureStatus>();
            foreach (FeatureSettingsTab featureSettingsTab in FeatureSettingsTab.FeatureSettingsTabsInstances)
            {
                featuresStatus.Add(featureSettingsTab.FeatureType, featureSettingsTab.GetFeatureStatus());
            }

            foreach (FeatureType featureType in FeatureTypeEditorData.FeaturesToDisplay)
            {
                GUILayout.Space(QuickAccessGUIStyles.Window.SpaceBetweenFeatureRows);
                FeatureStatus status;
                if (!featuresStatus.TryGetValue(featureType, out status))
                {
                    status = _dependencies.FeatureDeploymentOrchestrator.GetFeatureStatus(featureType);
                }
                DrawFeatureRow(new FeatureRowData(featureType), status);
            }
        }

        private void DrawFeatureRow(FeatureRowData featureRowData, FeatureStatus deploymentStatus)
        {
            using (EditorGUILayout.HorizontalScope horizontalScope = new EditorGUILayout.HorizontalScope(QuickAccessGUIStyles.FeatureRow.HorizontalLayout, QuickAccessGUIStyles.FeatureRow.HorizontalLayoutOptions))
            { 
                Rect featureRowRect = horizontalScope.rect;
                // Background Button
                Texture buttonTexture = EditorResources.Textures.Colors.Transparent.Get();
                GUIStyle buttonStyle = QuickAccessGUIStyles.FeatureRow.Button;
                if (GUI.Button(featureRowRect, buttonTexture, buttonStyle))
                {
                    SettingsWindow.OpenPage(featureRowData.PageType);
                }

                // Change mouse cursor to a "pointer" when hovering over the button
                EditorGUIUtility.AddCursorRect(featureRowRect, MouseCursor.Link);

                // Icon
                GUILayout.Box(featureRowData.Icon, QuickAccessGUIStyles.FeatureRow.Icon);

                GUILayout.Space(3.4f);

                // Name & Description
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(featureRowData.Name, QuickAccessGUIStyles.FeatureRow.Name);
                    GUILayout.Space(1f);
                    EditorGUILayout.LabelField(featureRowData.Description, QuickAccessGUIStyles.FeatureRow.Description);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Space(10f);

                // Deployment Status
                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayoutElements.DeploymentStatusIcon(deploymentStatus);
                    GUILayout.FlexibleSpace();
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(deploymentStatus.GetDisplayName());
                    GUILayout.FlexibleSpace();
                }
            }
        }

        /// <summary>
        /// Repaint the canvas to make the buttons instantly change their color when the mouse is hovering over them.
        ///
        /// This is a workaround for strange behavior by GUI.Button():
        /// - An un-styled GUI.Button() is instantly responsive. It changes color the instant the mouse moves on top or off of the button.
        /// - A styled GUI.Button() is very slow to respond (on the order of 1-2 seconds).
        ///
        /// Without this method, it's not possible to have a responsive button with custom colors.
        /// </summary>
        private void RefreshButtonColors()
        {
            wantsMouseMove = true;

            if (Event.current.type == EventType.MouseMove)
            {
                // It is taxing on the GPU to repaint the GUI every frame.
                // So we only repaint when the mouse is on the window and it moved during the last frame.
                // This limits the increased GPU usage so it only occurs while the user is interacting with the Quick Access window.

                Repaint();
            }
        }
    }
}
