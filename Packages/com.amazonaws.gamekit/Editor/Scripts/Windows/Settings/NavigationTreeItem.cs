// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor.IMGUI.Controls;

namespace AWS.GameKit.Editor.Windows.Settings
{
    public sealed class NavigationTreeItem : TreeViewItem
    {
        public Page Page { get; }

        public NavigationTreeItem(int id, int depth, Page page) : base(id, depth)
        {
            Page = page;
            displayName = page.DisplayName;
        }
    }
}
