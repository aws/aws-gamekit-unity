// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.GUILayoutExtensions
{
    /// <summary>
    /// A tab to be displayed in a TabViewWidget.
    /// </summary>
    public class Tab
    {
        public string DisplayName { get; }
        public IDrawable TabContent { get; }

        /// <summary>
        /// Create a new Tab.
        /// </summary>
        /// <param name="displayName">The string to show on this tab's button in the tab selector.</param>
        /// <param name="tabContent">The content to draw while this tab is selected in the TabViewWidget.</param>
        public Tab(string displayName, IDrawable tabContent)
        {
            DisplayName = displayName;
            TabContent = tabContent;
        }
    }
}