// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.UnitTests;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class TabWidgetTests : GameKitTestBase
    {
        private class DummyDrawable : IDrawable
        {
            public void OnGUI()
            {
                // do nothing
            }
        }

        [Test]
        public void Initialize_WhenTabsArrayIsEmpty_ThrowsArgumentException()
        {
            // arrange
            TabWidget tabWidget = new TabWidget();
            Tab[] emptyTabArray = new Tab[] { };

            // act / assert
            Assert.Throws<ArgumentException>(() =>
                {
                    tabWidget.Initialize(emptyTabArray, GUI.ToolbarButtonSize.Fixed);
                }
            );
        }

        [Test]
        public void Initialize_WhenTabsArrayNonEmpty_Succeeds()
        {
            // arrange
            TabWidget tabWidget = new TabWidget();
            Tab[] nonEmptyTabArray = new Tab[]
            {
                new Tab("dummyTab", new DummyDrawable())
            };

            // act / assert
            Assert.DoesNotThrow(() =>
                {
                    tabWidget.Initialize(nonEmptyTabArray, GUI.ToolbarButtonSize.Fixed);
                }
            );
        }

        [Test]
        public void SelectTab_WhenTabNameDoesNotExist_LogsError()
        {
            // arrange
            string existingTabName = "dummyTab";
            string nonExistentTabName = "nonExistentTabName";

            TabWidget tabWidget = new TabWidget();
            Tab[] nonEmptyTabArray = new Tab[]
            {
                new Tab("dummyTab", new DummyDrawable())
            };
            tabWidget.Initialize(nonEmptyTabArray, GUI.ToolbarButtonSize.Fixed);

            string expectedErrorMessage = $"There is no tab named \"{nonExistentTabName}\". Valid tab names are: \"{existingTabName}\". The selected tab will remain \"{existingTabName}\".";

            // act
            tabWidget.SelectTab(nonExistentTabName);

            // assert
            Assert.AreEqual(1, Log.Count);
            Assert.AreEqual(expectedErrorMessage, Log[0]);
        }

        [Test]
        public void SelectTab_WhenTabNameExists_DoesNotLogError()
        {
            // arrange
            TabWidget tabWidget = new TabWidget();
            Tab[] nonEmptyTabArray = new Tab[]
            {
                new Tab("dummyTab", new DummyDrawable())
            };
            tabWidget.Initialize(nonEmptyTabArray, GUI.ToolbarButtonSize.Fixed);

            // act
            tabWidget.SelectTab("dummyTab");

            // assert
            Assert.AreEqual(0, Log.Count);
        }
    }
}
