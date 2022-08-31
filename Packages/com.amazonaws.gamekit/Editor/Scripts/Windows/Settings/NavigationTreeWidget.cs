// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Collections.Generic;

// Unity
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// A GUI element which displays a navigable "table of contents" for the AWS GameKit Settings window in a collapsible tree format.
    /// </summary>
    public sealed class NavigationTreeWidget : TreeView
    {
        public delegate void OnSingleClickedPage(Page selectedPage);

        private readonly OnSingleClickedPage _onSingleClickedPageCallback;
        private TreeViewItem _rootItem;
        private List<TreeViewItem> _treeViewItems;
        private readonly Dictionary<Page, int> _pageToItemIdMap = new Dictionary<Page, int>();

        /// <summary>
        /// Create a new NavigationTreeWidget.
        /// </summary>
        /// <param name="allPages">All of the pages that can be navigated to.</param>
        /// <param name="treeViewState">The state to restore from. For example, which item was last selected, which items are collapsed/expanded, etc.</param>
        /// <param name="isBeingRestored">True if the treeViewState parameter is restoring the tree view from a saved state. False if the treeViewState is a new/default object.</param>
        /// <param name="onSingleClickedPageCallback">A callback function to invoke each time an item is single clicked.</param>
        public NavigationTreeWidget(AllPages allPages, TreeViewState treeViewState, bool isBeingRestored, OnSingleClickedPage onSingleClickedPageCallback)
            : base(treeViewState)
        {
            _onSingleClickedPageCallback = onSingleClickedPageCallback;

            BuildTreeViewItems(allPages);
            Reload();
        
            if (isBeingRestored)
            {
                // Restore the last selected element
                IList<int> selectedIds = GetSelection();
                SetSelection(selectedIds, TreeViewSelectionOptions.FireSelectionChanged);
        
                if (selectedIds.Count > 0)
                {
                    // There should be exactly one item selected because CanMultiSelect() returns false, so select that item here:
                    SingleClickedItem(selectedIds[0]);
                }
            }
            else
            {
                // Set the default initial view
                ExpandAll();
        
                // Select the first item
                int startingPageId = 1;
                ClickItem(startingPageId);
            }
        }

        /// <summary>
        /// Simulate a mouse click on the page's item in the tree view.
        /// </summary>
        public void ClickItem(Page page)
        {
            int itemId;
            if (!_pageToItemIdMap.TryGetValue(page, out itemId))
            {
                Debug.LogWarning($"There is no TreeViewItem corresponding to the pageType: {page}. Selecting the first page instead.");
                itemId = 1;
            }

            ClickItem(itemId);
        }

        protected override TreeViewItem BuildRoot()
        {
            SetupParentsAndChildrenFromDepths(_rootItem, _treeViewItems);
            return _rootItem;
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        /// <summary>
        /// Draw the row's text on the screen. This method is called once per visible row.
        /// </summary>
        protected override void RowGUI(RowGUIArgs args)
        {
            rowHeight = SettingsGUIStyles.NavigationTree.RowHeight;

            float indent = GetContentIndent(args.item);
            GUIStyle style = new GUIStyle(SettingsGUIStyles.NavigationTree.RowText)
            {
                contentOffset = new Vector2(indent, 0)
            };

            GUI.Label(args.rowRect, args.label, style);
        }

        /// <summary>
        /// This callback function is invoked whenever an item is clicked in the tree view.
        /// </summary>
        protected override void SingleClickedItem(int selectedId)
        {
            base.SingleClickedItem(selectedId);

            // Invoke callback delegate with the clicked-on page
            NavigationTreeItem selectedItem = (NavigationTreeItem)FindItem(selectedId, _rootItem);
            _onSingleClickedPageCallback(selectedItem.Page);
        }

        /// <summary>
        /// Simulate a mouse click on the specified item in the tree view.
        /// </summary>
        private void ClickItem(int itemId)
        {
            // Highlight the clicked-on row
            FrameItem(itemId);
            SetSelection(new List<int> { itemId }, TreeViewSelectionOptions.FireSelectionChanged);

            // Invoke the callback delegate
            SingleClickedItem(itemId);
        }

        /// <summary>
        /// Define the order in which the pages are displayed in the collapsible navigation tree.
        /// </summary>
        private void BuildTreeViewItems(AllPages allPages)
        {
            int id = 0;
            _rootItem = new TreeViewItem { id = id, depth = -1, displayName = "Root" };

            _treeViewItems = new List<TreeViewItem>()
            {
                // Before features:
                new NavigationTreeItem(++id, 0, allPages.EnvironmentAndCredentialsPage),
                new NavigationTreeItem(++id, 0, allPages.AllFeaturesPage),

                // Features:
                new NavigationTreeItem(++id, 1, allPages.IdentityAndAuthenticationPage),
                new NavigationTreeItem(++id, 1, allPages.GameStateCloudSavingPage),
                new NavigationTreeItem(++id, 1, allPages.AchievementsPage),
                new NavigationTreeItem(++id, 1, allPages.UserGameplayDataPage),

                // After features:
                new NavigationTreeItem(++id, 0, allPages.LogPage),
            };

            foreach (TreeViewItem item in _treeViewItems)
            {
                NavigationTreeItem navigationItem = (NavigationTreeItem)item;
                _pageToItemIdMap.Add(navigationItem.Page, navigationItem.id);
            }
        }
    }
}
