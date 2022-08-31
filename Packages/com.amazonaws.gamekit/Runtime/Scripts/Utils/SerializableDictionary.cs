// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEngine;

namespace AWS.GameKit.Runtime.Utils
{
    [Serializable]
    public class SerializableDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        public List<V> ListOfSerializedValues => _values;
        public string NameOfSerializedValuesList => nameof(_values);

        [HideInInspector] [SerializeField] private List<K> _keys = new List<K>();
        [HideInInspector] [SerializeField] private List<V> _values = new List<V>();

        /// <summary>
        /// Since a Dictionary is not Serializable in C# by default we breakout the KVPs that were stored in the dictionary into a key list and value list which can be Serialized.<br/><br/>
        ///
        /// OnBeforeSerialize will run whenever the SerializableDictionary is trying to be Serialized.
        /// </summary>
        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();

            foreach (var kvp in this)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Since a Dictionary is not Serializable in C# by default we take a list of keys and list of values that were serialized in OnBeforeSerialize() and turn them back into a dictionary.<br/><br/>
        /// 
        /// OnAfterDeserialize will run whenever the SerializableDictionary is trying to be Deserialized.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();

            if (_keys.Count != _values.Count)
            {
                Logging.LogError($"There were {_keys.Count} keys and {_values.Count} values found while deserializing the SerializableDictionary. This SerializableDictionary may contain inaccurate data.");
            }

            int lowestCount = Math.Min(_keys.Count, _values.Count);

            for (int i = 0; i != lowestCount; i++)
            {
                Add(_keys[i], _values[i]);
            }
        }

        /// <summary>
        /// Gets a value from the dictionary.<br/><br/>
        /// 
        /// Note: If the key does not exist yet, will first add the key with a initial value of new V().
        /// </summary>
        /// <param name="key">The key in the dictionary that holds the desired value.</param>
        /// <returns></returns>
        public V GetValue(K key)
        {
            if (!this.ContainsKey(key))
            {
                this.Add(key, default);
            }

            return this[key];
        }

        /// <summary>
        /// Adds a key value pair to the dictionary.<br/><br/>
        ///
        /// Note: If the key does not exist in the dictionary, will add it.
        /// </summary>
        /// <param name="key">The key in the dictionary that should hold the passed in value.</param>
        /// <param name="value">The new value the key should be matched to.</param>
        public void SetValue(K key, V value)
        {
            if (!this.ContainsKey(key))
            {
                this.Add(key, value);
            }

            this[key] = value;
        }
    }
}