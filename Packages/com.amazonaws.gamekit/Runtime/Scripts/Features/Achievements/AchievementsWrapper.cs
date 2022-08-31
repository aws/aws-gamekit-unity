// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

// Third Party
using Newtonsoft.Json;

namespace AWS.GameKit.Runtime.Features.GameKitAchievements
{
    /// <summary>
    /// Achievements wrapper for GameKit C++ SDK calls
    /// </summary>
    public class AchievementsWrapper : GameKitFeatureWrapperBase
    {
        // Select the correct source path based on the platform
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string IMPORT = "__Internal";
#else
        private const string IMPORT = "aws-gamekit-achievements";
#endif

        // DLL loading   
        [DllImport(IMPORT)] private static extern IntPtr GameKitAchievementsInstanceCreateWithSessionManager(IntPtr sessionManager, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitAchievementsInstanceRelease(IntPtr achievementsInstance);
        [DllImport(IMPORT)] private static extern uint GameKitListAchievements(IntPtr achievementsInstance, uint pageSize, bool waitForAllPages, IntPtr dispatchReceiver, FuncStringCallback responseCallback);
        [DllImport(IMPORT)] private static extern uint GameKitUpdateAchievement(IntPtr achievementsInstance, string achievementId, uint incrementBy, IntPtr dispatchReceiver, FuncStringCallback responseCallback);
        [DllImport(IMPORT)] private static extern uint GameKitGetAchievement(IntPtr achievementsInstance, string achievementId, IntPtr dispatchReceiver, FuncStringCallback responseCallback);
        [DllImport(IMPORT)] private static extern uint GameKitGetAchievementIconsBaseUrl(IntPtr achievementsInstance, IntPtr dispatchReceiver, FuncStringCallback responseCallback);

        [AOT.MonoPInvokeCallback(typeof(FuncStringCallback))]
        public static void AchievementListFromRecurringStringCallback(IntPtr dispatchReceiver, string responseValue)
        {
            // parse the string response
            AchievementListResult result = JsonConvert.DeserializeObject<JsonResponse<AchievementListResult>>(responseValue).data;

            // get a handle to the result callback from the dispatch receiver
            Action<AchievementListResult> resultCallback = Marshaller.GetDispatchObject<Action<AchievementListResult>>(dispatchReceiver);

            // call the callback and pass it the result
            resultCallback(result);
        }

        public uint ListAchievements(ListAchievementsDesc listAchievementsDesc, Action<AchievementListResult> resultCallback)
        {
            return DllLoader.TryDll(resultCallback, (IntPtr dispatchReceiver) => GameKitListAchievements(
                GetInstance(),
                listAchievementsDesc.PageSize,
                listAchievementsDesc.WaitForAllPages,
                dispatchReceiver,
                AchievementListFromRecurringStringCallback), nameof(GameKitListAchievements), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public AchievementResult UpdateAchievement(UpdateAchievementDesc updateAchievementDesc)
        {
            StringCallbackResult result = new StringCallbackResult();

            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitUpdateAchievement(
                GetInstance(),
                updateAchievementDesc.AchievementId,
                updateAchievementDesc.IncrementBy,
                dispatchReceiver,
                GameKitCallbacks.StringCallback), nameof(GameKitUpdateAchievement), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            return GetAchievementResult(result, status);
        }

        public AchievementResult GetAchievement(string achievementId)
        {
            StringCallbackResult result = new StringCallbackResult();

            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetAchievement(GetInstance(), achievementId, dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitGetAchievement), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            return GetAchievementResult(result, status);
        }

        public StringCallbackResult GetAchievementIconsBaseUrl()
        {
            StringCallbackResult result = new StringCallbackResult();

            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetAchievementIconsBaseUrl(GetInstance(), dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitGetAchievementIconsBaseUrl), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            result.ResultCode = status;

            return result;
        }

        protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitAchievementsInstanceCreateWithSessionManager(sessionManager, logCb), nameof(GameKitAchievementsInstanceCreateWithSessionManager), IntPtr.Zero);
        }

        protected override void Release(IntPtr instance)
        {
            DllLoader.TryDll(() => GameKitAchievementsInstanceRelease(instance), nameof(GameKitAchievementsInstanceRelease));
        }

        private AchievementResult GetAchievementResult(StringCallbackResult stringCallbackResult, uint status)
        {
            Achievement achievement = status == GameKitErrors.GAMEKIT_SUCCESS
                   ? JsonConvert.DeserializeObject<JsonResponse<Achievement>>(stringCallbackResult.ResponseValue).data
                   : new Achievement();

            return new AchievementResult
            {
                Achievement = achievement,
                ResultCode = status
            };
        }
    }
}
