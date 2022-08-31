// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.IO;

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Editor.Utils;

namespace AWS.GameKit.Editor.FileStructure
{
    /// <summary>
    /// Gives access to the assets stored in the "com.amazonaws.gamekit/Editor/Resources" folder.
    /// </summary>
    public static class EditorResources
    {
        public static class Textures
        {
            private const string TEXTURES = "Textures/";

            public static readonly IGettable<Texture> WindowIcon = new LazyLoadedEditorThemeAwareResource<Texture>(
                new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "WindowIcon-Dark.png").Replace("\\","/")),
                new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "WindowIcon-Light.png").Replace("\\","/")));

            public static readonly IGettable<Texture> FeatureStatusSuccess = new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "FeatureStatus-Success.png").Replace("\\","/"));
            public static readonly IGettable<Texture> FeatureStatusError = new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "FeatureStatus-Error.png").Replace("\\","/"));
            public static readonly IGettable<Texture> FeatureStatusWorking = new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "FeatureStatus-Working.png").Replace("\\","/"));

            public static readonly IGettable<Texture> FeatureStatusRefresh =
                new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "FeatureStatus-Refresh.png").Replace("\\","/"));

            public static readonly IGettable<Texture> FeatureStatusWaiting =
                new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "FeatureStatus-Waiting.png").Replace("\\","/"));

            public static readonly IGettable<Texture> Unsynchronized = new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "unsynchronized.png").Replace("\\","/"));

            public static class Colors
            {
                private const string COLORS = TEXTURES + "Colors/";

                public static readonly IGettable<Texture> Transparent = new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, TEXTURES, "Transparent.png").Replace("\\","/"));

                public static readonly IGettable<Texture2D> GUILayoutDivider = new LazyLoadedEditorThemeAwareResource<Texture2D>(
                    Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, COLORS, "GUILayoutDivider-Dark.png").Replace("\\","/"),
                    Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, COLORS, "GUILayoutDivider-Light.png").Replace("\\","/"));
            }

            public static class SettingsWindow
            {
                private const string SETTINGS_TEXTURES = TEXTURES + "SettingsWindow/";

                public static readonly IGettable<Texture> ExternalLinkIcon = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, SETTINGS_TEXTURES, "ExternalLink-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, SETTINGS_TEXTURES, "ExternalLink-Light.png").Replace("\\","/")));

                public static readonly IGettable<Texture> PlusIcon = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, SETTINGS_TEXTURES, "PlusIcon-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, SETTINGS_TEXTURES, "PlusIcon-Light.png").Replace("\\","/")));

                public static readonly IGettable<Texture> MinusIcon = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, SETTINGS_TEXTURES, "MinusIcon-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, SETTINGS_TEXTURES, "MinusIcon-Light.png").Replace("\\","/")));
            }

            public static class QuickAccessWindow
            {
                private const string QUICK_ACCESS_TEXTURES = TEXTURES + "QuickAccessWindow/";
                private const string QUICK_ACCESS_COLORS = QUICK_ACCESS_TEXTURES + "Colors/";

                // Feature Icons:
                public static readonly IGettable<Texture> FeatureIconAchievements = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-Achievements-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-Achievements-Light.png").Replace("\\","/")));

                public static readonly IGettable<Texture> FeatureIconGameStateCloudSaving = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-GameStateCloudSaving-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-GameStateCloudSaving-Light.png").Replace("\\","/")));

                public static readonly IGettable<Texture> FeatureIconIdentity = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-Identity-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-Identity-Light.png").Replace("\\","/")));

                public static readonly IGettable<Texture> FeatureIconUserGameplayData = new LazyLoadedEditorThemeAwareResource<Texture>(
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-UserGameplayData-Dark.png").Replace("\\","/")),
                    new LazyLoadedResource<Texture>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_TEXTURES, "FeatureIcon-UserGameplayData-Light.png").Replace("\\","/")));

                public static class Colors
                {
                    public static readonly IGettable<Texture2D> QuickAccessBackground = new LazyLoadedEditorThemeAwareResource<Texture2D>(
                        new LazyLoadedResource<Texture2D>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_COLORS, "QuickAccessWindow-Background-Dark.png").Replace("\\","/")),
                        new LazyLoadedResource<Texture2D>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_COLORS, "QuickAccessWindow-Background-Light.png").Replace("\\","/")));

                    public static readonly IGettable<Texture2D> QuickAccessButtonNormal = new LazyLoadedEditorThemeAwareResource<Texture2D>(
                        new LazyLoadedResource<Texture2D>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_COLORS, "QuickAccessWindow-ButtonNormal-Dark.png").Replace("\\","/")),
                        new LazyLoadedResource<Texture2D>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_COLORS, "QuickAccessWindow-ButtonNormal-Light.png").Replace("\\","/")));

                    public static readonly IGettable<Texture2D> QuickAccessButtonHover = new LazyLoadedEditorThemeAwareResource<Texture2D>(
                        new LazyLoadedResource<Texture2D>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_COLORS, "QuickAccessWindow-ButtonHover-Dark.png").Replace("\\","/")),
                        new LazyLoadedResource<Texture2D>(Path.Combine(GameKitPaths.Get().PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, QUICK_ACCESS_COLORS, "QuickAccessWindow-ButtonHover-Light.png").Replace("\\","/")));
                }
            }
        }
    }
}
