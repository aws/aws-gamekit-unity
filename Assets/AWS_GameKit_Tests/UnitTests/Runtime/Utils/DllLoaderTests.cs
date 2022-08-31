// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class DllLoaderTests : GameKitTestBase
    {
        const int TEST_RETURN_VALUE = 42;
        const int TEST_ERROR_VALUE = -1;
        const string TEST_ERROR_STRING = "DLL not linked";

        int _tryDllCallCount = 0;

        GameKitFeatureWrapperBaseTarget _target;

        [SetUp]
        public void SetUp()
        {
            _tryDllCallCount = 0;
            _target = new GameKitFeatureWrapperBaseTarget();
        }


        [Test]
        public void TryDllWithNoReturn_StandardCase_CallsSuccessfully()
        {
            // act
            DllLoader.TryDll(TestDllMethod, nameof(TestDllMethod));

            // assert
            Assert.AreEqual(1, _tryDllCallCount, $"Expected TestDllMethod called one time. Count = {_tryDllCallCount}");
        }

        [Test]
        public void TryDllWithNoReturn_WhenDllMethodExceptionThrown_EatsTheException()
        {
            // act
            Assert.DoesNotThrow(() => DllLoader.TryDll(TestDllMethodException, nameof(TestDllMethodException)), "Expected TryDll eats the DllNotFoundException.");

            //assert
            LinkedList<string> exceptionLogs = new LinkedList<string>(Logging.LogQueue);
            Assert.IsTrue(exceptionLogs.Count == 1);
            Assert.IsTrue(exceptionLogs.First.Value.Contains(TEST_ERROR_STRING));
        }

        [Test]
        public void TryDllWithReturn_StandardCase_CallsSuccessfully()
        {
            // act
            int valueReturned = DllLoader.TryDll(TestDllMethodWithReturn, nameof(TestDllMethodWithReturn), TEST_ERROR_VALUE);

            // assert
            Assert.AreEqual(1, _tryDllCallCount, $"Expected TestDllMethodWithReturn called one time. Count = {_tryDllCallCount}");
            Assert.AreEqual(TEST_RETURN_VALUE, valueReturned);
        }

        [Test]
        public void TryDllWithReturn_WhenDllMethodExceptionThrown_EatsTheException()
        {
            // act
            int valueReturned = DllLoader.TryDll(TestDllMethodWithReturnException, nameof(TestDllMethodWithReturnException), TEST_ERROR_VALUE);

            // assert
            Assert.AreEqual(TEST_ERROR_VALUE, valueReturned);
            
            LinkedList<string> exceptionLogs = new LinkedList<string>(Logging.LogQueue);
            Assert.IsTrue(exceptionLogs.Count == 1);
            Assert.IsTrue(exceptionLogs.First.Value.Contains(TEST_ERROR_STRING));
        }

        void TestDllMethod()
        {
            ++_tryDllCallCount;
        }

        int TestDllMethodWithReturn()
        {
            ++_tryDllCallCount;

            return TEST_RETURN_VALUE;
        }

        void TestDllMethodException()
        {
            throw new DllNotFoundException();
        }

        int TestDllMethodWithReturnException()
        {
            throw new DllNotFoundException();
        }
    }
}
