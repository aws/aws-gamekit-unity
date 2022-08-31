// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Editor.Windows.Settings;

namespace AWS.GameKit.Editor.Models
{
    /// <summary>
    /// Editor metadata associated with a FeatureType enum.
    /// </summary>
    public class FeatureTypeEditorData
    {
        /// <summary>
        /// All of the features which are displayed in the UI, in the order they should be displayed.
        /// </summary>
        public static readonly IEnumerable<FeatureType> FeaturesToDisplay = new List<FeatureType>()
        {
            FeatureType.Identity,
            FeatureType.GameStateCloudSaving,
            FeatureType.Achievements,
            FeatureType.UserGameplayData,
        };

        public readonly string DisplayName;
        public readonly string ApiString;
        public readonly string Description;
        public readonly string VerboseDescription;
        public readonly string DocumentationUrl;
        public readonly string ResourcesUIString;
        public readonly IGettable<Texture> Icon;
        public readonly PageType PageType;

        public FeatureTypeEditorData(string displayName, string apiString, string description, string verboseDescription, string documentationUrl, string resourceUIString, IGettable<Texture> icon, PageType pageType)
        {
            DisplayName = displayName;
            ApiString = apiString;
            Description = description;
            VerboseDescription = verboseDescription;
            DocumentationUrl = documentationUrl;
            ResourcesUIString = resourceUIString;
            Icon = icon;
            PageType = pageType;
        }
    }

    /// <summary>
    /// Extension methods for the FeatureType enum which provide access to each enum's FeatureTypeEditorData.
    /// </summary>
    public static class FeatureTypeConverter
    {
        public static string GetDisplayName(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.DisplayName;
        }

        public static string GetApiString(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.ApiString;
        }

        public static string GetDescription(this FeatureType featureType, bool withEndingPeriod = false)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return withEndingPeriod ? editorData.Description + "." : editorData.Description;
        }

        public static string GetVerboseDescription(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.VerboseDescription;
        }

        public static string GetDocumentationUrl(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.DocumentationUrl;
        }

        public static string GetResourcesUIString(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.ResourcesUIString;
        }

        public static Texture GetIcon(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.Icon.Get();
        }

        public static PageType GetPageType(this FeatureType featureType)
        {
            FeatureTypeEditorData editorData = featureType.GetEditorData();
            return editorData.PageType;
        }

        /// <summary>
        /// Get the feature's dashboard URL.
        /// </summary>
        /// <param name="featureType">The feature to get the dashboard for.</param>
        /// <param name="gameName">The game's alias.</param>
        /// <param name="environmentCode">The short environment code where the feature and dashboard are deployed. Example: "dev", "qa".</param>
        /// <param name="region">The AWS region where the feature and dashboard are deployed. Example: "us-west-2"</param>
        /// <returns></returns>
        public static string GetDashboardUrl(this FeatureType featureType, string gameName, string environmentCode, string region)
        {
            string featureName = StringHelper.RemoveWhitespace(featureType.GetDisplayName())
                .Replace("&", "And");

            return $"https://console.aws.amazon.com/cloudwatch/home?region={region}#dashboards:name=GameKit-{gameName}-{environmentCode}-{region}-{featureName}";
        }

        private static FeatureTypeEditorData GetEditorData(this FeatureType featureType)
        {
            if (_featureTypeToEditorData.TryGetValue(featureType, out FeatureTypeEditorData editorData))
            {
                return editorData;
            }

            throw new ArgumentException($"No FeatureTypeEditorData found for FeatureType: {featureType}. This error should not be possible at runtime. It should be caught by unit tests.");
        }

