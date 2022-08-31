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
    public class FeatureSettingStringTests : FeatureSettingTests<string>
    {
        private const string ENGLISH_ALPHABET = "abcdefghijklmnopqrstuvwxyz";
        private const string NUMBERS = "0123456789";
        private const string SYMBOLS = ",!@#$%^&*()_+";
        private const string WHITESPACE = " ";
        private const string INTERNATIONAL_CHARACTERS = "您好, nǐn hǎo, أهلا, Привет, Χαίρετε";
        private const string ALL_TYPES_COMBINED = "abcdefghijklmnopqrstuvwxyz 0123456789 !@#$%^&*()_+-=[]\\{}| 您好 ";

        // This string is longer than any feature setting we ask for in the UI. This is for a sanity checking the string doesn't get truncated.
        private const string VERY_LONG_STRING = "012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";

        [Test]
        [TestCase(ENGLISH_ALPHABET)]
        [TestCase(NUMBERS)]
        [TestCase(SYMBOLS)]
        [TestCase(WHITESPACE)]
        [TestCase(INTERNATIONAL_CHARACTERS)]
        [TestCase(ALL_TYPES_COMBINED)]
        [TestCase(VERY_LONG_STRING)]
        public override void Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(string expectedDefaultValue)
        {
            // Pass parameters to base test:
            base.Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(expectedDefaultValue);
        }

        [Test]
        [TestCase(null, "not null")]
        [TestCase("myVariableName", null)]
        [TestCase(null, null)]
        public override void Constructor_WhenArgumentsAreNull_ThrowsException(string variableName, string defaultValue)
        {
            base.Constructor_WhenArgumentsAreNull_ThrowsException(variableName, defaultValue);
        }

        [Test]
        [TestCase(ENGLISH_ALPHABET, ENGLISH_ALPHABET)]
        [TestCase(NUMBERS, NUMBERS)]
        [TestCase(SYMBOLS, SYMBOLS)]
        [TestCase(WHITESPACE, WHITESPACE)]
        [TestCase(INTERNATIONAL_CHARACTERS, INTERNATIONAL_CHARACTERS)]
        [TestCase(ALL_TYPES_COMBINED, ALL_TYPES_COMBINED)]
        [TestCase(VERY_LONG_STRING, VERY_LONG_STRING)]
        public override void SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(string newValueString, string expectedCurrentValue)
        {
            base.SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(newValueString, expectedCurrentValue);
        }

        // This test is hidden from the test runner because an exception will never be thrown by FeatureSettingString.SetCurrentValueFromString().
        // It is hidden by commenting out the [Test] attribute & overriding the base class method.
        // This is preferred over using [Ignore("explanation")], because [Ignore()] is for temporary use cases and gets shown as a warning in the test runner.
        // [Test]
        public override void SetCurrentValueFromString_WhenStringHasInvalidFormat_ThrowsException(string newValueString, Type exceptionType)
        {
            base.SetCurrentValueFromString_WhenStringHasInvalidFormat_ThrowsException(newValueString, exceptionType);
        }

        [Test]
        [TestCase(ENGLISH_ALPHABET, ENGLISH_ALPHABET)]
        [TestCase(NUMBERS, NUMBERS)]
        [TestCase(SYMBOLS, SYMBOLS)]
        [TestCase(WHITESPACE, WHITESPACE)]
        [TestCase(INTERNATIONAL_CHARACTERS, INTERNATIONAL_CHARACTERS)]
        [TestCase(ALL_TYPES_COMBINED, ALL_TYPES_COMBINED)]
        [TestCase(VERY_LONG_STRING, VERY_LONG_STRING)]
        public override void DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(string defaultValue, string expectedDefaultValueString)
        {
            base.DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(defaultValue, expectedDefaultValueString);
        }

        [Test]
        // New value same as default:
        [TestCase("default", "default", "default")]
        // New value different than default:
        [TestCase("default", ENGLISH_ALPHABET, ENGLISH_ALPHABET)]
        [TestCase("default", NUMBERS, NUMBERS)]
        [TestCase("default", SYMBOLS, SYMBOLS)]
        [TestCase("default", WHITESPACE, WHITESPACE)]
        [TestCase("default", INTERNATIONAL_CHARACTERS, INTERNATIONAL_CHARACTERS)]
        [TestCase("default", ALL_TYPES_COMBINED, ALL_TYPES_COMBINED)]
        [TestCase("default", VERY_LONG_STRING, VERY_LONG_STRING)]
        public override void CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(string defaultValue, string newValue, string expectedCurrentValueString)
        {
            base.CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(defaultValue, newValue, expectedCurrentValueString);
        }

        protected override FeatureSetting<string> CreateFeatureSetting(string variableName, string defaultValue)
        {
            return new FeatureSettingString(variableName, defaultValue);
        }

        protected override FeatureSetting<string> CreateDefaultFeatureSetting()
        {
            return CreateFeatureSetting("myVariableName", defaultValue: "defaultValue");
        }
    }
}