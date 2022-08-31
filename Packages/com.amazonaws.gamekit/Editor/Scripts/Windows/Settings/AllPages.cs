// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;

// GameKit
using AWS.GameKit.Editor.Windows.Settings.Pages.Achievements;
using AWS.GameKit.Editor.Windows.Settings.Pages.AllFeatures;
using AWS.GameKit.Editor.Windows.Settings.Pages.EnvironmentAndCredentials;
using AWS.GameKit.Editor.Windows.Settings.Pages.GameStateCloudSaving;
using AWS.GameKit.Editor.Windows.Settings.Pages.IdentityAndAuthentication;
using AWS.GameKit.Editor.Windows.Settings.Pages.Log;
using AWS.GameKit.Editor.Windows.Settings.Pages.UserGameplayData;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// Holds references to every page that gets displayed in the AWS GameKit Settings window.
    /// </summary>
    [Serializable]
    public class AllPages
    {
        // Before features
        public EnvironmentAndCredentialsPage EnvironmentAndCredentialsPage;
        public AllFeaturesPage AllFeaturesPage;

        // Features
        public IdentityAndAuthenticationPage IdentityAndAuthenticationPage;
        public GameStateCloudSavingPage GameStateCloudSavingPage;
        public AchievementsPage AchievementsPage;
        public UserGameplayDataPage UserGameplayDataPage;

        // After features
        public LogPage LogPage;

        private Dictionary<PageType, Page> PageTypeToPageInstanceMap => new Dictionary<PageType, Page>()
        {
            // Before features
            {PageType.EnvironmentAndCredentialsPage, EnvironmentAndCredentialsPage},
            {PageType.AllFeaturesPage, AllFeaturesPage},

            // Features
            {PageType.IdentityAndAuthenticationPage, IdentityAndAuthenticationPage},
            {PageType.GameStateCloudSavingPage, GameStateCloudSavingPage},
            {PageType.AchievementsPage, AchievementsPage},
            {PageType.UserGameplayDataPage, UserGameplayDataPage},

            // After features
            {PageType.LogPage, LogPage},
        };

        /// <summary>
        /// Create a default AllPages.
        /// </summary>
        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            // Before features
            EnvironmentAndCredentialsPage.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(EnvironmentAndCredentialsPage)));

            // Features
            IdentityAndAuthenticationPage.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(IdentityAndAuthenticationPage)));
            GameStateCloudSavingPage.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(GameStateCloudSavingPage)));
            AchievementsPage.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(AchievementsPage)));
            UserGameplayDataPage.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(UserGameplayDataPage)));

            // Must be intialized after features
            AllFeaturesPage.Initialize(dependencies);
            
            // After features
            LogPage.Initialize(serializedProperty.FindPropertyRelative(nameof(LogPage)));
        }

        /// <summary>
        /// Get the page instance corresponding to the specified page type.
        /// </summary>
        public Page GetPage(PageType pageType)
        {
            if (PageTypeToPageInstanceMap.TryGetValue(pageType, out Page page))
            {
                return page;
            }

            throw new ArgumentException($"No page instance found for PageType: {pageType}. This error should not be possible at runtime. It should be caught by unit tests.");
        }
    }
}