        private static readonly ReadOnlyDictionary<FeatureType, FeatureTypeEditorData> _featureTypeToEditorData =
            new ReadOnlyDictionary<FeatureType, FeatureTypeEditorData>(
                new Dictionary<FeatureType, FeatureTypeEditorData>()
                {
                    {
                        FeatureType.Main,
                        new FeatureTypeEditorData(
                            displayName: "Main",
                            apiString: "main",
                            description: L10n.Tr("The Main feature does not have a description"),
                            verboseDescription: L10n.Tr("The Main feature does not have a verbose description"),
                            documentationUrl: DocumentationURLs.GAMEKIT_HOME,
                            resourceUIString: "The Main feature does not have a resources UI String. ",
                            icon: EditorResources.Textures.Colors.Transparent,
                            pageType: PageType.EnvironmentAndCredentialsPage)
                    },
                    {
                        FeatureType.Identity,
                        new FeatureTypeEditorData(
                            displayName: "Identity & Authentication",
                            apiString: "identity",
                            description: L10n.Tr("Create unique identities for each player and allow players to sign into your game. Verify player identities and manage player sessions"),
                            verboseDescription: L10n.Tr("Sign players into your game to create player IDs, authenticate players to prevent cheating and fraud."),
                            documentationUrl: DocumentationURLs.IDENTITY,
                            resourceUIString: "API Gateway, CloudWatch, Cognito, DynamoDB, IAM, Key Management Service, and Lambda. ",
                            icon: EditorResources.Textures.QuickAccessWindow.FeatureIconIdentity,
                            pageType: PageType.IdentityAndAuthenticationPage)
                    },
                    {
                        FeatureType.Authentication,
                        new FeatureTypeEditorData(
                            displayName: "Authentication",
                            apiString: "authentication",
                            description: L10n.Tr("The Authentication feature does not have a description"),
                            verboseDescription: L10n.Tr("The Authentication feature does not have a verbose description"),
                            documentationUrl: DocumentationURLs.IDENTITY,
                            resourceUIString: "The Authentication feature does not have a resources UI String. ",
                            icon: EditorResources.Textures.Colors.Transparent,
                            pageType: PageType.IdentityAndAuthenticationPage)
                    },
                    {
                        FeatureType.Achievements,
                        new FeatureTypeEditorData(
                            displayName: "Achievements",
                            apiString: "achievements",
                            description: L10n.Tr("Track and display game-related rewards earned by players"),
                            verboseDescription: L10n.Tr("Add an achievement system where players can earn awards for their gameplay prowess."),
                            documentationUrl: DocumentationURLs.ACHIEVEMENTS,
                            resourceUIString: "API Gateway, CloudFront, CloudWatch, Cognito, DynamoDB, Lambda, S3, and Security Token Service. ",
                            icon: EditorResources.Textures.QuickAccessWindow.FeatureIconAchievements,
                            pageType: PageType.AchievementsPage)
                    },
                    {
                        FeatureType.GameStateCloudSaving,
                        new FeatureTypeEditorData(
                            displayName: "Game State Cloud Saving",
                            apiString: "gamesaving",
                            description: L10n.Tr("Maintain a synchronized copy of player game progress in the cloud to allow players to resume gameplay across sessions"),
                            verboseDescription: L10n.Tr("Synchronize game saves in the cloud to let players seamlessly resume gameplay across sessions and multiple devices."),
                            documentationUrl: DocumentationURLs.GAME_STATE_SAVING,
                            resourceUIString: "API Gateway, CloudWatch, Cognito, DynamoDB, Lambda, and S3. ",
                            icon: EditorResources.Textures.QuickAccessWindow.FeatureIconGameStateCloudSaving,
                            pageType: PageType.GameStateCloudSavingPage)
                    },
                    {
                        FeatureType.UserGameplayData,
                        new FeatureTypeEditorData(
                            displayName: "User Gameplay Data",
                            apiString: "usergamedata",
                            description: L10n.Tr("Maintain game-related data for each player, such as inventory, statistics, cross-play persistence, etc"),
                            verboseDescription: L10n.Tr("Maintain player game data in the cloud, available when and where the player signs into the game."),
                            documentationUrl: DocumentationURLs.USER_GAMEPLAY_DATA,
                            resourceUIString: "API Gateway, CloudWatch, Cognito, DynamoDB, and Lambda. ",
                            icon: EditorResources.Textures.QuickAccessWindow.FeatureIconUserGameplayData,
                            pageType: PageType.UserGameplayDataPage)
                    },
                }
            );
    }
}
