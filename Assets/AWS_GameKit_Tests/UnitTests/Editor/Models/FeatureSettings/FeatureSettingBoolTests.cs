// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Models.FeatureSettings;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class FeatureSettingBoolTests : FeatureSettingTests<bool>
    {
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public override void Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(bool expectedDefaultValue)
        {
            // Pass parameters to base test:
            base.Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(expectedDefaultValue);
        }

        [Test]
        [TestCase(null, true)]
        // Note: we can't test for "defaultValue: null" because a null boolean gets automatically converted to "false".
        public override void Constructor_WhenArgumentsAreNull_ThrowsException(string variableName, bool defaultValue)
        {
            base.Constructor_WhenArgumentsAreNull_ThrowsException(variableName, defaultValue);
        }

        [Test]
        // These strings are in the format that is written to disk by this AWS GameKit Unity package:
        [TestCase("true", true)]
        [TestCase("false", false)]
        // These other strings are in formats that might be written by a user who is modifying the saveInfo.yml file by hand (not recommended!)
        [TestCase("true ", true)]
        [TestCase(" true", true)]
        [TestCase("True", true)]
        [TestCase("TRUE", true)]
        [TestCase("False", false)]
        [TestCase("FALSE", false)]
        public override void SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(string newValueString, bool expectedCurrentValue)
        {
            base.SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(newValueString, expectedCurrentValue);
        }

        [Test]
        // Incorrectly formatted string:
        [TestCase("not_a_boolean", typeof(FormatException))]
        [TestCase("tr ue", typeof(FormatException))]
        // Numbers that might be used as boolean:
        [TestCase("1", typeof(FormatException))]
        [TestCase("0", typeof(FormatException))]
        // Null
        [TestCase(null, typeof(ArgumentNullException))]
        [TestCase("null", typeof(FormatException))]
        [TestCase("Null", typeof(FormatException))]
        [TestCase("NULL", typeof(FormatException))]
        // Whitespace
        [TestCase("", typeof(FormatException))]
        [TestCase(" ", typeof(FormatException))]
        public override void SetCurrentValueFromString_WhenStringHasInvalidFormat_ThrowsException(string newValueString, Type exceptionType)
        {
            base.SetCurrentValueFromString_WhenStringHasInvalidFormat_ThrowsException(newValueString, exceptionType);
        }

        [Test]
        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public override void DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(bool defaultValue, string expectedDefaultValueString)
        {
            base.DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(defaultValue, expectedDefaultValueString);
        }

        [Test]
        // New value different than default:
        [TestCase(true, false, "false")]
        [TestCase(false, true, "true")]
        // New value same as default:
        [TestCase(true, true, "true")]
        [TestCase(false, false, "false")]
        public override void CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(bool defaultValue, bool newValue, string expectedCurrentValueString)
        {
            base.CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(defaultValue, newValue, expectedCurrentValueString);
        }

        protected override FeatureSetting<bool> CreateFeatureSetting(string variableName, bool defaultValue)
        {
            return new FeatureSettingBool(variableName, defaultValue);
        }

        protected override FeatureSetting<bool> CreateDefaultFeatureSetting()
        {
            return new FeatureSettingBool("myVariableName", defaultValue: false);
        }
    }
}