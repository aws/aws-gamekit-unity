// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;

// GameKit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// A version of <see cref="SerializableOrderedDictionary{TKey,TValue}"/> whose <see cref="TValue"/> implements <see cref="IHasSerializedPropertyOfSelf"/>.
    /// This class takes care of assigning and updating the <see cref="IHasSerializedPropertyOfSelf.SerializedPropertyOfSelf"/> reference whenever this dictionary is modified.<br/><br/>
    ///
    /// Make sure to call <see cref="FrameDelayedInitializeNewElements"/> at the beginning of every OnGUI method, before running any code in the dictionary's <see cref="TValue"/> elements.
    /// </summary>
    /// <inheritdoc cref="SerializableOrderedDictionary{K,V}"/>
    [Serializable]
    public class SerializablePropertyOrderedDictionary<TKey, TValue> : SerializableOrderedDictionary<TKey, TValue>
        where TValue : IHasSerializedPropertyOfSelf
    {
        // Serialization
        private SerializedProperty _valuesSerializedProperty;
        private int _numNewElements = 0;

        /// <summary>
        /// This method must be the first method called on a new instance of this class.
        /// </summary>
        /// <param name="serializedProperty">The <see cref="SerializedProperty"/> of a class's instance variable holding this instance of <see cref="SerializablePropertyOrderedDictionary{TKey,TValue}"/>.</param>
        public void Initialize(SerializedProperty serializedProperty)
        {
            _valuesSerializedProperty = serializedProperty.FindPropertyRelative($"{nameof(_values)}");
            UpdateSerializedPropertyForElements(0);
        }

        /// <summary>
        /// This method should be called at the beginning of every OnGUI() frame, before running any code in this collection's <see cref="TValue"/> elements.
        /// </summary>
        /// <remarks>
        /// New elements added to this collection by <see cref="Add"/> or <see cref="SerializablePropertyOrderedDictionary{TKey,TValue}.SetValue"/> will not
        /// have their <see cref="IHasSerializedPropertyOfSelf.SerializedPropertyOfSelf"/> set until <see cref="FrameDelayedInitializeNewElements"/> is called
        /// at least one frame after the element was added.<br/><br/>
        ///
        /// The <see cref="IHasSerializedPropertyOfSelf.SerializedPropertyOfSelf"/> can't be set during the same frame the element was added to this collection because it doesn't
        /// exist in Unity's serialization framework until one frame after the element was added to this class's internal <see cref="SerializablePropertyOrderedDictionary{TKey,TValue}._values"/> list.
        /// </remarks>
        public void FrameDelayedInitializeNewElements()
        {
            UpdateSerializedPropertyForElements(_dict.Count - _numNewElements);
        }

        public override void Add(TKey key, TValue value)
        {
            base.Add(key, value);

            ++_numNewElements;
        }

        public override void Remove(TKey key)
        {
            int indexOfElement = _keys.IndexOf(key);

            base.Remove(key);

            if (indexOfElement >= 0)
            {
                // All elements after the removed element need to be updated because they have a new position in the _values list.
                UpdateSerializedPropertyForElements(indexOfElement);
            }
        }

        public override void Clear()
        {
            base.Clear();

            _numNewElements = 0;
        }

        public override void Sort(Func<IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>> sortFunction)
        {
            base.Sort(sortFunction);

            // All elements need to be updated because they have a new position in the _values list.
            // By changing the _numNewElements to dict length we will cause all the serialized elements to be updated by FrameDelayedInitializeNewElements() in the next GUI frame
            _numNewElements = _dict.Count;
        }

        /// <summary>
        /// Update the serialized property of all elements in the _values list from the <see cref="startingFromIndex"/> to the end of the list.<br/><br/>
        ///
        /// After calling this method, <see cref="_numNewElements"/> is reset to <c>0</c>.
        /// </summary>
        /// <param name="startingFromIndex">The first element to update. All elements coming before this index will not be updated.</param>
        private void UpdateSerializedPropertyForElements(int startingFromIndex)
        {
            for (int i = startingFromIndex; i < _dict.Count; ++i)
            {
                _values[i].SerializedPropertyOfSelf = _valuesSerializedProperty.GetArrayElementAtIndex(i);
            }

            _numNewElements = 0;
        }
    }
}