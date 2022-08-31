// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// This class is only usable while in Editor
#if UNITY_EDITOR

// Standard
using System.Threading;

// Unity
using UnityEditor;
using UnityEngine;

namespace AWS.GameKit.Runtime.Utils
{
    /// <summary>
    /// Used to manage updates of the assigned SettingsWindow.
    /// </summary>
    public static class SettingsWindowUpdateController
    {
        // 0 == false, 1 == true
        private static int _shouldUpdate = 0;

        private static EditorWindow _settingsWindow = null;

        public static void AssignSettingsWindow(EditorWindow settingsWindow) => _settingsWindow = settingsWindow;

        public static void RequestUpdate()
        {
            // set to 1 for true
            Interlocked.Exchange(ref _shouldUpdate, 1);
        }

        public static void Update()
        {
            // if the original value found in _shouldUpdate is 1 (true) then force the repaint
            if (Interlocked.Exchange(ref _shouldUpdate, 0) == 1 && _settingsWindow && !Application.isPlaying)
            {
                // this method can only be called from Unity's main thread
                _settingsWindow.Repaint();
            }

        }
    }
}
#endif
