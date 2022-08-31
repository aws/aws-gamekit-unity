// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;
using Moq;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class GameKitFeatureBaseTests : GameKitTestBase
    {
        const string TEST_DESCRIPTION = "test description";

        Func<string, string> _testSimpleFunction = (x) => string.Empty;
        Func<string> _testSimpleWithNoDescriptionFunction = () => string.Empty;
        Action<string> _testSimpleWithNoResultFunction = (x) => { };
        Action _testSimpleWithNoDescriptionOrResultFunction = () => { };
        Func<string, Action<string>, string> _testRecurringFunction = (x, y) => string.Empty;
        Action<string> _testCallback = (x) => { };
        Action _testNoResultCallback = () => { };

        GameKitFeatureBaseTarget _target;

        Mock<Threader> _threaderMock;

        [SetUp]
        public void SetUp()
        {
            _threaderMock = new Mock<Threader>();
            _target = new GameKitFeatureBaseTarget();
            _target.SetThreader(_threaderMock.Object);
        }

        [Test]
        public void Awake_StandardCase_CallsSuccessfully()
        {
            // act
            _target.OnInitialize();

            // assert
            _threaderMock.Verify(m => m.Awake(), Times.Once, "Expected Awake for threader to be called once");

            Assert.IsTrue(_target.IsReady);
            Assert.AreEqual(1, _target.InitializedCallCount, $"Expected Initialize called one time. Count = {_target.InitializedCallCount}");
        }

        [Test]
        public void OnDestroy_StandardCase_CallsSuccessfully()
        {
            // act
            _target.OnDispose();

            // assert
            Assert.IsFalse(_target.IsReady);
            Assert.AreEqual(1, _target.DestroyCallCount, $"Expected Destroy called one time. Count = {_target.DestroyCallCount}");
            Assert.AreEqual(1, _target.FeatureWrapper.ReleaseCallCount, $"Expected the feature wrapper's Release method called one time. Count = {_target.FeatureWrapper.ReleaseCallCount}");
        }

        [Test]
        public void Update_WhenClassIsReady_CallsSuccessfully()
        {
            // arrange
            _target.OnInitialize();

            // act
            _target.Update();

            // assert
            _threaderMock.Verify(m => m.Update(), Times.Once, "Expected Update for threader to be called once");

            Assert.IsTrue(_target.IsReady);
            Assert.AreEqual(1, _target.UpdateCallCount, $"Expected Update called one time. Count = {_target.UpdateCallCount}");
        }

        [Test]
        public void Update_WhenClassIsNotReady_IsNotCalled()
        {
            // act
            _target.Update();

            // assert
            _threaderMock.Verify(m => m.Update(), Times.Never, "Expected Update for threader never called");

            Assert.IsFalse(_target.IsReady);
            Assert.AreEqual(0, _target.UpdateCallCount, $"Expected Update called zero times. Count = {_target.UpdateCallCount}");
        }

        [Test]
        public void CallSimple_WhenClassIsReady_CalledSuccessfully()
        {
            // arrange
            _target.OnInitialize();

            // act
            _target.Call(_testSimpleFunction, TEST_DESCRIPTION, _testCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleFunction, TEST_DESCRIPTION, _testCallback), Times.Once, "Expected one Threader Call");

            Assert.IsTrue(_target.IsReady);
        }

        [Test]
        public void CallSimple_WhenClassIsNotReady_IsNotCalled()
        {
            // arrange
            string errorMessage = $"The Call method of the AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTarget feature, called in the CallSimple_WhenClassIsNotReady_IsNotCalled method " +
                "of AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTests class, has not been initialized yet. Please make sure AWS.GameKit.Runtime.Core.GameKitManager is attached as a component and enabled.";

            // act
            _target.Call(_testSimpleFunction, TEST_DESCRIPTION, _testCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleFunction, TEST_DESCRIPTION, _testCallback), Times.Never, "Expected no Threader Call");

            Assert.IsFalse(_target.IsReady);
            Assert.IsTrue(Log.Count == 1);
            Assert.AreEqual(errorMessage, Log[0]);
        }

        [Test]
        public void CallSimpleWithNoDescription_WhenClassIsReady_CalledSuccessfully()
        {
            // arrange
            _target.OnInitialize();

            // act
            _target.Call(_testSimpleWithNoDescriptionFunction, _testCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleWithNoDescriptionFunction, _testCallback), Times.Once, "Expected one Threader Call");

            Assert.IsTrue(_target.IsReady);
        }

        [Test]
        public void CallSimpleWithNoDescription_WhenClassIsNotReady_IsNotCalled()
        {
            // arrange
            string errorMessage = $"The Call method of the AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTarget feature, called in the CallSimpleWithNoDescription_WhenClassIsNotReady_IsNotCalled method " +
                "of AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTests class, has not been initialized yet. Please make sure AWS.GameKit.Runtime.Core.GameKitManager is attached as a component and enabled.";

            // act
            _target.Call(_testSimpleWithNoDescriptionFunction, _testCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleWithNoDescriptionFunction, _testCallback), Times.Never, "Expected no Threader Call");

            Assert.IsFalse(_target.IsReady);
            Assert.IsTrue(Log.Count == 1);
            Assert.AreEqual(errorMessage, Log[0]);
        }

        [Test]
        public void CallSimpleWithNoResult_WhenClassIsReady_CalledSuccessfully()
        {
            // arrange
            _target.OnInitialize();

            // act
            _target.Call(_testSimpleWithNoResultFunction, TEST_DESCRIPTION, _testNoResultCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleWithNoResultFunction, TEST_DESCRIPTION, _testNoResultCallback), Times.Once, "Expected one Threader Call");

            Assert.IsTrue(_target.IsReady);
        }

        [Test]
        public void CallSimpleWithNoResult_WhenClassIsNotReady_IsNotCalled()
        {
            // arrange
            string errorMessage = $"The Call method of the AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTarget feature, called in the CallSimpleWithNoResult_WhenClassIsNotReady_IsNotCalled method " +
                "of AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTests class, has not been initialized yet. Please make sure AWS.GameKit.Runtime.Core.GameKitManager is attached as a component and enabled.";

            // act
            _target.Call(_testSimpleWithNoResultFunction, TEST_DESCRIPTION, _testNoResultCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleWithNoResultFunction, TEST_DESCRIPTION, _testNoResultCallback), Times.Never, "Expected no Threader Call");

            Assert.IsFalse(_target.IsReady);
            Assert.IsTrue(Log.Count == 1);
            Assert.AreEqual(errorMessage, Log[0]);
        }

        [Test]
        public void CallSimpleWithNoDescriptionOrResult_WhenClassIsReady_CalledSuccessfully()
        {
            // arrange
            _target.OnInitialize();

            // act
            _target.Call(_testSimpleWithNoDescriptionOrResultFunction, _testNoResultCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleWithNoDescriptionOrResultFunction, _testNoResultCallback), Times.Once, "Expected one Threader Call");

            Assert.IsTrue(_target.IsReady);
        }

        [Test]
        public void CallSimpleWithNoDescriptionOrResult_WhenClassIsNotReady_IsNotCalled()
        {
            // arrange
            string errorMessage = $"The Call method of the AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTarget feature, called in the CallSimpleWithNoDescriptionOrResult_WhenClassIsNotReady_IsNotCalled method " +
                "of AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTests class, has not been initialized yet. Please make sure AWS.GameKit.Runtime.Core.GameKitManager is attached as a component and enabled.";

            // act
            _target.Call(_testSimpleWithNoDescriptionOrResultFunction, _testNoResultCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testSimpleWithNoDescriptionOrResultFunction, _testNoResultCallback), Times.Never, "Expected no Threader Call");

            Assert.IsFalse(_target.IsReady);
            Assert.IsTrue(Log.Count == 1);
            Assert.AreEqual(errorMessage, Log[0]);
        }

        [Test]
        public void CallRecurring_WhenClassIsReady_CalledSuccessfully()
        {
            // arrange
            _target.OnInitialize();

            // act
            _target.Call(_testRecurringFunction, TEST_DESCRIPTION, _testCallback, _testCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testRecurringFunction, TEST_DESCRIPTION, _testCallback, _testCallback), Times.Once, "Expected one Threader Call");

            Assert.IsTrue(_target.IsReady);
        }

        [Test]
        public void CallRecurring_WhenClassIsNotReady_IsNotCalled()
        {
           // arrange
           string errorMessage = $"The Call method of the AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTarget feature, called in the CallRecurring_WhenClassIsNotReady_IsNotCalled method " +
                "of AWS.GameKit.Runtime.UnitTests.GameKitFeatureBaseTests class, has not been initialized yet. Please make sure AWS.GameKit.Runtime.Core.GameKitManager is attached as a component and enabled.";

            // act
            _target.Call(_testRecurringFunction, TEST_DESCRIPTION, _testCallback, _testCallback);

            // assert
            _threaderMock.Verify(m => m.Call(_testRecurringFunction, TEST_DESCRIPTION, _testCallback, _testCallback), Times.Never, "Expected no Threader Call");

            Assert.IsFalse(_target.IsReady);
            Assert.IsTrue(Log.Count == 1);
            Assert.AreEqual(errorMessage, Log[0]);
        }
    }

    public class GameKitFeatureBaseTarget : GameKitFeatureBase
    {
        public override FeatureType FeatureType => FeatureType.Main;

        public class GameKitFeatureWrapperBaseStub : GameKitFeatureWrapperBase
        {
            public int ReleaseCallCount = 0;

            public override void Release() => ++ReleaseCallCount;

            protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb) => IntPtr.Zero;
            protected override void Release(IntPtr instance) { }
        }

        public int InitializedCallCount = 0;
        public int UpdateCallCount = 0;
        public int DestroyCallCount = 0;
        public GameKitFeatureWrapperBaseStub FeatureWrapper = new GameKitFeatureWrapperBaseStub();

        public void SetThreader(Threader threader) => _threader = threader;
        public new void Call<DESCRIPTION, RESULT>(Func<DESCRIPTION, RESULT> function, DESCRIPTION description, Action<RESULT> callback) => base.Call(function, description, callback);
        public new void Call<RESULT>(Func<RESULT> function, Action<RESULT> callback) => base.Call(function, callback);
        public new void Call<DESCRIPTION>(Action<DESCRIPTION> function, DESCRIPTION description, Action callback) => base.Call(function, description, callback);
        public new void Call(Action function, Action callback) => base.Call(function, callback);
        public new void Call<DESCRIPTION, RESULT, RETURN_RESULT>(
            Func<DESCRIPTION, Action<RESULT>, RETURN_RESULT> function, 
            DESCRIPTION description, 
            Action<RESULT> callback, 
            Action<RETURN_RESULT> onCompleteCallback) => base.Call<DESCRIPTION, RESULT, RETURN_RESULT>(function, description, callback, onCompleteCallback);

        protected override void InitializeFeature() => ++InitializedCallCount;
        protected override void UpdateFeature() => ++UpdateCallCount;
        protected override void DestroyFeature() => ++DestroyCallCount;
        protected override GameKitFeatureWrapperBase GetFeatureWrapperBase() => FeatureWrapper;
    }
}
