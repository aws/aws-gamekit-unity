// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Runtime.Models
{
    public enum DeploymentActionBlockedReason : uint
    {
        NotBlocked = 0, 
        FeatureMustBeCreated, 
        FeatureMustBeDeleted,
        FeatureStatusIsUnknown,
        OngoingDeployments,
        DependenciesMustBeCreated,
        DependenciesMustBeDeleted,
        DependenciesStatusIsInvalid,
        CredentialsInvalid,
        MainStackNotReady
    }
}
