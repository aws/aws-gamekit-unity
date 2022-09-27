
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
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// Base class for all feature-specific example tabs.
    /// </summary>
    [Serializable]
    public abstract class FeatureExamplesTab : IDrawable
    {
        private readonly string TO_ENABLE_LOGIN_MESSAGE = L10n.Tr("To enable, deploy Identity and Authentication.");
        private readonly string TO_ENABLE_DEPLOYED_FEATURE_MESSAGE = L10n.Tr("To enable, login with a player.");

        /// <summary>
        /// The feature which these examples are for.
        /// </summary>
        public abstract FeatureType FeatureType { get; }

        protected bool _displayLoginWidget = true;

        protected FeatureDeploymentOrchestrator _featureDeploymentOrchestrator;
        protected GameKitManager _gameKitManager;
        protected GameKitEditorManager _gameKitEditorManager;
        protected UserInfo _userInfo;

        [SerializeField] private UserLoginWidget _userLoginWidget;
        [SerializeField] private Vector2 _scrollPosition;

        private SerializedProperty _serializedProperty;

        protected virtual bool RequiresLogin => true;

        public virtual void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _serializedProperty = serializedProperty;
            _gameKitManager = dependencies.GameKitManager;
            _gameKitEditorManager = dependencies.GameKitEditorManager;
            _featureDeploymentOrchestrator = dependencies.FeatureDeploymentOrchestrator;
            _userInfo = dependencies.UserInfo;

            _userLoginWidget.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_userLoginWidget)), OnLogout);
        }

        public virtual void OnLogout()
        {
            // No-op should be overwritten by implementing class if needed
        }

        public void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            string description = RequiresLogin
                ? L10n.Tr("Simulate API calls from a game client to the deployed backend for this feature. You must be logged in as a registered player to make test API calls.")
                : L10n.Tr("Simulate API calls from a game client to the deployed backend for this feature.");
            EditorGUILayoutElements.Description(description, indentationLevel: 0);
            EditorGUILayout.Space();

            DrawLoginGUI();
            DrawExampleGUI();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Draw the examples for this feature.
        /// </summary>
        protected abstract void DrawExamples();

        private void DrawLoginGUI()
        {
            if (_displayLoginWidget)
            {
                bool isIdentityDeployed = _featureDeploymentOrchestrator.GetFeatureStatus(FeatureType.Identity) == FeatureStatus.Deployed;

                if (!isIdentityDeployed)
                {
                    DrawBanner(TO_ENABLE_LOGIN_MESSAGE);
                }

                using (new GUILayout.VerticalScope())
                {
                    using (new EditorGUI.DisabledScope(!isIdentityDeployed))
                    {
                        _userLoginWidget.OnGUI();
                    }
                }

                if (!isIdentityDeployed)
                {
                    EditorGUILayoutElements.DrawToolTip(TO_ENABLE_LOGIN_MESSAGE);
                }

                EditorGUILayoutElements.SectionDivider();
            }
        }

        private void DrawExampleGUI()
        {
            
            bool shouldDisableForNotLoggedIn = _displayLoginWidget && !_userInfo.IsLoggedIn;
            bool shouldDisableForNotDeployed = _featureDeploymentOrchestrator.GetFeatureStatus(FeatureType) != FeatureStatus.Deployed;
            bool shouldDisable = shouldDisableForNotLoggedIn || shouldDisableForNotDeployed;

            string message = shouldDisableForNotLoggedIn ? TO_ENABLE_DEPLOYED_FEATURE_MESSAGE : L10n.Tr($"To enable, deploy {FeatureType}.");

            if (shouldDisable)
            {
                DrawBanner(message);
            }

            using (new GUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(shouldDisable))
                {
                    DrawExamples();
                }
            }

            if (shouldDisable)
            {
                EditorGUILayoutElements.DrawToolTip(message);
            }
        }

        private void DrawBanner(string message)
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.Page.BannerBox))
            {
                GUIStyle style = SettingsGUIStyles.Page.BannerBoxLabel;

                Texture entryIcon = SettingsGUIStyles.Icons.WarnIcon;
                GUIContent content = new GUIContent(message, entryIcon);
                Rect entrySize = GUILayoutUtility.GetRect(content, style); 

                EditorGUI.LabelField(entrySize, content, style);
            }
        }
    }
}