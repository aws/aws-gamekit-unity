// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// The base class for all pages displayed in the AWS GameKit Settings window.
    /// </summary>
    [Serializable]
    public abstract class Page
    {
        /// <summary>
        /// The name to display for this page in the navigation tree, the page title, and elsewhere.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Change the currently selected tab to the named tab.
        /// </summary>
        /// <remarks>
        /// If the named tab does not exist, then an Error will be logged and the page's currently selected tab will not change.<br/><br/>
        ///
        /// When overriding this method, do not call <c>base.SelectTab(tabName)</c>.
        /// </remarks>
        /// <param name="tabName">The name of the tab to select.</param>
        public virtual void SelectTab(string tabName)
        {
            Logging.LogError($"There is no tab named \"{tabName}\" on the page \"{GetType().Name}\". The page does not have any tabs.");
        }

        /// <summary>
        /// This method is called each time the page is switched to from another page.
        ///
        /// This method does nothing by default. It is not necessary to call `base.OnNavigatedTo()` when overriding this method.
        /// </summary>
        public virtual void OnNavigatedTo()
        {
            // empty call
        }

        /// <summary>
        /// This method is called each time the page is switched out with another page.
        ///
        /// This method does nothing by default. It is not necessary to call `base.OnNavigatedFrom()` when overriding this method.
        /// </summary>
        public virtual void OnNavigatedFrom()
        {
            // empty call
        }

        /// <summary>
        /// Draw the page's title and content.
        /// </summary>
        public void OnGUI()
        {
            DrawTitle();

            GUILayout.Space(SettingsGUIStyles.Page.SpaceAfterTitle);

            DrawContent();
        }

        /// <summary>
        /// Get the title of this page.
        ///
        /// By default the title is "{Environment} > {Region} > {GetTitleSuffix()}".
        /// </summary>
        protected virtual IList<string> GetTitle()
        {
            List<string> titleParts = new List<string>() { "Environment", "Region" };
            titleParts.AddRange(GetTitleSuffix());

            return titleParts;
        }

        /// <summary>
        /// Get the portion of this page's title that comes after the "{Environment} > {Region}" prefix.
        ///
        /// By default returns this page's DisplayName.
        /// </summary>
        protected virtual IList<string> GetTitleSuffix()
        {
            return new List<string>() { DisplayName };
        }

        /// <summary>
        /// Draw the page's content, which is everything below the title.
        /// </summary>
        protected abstract void DrawContent();

        protected virtual void DrawTitle()
        {
            GUIStyle titleStyle = SettingsGUIStyles.Page.Title;
            EditorGUILayout.LabelField(DisplayName, titleStyle);
        }
    }
}
