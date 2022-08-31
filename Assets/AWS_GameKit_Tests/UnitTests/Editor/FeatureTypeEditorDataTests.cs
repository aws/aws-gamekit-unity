// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Runtime.UnitTests;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class FeatureTypeConverterTests : GameKitTestBase
    {
        [Test]
        public void GetEditorData_AllFeatures_HaveEditorData()
        {
            // This test ensures every FeatureType has a FeatureTypeEditorData.
            // FeatureType.GetEditorData() is private, so we test through a proxy of GetDescription() instead.

            foreach (FeatureType feature in Enum.GetValues(typeof(FeatureType)))
            {
                Assert.DoesNotThrow(() => feature.GetDescription());
            }
        }

        [Test]
        [TestCase(FeatureType.Main, "Main", "main")]
        [TestCase(FeatureType.Identity, "Identity & Authentication", "identity")]
        [TestCase(FeatureType.Authentication, "Authentication", "authentication")]
        [TestCase(FeatureType.Achievements, "Achievements", "achievements")]
        [TestCase(FeatureType.GameStateCloudSaving, "Game State Cloud Saving", "gamesaving")]
        [TestCase(FeatureType.UserGameplayData, "User Gameplay Data", "usergamedata")]
        public void GetUIStringAndToApiString_AllFeatures_ContainCorrectValues(FeatureType feature, string expectedUIString, string expectedApiString)
        {
            Assert.AreEqual(feature.GetDisplayName(), expectedUIString);
            Assert.AreEqual(feature.GetApiString(), expectedApiString);
        }

        [Test]
        public void GetDocumentationUrlAndToResourcesUIString_AllFeatures_ContainNonEmptyValues()
        {
            // Since the urls and resources may change over time, we want only want to ensure they are not empty

            foreach (FeatureType feature in Enum.GetValues(typeof(FeatureType)))
            {
                Assert.True(feature.GetDocumentationUrl().Length > 0);
                Assert.True(feature.GetResourcesUIString().Length > 0);
            }
        }

        [Test]
        public void GetDescription_WithEndingPeriod_ContainsSingleEndingPeriod()
        {
            foreach (FeatureType feature in Enum.GetValues(typeof(FeatureType)))
            {
                // act
                string description = feature.GetDescription(withEndingPeriod: true);

                // assert
                Assert.IsTrue(description.EndsWith("."));
                Assert.IsTrue(!description.EndsWith(".."));
            }
        }

        [Test]
        public void GetDescription_DefaultNoEndingPeriod_ContainsNoEndingPeriod()
        {
            foreach (FeatureType feature in Enum.GetValues(typeof(FeatureType)))
            {
                // act
                string description = feature.GetDescription(); // default is no ending period

                // assert
                Assert.IsTrue(!description.EndsWith("."));
            }
        }

        [Test]
        public void GetDashboardUrl_UserGameplayData_ContainsCorrectValue()
        {
            // This test ensures the dashboard URLs are formatted correctly.
            // It tests User Gameplay Data because it is a feature name that has spaces in it.

            // arrange
            FeatureType featureType = FeatureType.UserGameplayData;
            string gameName = "testgame";
            string environmentCode = "dev";
            string region = "us-west-2";
            string expectedUrl = "https://console.aws.amazon.com/cloudwatch/home?region=us-west-2#dashboards:name=GameKit-testgame-dev-us-west-2-UserGameplayData";

            // act
            string actualUrl = featureType.GetDashboardUrl(gameName, environmentCode, region);

            // assert
            Assert.AreEqual(expectedUrl, actualUrl);
        }
    }
}
