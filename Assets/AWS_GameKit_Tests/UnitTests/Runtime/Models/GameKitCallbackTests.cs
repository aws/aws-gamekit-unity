// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class GameKitCallbackTests : GameKitTestBase
    {
        const string TEST_KEY = "test key";
        const string TEST_VALUE = "test value";
        const string TEST_RESULT = "test result";

        [Test]
        public void KeyValueStringCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            KeyValueStringCallbackResult result = new KeyValueStringCallbackResult();

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.KeyValueStringCallback(dispatchReceiver, TEST_KEY, TEST_VALUE));

            // assert
            Assert.AreEqual(TEST_KEY, result.ResponseKey);
            Assert.AreEqual(TEST_VALUE, result.ResponseValue);
        }

        [Test]
        public void MultiKeyValueStringCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            MultiKeyValueStringCallbackResult result = new MultiKeyValueStringCallbackResult();

            Dictionary<string, string> testMap = new Dictionary<string, string>
            {
                { "test key 1", "test value 1" },
                { "test key 2", "test value 2" },
                { "test key 3", "test value 3" }
            };

            // act
            foreach (KeyValuePair<string, string> entry in testMap)
            {
                Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.MultiKeyValueStringCallback(dispatchReceiver, entry.Key, entry.Value));
            }

            // assert
            Assert.AreEqual(result.ResponseValues.Length, result.ResponseKeys.Length);
            Assert.AreEqual(result.ResponseValues.Length, testMap.Count);
            for (uint i = 0; i < testMap.Count; ++i)
            {
                Assert.AreEqual(testMap[result.ResponseKeys[i]], result.ResponseValues[i]);
            }
        }

        [Test]
        public void RecurringCallStringCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            StringCallbackResult actionOutput = new StringCallbackResult();
            Action<StringCallbackResult> action = (stringCallbackResult) => { actionOutput = stringCallbackResult; };

            // act
            Marshaller.Dispatch(action, (IntPtr dispatchReceiver) => GameKitCallbacks.RecurringCallStringCallback(dispatchReceiver, TEST_VALUE));

            // assert
            Assert.AreEqual(TEST_VALUE, actionOutput.ResponseValue);
        }

        [Test]
        public void StringCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            StringCallbackResult result = new StringCallbackResult();

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.StringCallback(dispatchReceiver, TEST_VALUE));

            // assert
            Assert.AreEqual(TEST_VALUE, result.ResponseValue);
        }

        [Test]
        public void MultiStringCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            MultiStringCallbackResult result = new MultiStringCallbackResult();

            string[] testArray = new string[]
            {
                "test value 1",
                "test value 2",
                "test value 3"
            };

            // act
            foreach (string entry in testArray)
            {
                Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.MultiStringCallback(dispatchReceiver, entry));
            }

            // assert
            Assert.AreEqual(testArray.Length, result.ResponseValues.Length);
            for (uint i = 0; i < testArray.Length; ++i)
            {
                Assert.AreEqual(testArray[i], result.ResponseValues[i]);
            }
        }

        [Test]
        public void ResourceInfoCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            ResourceInfoCallbackResult result = new ResourceInfoCallbackResult();

            const string TEST_ID = "test id";
            const string TEST_TYPE = "test type";
            const string TEST_STATUS = "test status";

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.ResourceInfoCallback(dispatchReceiver, TEST_ID, TEST_TYPE, TEST_STATUS));

            // assert
            Assert.AreEqual(TEST_ID, result.LogicalResourceId);
            Assert.AreEqual(TEST_TYPE, result.ResourceType);
            Assert.AreEqual(TEST_STATUS, result.ResourceStatus);
        }

        [Test]
        public void DeploymentResponseCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            DeploymentResponseCallbackResult result = new DeploymentResponseCallbackResult();
            FeatureType[] features = new FeatureType[] { FeatureType.Main, FeatureType.Identity };
            FeatureStatus[] statuses = new FeatureStatus[] { FeatureStatus.Deployed, FeatureStatus.Undeployed };
            uint resultCode = GameKitErrors.GAMEKIT_SUCCESS;

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.DeploymentResponseCallback(dispatchReceiver, features, statuses, (uint)features.Length, resultCode));

            // assert
            Assert.AreEqual(features, result.Features);
            Assert.AreEqual(statuses, result.FeatureStatuses);
            Assert.AreEqual(resultCode, result.ResultCode);
        }

        [Test]
        public void CanExecuteDeploymentActionCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            CanExecuteDeploymentActionResult result = new CanExecuteDeploymentActionResult();
            FeatureType targetFeature = FeatureType.GameStateCloudSaving;
            bool canExecuteAction = false;
            DeploymentActionBlockedReason reason = DeploymentActionBlockedReason.DependenciesMustBeCreated;
            FeatureType[] blockingFeatures = new FeatureType[] { FeatureType.Identity };

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameKitCallbacks.CanExecuteDeploymentActionCallback(dispatchReceiver, targetFeature, canExecuteAction, reason, blockingFeatures, (uint) blockingFeatures.Length));

            // assert
            Assert.AreEqual(targetFeature, result.TargetFeature);
            Assert.AreEqual(canExecuteAction, result.CanExecuteAction);
            Assert.AreEqual(reason, result.Reason);
            Assert.AreEqual(blockingFeatures, result.BlockingFeatures);
        }
    }
}
