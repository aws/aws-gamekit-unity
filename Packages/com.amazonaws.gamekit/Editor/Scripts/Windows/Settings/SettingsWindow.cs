// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// Displays the Settings window and receives its UI events.
    /// </summary>
    public class SettingsWindow : EditorWindow
    {
        private const string WINDOW_TITLE = "AWS GameKit Settings";

        private enum InitializationLevel
        {
            // Initialize() has not been called.
            // The GUI is empty and most public method calls are ignored (ex: OpenPage).
            Uninitialized,

            // Initialize() has been called, but some delayed setup will occur during the next OnGUI() call.
            // The GUI is empty and all public method calls are acted on.
            PartiallyInitialized,

            // Initialize() has been called and all setup is complete.
            // The GUI is drawn and all public method calls are acted on.
            FullyInitialized
        }

        // Data
        private SettingsModel _model;

        // State
        private InitializationLevel _initLevel = InitializationLevel.Uninitialized;
        
        // GUI Widgets
        private readonly PageContainerWidget _pageContainerWidget = new PageContainerWidget();
        private NavigationTreeWidget _navigationTreeWidget;

        // Events
        public static event OnWindowEnabled Enabled;

        public delegate void OnWindowEnabled(SettingsWindow enabledSettingsWindow);

        /// <summary>
        /// Open the Settings window and navigate to the specified page.
        /// </summary>
        public static void OpenPage(PageType pageType)
        {
            SettingsWindow window = GetWindow<SettingsWindow>();
            window.TryOpenPage(pageType);
        }

        /// <summary>
        /// Open the Settings window and navigate to the specified tab on the specified page.
        /// </summary>
        /// <remarks>
        /// If the specified tab does not exist on the page, then an Error will be logged and the page's currently selected tab will not change. The page will still be opened.
        /// </remarks>
        public static void OpenPageToTab(PageType pageType, string tabName)
        {
            SettingsWindow window = GetWindow<SettingsWindow>();
            window.TryOpenPage(pageType, tabName);
        }

        /// <summary>
        /// Navigate to the specified page if the window is initialized. Then optionally change the selected tab.
        /// </summary>
        private void TryOpenPage(PageType pageType, string tabName = null)
        {
            if (_initLevel == InitializationLevel.Uninitialized)
            {
                return;
            }

            Page page = _model.AllPages.GetPage(pageType);

            // Open the page
            _navigationTreeWidget.ClickItem(page);

            // Select the tab
            if (!string.IsNullOrEmpty(tabName))
            {
                page.SelectTab(tabName);
            }
        }

        private void OnNavigationTreeItemSelected(Page selectedPage)
        {
            _pageContainerWidget.ChangeTo(selectedPage);
        }

        private void OnEnable()
        {
            // Set title
            string windowTitle = WINDOW_TITLE;
            Texture windowIcon = EditorResources.Textures.WindowIcon.Get();
            titleContent = new GUIContent(windowTitle, windowIcon);

            Enabled?.Invoke(this);
        }

        /// <summary>
        /// Initialize this window so it can start drawing the GUI and acting on public methods (ex: OpenPage).
        ///
        /// This is effectively the window's constructor.
        /// </summary>
        public void Initialize(SettingsModel model)
        {
            _model = model;
            bool isFirstTimeWindowHasEverBeenOpened = !_model.SettingsWindowHasEverBeenOpened;

            if (isFirstTimeWindowHasEverBeenOpened)
            {
                // Create new navigation tree
                _model.NavigationTreeState = new TreeViewState();
                _navigationTreeWidget = new NavigationTreeWidget(_model.AllPages, _model.NavigationTreeState, false, OnNavigationTreeItemSelected);

                // Delay the rest of initialization until the first OnGUI call so the window's position is refreshed.
                // The position is (x=0, y=0) during the window's very first OnEnable() call ever (across all Unity sessions).
                // The position refreshes to the "real" value (near the center of the screen) after the first OnGUI() call.
                _initLevel = InitializationLevel.PartiallyInitialized;
            }
            else
            {
                // Restore navigation tree from the previous session
                _navigationTreeWidget = new NavigationTreeWidget(_model.AllPages, _model.NavigationTreeState, true, OnNavigationTreeItemSelected);

                // Set window bounds
                minSize = SettingsGUIStyles.Window.MinSize;

                _initLevel = InitializationLevel.FullyInitialized;
            }

            SettingsWindowUpdateController.AssignSettingsWindow(this);
        }

        /// <summary>
        /// Set the window's initial width & height.
        ///
        /// This only gets called the very first time the window is opened in this game project across all Unity sessions.
        /// After the first time, we let Unity automatically re-use the window's last size and position.
        ///
        /// This call needs to be delayed until at least one frame after the first OnGUI() call,
        /// otherwise the content may shift around when the window resizes.
        /// </summary>
        private void DelayedInitialize()
        {
            EditorWindowHelper.SetWindowSizeAndBounds(this, SettingsGUIStyles.Window.InitialSize, SettingsGUIStyles.Window.MinSize, maxSize);

            _model.SettingsWindowHasEverBeenOpened = true;
            _initLevel = InitializationLevel.FullyInitialized;
        }

        private void OnDisable()
        {
            _pageContainerWidget.CloseWindow();
        }

        private void OnGUI()
        {
            if (!IsReadyToDrawGUI())
            {
                if (_initLevel == InitializationLevel.PartiallyInitialized)
                {
                    DelayedInitialize();
                }

                GUILayout.Space(0f);
                return;
            }

            _model.SerializedObject.Update();
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawNavigationTree();
                EditorGUILayoutElements.VerticalDivider();
                DrawPageContents();
            }
            _model.SerializedObject.ApplyModifiedProperties();
        }

        private bool IsReadyToDrawGUI()
        {
            return _initLevel == InitializationLevel.FullyInitialized;
        }

        private void DrawNavigationTree()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.NavigationTree.VerticalLayout))
            {
                // The NavigationTreeWidget doesn't interact with EditorGUILayout.VerticalScope(), so we have to:

                // (1) manually specify the Rect for where to draw the tree:
                Vector2 topLeft = new Vector2(
                    x: 0f,
                    y: 0f
                );
                Vector2 topLeftWithMargin = new Vector2(
                    x: topLeft.x + SettingsGUIStyles.NavigationTree.VerticalLayout.margin.left,
                    y: topLeft.y + SettingsGUIStyles.NavigationTree.VerticalLayout.margin.top
                );
                Vector2 bottomRight = new Vector2(
                    x: SettingsGUIStyles.NavigationTree.FIXED_WIDTH,
                    y: position.height
                );
                Vector2 bottomRightWithMargin = new Vector2(
                    x: bottomRight.x - SettingsGUIStyles.NavigationTree.VerticalLayout.margin.right,
                    y: bottomRight.y - SettingsGUIStyles.NavigationTree.VerticalLayout.margin.bottom
                );

                Rect boxRect = new Rect(topLeftWithMargin, bottomRightWithMargin);
                _navigationTreeWidget.OnGUI(boxRect);

                // (2) add an empty GUI element to make the vertical group non-empty and therefore have a non-zero width:
                GUILayout.Space(0f);
            }
        }

        private void DrawPageContents()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.PageContainer.VerticalLayout, GUILayout.ExpandWidth(true)))
            {
                _pageContainerWidget.OnGUI();
            }
        }
    }
}
