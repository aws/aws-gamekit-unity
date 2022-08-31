// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine.Events;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Editor.AchievementsAdmin;
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Features.GameKitAchievements;
using AWS.GameKit.Runtime.Features.GameKitGameSaving;
using AWS.GameKit.Runtime.Features.GameKitIdentity;
using AWS.GameKit.Runtime.Features.GameKitUserGameplayData;

namespace AWS.GameKit.Editor.Windows.Settings
{
    public struct SettingsDependencyContainer
    {
        // Editor
        public GameKitEditorManager GameKitEditorManager;
        public CredentialsManager CredentialsManager;

        // Runtime
        public GameKitManager GameKitManager;
        public ICoreWrapperProvider CoreWrapper;
        public IFileManager FileManager;

        // Feature management
        public FeatureResourceManager FeatureResourceManager;
        public FeatureDeploymentOrchestrator FeatureDeploymentOrchestrator;

        // Features
        public IAchievementsProvider Achievements;
        public IAchievementsAdminProvider AchievementsAdmin;
        public IGameSavingProvider GameSaving;
        public IIdentityProvider Identity;
        public IUserGameplayDataProvider UserGameplayData;

        // User State
        public UserInfo UserInfo;

        // Events
        public UnityEvent OnEnvironmentOrRegionChange;
    }
}
