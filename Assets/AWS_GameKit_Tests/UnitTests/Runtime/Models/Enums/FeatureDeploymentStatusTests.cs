// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Models;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class FeatureDeploymentStatusTests : GameKitTestBase
    {
        [Test]
        public void AllEnumValues_HaveFeatureDeploymentStatusData()
        {
            // This test ensures every FeatureDeploymentStatus enum is labeled with a FeatureDeploymentStatusData attribute.

            foreach (FeatureStatus deploymentStatus in Enum.GetValues(typeof(FeatureStatus)))
            {
                // act/assert
                Assert.DoesNotThrow(() => deploymentStatus.GetDisplayName(),
                    $"Expected enum {deploymentStatus} to have a DisplayName, but found none. " +
                    $"This means the enum is not labeled with a {nameof(FeatureStatusData)} attribute.");
            }
        }

        [Test]
        public void GetDisplayName_AllFeatures_HaveNonEmptyName()
        {
            foreach (FeatureStatus deploymentStatus in Enum.GetValues(typeof(FeatureStatus)))
            {
                // act
                string displayName = deploymentStatus.GetDisplayName();

                // assert
                Assert.False(String.IsNullOrWhiteSpace(displayName));
            }
        }
    }
}
