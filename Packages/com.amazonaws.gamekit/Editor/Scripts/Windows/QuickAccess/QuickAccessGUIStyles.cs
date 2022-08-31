
// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.FileStructure;

namespace AWS.GameKit.Editor.Windows.QuickAccess
{
    /// <summary>
    /// GUIStyles for the AWS GameKit QuickAccess window.
    /// </summary>
    public static class QuickAccessGUIStyles
    {
        public static class Window
        {
            public static readonly float MinWidth = 300f;
            public static readonly float MinHeight = 200f;

            public static readonly Texture2D BackgroundColor = EditorResources.Textures.QuickAccessWindow.Colors.QuickAccessBackground.Get();

            public static readonly float SpaceBetweenFeatureRows = 7f;
        }

        public static class FeatureRow
        {
            private static readonly GUIStyleState BackgroundColorNormal = new GUIStyleState()
            {
                background = EditorResources.Textures.QuickAccessWindow.Colors.QuickAccessButtonNormal.Get()
            };
            private static readonly GUIStyleState BackgroundColorHover = new GUIStyleState()
            {
                background = EditorResources.Textures.QuickAccessWindow.Colors.QuickAccessButtonHover.Get()
            };

            public static readonly GUIStyle HorizontalLayout = new GUIStyle()
            {
                padding = new RectOffset(10, 10, 10, 10),
                normal = BackgroundColorNormal
            };

            public static readonly GUILayoutOption[] HorizontalLayoutOptions = new GUILayoutOption[]
            {
                GUILayout.MinHeight(66f)
            };

            public static readonly GUIStyle Button = new GUIStyle(GUI.skin.button)
            {
                normal = BackgroundColorNormal,
                onNormal = BackgroundColorNormal,
                focused = BackgroundColorNormal,
                onFocused = BackgroundColorNormal,

                hover = BackgroundColorHover,
                onHover = BackgroundColorHover,
                active = BackgroundColorHover,
                onActive = BackgroundColorHover
            };

            public static readonly GUIStyle Icon = new GUIStyle()
            {
                fixedWidth = 25,
                fixedHeight = 25,
                alignment = TextAnchor.UpperCenter
            };

            public static readonly GUIStyle Name = new GUIStyle(GUI.skin.label)
            {
                fontSize = GUI.skin.label.fontSize + 6,
                wordWrap = true
            };

            public static readonly GUIStyle Description = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true
            };
        }
    }
}