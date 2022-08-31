// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Common.Models
{
    /// <summary>
    /// This enum describes a type of GameKit feature.
    /// </summary>
    public enum FeatureType : uint
    {
        Main,
        Identity,
        Authentication,
        Achievements,
        GameStateCloudSaving,
        UserGameplayData
    }
}
