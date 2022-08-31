// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// A lazy loading wrapper for an asset stored in a "Resources" folder.
    ///
    /// The asset is loaded and cached the first time Get() is called.
    ///
    /// For details on Unity's special "Resources" folders and how to load data from them, please read: https://docs.unity3d.com/ScriptReference/Resources.Load.html
    /// </summary>
    /// <typeparam name="T">The type to load the asset as.</typeparam>
    public class LazyLoadedResource<T> : IGettable<T> where T : UnityEngine.Object
    {
        private readonly string _path;
        private Lazy<T> _cachedResource;


        /// <summary>
        /// Create a new LazyLoadedResource for the specified resource path.
        /// </summary>
        /// <param name="path">The path to the resource relative to any Resources folder. See official docs for details: https://docs.unity3d.com/ScriptReference/Resources.Load.html</param>
        public LazyLoadedResource(string path)
        {
            _path = path;

            _cachedResource = new Lazy<T>(() => (T)AssetDatabase.LoadAssetAtPath(path, typeof(T)));
        }

        /// <summary>
        /// Get the resource. Cache the resource if this is the first time calling Get().
        /// </summary>
        public T Get()
        {
            return _cachedResource.Value;
        }
    }
}
