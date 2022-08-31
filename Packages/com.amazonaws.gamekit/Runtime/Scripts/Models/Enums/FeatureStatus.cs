// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Runtime.Models
{
    public enum FeatureStatus
    {
        [FeatureStatusData(displayName: "Deployed")]
        Deployed,

        [FeatureStatusData(displayName: "Undeployed")]
        Undeployed,

        [FeatureStatusData(displayName: "Error")]
        Error,

        [FeatureStatusData(displayName: "Rollback Complete")]
        RollbackComplete,

        [FeatureStatusData(displayName: "Running")]
        Running,

        [FeatureStatusData(displayName: "Generating Templates")]
        GeneratingTemplates,

        [FeatureStatusData(displayName: "Uploading Dashboards")]
        UploadingDashboards,

        [FeatureStatusData(displayName: "Uploading Layers")]
        UploadingLayers,

        [FeatureStatusData(displayName: "Uploading Functions")]
        UploadingFunctions,

        [FeatureStatusData(displayName: "Deploying Resources")]
        DeployingResources,

        [FeatureStatusData(displayName: "Deleting Resources")]
        DeletingResources,

        [FeatureStatusData(displayName: "Unknown")]
        Unknown,
    };

    [AttributeUsage(AttributeTargets.Field)]
    public class FeatureStatusData : Attribute
    {
        public readonly string DisplayName;

        public FeatureStatusData(string displayName)
        {
            DisplayName = displayName;
        }
    }

    /// <summary>
    /// Extension methods for <c>FeatureStatus</c> which give access to it's enum metadata.
    /// </summary>
    /// <example>
    /// This shows how to use the extension methods.
    /// <code>
    ///     // On the enum class:
    ///     FeatureStatus.Undeployed.GetDisplayName();
    ///
    ///     // On an enum variable:
    ///     FeatureStatus myStatus = FeatureStatus.Deployed
    ///     myStatus.GetDisplayName();
    /// </code>
    /// </example>
    public static class FeatureStatusConverter
    {
        public static string GetDisplayName(this FeatureStatus status)
        {
            return status.GetAttribute<FeatureStatusData>().DisplayName;
        }
    }

    public enum FeatureStatusSummary
    {
        Deployed = 0,
        Undeployed,
        Error,
        Running,
        Unknown
    };
}