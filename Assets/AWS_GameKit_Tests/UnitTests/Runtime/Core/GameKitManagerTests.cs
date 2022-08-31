// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.FeatureUtils;

// Third Party
using Moq;
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class GameKitManagerTests : GameKitTestBase
    {
        public Mock<SessionManager> _sessionManagerMock = new Mock<SessionManager>();
        GameObject _gameObject;
        GameKitManagerTarget _target;

        [SetUp]
        public void SetUp()
        {
            _sessionManagerMock.Setup(m => m.Release()).Verifiable();

            _target = new GameKitManagerTarget(_sessionManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _target.FeaturesClear();
        }

        [Test]
        public void EnsureFeaturesAreInitialized_StandardCase_CallsFeaturesAwakeMethodOnce()
        {
            // arrange
            Mock<GameKitFeatureBase> gameKitFeatureMock = new Mock<GameKitFeatureBase>();
            _target.FeatureAdd(gameKitFeatureMock.Object);

            // act
            _target.EnsureFeaturesAreInitialized();

            // assert
            gameKitFeatureMock.Verify(m => m.OnInitialize(), Times.Once, "Expected Awake for feature to be called once");

            Assert.AreEqual(1, _target.FeatureCount, $"Expected one feature in feature list, feature count = { _target.FeatureCount}");
        }

        [Test]
        public void Dispose_StandardCase_CallsFeaturesOnDestroyMethod()
        {
            // arrange
            Mock<GameKitFeatureBase> gameKitFeatureMock = new Mock<GameKitFeatureBase>();
            _target.FeatureAdd(gameKitFeatureMock.Object);
        
            // act
            _target.Dispose();
        
            // assert
            gameKitFeatureMock.Verify(m => m.OnDispose(), Times.Once, "Expected OnDestroy for feature to be called once");
        
            Assert.AreEqual(1, _target.FeatureCount, $"Expected one feature in feature list, feature count = {_target.FeatureCount}");

            _sessionManagerMock.Verify(m => m.Release(), Times.Once);
        }

        [Test]
        public void OnDestroy_WhenThereIsAnException_FeatureListIsKept()
        {
            // arrange
            Mock<GameKitFeatureBase> gameKitFeatureMock = new Mock<GameKitFeatureBase>();
            gameKitFeatureMock.Setup(m => m.OnDispose()).Throws<System.Exception>();
            _target.FeatureAdd(gameKitFeatureMock.Object);
        
            // act
            Assert.Throws<System.Exception>(() => _target.Dispose());
        
            // assert
            gameKitFeatureMock.Verify(m => m.OnDispose(), Times.Once, "Expected OnDestroy for feature to be called once");
        
            Assert.AreEqual(1, _target.FeatureCount, $"Expected one feature in feature list, feature count = {_target.FeatureCount}");
        }

        [Test]
        public void Update_StandardCase_CallsFeaturesUpdateMethod()
        {
            // arrange
            Mock<GameKitFeatureBase> gameKitFeatureMock = new Mock<GameKitFeatureBase>();
            _target.FeatureAdd(gameKitFeatureMock.Object);

            // act
            _target.Update();

            // assert
            gameKitFeatureMock.Verify(m => m.Update(), Times.Once, "Expected Update for feature to be called once");

            Assert.AreEqual(1, _target.FeatureCount, $"Expected one feature in feature list, feature count = {_target.FeatureCount}");
        }
    }

    public class GameKitManagerTarget : GameKitManager
    {
        public GameKitManagerTarget(SessionManager sessionManager)
        {
            SetSessionManager(sessionManager);
        }

        public void FeaturesClear() => _features.Clear();
        public void FeatureAdd(GameKitFeatureBase feature) => _features.Add(feature);
        public new void Update() => base.Update();
        protected override void AddFeatures()
        {
            /* add nothing */
        }
    }
}
