// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;

// GameKit
using AWS.GameKit.Common;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// Creates the SettingsWindow and provides the rest of AWS GameKit with an interface to access the SettingsWindow.
    /// </summary>
    public class SettingsController
    {
        private readonly SettingsModel _model;
        private readonly SettingsDependencyContainer _dependencies;

        public SettingsController(SettingsDependencyContainer dependencies)
        {
            _dependencies = dependencies;
            
            // We Initialize() the model once all of its dependencies have finished bootstrapping,
            // which happens in the OnSettingsWindowEnabled() callback below.
            _model = SettingsModel.LoadFromDisk(GameKitPaths.Get().ASSETS_SETTINGS_WINDOW_STATE_RELATIVE_PATH);

            SettingsWindow.Enabled += OnSettingsWindowEnabled;
        }

        /// <summary>
        /// Give focus to the existing window instance, or create one if it doesn't exist.
        /// </summary>
        /// <returns>The existing or new window instance.</returns>
        public SettingsWindow GetOrCreateSettingsWindow()
        {
            return EditorWindow.GetWindow<SettingsWindow>();
        }

        private void OnSettingsWindowEnabled(SettingsWindow enabledSettingsWindow)
        {
            _model.Initialize(_dependencies);

            enabledSettingsWindow.Initialize(_model);
        }
    }
}
