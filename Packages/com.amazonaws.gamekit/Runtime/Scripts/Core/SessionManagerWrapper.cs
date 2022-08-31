// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Utils;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Runtime.Core
{
    public class SessionManagerWrapper : Singleton<SessionManagerWrapper>
    {
        // Select the correct source path based on the platform
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string IMPORT = "__Internal";
#else
        private const string IMPORT = "aws-gamekit-authentication";
#endif

        // Saved instance
        private IntPtr _instance = IntPtr.Zero;

        // DLL loading
        [DllImport(IMPORT)] private static extern IntPtr GameKitSessionManagerInstanceCreate(string clientConfigFilePath, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitSessionManagerInstanceRelease(IntPtr sessionManagerInstance);
        [DllImport(IMPORT)] private static extern void GameKitSessionManagerSetToken(IntPtr sessionManagerInstance, TokenType tokenType, string value);
        [DllImport(IMPORT)] private static extern bool GameKitSessionManagerAreSettingsLoaded(IntPtr sessionManagerInstance, FeatureType featureType);
        [DllImport(IMPORT)] private static extern void GameKitSessionManagerReloadConfigFile(IntPtr sessionManagerInstance, string clientConfigFilePath);
        [DllImport(IMPORT)] private static extern void GameKitSessionManagerReloadConfigContents(IntPtr sessionManagerInstance, string clientConfigFileContents);

        /// <summary>
        /// Gets (and creates if necessary) a GameKitSessionManager instance, which can be used to access the SessionManager API.
        /// </summary>
        /// <remarks>
        /// Make sure to call ReleaseInstance() to destroy the returned object when finished with it.
        /// </remarks>
        /// <returns>Pointer to the new GameKitSessionManager instance.</returns>
        public IntPtr GetInstance()
        {
            if (_instance == IntPtr.Zero)
            {
                // Start without any config settings loaded.
                string clientConfigFilePath = string.Empty;

                _instance = SessionManagerInstanceCreate(clientConfigFilePath, Logging.LogCb);
            }

            return _instance;
        }

        /// <summary>
        /// Destroy the GameKitSessionManager instance.
        /// </summary>
        public void ReleaseInstance()
        {
            SessionManagerInstanceRelease(_instance);
            _instance = IntPtr.Zero;
        }

        public void SessionManagerSetToken(TokenType tokenType, string value)
        {
            DllLoader.TryDll(() => GameKitSessionManagerSetToken(GetInstance(), tokenType, value), nameof(GameKitSessionManagerSetToken));
        }

        public bool SessionManagerAreSettingsLoaded(FeatureType featureType)
        {
            return DllLoader.TryDll(() => GameKitSessionManagerAreSettingsLoaded(GetInstance(), featureType), nameof(GameKitSessionManagerAreSettingsLoaded), false);
        }

        public void SessionManagerReloadConfigFile(string clientConfigFilePath)
        {
            DllLoader.TryDll(() => GameKitSessionManagerReloadConfigFile(GetInstance(), clientConfigFilePath), nameof(GameKitSessionManagerReloadConfigFile));
        }

        public void SessionManagerReloadConfigContents(string clientConfigFileContents)
        {
            DllLoader.TryDll(() => GameKitSessionManagerReloadConfigContents(GetInstance(), clientConfigFileContents), nameof(GameKitSessionManagerReloadConfigContents));
        }

        private IntPtr SessionManagerInstanceCreate(string clientConfigFilePath, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitSessionManagerInstanceCreate(clientConfigFilePath, logCb), nameof(GameKitSessionManagerInstanceCreate), IntPtr.Zero);
        }

        private void SessionManagerInstanceRelease(IntPtr sessionManagerInstance)
        {
            DllLoader.TryDll(() => GameKitSessionManagerInstanceRelease(sessionManagerInstance), nameof(GameKitSessionManagerInstanceRelease));
        }
    }
}
