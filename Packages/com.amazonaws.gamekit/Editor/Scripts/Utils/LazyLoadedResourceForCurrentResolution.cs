// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

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
    public class LazyLoadedResourceForCurrentResolution<T> : IGettable<T> where T : UnityEngine.Object
    {
        public delegate T ResourceLoader(string path);

        private enum ResourceSize
        {
            ONE_K,
            TWO_K,
            FOUR_K,
            EIGHT_K
        }

        private const string TWO_K_POSTFIX = "-2k";
        private const string FOUR_K_POSTFIX = "-4k";
        private const string EIGHT_K_POSTFIX = "-8k";

        private readonly string[] _paths = new string[Enum.GetNames(typeof(ResourceSize)).Length];
        private Lazy<T> _cachedResource;
        private Resolution _resolution;
        private ResourceLoader _resourceLoader;

        /// <summary>
        /// Create a new LazyLoadedResource for the specified resource path.
        /// </summary>
        /// <param name="path">The path to the resource relative to any Resources folder. See official docs for details: https://docs.unity3d.com/ScriptReference/Resources.Load.html</param>
        /// <param name="resolution">A Unity object that specifies the height and width of the desktop.</param>
        /// <param name="resourceLoader">A delegate that takes a string and returns a type T. This delegate is used to perform that actual resource loading.</param>
        public LazyLoadedResourceForCurrentResolution(string path, Resolution resolution, ResourceLoader resourceLoader)
        {
            _resolution = resolution;
            _resourceLoader = resourceLoader;

            _paths[(int)ResourceSize.ONE_K] = path;
            _paths[(int)ResourceSize.TWO_K] = path + TWO_K_POSTFIX;
            _paths[(int)ResourceSize.FOUR_K] = path + FOUR_K_POSTFIX;
            _paths[(int)ResourceSize.EIGHT_K] = path + EIGHT_K_POSTFIX;

            _cachedResource = new Lazy<T>(() => LoadResource(IdealResolution()));
        }

        /// <summary>
        /// Create a new LazyLoadedResource for the specified resource path.
        /// </summary>
        /// <param name="path">The path to the resource relative to any Resources folder. See official docs for details: https://docs.unity3d.com/ScriptReference/Resources.Load.html</param>
        public LazyLoadedResourceForCurrentResolution(string path) : this(path, Screen.currentResolution, (path) => (T)AssetDatabase.LoadAssetAtPath(path, typeof(T))) { }

        /// <summary>
        /// Get the resource. Cache the resource if this is the first time calling Get().
        /// </summary>
        public T Get()
        {
            return _cachedResource.Value;
        }

        private ResourceSize IdealResolution()
        {
            // get the longest edge, in case the monitor is rotated - landscape vs portrait
            double longestEdge = Math.Max(_resolution.width, _resolution.height);

            // convert to kilos and then determine how many we have
            double numberOfK = Math.Round(longestEdge / 1000.0);

            if (numberOfK <= 1)
            {
                return ResourceSize.ONE_K;
            }
            else if (numberOfK <= 2)
            {
                return ResourceSize.TWO_K;
            }
            else if (numberOfK <= 4)
            {
                return ResourceSize.FOUR_K;
            }
            else
            {
                return ResourceSize.EIGHT_K;
            }
        }
        
        private T LoadResource(ResourceSize idealResourceSize)
        {
            int size = (int)idealResourceSize;
            T resource = null;

            // attempt to load the current ideal resolution for the resource, if nothing loads then decrement and attempt the next best resolution
            do
            {
                resource = _resourceLoader(_paths[size]);

                --size;
            }
            while (size >= (int)ResourceSize.ONE_K && resource == null);

            return resource;
        }
    }
}
