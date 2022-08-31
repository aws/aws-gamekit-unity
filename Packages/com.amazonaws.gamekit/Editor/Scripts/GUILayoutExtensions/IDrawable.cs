// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.GUILayoutExtensions
{
    /// <summary>
    /// Interface for a GUI element which can be drawn on the screen with Unity's EditorGUILayout or GUILayout.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Draw this element on the screen using functions in UnityEditor.EditorGUILayout or UnityEditor.GUILayout.
        /// </summary>
        public void OnGUI();
    }
}