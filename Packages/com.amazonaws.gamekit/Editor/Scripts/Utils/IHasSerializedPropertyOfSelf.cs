// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// Has an instance of <see cref="SerializedProperty"/> which points to <c>this</c>.
    /// </summary>
    public interface IHasSerializedPropertyOfSelf
    {
        /// <summary>
        /// A <see cref="SerializedProperty"/> which points to <c>this</c>.
        /// </summary>
        public SerializedProperty SerializedPropertyOfSelf
        {
            get;
            set;
        }
    }
}