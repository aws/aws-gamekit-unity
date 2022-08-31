// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Libraries
using System;
using System.Linq;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.GUILayoutExtensions
{
    /// <summary>
    /// A GUI element which displays a tab selector at the top and the selected tab's contents below.
    ///
    /// This is a wrapper around the GUILayout.Toolbar() function: https://docs.unity3d.com/ScriptReference/GUILayout.Toolbar.html
    /// </summary>
    [Serializable]
    public class TabWidget : IDrawable
    {
        // State
        [SerializeField] private int _selectedTabId;

        // Data
        private string[] _tabNames;
        private IDrawable[] _tabContents;
        private GUI.ToolbarButtonSize _tabSelectorButtonSize;

        /// <summary>
        /// Initializes a tab widget.
        ///
        /// See Unity's GUILayout.Toolbar() for details on the parameters: https://docs.unity3d.com/ScriptReference/GUILayout.Toolbar.html
        /// </summary>
        /// <param name="tabs">An array of all the tabs to display with this tab widget. It must contain at least one element.</param>
        /// <param name="tabSelectorButtonSize">Determines how the toolbar button size is calculated.</param>
        /// <exception cref="ArgumentException">Thrown when the <see cref="tabs"/> array is empty.</exception>
        public void Initialize(Tab[] tabs, GUI.ToolbarButtonSize tabSelectorButtonSize)
        {
            if (tabs.Length == 0)
            {
                throw new ArgumentException($"The '{nameof(tabs)}' parameter is empty. It must contain at least one element.");
            }

            // Data
            _tabNames = tabs.Select(tab => tab.DisplayName).ToArray();
            _tabContents = tabs.Select(tab => tab.TabContent).ToArray();

            // Clamp the selected tab id in case our previously selected tab is out of bounds
            _selectedTabId = Mathf.Clamp(_selectedTabId, 0, tabs.Length);
            _tabSelectorButtonSize = tabSelectorButtonSize;
        }

        /// <summary>
        /// Change the currently selected tab to the named tab.
        /// </summary>
        /// <param name="tabName">The name of the tab to select. Must be the name of one of the tabs passed into the constructor.
        /// Otherwise, an Error will be logged and the tab selection will not change.</param>
        public void SelectTab(string tabName)
        {
            int nextTabId = Array.IndexOf(_tabNames, tabName);

            if (nextTabId < 0)
            {
                // Wrap the tab names in quotes, like: "NameOne", "NameTwo", "NameThree"
                string validTabNames = $"\"{string.Join("\", \"", _tabNames)}\"";
                string currentTabName = _tabNames[_selectedTabId];
                Logging.LogError($"There is no tab named \"{tabName}\". Valid tab names are: {validTabNames}. The selected tab will remain \"{currentTabName}\".");

                return;
            }

            _selectedTabId = nextTabId;
        }

        /// <summary>
        /// Draw the tab selector and the contents of the selected tab.
        ///
        /// The tab selector buttons will use the default <c>button</c> style from the current UnityEngine.GUISkin.
        /// </summary>
        public void OnGUI()
        {
            OnGUI(GUI.skin.button);
        }

        /// <summary>
        /// Draw the tab selector and the contents of the selected tab.
        /// </summary>
        /// <param name="tabSelectorStyle">The style to use for the tab selector buttons.</param>
        public void OnGUI(GUIStyle tabSelectorStyle)
        {
            DrawTabSelector(tabSelectorStyle);
            DrawTabContent();
        }

        private void DrawTabSelector(GUIStyle tabSelectorStyle)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                
                _selectedTabId = EditorGUILayoutElements.CreateFeatureToolbar(_selectedTabId, _tabNames, _tabSelectorButtonSize, tabSelectorStyle);

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawTabContent()
        {
            IDrawable selectedTab = _tabContents[_selectedTabId];
            selectedTab.OnGUI();
        }
    }
}