// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// System
using System;

// Unity
using UnityEditor;
using UnityEditor.IMGUI.Controls;

// GameKit
using AWS.GameKit.Editor.Utils;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// Contains all the data the SettingsWindow needs during construction.
    /// </summary>
    [Serializable]
    public class SettingsModel : PersistentScriptableObject<SettingsModel>
    {
        public AllPages AllPages;

        public SerializedObject SerializedObject;

        /// <summary>
        /// True if the AWS GameKit Settings window has been opened at least once in this project during any Unity session.
        /// </summary>
        public bool SettingsWindowHasEverBeenOpened;

        /// <summary>
        /// The currently selected navigation tree item and which items are collapsed/expanded.
        /// </summary>
        public TreeViewState NavigationTreeState;

        public void Initialize(SettingsDependencyContainer dependencies)
        {
            SerializedObject = new UnityEditor.SerializedObject(this);

            AllPages.Initialize(dependencies, SerializedObject.FindProperty(nameof(AllPages)));
        }
    }
}
