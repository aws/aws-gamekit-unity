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
    public class FeatureSettingFloatTests : FeatureSettingTests<float>
    {
        /// <summary>
        /// This string will cause an <c>OverflowException</c> when parsed to a <c>float</c>.<br/>
        /// This number is equal to <c>float.MaxValue</c> with one additional zero added.
        /// </summary>
        private const string OVERFLOWED_FLOAT_VALUE = "3402823470000000000000000000000000000000";

        [Test]
        [TestCase(-1.1f)]
        [TestCase(0.0f)]
        [TestCase(1.1f)]
        public override void Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(float expectedDefaultValue)
        {
            // Pass parameters to base test:
            base.Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(expectedDefaultValue);
        }

        [Test]
        [TestCase(null, 1f)]
        // Note: we can't test for "defaultValue: null" because a null float gets automatically converted to "0f".
        public override void Constructor_WhenArgumentsAreNull_ThrowsException(string variableName, float defaultValue)
        {
            base.Constructor_WhenArgumentsAreNull_ThrowsException(variableName, defaultValue);
        }

        [Test]
        // These strings are in the format that is written to disk by this AWS GameKit Unity package:
        [TestCase("0", 0f)]
        [TestCase("1", 1f)]
        [TestCase("1.1", 1.1f)]
        [TestCase("-1", -1f)]
        [TestCase("-1.1", -1.1f)]
        // These other strings are in formats that might be written by a user who is modifying the saveInfo.yml file by hand (not recommended!)
        [TestCase(" 0", 0f)]
        [TestCase("0 ", 0f)]
        [TestCase("0.0", 0f)]
        [TestCase("0.0000", 0f)]
        [TestCase("1.0", 1f)]
        [TestCase("-1.0", -1f)]
        public override void SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(string newValueString, float expectedCurrentValue)
        {
            base.SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(newValueString, expectedCurrentValue);
        }

        // [Test]
        // Incorrectly formatted numbers:
        [TestCase("not_a_float", typeof(FormatException))]
        [TestCase("1. 0", typeof(FormatException))]
        [TestCase("1f", typeof(FormatException))]
        [TestCase("1F", typeof(FormatException))]
        [TestCase("1.0f", typeof(FormatException))]
        // Overflow
        [TestCase(OVERFLOWED_FLOAT_VALUE, typeof(OverflowException))]
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
        [TestCase(0f, "0")]
        [TestCase(1f, "1")]
        [TestCase(1.1f, "1.1")]
        [TestCase(-1f, "-1")]
        [TestCase(-1.1f, "-1.1")]
        public override void DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(float defaultValue, string expectedDefaultValueString)
        {
            base.DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(defaultValue, expectedDefaultValueString);
        }

        [Test]
        // New value different than default:
        [TestCase(0f, 1f, "1")]
        // New value same as default:
        [TestCase(0f, 0f, "0")]
        public override void CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(float defaultValue, float newValue, string expectedCurrentValueString)
        {
            base.CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(defaultValue, newValue, expectedCurrentValueString);
        }

        protected override FeatureSetting<float> CreateFeatureSetting(string variableName, float defaultValue)
        {
            return new FeatureSettingFloat(variableName, defaultValue);
        }

        protected override FeatureSetting<float> CreateDefaultFeatureSetting()
        {
            return CreateFeatureSetting("myVariableName", defaultValue: 0f);
        }
    }
}