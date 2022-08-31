// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.IO;

// Unity
using UnityEngine;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.UnitTests;
using AWS.GameKit.Runtime.Utils;

// Third Party
using Moq;
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class FeatureResourceManagerTests : GameKitTestBase
    {
        const string PLUGIN_BASE_DIR = "/AWS_GameKit_Tests/Test Resources";
        const string TEST_RESOURCES_FOLDER = "Test Resources";
        const string TEST_RESOURCES_DIR = "../AwsGameTechGameKitUnityPackage/Assets/AWS_GameKit_Tests/" + TEST_RESOURCES_FOLDER + "/";
        const string SAVE_INFO_DIR = TEST_RESOURCES_DIR + "Editor/CloudResources/InstanceFiles/test_game/";
        const string SAVE_INFO_FILE_PATH = SAVE_INFO_DIR + "saveInfo.yml";
        const string GAME_NAME = "test_game";
        const string ENV = "tst";
        const string ENV_VALUE = "tst";
        const string ACCOUNT_ID = "test account id";
        const string REGION = "test_region";
        const string ACCESS_KEY = "test access key";
        const string ACCESS_SECRET = "test access secret";
        const string TEST_KEY = "test key";
        const string TEST_VALUE = "test value";

        private class GameKitTestPaths : GameKitPaths
        {
            public override string ASSETS_FULL_PATH => Application.dataPath + PLUGIN_BASE_DIR;
            public override string ASSETS_RELATIVE_PATH => Application.dataPath + PLUGIN_BASE_DIR;
        }

        AccountDetails accountDetails = new AccountDetails()
        {
            GameName = GAME_NAME,
            Environment = ENV,
            AccountId = ACCOUNT_ID,
            Region = REGION,
            AccessKey = ACCESS_KEY,
            AccessSecret = ACCESS_SECRET
        };

        Threader _threader;
        FeatureResourceManager _target;

        Mock<ICoreWrapperProvider> _coreWrapperMock;

        [SetUp]
        public void SetUp()
        {
            // generate the saveInfo.yml file
            CreateSaveInfoFile();

            // mocks
            _coreWrapperMock = new Mock<ICoreWrapperProvider>();
            _coreWrapperMock.Setup(cw => cw.SettingsPopulateAndSave(GAME_NAME, ENV, REGION)).Returns(GameKitErrors.GAMEKIT_SUCCESS);
            _coreWrapperMock.Setup(cw => cw.SettingsGetGameName()).Returns(GAME_NAME);
            _coreWrapperMock.Setup(cw => cw.SettingsGetFeatureVariables(FeatureType.Main)).Returns(new Dictionary<string, string>());
            _coreWrapperMock.Setup(cw => cw.SettingsSave()).Returns(GameKitErrors.GAMEKIT_SUCCESS);
            _coreWrapperMock.Setup(cw => cw.SettingsAddCustomEnvironment(ENV, ENV_VALUE));
            _coreWrapperMock.Setup(cw => cw.SettingsSetFeatureVariables(FeatureType.Main, new string[] { TEST_KEY }, new string[] { TEST_VALUE }, 1));
            _coreWrapperMock.Setup(cw => cw.SettingsGetCustomEnvironments()).Returns(new Dictionary<string, string>() { { TEST_KEY, TEST_VALUE } });

            // target
            _threader = new Threader();
            _target = new FeatureResourceManager(_coreWrapperMock.Object, new GameKitTestPaths(), _threader);
            _target.SetAccountDetails(accountDetails);
        }

        [TearDown]
        public void TearDown()
        {
            // clean up saveInfo.yml file
            DeleteSaveInfoFile();
        }

        [Test]
        public void SettingsSaveSettings_StandardCase_CallsSuccessfully()
        {
            // act
            uint result = _target.SaveSettings();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsPopulateAndSave(GAME_NAME, ENV, REGION), Times.Once);

            Assert.AreEqual(GameKitErrors.GAMEKIT_SUCCESS, result);
            Assert.IsTrue(Log.Count == 0);
        }

        [Test]
        public void SettingsSaveSettings_NonSuccessResult_LogsError()
        {
            // arrange
            _coreWrapperMock.Setup(cw => cw.SettingsPopulateAndSave(GAME_NAME, ENV, REGION)).Returns(GameKitErrors.GAMEKIT_ERROR_GENERAL);

            // act
            uint result = _target.SaveSettings();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsPopulateAndSave(GAME_NAME, ENV, REGION), Times.Once);

            Assert.AreNotEqual(GameKitErrors.GAMEKIT_SUCCESS, result);
            Assert.IsTrue(Log.Count == 1);
            Assert.AreEqual($"Error: FeatureResourceManager.SaveSettings() Failed to save : { GameKitErrors.ToString(GameKitErrors.GAMEKIT_ERROR_GENERAL) }", Log[0]);
        }

        [Test]
        public void SettingsGetGameName_LocalGameNameExists_ReturnsLocal()
        {
            // act
            string result = _target.GetGameName();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsGetGameName(), Times.Never);

            Assert.AreEqual(GAME_NAME, result);
        }

        [Test]
        public void SettingsGetGameName_LocalGameNameEmpty_CallsDll()
        {
            // arrange
            _target.SetGameName(string.Empty);

            // act
            string result = _target.GetGameName();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsGetGameName(), Times.Once);

            Assert.AreEqual(GAME_NAME, result);
        }

        [Test]
        public void GetSettingsEnvironments_SaveInfoNotFound_ReturnsEmptyMap()
        {
            // arrange
            _target.SetGameName(string.Empty);

            // act
            Dictionary<string, string> result = _target.GetCustomEnvironments();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsGetCustomEnvironments(), Times.Never);

            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void GetSettingsEnvironments_SaveInfoExists_CallsDll()
        {
            // act
            Dictionary<string, string> result = _target.GetCustomEnvironments();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsGetCustomEnvironments(), Times.Once);

            Assert.IsTrue(result.Count == 1);
            Assert.AreEqual(TEST_VALUE, result[TEST_KEY]);
        }

        [Test]
        public void SetFeatureVariable_WhenLocalMethodCalledMultipleTime_DllCalledOnlyOnce()
        {
            // act
            _target.SetFeatureVariable(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });
            _target.SetFeatureVariable(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });
            _target.SetFeatureVariable(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });

            // wait for each threaded item to be executed
            _threader.WaitForThreadedWork_TestOnly();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsSetFeatureVariables(FeatureType.Main, new string[] { TEST_KEY }, new string[] { TEST_VALUE }, 1), Times.Exactly(3));
            _coreWrapperMock.Verify(cw => cw.SettingsSave(), Times.Once);
        }

        [Test]
        public void SetFeatureVariableIfUnset_WhenLocalMethodCalledMultipleTime_DllCalledOnlyOnce()
        {
            // act
            _target.SetFeatureVariable(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });
            _target.SetFeatureVariable(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });
            _target.SetFeatureVariable(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });

            // wait for each threaded item to be executed
            _threader.WaitForThreadedWork_TestOnly();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsSetFeatureVariables(FeatureType.Main, new string[] { TEST_KEY }, new string[] { TEST_VALUE }, 1), Times.Exactly(3));
            _coreWrapperMock.Verify(cw => cw.SettingsSave(), Times.Once);
        }

        [Test]
        public void SetFeatureVariableIfUnset_WhenTheValueDoesNotExist_DllIsCalled()
        {
            // act
            _target.SetFeatureVariableIfUnset(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });
            _threader.WaitForThreadedWork_TestOnly();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsGetFeatureVariables(FeatureType.Main), Times.Once);
            _coreWrapperMock.Verify(cw => cw.SettingsSave(), Times.Once);
        }

        [Test]
        public void SetFeatureVariableIfUnset_WhenTheValueDoesExist_DllIsNotCalled()
        {
            // arrange
            _coreWrapperMock.Setup(cw => cw.SettingsGetFeatureVariables(FeatureType.Main)).Returns(new Dictionary<string, string>() { { TEST_KEY, TEST_VALUE } });

            // act
            _target.SetFeatureVariableIfUnset(FeatureType.Main, TEST_KEY, TEST_VALUE, () => { });
            _threader.WaitForThreadedWork_TestOnly();

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsGetFeatureVariables(FeatureType.Main), Times.Once);
            _coreWrapperMock.Verify(cw => cw.SettingsSave(), Times.Never);
        }

        [Test]
        public void SetFeatureVariables_StandardCase_CallsSuccessfully()
        {
            // act
            _target.SetFeatureVariables(new []{ Tuple.Create(FeatureType.Main, TEST_KEY, TEST_VALUE) } , () => { });

            // assert
            _coreWrapperMock.Verify(cw => cw.SettingsSetFeatureVariables(FeatureType.Main, new string[] { TEST_KEY }, new string[] { TEST_VALUE }, 1), Times.Once);
            _coreWrapperMock.Verify(cw => cw.SettingsSave(), Times.Once);
        }

        private void CreateSaveInfoFile()
        {
            string contents = 
                "#\n" +
                "# DO NOT MANUALLY EDIT\n" +
                "#\n" +
                "# Autogenerated by AWS GameKit\n" +
                "#\n" +
                "game:\n" +
                $"  name: { GAME_NAME }\n" +
                $"  short_name: { GAME_NAME }\n" +
                "lastUsedEnvironment:\n" +
                $"  code: { ENV }\n" +
                $"lastUsedRegion: { REGION }\n" +
                "gamekitPluginVersion: 1.1\n";

            Directory.CreateDirectory(SAVE_INFO_DIR);
            File.WriteAllText(SAVE_INFO_FILE_PATH, contents);
        }

        private void DeleteSaveInfoFile()
        {
            if (Directory.Exists(TEST_RESOURCES_DIR))
            {
                Directory.Delete(TEST_RESOURCES_DIR, true);
            }

            if (File.Exists(TEST_RESOURCES_FOLDER + ".meta"))
            {
                File.Delete(TEST_RESOURCES_FOLDER + ".meta");
            }
        }
    }
}
