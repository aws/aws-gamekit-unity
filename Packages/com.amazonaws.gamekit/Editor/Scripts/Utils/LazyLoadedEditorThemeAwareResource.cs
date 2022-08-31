// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// A lazy loading wrapper for a pair of dark-theme and light-theme assets stored in a "Resources" folder.
    ///
    /// The Get() method returns the correct asset based on the Unity Editor theme.
    ///
    /// The assets are loaded and cached the first time Get() is called.
    ///
    /// For details on Unity's special "Resources" folders and how to load data from them, please read: https://docs.unity3d.com/ScriptReference/Resources.Load.html
    /// </summary>
    /// <typeparam name="T">The type to load the assets as.</typeparam>
    public class LazyLoadedEditorThemeAwareResource<T> : IGettable<T> where T : UnityEngine.Object 
    {
        private readonly EditorThemeAware<IGettable<T>> _cachedResources;

        /// <summary>
        /// Create a new LazyLoadedEditorThemeAwareResource for the specified resource paths.
        ///
        /// See official docs for details on the resource paths: https://docs.unity3d.com/ScriptReference/Resources.Load.html
        /// </summary>
        /// <param name="darkThemeResourcePath">The path to the Dark-themed resource relative to any Resources folder.</param>
        /// <param name="lightThemeResourcePath">The path to the Light-themed resource relative to any Resources folder.</param>
        public LazyLoadedEditorThemeAwareResource(string darkThemeResourcePath, string lightThemeResourcePath)
        {
            _cachedResources = new EditorThemeAware<IGettable<T>>(
                new LazyLoadedResource<T>(darkThemeResourcePath),
                new LazyLoadedResource<T>(lightThemeResourcePath)
            );
        }

        /// <summary>
        /// Create a new LazyLoadedEditorThemeAwareResource for the specified resource paths.
        ///
        /// See official docs for details on the resource paths: https://docs.unity3d.com/ScriptReference/Resources.Load.html
        /// </summary>
        /// <param name="darkThemeResourceGetter">An IGetter for the dark theme resource.</param>
        /// <param name="lightThemeResourceGetter">An IGetter for the light theme resource.</param>
        public LazyLoadedEditorThemeAwareResource(IGettable<T> darkThemeResourceGetter, IGettable<T> lightThemeResourceGetter)
        {
            _cachedResources = new EditorThemeAware<IGettable<T>>(darkThemeResourceGetter, lightThemeResourceGetter);
        }

        /// <summary>
        /// Get the asset matching the current Unity Editor theme.
        /// </summary>
        /// <returns>The dark-themed asset if the Unity Editor theme is "Dark", otherwise the light-themed asset.</returns>
        public T Get()
        {
            return _cachedResources.Get().Get();
        }
    }
}
