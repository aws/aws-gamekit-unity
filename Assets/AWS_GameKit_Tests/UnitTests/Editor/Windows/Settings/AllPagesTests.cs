// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKIt
using AWS.GameKit.Editor.Windows.Settings;
using AWS.GameKit.Runtime.UnitTests;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class AllPagesTests : GameKitTestBase
    {
        [Test]
        public void GetPage_EveryPageType_Exists()
        {
            // Arrange
            AllPages allPages = new AllPages();

            // Act/Assert
            foreach (PageType pageType in Enum.GetValues(typeof(PageType)))
            {
                Assert.DoesNotThrow(() => allPages.GetPage(pageType));
            }
        }
    }
}