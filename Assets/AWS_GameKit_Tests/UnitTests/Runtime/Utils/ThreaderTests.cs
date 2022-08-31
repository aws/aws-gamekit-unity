// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class ThreaderTests : GameKitTestBase
    {
        const string TEST_DESCRIPTION = "test description";

        static int _callCount;

        Threader _target;
        
        Func<string, string> _simpleFunction = (description) => string.Empty;  
        Func<string> _simpleNoDescriptionFunction = () => string.Empty;
        Action<string> _simpleNoResultFunction = (description) => { };
        Action _simpleNoDescriptionOrResultFunction = () => { };
        Action<string> _callback = (result) => { ++_callCount; };
        Action _callbackNoResult = () => { ++_callCount; };
        Action<string> _callback_exception = (result) => { throw new Exception(); };
        Func<string, Action<string>, string> _recurringFunction = (description, callback) => { 
            callback(description); return string.Empty; 
        };

        [SetUp]
        public void SetUp()
        {
            _target = new Threader();
            _callCount = 0;
        }

        [Test]
        public void WaitForThreadedWork_CallsSuccessfully()
        {
            // arrange
            _target.Call((string param) => { System.Threading.Thread.Sleep(250); }, TEST_DESCRIPTION, _callbackNoResult);
            Assert.AreEqual(0, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");

            // act
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(1, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");
        }

        [Test]
        public void CallSimple_StandardCase_CallsSuccessfully()
        {
            // act
            _target.Call(_simpleFunction, TEST_DESCRIPTION, _callback);
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(1, _target.WaitingQueueCount, $"Expected one callback in waiting queue, count = {_target.WaitingQueueCount}");
        }

        [Test]
        public void CallRecurring_StandardCase_CallsSuccessfully()
        {
            // act
            _target.Call(_recurringFunction, TEST_DESCRIPTION, _callback, _callback);
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(2, _target.WaitingQueueCount, $"Expected two callbacks in waiting queue, count = {_target.WaitingQueueCount}");
        }

        [Test]
        public void CallSimpleWithNoDescription_StandardCase_CallsSuccessfully()
        {
            // act
            _target.Call(_simpleNoDescriptionFunction, _callback);
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(1, _target.WaitingQueueCount, "Expected one callback in waiting queue, count = {0}", _target.WaitingQueueCount);
        }

        [Test]
        public void CallSimpleJobNoResult_StandardCase_CallsSuccessfully()
        {
            // act
            _target.Call(_simpleNoResultFunction, TEST_DESCRIPTION, _callbackNoResult);
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(1, _target.WaitingQueueCount, "Expected one callback in waiting queue, count = {0}", _target.WaitingQueueCount);
        }

        [Test]
        public void CallSimpleJobNoDescriptionOrResult_StandardCase_CallsSuccessfully()
        {
            // act
            _target.Call(_simpleNoDescriptionOrResultFunction, _callbackNoResult);
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(1, _target.WaitingQueueCount, "Expected one callback in waiting queue, count = {0}", _target.WaitingQueueCount);
        }

        [Test]
        public void Awake_StandardCase_CallsSuccessfully()
        {
            // arrange
            _target.Call(_simpleFunction, TEST_DESCRIPTION, _callback);
            _target.WaitForThreadedWork_TestOnly();
            Assert.AreEqual(1, _target.WaitingQueueCount, $"Expected one callback in waiting queue during test set up, count = {_target.WaitingQueueCount}");

            // act
            _target.Awake();

            // assert
            Assert.AreEqual(0, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");
        }

        [Test]
        public void Awake_StandardCase_NoAsyncWorkRaceConditions()
        {
            // arrange
            _target.Call((string param) => { System.Threading.Thread.Sleep(250); }, TEST_DESCRIPTION, _callbackNoResult);
            Assert.AreEqual(0, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");

            // act
            _target.Awake();
            _target.WaitForThreadedWork_TestOnly();

            // assert
            Assert.AreEqual(0, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");
        }

        [Test]
        public void Update_StandardCase_CallsSuccessfully()
        {
            // arrange
            _target.Call(_simpleFunction, TEST_DESCRIPTION, _callback);
            _target.WaitForThreadedWork_TestOnly();
            Assert.AreEqual(1, _target.WaitingQueueCount, $"Expected one callback in waiting queue during test set up, count = {_target.WaitingQueueCount}");

            // act
            _target.Update();

            // assert
            Assert.AreEqual(0, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");
            Assert.AreEqual(1, _callCount, "Expected callback called 1 time, count = {0}", _callCount);
        }

        [Test]
        public void Update_WhenAndExceptionIsThrown_TheQueueIsStillCleared()
        {
            // arrange
            _target.Call(_simpleFunction, TEST_DESCRIPTION, _callback_exception);
            _target.WaitForThreadedWork_TestOnly();
            Assert.AreEqual(1, _target.WaitingQueueCount, $"Expected one callback in waiting queue during test set up, count = {_target.WaitingQueueCount}");

            // act
            Assert.Throws<Exception>(() => _target.Update());

            // assert
            Assert.AreEqual(0, _target.WaitingQueueCount, $"Expected zero callbacks in waiting queue, count = {_target.WaitingQueueCount}");
        }

        [Test]
        public void Update_WithASimpleJobNoResult_CallsSuccessfully()
        {
            // arrange
            _target.Call(_simpleNoResultFunction, TEST_DESCRIPTION, _callbackNoResult);
            _target.WaitForThreadedWork_TestOnly();
            Assert.AreEqual(1, _target.WaitingQueueCount, "Expected one callback in waiting queue during test set up, count = {0}", _target.WaitingQueueCount);

            // act
            _target.Update();

            // assert
            Assert.AreEqual(0, _target.WaitingQueueCount, "Expected zero callbacks in waiting queue, count = {0}", _target.WaitingQueueCount);
            Assert.AreEqual(1, _callCount, "Expected callback called 1 time, count = {0}", _callCount);
        }
    }
}
