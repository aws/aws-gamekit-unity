// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// GameKit
using AWS.GameKit.Common;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class SingletonTests : GameKitTestBase
    {
        [Test]
        public void Get_StandardCase_ReturnsPointerToInstance()
        {
            // act
            string valueReturned = Singleton<FakeClass>.Get().GetTestValue();

            // assert
            Assert.AreEqual(FakeClass.TEST_VALUE, valueReturned);
        }

        [Test]
        public void Get_WhenCalledTwice_AlwaysReturnsSameInstance()
        {
            // assert
            Assert.IsTrue(ReferenceEquals(Singleton<FakeClass>.Get(), Singleton<FakeClass>.Get()), "The instance that is assigned should be the same between calls");
        }
    }

    public class FakeClass
    {
        public const string TEST_VALUE = "test value";

        public string GetTestValue()
        {
            return TEST_VALUE;
        }
    }
}
