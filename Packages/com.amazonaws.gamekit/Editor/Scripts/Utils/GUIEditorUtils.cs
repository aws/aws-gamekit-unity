// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

namespace AWS.GameKit.Editor.Utils
{
    public static class GUIEditorUtils
    {
        /// <summary>
        /// Generates a 1 pixel texture that can be used to assign to a GUIStyleState's background property.
        /// </summary>
        /// <param name="color">Color to use for the background</param>
        /// <returns>A Texture2D class which can be assigned as a background</returns>
        public static Texture2D CreateBackground(Color color)
        {
            Texture2D result = new Texture2D(1, 1);
            result.SetPixels(new Color[] { color });
            result.Apply();

            return result;
        }
    }
}
