// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.Utils;

namespace AWS.GameKit.Editor
{
    /// <summary>
    /// The "AWS GameKit" dropdown menu found along the top toolbar (alongside File, Edit, Assets, etc.).
    /// </summary>
    public static class ToolbarMenu
    {
        /// <summary>
        /// Names of the items in the dropdown menu.
        /// </summary>
        private static class ItemNames
        {
            public const string QUICK_ACCESS = AWS_GAMEKIT + "/QuickAccess";
            public const string SETTINGS = AWS_GAMEKIT + "/Settings";
            public const string DOCUMENTATION = AWS_GAMEKIT + "/Documentation";

            private const string AWS_GAMEKIT = "AWS GameKit";
        }

        /// <summary>
        /// Sort order of the items in the dropdown menu. The menu is sorted in ascending order.
        /// </summary>
        private static class ItemPriorities
        {
            private const int TOP_OF_LIST = 0;

            // Unity adds a divider between menu items when their priority is more than 10 apart.
            private const int ADD_DIVIDER = 11;

            // First group
            public const int QUICK_ACCESS = TOP_OF_LIST;
            public const int SETTINGS = QUICK_ACCESS + 1;

            // Second group
            public const int DOCUMENTATION = ADD_DIVIDER + SETTINGS;
        }

        [MenuItem(ItemNames.QUICK_ACCESS, priority = ItemPriorities.QUICK_ACCESS)]
        public static void OpenQuickAccess()
        {
            GameKitEditorManager.Get().QuickAccessWindowController.GetOrCreateQuickAccessWindow();
        }

        [MenuItem(ItemNames.SETTINGS, priority = ItemPriorities.SETTINGS)]
        public static void OpenSettings()
        {
            GameKitEditorManager.Get().SettingsWindowController.GetOrCreateSettingsWindow();
        }

        [MenuItem(ItemNames.DOCUMENTATION, priority = ItemPriorities.DOCUMENTATION)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(DocumentationURLs.GAMEKIT_HOME);
        }
    }
}
