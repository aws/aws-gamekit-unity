// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// A wrapper for a pair of dark-theme and light-theme objects.
    ///
    /// Use this class when you need to use one of two objects depending on whether the Unity Editor Theme is Dark or Light.
    /// </summary>
    /// <typeparam name="T">The type of object to wrap.</typeparam>
    public class EditorThemeAware<T> : IGettable<T>
    {
        /// <summary>
        /// The object to use when the Unity Editor theme is Dark.
        /// </summary>
        public T DarkThemeObject { get; }

        /// <summary>
        /// The object to use when the Unity Editor theme is Light.
        /// </summary>
        public T LightThemeObject { get; }

        /// <summary>
        /// Create a new EditorThemeAware wrapper for the two provided objects.
        /// </summary>
        /// <param name="darkThemeObject">The object to use when the Unity Editor theme is Dark.</param>
        /// <param name="lightThemeObject">The object to use when the Unity Editor theme is Light.</param>
        public EditorThemeAware(T darkThemeObject, T lightThemeObject)
        {
            DarkThemeObject = darkThemeObject;
            LightThemeObject = lightThemeObject;
        }

        /// <summary>
        /// Get the object matching the current Unity Editor theme.
        /// </summary>
        /// <returns>The DarkThemeObject if the Unity Editor theme is "Dark", otherwise the LightThemeObject.</returns>
        public virtual T Get()
        {
            return EditorGUIUtility.isProSkin ? DarkThemeObject : LightThemeObject;
        }
    }
}