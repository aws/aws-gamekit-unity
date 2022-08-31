// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Reflection;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.Windows.Settings;

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// Helper methods for Unity's EditorWindows class.
    ///
    /// See: https://docs.unity3d.com/ScriptReference/EditorWindow.html
    /// </summary>
    public static class EditorWindowHelper
    {
        private const string INSPECTOR_WINDOW_TYPE_NAME = "UnityEditor.InspectorWindow";

        /// <summary>
        /// Set the background color of the editor window.
        ///
        /// This method should be called before drawing any other GUI elements in the window,
        /// because it draws on top of everything drawn previously.
        /// </summary>
        /// <param name="window">The window to set the background color of.</param>
        /// <param name="newBackgroundColorTexture">The color to set the window's background to. This texture will be stretched to fit the entire window.</param>
        public static void SetBackgroundColor(EditorWindow window, Texture2D newBackgroundColorTexture)
        {
            Vector2 windowTopLeft = new Vector2(0, 0);
            Vector2 windowBottomRight = new Vector2(window.position.width, window.position.height);
            GUI.DrawTexture(new Rect(windowTopLeft, windowBottomRight), newBackgroundColorTexture, ScaleMode.StretchToFill);
        }

        /// <summary>
        /// Get the Type of Unity's Inspector window.
        ///
        /// This type can be used as a desired dock target when calling EditorWindow.GetWindow(desiredDockNextTo).
        ///
        /// Warning: This method relies on reflection to get Unity's internal InspectorWindow type.
        /// If Unity changes the InspectorWindow's type name, this method will log a warning and return null.
        /// It's safe to pass the null value into EditorWindow.GetWindow(desiredDockNextTo) - the InspectorWindow will
        /// simply be skipped as a dock target.
        /// </summary>
        public static Type GetInspectorWindowType()
        {
            Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
            Type inspectorWindowType = editorAssembly.GetType(INSPECTOR_WINDOW_TYPE_NAME);

            if (inspectorWindowType == null)
            {
                Debug.LogWarning($"Could not find the System.Type of Unity's InspectorWindow through reflection. " +
                                        $"Tried using the name: \"{INSPECTOR_WINDOW_TYPE_NAME}\". " +
                                        $"Unity most likely changed the InspectorWindow's name, namespace, or assembly. " +
                                        $"Please update this method to use it's new type. " +
                                        $"Last known type: https://github.com/Unity-Technologies/UnityCsReference/blob/e740821767d2290238ea7954457333f06e952bad/Editor/Mono/Inspector/InspectorWindow.cs#L20");
            }

            return inspectorWindowType;
        }

        /// <summary>
        /// Change the window's current size and it's minimum/maximum size.
        ///
        /// The window will expand/contract from it's center, instead of it's bottom right corner like it normally would.
        ///
        /// If called during window initialization, it should be called during the first OnGUI() call instead of during OnEnable().
        /// Otherwise it will expand from it's bottom right corner instead of the center.
        /// </summary>
        /// <param name="window">The window to modify.</param>
        /// <param name="newSize">The width and height to give the window.</param>
        /// <param name="minSize">The minimum size to give the window.</param>
        /// <param name="maxSize">The maximum size to give the window.</param>
        public static void SetWindowSizeAndBounds(EditorWindow window, Vector2 newSize, Vector2 minSize, Vector2 maxSize)
        {
            SetWindowSizeExpandingFromCenter(window, newSize);

            // Bounds must be set after changing the size, otherwise the window will not be centered.
            window.minSize = minSize;
            window.maxSize = maxSize;
        }

        private static void SetWindowSizeExpandingFromCenter(EditorWindow window, Vector2 newSize)
        {
            // To keep the window centered on it's original location, shift it's upper left corner
            // by half of the increase/decrease in size (deltaWidth & deltaHeight).
            float newWidth = newSize.x;
            float newHeight = newSize.y;
            float deltaWidth = newWidth - window.position.width;
            float deltaHeight = newHeight - window.position.height;
            float newXPosition = window.position.x - (deltaWidth / 2f);
            float newYPosition = window.position.y - (deltaHeight / 2f);
            window.position = new Rect(newXPosition, newYPosition, newWidth, newHeight);
        }
    }
}