// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Third Party
using NUnit.Framework;

// Game Kit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class AwsGameKitPersistentSettingsTests
    {
        private const string UNIT_TEST_KEY = "UnitTestingEntry";
        private const string UNIT_TEST_VALUE = "GameKit";
        private const string UNIT_TEST_VALUE_DEFAULT = "Default";

        [TearDown]
        public void TearDown()
        {
            AwsGameKitPersistentSettings.Delete(UNIT_TEST_KEY);
        }

        [Test]
        public void SaveString_WhenCalledWithAnExistingKey_SavesTheNewValue()
        {
            // arrange
            AwsGameKitPersistentSettings.SaveString(UNIT_TEST_KEY, UNIT_TEST_VALUE);
            const string NEW_TEST_VALUE = "Aws";

            // act
            AwsGameKitPersistentSettings.SaveString(UNIT_TEST_KEY, NEW_TEST_VALUE);
            string valueReturned = AwsGameKitPersistentSettings.LoadString(UNIT_TEST_KEY, UNIT_TEST_VALUE_DEFAULT);

            // assert
            Assert.AreEqual(NEW_TEST_VALUE, valueReturned);
        }

        [Test]
        public void LoadString_WhenCalledWithAnExistingKey_ReturnsTheExpectedValue()
        {
            // arrange
            AwsGameKitPersistentSettings.SaveString(UNIT_TEST_KEY, UNIT_TEST_VALUE);

            // act
            string valueReturned = AwsGameKitPersistentSettings.LoadString(UNIT_TEST_KEY, UNIT_TEST_VALUE_DEFAULT);

            // assert
            Assert.AreEqual(UNIT_TEST_VALUE, valueReturned);
        }

        [Test]
        public void LoadString_WhenCalledWithANonExistingKey_ReturnsTheDefaultValue()
        {
            // act
            string valueReturned = AwsGameKitPersistentSettings.LoadString(UNIT_TEST_KEY, UNIT_TEST_VALUE_DEFAULT);

            // assert
            Assert.AreEqual(UNIT_TEST_VALUE_DEFAULT, valueReturned);
        }

        [Test]
        public void Delete_WhenCalledWithANonExistingKey_FailsQuietly()
        {
            // act 
            AwsGameKitPersistentSettings.Delete("fake key");
        }

        [Test]
        public void Delete_WhenCalledWithAnExistingKey_RemovesTheKeyValuePair()
        {
            // arrange
            AwsGameKitPersistentSettings.SaveString(UNIT_TEST_KEY, UNIT_TEST_VALUE);

            // act 
            AwsGameKitPersistentSettings.Delete(UNIT_TEST_KEY);
            string valueReturned = AwsGameKitPersistentSettings.LoadString(UNIT_TEST_KEY, UNIT_TEST_VALUE_DEFAULT);

            // assert
            Assert.AreEqual(UNIT_TEST_VALUE_DEFAULT, valueReturned);
        }
    }
}
