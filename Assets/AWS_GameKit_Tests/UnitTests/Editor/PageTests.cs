// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// GameKit
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Windows.Settings;
using AWS.GameKit.Runtime.UnitTests;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class PageTests : GameKitTestBase
    {
        /// <summary>
        /// This page does NOT have any tabs. Therefore it does not override the method <see cref="Page.SelectTab"/>.
        /// </summary>
        private class PageWithoutTabs : Page
        {
            public override string DisplayName => nameof(PageWithoutTabs);

            protected override void DrawContent()
            {
                // do nothing
            }
        }

        /// <summary>
        /// This page DOES have tabs. Therefore it overrides the method <see cref="Page.SelectTab"/>.
        /// </summary>
        private class PageWithTabs : Page
        {
            public override string DisplayName => nameof(PageWithoutTabs);

            public override void SelectTab(string tabName)
            {
                // do nothing
                // all tab names are valid

                // Note: We are not calling base.SelectTab(), as we are instructed not to call it by the base method's documentation.
            }

            protected override void DrawContent()
            {
                // do nothing
            }
        }

        private class DummyDrawable : IDrawable
        {
            public void OnGUI()
            {
                // do nothing
            }
        }

        public void SelectTab_WhenSubclassDoesNotOverride_LogsError()
        {
            // arrange
            Page pageWithoutTabs = new PageWithoutTabs();
            string tabName = "DoesNotExist";
            string expectedErrorMessage = $"There is no tab named \"{tabName}\" on the page \"{nameof(PageWithoutTabs)}\". The page does not have any tabs.";

            // act
            pageWithoutTabs.SelectTab(tabName);

            // assert
            Assert.AreEqual(1, Log.Count);
            Assert.AreEqual(expectedErrorMessage, Log[0]);
        }

        public void SelectTab_WhenSubclassOverridesAndTabNameIsValid_DoesNotLogError()
        {
            // arrange
            Page pageWithTabs = new PageWithTabs();

            // act
            pageWithTabs.SelectTab("ValidTabName");

            // assert
            Assert.AreEqual(0, Log.Count);
        }
    }
}
