// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.UnitTests;
using AWS.GameKit.Editor.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class EditorWindowHelperTests : GameKitTestBase
    {
        [Test]
        public void GetInspectorWindowType_Always_ReturnsNonNull()
        {
            // act
            Type inspectorWindowType = EditorWindowHelper.GetInspectorWindowType();

            // assert
            Assert.IsNotNull(inspectorWindowType, "Expected the InspectorWindow's Type to be non-null, but it was null. " +
                                                  "Please see the warning logged from GetInspectorWindowType() for details on how to fix this failure.");
        }
    }
}
