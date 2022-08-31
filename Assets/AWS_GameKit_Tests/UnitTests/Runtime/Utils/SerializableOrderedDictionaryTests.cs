// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

// Third Party
using NUnit.Framework;

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class SerializableOrderedDictionaryTests
    {
        [Test]
        public void SerializableOrderedDictionary_WhenSerializedToJsonAndBack_DeserializedDictionaryRemembersOriginalOrderOfElements(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            List<KeyValuePair<string, int>> originalElements = dict.KeyValuePairs.ToList();

            // act
            string serializedDict = JsonUtility.ToJson(dict);
            SerializableOrderedDictionary<string, int> deserializedDict = JsonUtility.FromJson<SerializableOrderedDictionary<string, int>>(serializedDict);

            // assert
            Assert.AreEqual(numStartingElements, deserializedDict.Count);
            for (int i = 0; i < numStartingElements; ++i)
            {
                originalElements[i] = deserializedDict.KeyValuePairs.ToList()[i];
            }
        }

        [Test]
        public void ContainsKey_WhenKeyExists_ReturnsTrue(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            foreach (string key in dict.Keys)
            {
                // act
                bool doesContainKey = dict.ContainsKey(key);

                // assert
                Assert.True(doesContainKey);
            }
            Assert.AreEqual(numStartingElements, dict.Count);
        }

        [Test]
        public void ContainsKey_WhenKeyNotExists_ReturnsFalse(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act
            bool doesContainKey = dict.ContainsKey("does not exist");

            // assert
            Assert.False(doesContainKey);
        }

        [Test]
        public void ContainsKey_WhenKeyIsNull_RaisesArgumentNullException(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act / assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                dict.ContainsKey(null);
            });
        }

        [Test]
        public void Add_WhenKeyNotAlreadyExists_NewElementIsAddedToEnd(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            string newKey = "newKey";
            int newValue = 10;

            // act
            dict.Add(newKey, newValue);

            // assert
            KeyValuePair<string, int> lastEntry = dict.KeyValuePairs.Last();
            Assert.AreEqual(newKey, lastEntry.Key);
            Assert.AreEqual(newValue, lastEntry.Value);
            Assert.AreEqual(numStartingElements + 1, dict.Count);
        }

        [Test]
        public void Add_WhenMultipleElementsAdded_NewElementsAreRememberedInSequentialOrder(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            string newKey1 = "newKey1";
            string newKey2 = "newKey2";
            string newKey3 = "newKey3";
            int newValue1 = 10;
            int newValue2 = 20;
            int newValue3 = 30;
            int newElement1Index = dict.Count + 0;
            int newElement2Index = dict.Count + 1;
            int newElement3Index = dict.Count + 2;

            // act
            dict.Add(newKey1, newValue1);
            dict.Add(newKey2, newValue2);
            dict.Add(newKey3, newValue3);

            // assert
            List<KeyValuePair<string, int>> elements = dict.KeyValuePairs.ToList();
            Assert.AreEqual(newKey1, elements[newElement1Index].Key);
            Assert.AreEqual(newKey2, elements[newElement2Index].Key);
            Assert.AreEqual(newKey3, elements[newElement3Index].Key);
            Assert.AreEqual(newValue1, elements[newElement1Index].Value);
            Assert.AreEqual(newValue2, elements[newElement2Index].Value);
            Assert.AreEqual(newValue3, elements[newElement3Index].Value);
            Assert.AreEqual(numStartingElements + 3, dict.Count);
        }

        [Test]
        public void Add_WhenKeyAlreadyExists_RaisesArgumentException(
            [Values(1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            KeyValuePair<string, int> firstEntry = dict.KeyValuePairs.First();

            // act / assert
            Assert.Throws<ArgumentException>(() =>
            {
                dict.Add(firstEntry.Key, 0);
            });
        }

        [Test]
        public void Add_WhenKeyIsNull_RaisesArgumentNullException(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act / assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                dict.Add(null, 0);
            });
        }

        [Test]
        public void SetValue_WhenKeyNotAlreadyExists_NewElementIsAddedToEnd(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            string newKey = "newKey";
            int newValue = 10;

            // act
            dict.SetValue(newKey, newValue);

            // assert
            KeyValuePair<string, int> lastEntry = dict.KeyValuePairs.Last();
            Assert.AreEqual(newKey, lastEntry.Key);
            Assert.AreEqual(newValue, lastEntry.Value);
            Assert.AreEqual(numStartingElements + 1, dict.Count);
        }

        [Test]
        public void SetValue_WhenKeyAlreadyExists_ExistingElementIsUpdated(
            [Values(1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            KeyValuePair<string, int> firstEntry = dict.KeyValuePairs.First();
            int newValue = firstEntry.Value + 10;

            // act
            dict.SetValue(firstEntry.Key, newValue);

            // assert
            Assert.AreEqual(newValue, dict.GetValue(firstEntry.Key));
            Assert.AreEqual(numStartingElements, dict.Count);
        }

        [Test]
        public void SetValue_WhenMultipleElementsAdded_NewElementsAreRememberedInSequentialOrder(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            string newKey1 = "newKey1";
            string newKey2 = "newKey2";
            string newKey3 = "newKey3";
            int newValue1 = 10;
            int newValue2 = 20;
            int newValue3 = 30;
            int newElement1Index = dict.Count + 0;
            int newElement2Index = dict.Count + 1;
            int newElement3Index = dict.Count + 2;

            // act
            dict.SetValue(newKey1, newValue1);
            dict.SetValue(newKey2, newValue2);
            dict.SetValue(newKey3, newValue3);

            // assert
            List<KeyValuePair<string, int>> elements = dict.KeyValuePairs.ToList();
            Assert.AreEqual(newKey1, elements[newElement1Index].Key);
            Assert.AreEqual(newKey2, elements[newElement2Index].Key);
            Assert.AreEqual(newKey3, elements[newElement3Index].Key);
            Assert.AreEqual(newValue1, elements[newElement1Index].Value);
            Assert.AreEqual(newValue2, elements[newElement2Index].Value);
            Assert.AreEqual(newValue3, elements[newElement3Index].Value);
            Assert.AreEqual(numStartingElements + 3, dict.Count);
        }

        [Test]
        public void SetValue_WhenKeyIsNull_RaisesArgumentNullException(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act / assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                dict.SetValue(null, 0);
            });
        }

        [Test]
        public void GetValue_WhenKeyExists_ReturnsCorrectValue(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            string newKey = "newKey";
            int newValue = 10;
            dict.Add(newKey, newValue);

            // act
            int fetchedValue = dict.GetValue(newKey);

            // assert
            Assert.AreEqual(newValue, fetchedValue);
        }

        [Test]
        public void GetValue_WhenKeyIsNull_RaisesArgumentNullException(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act / assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                dict.GetValue(null);
            });
        }

        [Test]
        public void GetValue_WhenKeyNotAlreadyExists_RaisesKeyNotFoundException(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act / assert
            Assert.Throws<KeyNotFoundException>(() =>
            {
                dict.GetValue("does not exist");
            });
        }

        [Test]
        public void Remove_WhenKeyExists_CorrespondingElementIsRemoved(
            [Values(1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            KeyValuePair<string, int> lastEntry = dict.KeyValuePairs.Last();

            // assume
            Assume.That(dict.ContainsKey(lastEntry.Key));
            Assume.That(dict.Keys.Contains(lastEntry.Key));
            Assume.That(dict.Values.Contains(lastEntry.Value));
            Assume.That(dict.KeyValuePairs.Contains(lastEntry));
            Assume.That(numStartingElements == dict.Count);

            // act
            dict.Remove(lastEntry.Key);

            // assert
            Assert.False(dict.ContainsKey(lastEntry.Key));
            Assert.False(dict.Keys.Contains(lastEntry.Key));
            Assert.False(dict.Values.Contains(lastEntry.Value));
            Assert.False(dict.KeyValuePairs.Contains(lastEntry));
            Assert.AreEqual(numStartingElements - 1, dict.Count);
        }

        [Test]
        public void Remove_WhenKeyNotExists_NoElementIsRemoved(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            string nonExistentKey = "does not exist";

            // assume
            Assume.That(!dict.ContainsKey(nonExistentKey));
            Assume.That(!dict.Keys.Contains(nonExistentKey));
            Assume.That(numStartingElements == dict.Count);

            // act
            dict.Remove(nonExistentKey);

            // assert
            Assert.False(dict.ContainsKey(nonExistentKey));
            Assert.False(dict.Keys.Contains(nonExistentKey));
            Assert.AreEqual(numStartingElements, dict.Count);
        }

        [Test]
        public void Remove_WhenElementIsRemovedFromMiddle_OrderOfElementsBeforeAndAfterIsPreserved()
        {
            // arrange
            int numStartingElements = 5;
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            KeyValuePair<string, int> middleElement = dict.KeyValuePairs.ElementAt(2);

            List<KeyValuePair<string, int>> expectedElementsAfterRemove = new List<KeyValuePair<string, int>>()
            {
                dict.KeyValuePairs.ElementAt(0),
                dict.KeyValuePairs.ElementAt(1),
                // dict.KeyValuePairs.ElementAt(2),  // <-- this element will get removed
                dict.KeyValuePairs.ElementAt(3),
                dict.KeyValuePairs.ElementAt(4),
            };

            // act
            dict.Remove(middleElement.Key);

            // assert
            Assert.False(dict.ContainsKey(middleElement.Key));
            Assert.AreEqual(numStartingElements - 1, dict.Count);
            for (int i = 0; i < numStartingElements - 1; ++i)
            {
                Assert.AreEqual(expectedElementsAfterRemove[i], dict.KeyValuePairs.ToList()[i]);
            }
        }

        [Test]
        public void Remove_WhenKeyIsNull_RaisesArgumentNullException(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);

            // act / assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                dict.Remove(null);
            });
        }

        [Test]
        public void Clear_WhenFinished_DictionaryIsEmpty(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            List<KeyValuePair<string, int>> elementsBeforeClearing = dict.KeyValuePairs.ToList();

            // assume
            Assume.That(dict.Count == numStartingElements);
            Assume.That(dict.Keys.Count() == numStartingElements);
            Assume.That(dict.Values.Count() == numStartingElements);
            Assume.That(dict.KeyValuePairs.Count() == numStartingElements);
            foreach (KeyValuePair<string, int> originalElement in elementsBeforeClearing)
            {
                Assert.True(dict.ContainsKey(originalElement.Key));
            }

            // act
            dict.Clear();

            // assert
            Assert.AreEqual(0, dict.Count);
            Assert.AreEqual(0, dict.Keys.Count());
            Assert.AreEqual(0, dict.Values.Count());
            Assert.AreEqual(0, dict.KeyValuePairs.Count());
            foreach (KeyValuePair<string, int> originalElement in elementsBeforeClearing)
            {
                Assert.False(dict.ContainsKey(originalElement.Key));
            }
        }

        [Test]
        public void Sort_WhenFinished_NewOrderIsRemembered(
            [Values(0, 1, 5)] int numStartingElements)
        {
            // arrange
            SerializableOrderedDictionary<string, int> dict = CreateDictionaryWith(numStartingElements);
            IEnumerable<string> originalOrderKeys = dict.Keys.ToList();
            IEnumerable<int> originalOrderValues = dict.Values.ToList();
            IEnumerable<KeyValuePair<string, int>> originalOrderKeyValuePairs = dict.KeyValuePairs.ToList();

            // act
            dict.Sort( elements => elements.Reverse());

            // assert
            Assert.AreEqual(originalOrderKeys.Reverse(), dict.Keys);
            Assert.AreEqual(originalOrderValues.Reverse(), dict.Values);
            Assert.AreEqual(originalOrderKeyValuePairs.Reverse(), dict.KeyValuePairs);
        }

        private SerializableOrderedDictionary<string, int> CreateDictionaryWith(int numElements)
        {
            SerializableOrderedDictionary<string, int> dict = new SerializableOrderedDictionary<string, int>();
            for (int i = 0; i < numElements; ++i)
            {
                string key = i.ToString(CultureInfo.CurrentCulture);
                int value = i;
                dict.Add(key, value);
            }

            return dict;
        }
    }
}
