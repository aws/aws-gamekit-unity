// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0
 
// Unity
using UnityEditor;
using UnityEngine;
 
namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// A ScriptableObject which survives when the Unity editor restarts or does a Domain Reload.
    ///
    /// The object automatically writes itself to disk during OnDisable() and can be loaded with LoadObject().
    /// </summary>
    public class PersistentScriptableObject<T> : ScriptableObject
        where T : PersistentScriptableObject<T>
    {
        private string _assetFilePath;
        private bool _shouldBeSaved;
 
        /// <summary>
        /// Load the ScriptableObject from a file on disk.
        /// </summary>
        /// <param name="assetFilePath">The path where the ScriptableObject's file will be loaded from and written to.</param>
        /// <returns>The ScriptableObject deserialized from the file.</returns>
        public static T LoadFromDisk(string assetFilePath)
        {
            T scriptableObject = AssetDatabase.LoadAssetAtPath<T>(assetFilePath);
 
            if (scriptableObject == null)
            {
                scriptableObject = CreateInstance<T>();
                AssetDatabase.CreateAsset(scriptableObject, assetFilePath);
            }
 
            if (!string.IsNullOrEmpty(assetFilePath))
            {
                scriptableObject._assetFilePath = assetFilePath;
                scriptableObject._shouldBeSaved = true;
            }
 
            return scriptableObject;
        }
 
        private void WriteToDisk()
        {
            // Implementation note:
            // We persist a clone instead of the original object to ensure the state is non-null when loaded by LoadAssetAtPath() in LoadFromDisk().
            // If we persist the object by other means, then LoadAssetAtPath() returns null when LoadFromDisk() is called immediately after script recompilation.
            _shouldBeSaved = false;
            T clonedScriptableObject = (T)Instantiate(this);
            AssetDatabase.CreateAsset(clonedScriptableObject, _assetFilePath);
        }
 
        private void OnDisable()
        {
            if (_shouldBeSaved)
            {
                WriteToDisk();
            }
        }
    }
}
