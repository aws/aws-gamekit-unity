// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard
using System;
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Common.Models;

namespace AWS.GameKit.Runtime.Models
{
    public class DeploymentResponseCallbackResult
    {
        public uint ResultCode;
        public FeatureType[] Features = new FeatureType[0];
        public FeatureStatus[] FeatureStatuses = new FeatureStatus[0];
    }

    public class DeploymentResponseResult
    {
        public uint ResultCode;
        public Dictionary<FeatureType, FeatureStatus> FeatureStatuses = new Dictionary<FeatureType, FeatureStatus>();
    }

    public class SetCredentialsDesc
    {
        public AccountInfo AccountInfo;
        public AccountCredentials AccountCredentials;
    }

    public class CanExecuteDeploymentActionResult
    {
        public FeatureType TargetFeature;
        public bool CanExecuteAction;
        public DeploymentActionBlockedReason Reason;
        public FeatureType[] BlockingFeatures = new FeatureType[0];
    }
}
