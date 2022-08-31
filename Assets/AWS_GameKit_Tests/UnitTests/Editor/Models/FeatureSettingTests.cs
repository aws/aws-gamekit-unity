// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Runtime.UnitTests;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public abstract class FeatureSettingTests<T> : GameKitTestBase
    {
        [Test]
        // Do not add [TestCase()] to this base class method.
        // All test cases should be added to child classes.
        public virtual void Constructor_WhenArgumentsAreNonNull_SetsCurrentValueToDefaultValue(T expectedDefaultValue)
        {
            // act
            FeatureSetting<T> featureSetting = CreateFeatureSetting("myVariableName", expectedDefaultValue);

            // assert
            Assert.AreEqual(expectedDefaultValue, featureSetting.DefaultValue);
            Assert.AreEqual(featureSetting.DefaultValue, featureSetting.CurrentValue);
            Assert.AreEqual(featureSetting.DefaultValueString, featureSetting.CurrentValueString);
        }

        [Test]
        // Do not add [TestCase()] to this base class method.
        // All test cases should be added to child classes.
        public virtual void Constructor_WhenArgumentsAreNull_ThrowsException(string variableName, T defaultValue)
        {
            // act / assert
            Assert.Throws<ArgumentNullException>(
                () => CreateFeatureSetting(variableName, defaultValue));
        }

        [Test]
        // Do not add [TestCase()] to this base class method.
        // All test cases should be added to child classes.
        public virtual void SetCurrentValueFromString_WhenStringHasValidFormat_SuccessfullySetsCurrentValue(string newValueString, T expectedCurrentValue)
        {
            // arrange
            FeatureSetting<T> featureSetting = CreateDefaultFeatureSetting();

            // act
            featureSetting.SetCurrentValueFromString(newValueString);

            // assert
            Assert.AreEqual(expectedCurrentValue, featureSetting.CurrentValue);
        }

        [Test]
        // Do not add [TestCase()] to this base class method.
        // All test cases should be added to child classes.
        public virtual void SetCurrentValueFromString_WhenStringHasInvalidFormat_ThrowsException(string newValueString, Type exceptionType)
        {
            // arrange
            FeatureSetting<T> featureSetting = CreateDefaultFeatureSetting();

            // act / assert
            Assert.Throws(exceptionType,
                () => featureSetting.SetCurrentValueFromString(newValueString));
        }

        [Test]
        public void SetCurrentValueFromString_WhenStringIsNull_ThrowsException()
        {
            // arrange
            FeatureSetting<T> featureSetting = CreateDefaultFeatureSetting();
            string nullString = null;

            // act / assert
            Assert.Throws<ArgumentNullException>(
                () => featureSetting.SetCurrentValueFromString(nullString));
        }

        [Test]
        // Do not add [TestCase()] to this base class method.
        // All test cases should be added to child classes.
        public virtual void DefaultAndCurrentValueString_AfterConstructor_AreCorrectlyFormatted(T defaultValue, string expectedDefaultValueString)
        {
            // This tests the protected method "ValueToString()"

            // arrange
            FeatureSetting<T> featureSetting = CreateFeatureSetting("myVariableName", defaultValue);

            // act / assert
            Assert.AreEqual(expectedDefaultValueString, featureSetting.DefaultValueString);
            Assert.AreEqual(expectedDefaultValueString, featureSetting.CurrentValueString);
        }

        [Test]
        // Do not add [TestCase()] to this base class method.
        // All test cases should be added to child classes.
        public virtual void CurrentValuePropertySetter_WhenCalled_CurrentValueStringIsCorrectlyFormatted(T defaultValue, T newValue, string expectedCurrentValueString)
        {
            // arrange
            FeatureSetting<T> featureSetting = CreateFeatureSetting("myVariableName", defaultValue);

            // act
            featureSetting.CurrentValue = newValue;

            // assert
            Assert.AreEqual(expectedCurrentValueString, featureSetting.CurrentValueString);
        }

        protected abstract FeatureSetting<T> CreateFeatureSetting(string variableName, T defaultValue);
        protected abstract FeatureSetting<T> CreateDefaultFeatureSetting();
    }
}
