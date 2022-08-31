// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.IO;

// Third Party
using NUnit.Framework;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class SerializableDictionaryTests
    {
        public static string TEST_ASSET_PATH = "Assets/AWS_GameKit_Tests/UnitTests/TemporaryFiles/serializableDictionaryTest.asset";

        [TearDown]
        public void TearDown()
        {
#if UNITY_EDITOR
            FileUtil.DeleteFileOrDirectory(TEST_ASSET_PATH);
            AssetDatabase.Refresh();
#endif
        }

        [Test]
        public void SerializableDictionary_WhenSettingNotExistentValue_CorrectlyAddsValueToDictionary()
        {
            // arrange
            const string testKey = "testKey";
            SerializableDictionary<string, Vector2> testSerializableDictionary = new SerializableDictionary<string, Vector2>();

            // act 
            testSerializableDictionary.SetValue(testKey, Vector2.up);

            // assert
            Assert.AreEqual(1, testSerializableDictionary.Count);
            Assert.AreEqual(Vector2.up, testSerializableDictionary.GetValue(testKey));
        }

        [Test]
        public void SerializableDictionary_GetNotExistentKey_CorrectlyAddsEmptyValue()
        {
            // arrange
            SerializableDictionary<int, float> testSerializableDictionary = new SerializableDictionary<int, float>();

            // act 
            testSerializableDictionary.GetValue(1);

            // assert
            Assert.AreEqual(0f, testSerializableDictionary.GetValue(1));
        }

        [Test]
        public void SerializableDictionary_SaveSerializedJSONToDiskThenReload_CorrectlyDeserializesDictionary()
        {
#if UNITY_EDITOR
            // arrange
            SerializableDictionary<string, Vector2> testSerializableDictionary = new SerializableDictionary<string, Vector2>();

            // act 
            testSerializableDictionary.SetValue("Hello", Vector2.up);
            testSerializableDictionary.SetValue("World", Vector2.down);

            string jsonString = JsonUtility.ToJson(testSerializableDictionary);
            File.WriteAllText(TEST_ASSET_PATH, jsonString);

            string fileContents = File.ReadAllText(TEST_ASSET_PATH);
            SerializableDictionary<string, Vector2> serializableDictionaryFromFile = JsonUtility.FromJson<SerializableDictionary<string, Vector2>>(fileContents);

            // assert
            Assert.AreEqual(2, serializableDictionaryFromFile.Count);
            Assert.True(serializableDictionaryFromFile.ContainsKey("Hello"));
            Assert.AreEqual(Vector2.up, serializableDictionaryFromFile["Hello"]);
            Assert.True(serializableDictionaryFromFile.ContainsKey("World"));
            Assert.AreEqual(Vector2.down, serializableDictionaryFromFile["World"]);
#endif
        }
    }
}
