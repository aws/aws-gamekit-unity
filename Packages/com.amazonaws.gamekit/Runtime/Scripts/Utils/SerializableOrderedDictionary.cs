// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Linq;

// Unity
using UnityEngine;

namespace AWS.GameKit.Runtime.Utils
{
    /// <summary>
    /// A Serializable <see cref="Dictionary{TKey,TValue}"/> that remembers the order the elements were added.
    /// </summary>
    /// <remarks>
    /// [Serialization]
    /// <para>
    /// The <see cref="Dictionary{TKey,TValue}"/> class is not Serializable in C# by default.
    /// This class is able to serialize a dictionary by breaking out the key/value pairs into a key list and value list which can be Serialized.
    /// </para>
    ///
    /// [Order Remembered]
    /// <para>
    /// This collection remembers the order the elements were added.
    /// The ordered elements can be accessed through <see cref="Keys"/>, <see cref="Values"/>, and <see cref="KeyValuePairs"/>.
    /// The elements can be sorted in a custom order by calling <see cref="Sort"/>.
    /// </para>
    ///
    /// [Time Complexity]
    /// <para>
    /// Each method has the following time complexity:
    /// <list type="bullet">
    ///   <item><see cref="ContainsKey"/> - O(1)</item>
    ///   <item><see cref="Add"/> - O(1)</item>
    ///   <item><see cref="SetValue"/> - O(1)</item>
    ///   <item><see cref="GetValue"/> - O(1)</item>
    ///   <item><see cref="Remove"/> - O(N)</item>
    ///   <item><see cref="Sort"/> - depends on the provided callback function, usually O(N log(N))</item>
    ///   <item><see cref="Clear"/> - O(N)</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    [Serializable]
    public class SerializableOrderedDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        protected IDictionary<TKey, TValue> _dict = new Dictionary<TKey, TValue>();

        // Serialization
        // The keys and values are stored in sorted order.
        [HideInInspector] [SerializeField] protected List<TKey> _keys = new List<TKey>();
        [HideInInspector] [SerializeField] protected List<TValue> _values = new List<TValue>();

        /// <summary>
        /// The number of key/value pairs in the <see cref="SerializableOrderedDictionary{K,V}"/> collection.
        /// </summary>
        public int Count => _dict.Count;

        /// <summary>
        /// Get all keys in the <see cref="SerializableOrderedDictionary{K,V}"/> collection in the order they were added or later sorted with <see cref="Sort"/>.
        /// </summary>
        public IEnumerable<TKey> Keys => _keys;

        /// <summary>
        /// Get all values in the <see cref="SerializableOrderedDictionary{K,V}"/> collection in the order they were added or later sorted with <see cref="Sort"/>.
        /// </summary>
        public IEnumerable<TValue> Values => _values;

        /// <summary>
        /// Get all key/value pairs in the <see cref="SerializableOrderedDictionary{K,V}"/> collection in the order they were added or later sorted with <see cref="Sort"/>.
        /// </summary>
        public IEnumerable<KeyValuePair<TKey, TValue>> KeyValuePairs => _keys.Zip(_values, (key, value) => new KeyValuePair<TKey, TValue>(key, value));

        /// <summary>
        /// This method is called by Unity when this class needs to be Serialized.
        /// </summary>
        public virtual void OnBeforeSerialize()
        {
            // Nothing to do.
            // The _keys and _values lists are already up-to-date. They will be used in OnAfterDeserialize() to reconstruct this object.
        }

        /// <summary>
        /// This method is called by Unity when this class needs to be Deserialized.
        /// </summary>
        public virtual void OnAfterDeserialize()
        {
            // Rebuild the dictionary from the serialized keys/values.
            _dict.Clear();

            if (_keys.Count != _values.Count)
            {
                Logging.LogError($"There were {_keys.Count} keys and {_values.Count} values found while deserializing the {nameof(SerializableOrderedDictionary<TKey, TValue>)}. " +
                                 $"This collection may contain inaccurate data.");
            }

            int lowestCount = Math.Min(_keys.Count, _values.Count);

            for (int i = 0; i < lowestCount; ++i)
            {
                _dict.Add(_keys[i], _values[i]);
            }
        }

        /// <summary>
        /// Determine whether the <see cref="SerializableOrderedDictionary{TKey,TValue}"/> collection contains the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        public virtual bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        /// <summary>
        /// Add the specified key and value to the dictionary at the end of the ordered list.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        /// <exception cref="ArgumentException">Thrown when an element with the same key already exists in the <see cref="SerializableOrderedDictionary{K,V}"/> collection.</exception>
        public virtual void Add(TKey key, TValue value)
        {
            _dict.Add(key, value);

            _keys.Add(key);
            _values.Add(value);
        }

        /// <summary>
        /// Set the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to set.</param>
        /// <param name="value">The value associated with the specified key. If the specified key is not found, creates a new element with the specified key. Otherwise overwrites the existing element.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        public virtual void SetValue(TKey key, TValue value)
        {
            if (_dict.ContainsKey(key))
            {
                // Update existing value
                _dict[key] = value;
            }
            else
            {
                // Add new element
                Add(key, value);
            }
        }

        /// <summary>
        /// Get the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key. If the specified key is not found, throws a <see cref="KeyNotFoundException"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the specified key does not exist in the <see cref="SerializableOrderedDictionary{K,V}"/> collection.</exception>
        public virtual TValue GetValue(TKey key)
        {
            return _dict[key];
        }

        /// <summary>
        /// Remove the value with the specified key from the <see cref="SerializableOrderedDictionary{K,V}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided key is null.</exception>
        public virtual void Remove(TKey key)
        {
            if (!_dict.ContainsKey(key))
            {
                return;
            }

            _dict.Remove(key);
            int indexOfElement = _keys.IndexOf(key);
            _keys.RemoveAt(indexOfElement);
            _values.RemoveAt(indexOfElement);
        }

        /// <summary>
        /// Remove all elements from the <see cref="SerializableOrderedDictionary{K,V}"/> collection.
        /// </summary>
        public virtual void Clear()
        {
            _dict.Clear();
            _keys.Clear();
            _values.Clear();
        }

        /// <summary>
        /// Sort the collection's elements in-place using using the provided sorting function.
        /// </summary>
        /// <remarks>
        /// After calling this method, the new ordering is reflected in <see cref="Keys"/>, <see cref="Values"/>, and <see cref="KeyValuePairs"/>.
        /// </remarks>
        /// <param name="sortFunction">A function which sorts a list of key/value pairs. The function's input is this collection's ordered key/value pairs.
        /// The function's output is the same key/value pairs in a newly sorted order.</param>
        public virtual void Sort(Func<IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>> sortFunction)
        {
            IEnumerable<KeyValuePair<TKey, TValue>> sortedElements = sortFunction(KeyValuePairs.ToList());

            _keys.Clear();
            _values.Clear();

            foreach (KeyValuePair<TKey, TValue> keyValuePair in sortedElements)
            {
                _keys.Add(keyValuePair.Key);
                _values.Add(keyValuePair.Value);
            }
        }
    }
}