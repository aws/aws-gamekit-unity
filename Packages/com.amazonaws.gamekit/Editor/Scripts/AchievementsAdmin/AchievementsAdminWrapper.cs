// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Editor.Core;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

// Third Party
using Newtonsoft.Json;

namespace AWS.GameKit.Editor.AchievementsAdmin
{
    /// <summary>
    /// Achievements Admin wrapper for GameKit C++ SDK calls
    /// </summary>
    public class AchievementsAdminWrapper : GameKitFeatureWrapperBase
    {
        // Select the correct source path based on the platform
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string IMPORT = "__Internal";
#else
        private const string IMPORT = "aws-gamekit-achievements";
#endif

        // Dependencies
        private FeatureResourceManager _featureResourceManager;
        private CredentialsManager _credentialsManager;

        // DLL loading   
        [DllImport(IMPORT)] private static extern IntPtr GameKitAdminAchievementsInstanceCreateWithSessionManager(IntPtr sessionManager, string baseTemplatesFolder, AccountCredentials accountCredentials, AccountInfo accountInfo, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitAdminAchievementsInstanceRelease(IntPtr achievementsInstance);
        [DllImport(IMPORT)] private static extern uint GameKitAdminListAchievements(IntPtr achievementsInstance, uint pageSize, bool waitForAllPages, IntPtr dispatchReceiver, FuncStringCallback responseCallback);
        [DllImport(IMPORT)] private static extern uint GameKitAdminAddAchievements(IntPtr achievementsInstance, AdminAchievement[] achievements, uint batchSize);
        [DllImport(IMPORT)] private static extern uint GameKitAdminDeleteAchievements(IntPtr achievementsInstance, string[] achievementIdentifiers, uint batchSize);
        [DllImport(IMPORT)] private static extern uint GameKitGetAchievementIconsBaseUrl(IntPtr achievementsInstance, IntPtr dispatchReceiver, FuncStringCallback responseCallback);
        [DllImport(IMPORT)] private static extern bool GameKitIsAchievementIdValid(string achievementId);
        [DllImport(IMPORT)] private static extern uint GameKitAdminCredentialsChanged(IntPtr achievementsInstance, AccountCredentials accountCredentials, AccountInfo accountInfo);

        /// <summary>
        /// Initialize Achievements wrapper
        /// </summary>
        /// <param name="featureResourceManager">Feature resource manager instance.</param>
        /// <param name="credentialsManager">Credentials manager instance.</param>
        public void Initialize(FeatureResourceManager featureResourceManager, CredentialsManager credentialsManager)
        {
            _featureResourceManager = featureResourceManager;
            _credentialsManager = credentialsManager;
        }

        /// <summary>
        /// Checks if Achievement ID is valid.
        /// </summary>
        /// <param name="achievementId">ID of the achievement to check.</param>
        /// <returns>True if valid, false if not</returns>
        public bool IsAchievementIdValid(string achievementId)
        {
            return DllLoader.TryDll(() => GameKitIsAchievementIdValid(achievementId), nameof(GameKitIsAchievementIdValid), false);
        }

        /// <summary>
        /// Changes credentials to the indicated ones.
        /// </summary>
        /// <param name="changeCredentialsDesc">New credentials.</param>
        /// <returns>A GameKit status code indicating the result of the API call.</returns>
        public uint ChangeCredentials(ChangeCredentialsDesc changeCredentialsDesc)
        {
            return DllLoader.TryDll(() => GameKitAdminCredentialsChanged(
                GetInstance(), 
                changeCredentialsDesc.AccountCredentials, 
                changeCredentialsDesc.AccountInfo), 
                nameof(GameKitAdminCredentialsChanged), 
                GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        /// <summary>
        /// Lists all the metadata for every achievement for the current game and environment
        /// </summary>
        /// <param name="listAchievementsDesc">Struct indicates the number of records to scan or if it should retrieve all items.</param>
        /// <param name="resultCallback">Callback for the list of achievements.</param>
        /// <returns>A GameKit status code indicating the result of the API call.</returns>
        public uint AdminListAchievements(ListAchievementsDesc listAchievementsDesc, Action<AchievementListResult> resultCallback)
        {
            return DllLoader.TryDll(resultCallback, (IntPtr dispatchReceiver) => GameKitAdminListAchievements(
                GetInstance(),
                listAchievementsDesc.PageSize,
                listAchievementsDesc.WaitForAllPages,
                dispatchReceiver,
                AchievementListFromRecurringStringCallback), nameof(GameKitAdminListAchievements), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        /// <summary>
        /// Adds or updates the achievements in the backend for the current game and environment to have new metadata items.
        /// </summary>
        /// <param name="addAchievementDesc">Contains the list of achievements to add and the batch size.</param>
        /// <returns>A GameKit status code indicating the result of the API call.</returns>
        public uint AdminAddAchievements(AddAchievementDesc addAchievementDesc)
        {
            return DllLoader.TryDll(() => GameKitAdminAddAchievements(
                GetInstance(),
                addAchievementDesc.Achievements,
                addAchievementDesc.BatchSize), nameof(GameKitAdminAddAchievements), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        /// <summary>
        /// Deletes the achievements in the backend for the current game and environment specified ID's.
        /// </summary>
        /// <param name="deleteAchievementsDesc">Contains the achievement ids and batch size.</param>
        /// <returns>A GameKit status code indicating the result of the API call.</returns>
        public uint AdminDeleteAchievements(DeleteAchievementsDesc deleteAchievementsDesc)
        {
            return DllLoader.TryDll(() => GameKitAdminDeleteAchievements(
                GetInstance(),
                deleteAchievementsDesc.AchievementIdentifiers,
                deleteAchievementsDesc.BatchSize), nameof(GameKitAdminDeleteAchievements), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }
        
        /// <summary>
        /// Retrieve base url for achievement icons.
        /// </summary>
        /// <returns>StringCallback with base url and GameKit status code indicating the result of the API call.</returns>
        public StringCallbackResult GetAchievementIconsBaseUrl()
        {
            StringCallbackResult result = new StringCallbackResult();

            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetAchievementIconsBaseUrl(GetInstance(), dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitGetAchievementIconsBaseUrl), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            result.ResultCode = status;

            return result; 
        }

        protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb)
        {
            if (string.IsNullOrEmpty(_featureResourceManager.GetAccountInfo().AccountId))
            {
                throw new InvalidOperationException($"Cannot call {nameof(GameKitAdminAchievementsInstanceCreateWithSessionManager)}. AWS credentials need to be submitted first. This indicates an {nameof(AdminAchievement)} API is being called too early.");
            }

            AccountDesc account = new AccountDesc()
            {
                Credentials = _featureResourceManager.GetAccountCredentials(),
                Info = _featureResourceManager.GetAccountInfo()
            };
            string baseTemplatesFolder = _featureResourceManager.Paths.PACKAGES_BASE_TEMPLATES_FULL_PATH;

            return DllLoader.TryDll<IntPtr>(() => GameKitAdminAchievementsInstanceCreateWithSessionManager(sessionManager, baseTemplatesFolder, account.Credentials, account.Info, logCb), nameof(GameKitAdminAchievementsInstanceCreateWithSessionManager), IntPtr.Zero);
        }

        protected override void Release(IntPtr instance)
        {
            DllLoader.TryDll(() => GameKitAdminAchievementsInstanceRelease(instance), nameof(GameKitAdminAchievementsInstanceRelease));
        }

        [AOT.MonoPInvokeCallback(typeof(FuncStringCallback))]
        private static void AchievementListFromRecurringStringCallback(IntPtr dispatchReceiver, string responseValue)
        {
            // parse the string response
            AchievementListResult result = JsonConvert.DeserializeObject<JsonResponse<AchievementListResult>>(responseValue).data;

            // get a handle to the result callback from the dispatch receiver
            Action<AchievementListResult> resultCallback = Marshaller.GetDispatchObject<Action<AchievementListResult>>(dispatchReceiver);

            // call the callback and pass it the result
            resultCallback(result);
        }
    }
}