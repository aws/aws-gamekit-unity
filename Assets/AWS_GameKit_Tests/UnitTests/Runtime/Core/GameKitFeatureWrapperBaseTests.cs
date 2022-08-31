// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class GameKitFeatureWrapperBaseTests : GameKitTestBase
    {
        GameKitFeatureWrapperBaseTarget _target;

        [SetUp]
        public void SetUp()
        {
            _target = new GameKitFeatureWrapperBaseTarget();
        }

        [Test]
        public void GetInstance_StandardCase_ReturnsPointer()
        {
            // act
            IntPtr valueReturned = _target.GetInstance();

            // assert
            Assert.AreEqual(1, _target.CreateCallCount, $"Expected that Create is called one time. Count = {_target.CreateCallCount}");
            Assert.AreEqual(_target.TestIntPtr, valueReturned);
        }

        [Test]
        public void GetInstance_WhenInstanceIsNotZero_CreateIsNotCalled()
        {
            // arrange
            IntPtr valueReturnedFirstCall = _target.GetInstance();

            // act
            IntPtr valueReturnedSecondCall = _target.GetInstance();

            // assert
            Assert.AreEqual(1, _target.CreateCallCount, $"Expected that Create is called one time. Count = {_target.CreateCallCount}");
            Assert.AreEqual(valueReturnedFirstCall, valueReturnedSecondCall);
            Assert.AreEqual(_target.TestIntPtr, valueReturnedSecondCall);
        }

        [Test]
        public void Release_StandardCase_CallsSuccessfully()
        {
            // arrange
            _target.GetInstance();

            // act
            _target.Release();

            // assert
            Assert.AreEqual(1, _target.ReleaseCallCount, $"Expected that Release is called one time. Count = {_target.ReleaseCallCount}");
        }

        [Test]
        public void Release_WhenInstanceIsNotZero_ReleaseIsNotCalled()
        {
            // act
            _target.Release();

            // assert
            Assert.AreEqual(0, _target.ReleaseCallCount, $"Expected that Release is called zero times. Count = {_target.ReleaseCallCount}");
        }
    }

    public class GameKitFeatureWrapperBaseTarget : GameKitFeatureWrapperBase
    {
        public int CreateCallCount = 0;
        public int ReleaseCallCount = 0;

        public IntPtr TestIntPtr => _testPtr;

        private IntPtr _testPtr;
        private GCHandle _handle;

        public GameKitFeatureWrapperBaseTarget()
        {
            GCHandle _handle = GCHandle.Alloc(this);
            _testPtr = (IntPtr)_handle;
        }

        ~GameKitFeatureWrapperBaseTarget()
        {
            _handle.Free();
        }

        public new IntPtr GetInstance() => base.GetInstance();

        protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb)
        {
            ++CreateCallCount;

            return _testPtr;
        }
        protected override void Release(IntPtr instance)
        {
            ++ReleaseCallCount;
        }
    }
}
