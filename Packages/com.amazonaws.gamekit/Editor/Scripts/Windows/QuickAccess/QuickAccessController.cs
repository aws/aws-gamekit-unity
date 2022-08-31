// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;

// GameKit
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Editor.Windows.Settings;

namespace AWS.GameKit.Editor.Windows.QuickAccess
{
    /// <summary>
    /// Creates the QuickAccessWindow and provides the rest of AWS GameKit with an interface to access the QuickAccessWindow.
    /// </summary>
    public class QuickAccessController
    {
        private readonly SettingsController _settingsWindowController;
        private readonly SettingsDependencyContainer _dependencies;

        public QuickAccessController(SettingsController settingsWindowController, SettingsDependencyContainer dependencies)
        {
            _settingsWindowController = settingsWindowController;
            _dependencies = dependencies;
            QuickAccessWindow.Enabled += OnQuickAccessWindowEnabled;
        }

        /// <summary>
        /// Give focus to the existing window instance, or create one if it doesn't exist.
        /// </summary>
        /// <returns>The existing or new window instance.</returns>
        public QuickAccessWindow GetOrCreateQuickAccessWindow()
        {
            return EditorWindow.GetWindow<QuickAccessWindow>(GetDesiredDockNextToWindows());
        }

        /// <summary>
        /// Get the list of editor windows which the Quick Access window wants to dock next to in priority order.
        /// </summary>
        private static Type[] GetDesiredDockNextToWindows()
        {
            return new Type[]
            {
                EditorWindowHelper.GetInspectorWindowType()
            };
        }

        private void OnQuickAccessWindowEnabled(QuickAccessWindow enabledQuickAccessWindow)
        {
            enabledQuickAccessWindow.Initialize(_settingsWindowController, _dependencies);
        }
    }
}
