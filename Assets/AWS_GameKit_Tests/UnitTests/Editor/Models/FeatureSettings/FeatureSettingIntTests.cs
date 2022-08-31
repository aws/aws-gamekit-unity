// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Models.FeatureSettings;

// Third Party
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

namespace AWS.GameKit.Editor.UnitTests
{
    public class FeatureSettingIntTests : FeatureSettingTests<int>
    {
        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public override void Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(int expectedDefaultValue)
        {
            // Pass parameters to base test:
            base.Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(expectedDefaultValue);
        }

        [Test]
        [TestCase(null, 1)]
        // Note: we can't test for "defaultValue: null" because a null int gets automatically converted to "0".
        public override void Constructor_WhenArgumentsAreNull_ThrowsException(string variableName, int defaultValue)
        {
            base.Constructor_WhenArgumentsAreNull_ThrowsException(variableName, defaultValue);
        }

        [Test]
        // These strings are in the format that is written to disk by this AWS GameKit Unity package:
        [TestCase("0", 0)]
        [TestCase("1", 1)]
        [TestCase("-1", -1)]
        // These other strings are in formats that might be written by a user who is modifying the saveInfo.yml file by hand (not recommended!)
        [TestCase(" 0", 0)]
        [TestCase("0 ", 0)]
        public override void SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(string newValueString, int expectedCurrentValue)
        {
            base.SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(newValueString, expectedCurrentValue);
        }

        // [Test]
        // Incorrectly formatted numbers:
        [TestCase("not_an_int", typeof(FormatException))]
        [TestCase("1 0", typeof(FormatException))]
        [TestCase("1,000", typeof(FormatException))]
        // Overflow
        [TestCase("2147483648", typeof(OverflowException))]
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
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(-1, "-1")]
        public override void DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(int defaultValue, string expectedDefaultValueString)
        {
            base.DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(defaultValue, expectedDefaultValueString);
        }

        [Test]
        // New value different than default:
        [TestCase(0, 1, "1")]
        // New value same as default:
        [TestCase(0, 0, "0")]
        public override void CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(int defaultValue, int newValue, string expectedCurrentValueString)
        {
            base.CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(defaultValue, newValue, expectedCurrentValueString);
        }

        protected override FeatureSetting<int> CreateFeatureSetting(string variableName, int defaultValue)
        {
            return new FeatureSettingInt(variableName, defaultValue);
        }

        protected override FeatureSetting<int> CreateDefaultFeatureSetting()
        {
            return CreateFeatureSetting("myVariableName", defaultValue: 0);
        }
    }
}