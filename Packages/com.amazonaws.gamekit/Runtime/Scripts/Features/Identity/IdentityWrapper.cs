// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Features.GameKitIdentity
{
    /// <summary>
    /// Identity wrapper for GameKit C++ SDK calls
    /// </summary>
    public class IdentityWrapper : GameKitFeatureWrapperBase
    {
        // Select the correct source path based on the platform
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string IMPORT = "__Internal";
#else
        private const string IMPORT = "aws-gamekit-identity";
#endif

        // DLL loading   
        [DllImport(IMPORT)] private static extern IntPtr GameKitIdentityInstanceCreateWithSessionManager(IntPtr sessionManager, FuncLoggingCallback logCb);
        [DllImport(IMPORT)] private static extern void GameKitIdentityInstanceRelease(IntPtr identityInstance);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityRegister(IntPtr identityInstance, UserRegistration userRegistration);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityConfirmRegistration(IntPtr identityInstance, ConfirmRegistrationRequest request);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityResendConfirmationCode(IntPtr identityInstance, ResendConfirmationCodeRequest request);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityLogin(IntPtr identityInstance, UserLogin userLogin);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityLogout(IntPtr identityInstance);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityGetUser(IntPtr identityInstance, IntPtr dispatchReceiver, FuncIdentityGetUserResponseCallback responseCallback);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityForgotPassword(IntPtr identityInstance, ForgotPasswordRequest request);
        [DllImport(IMPORT)] private static extern uint GameKitIdentityConfirmForgotPassword(IntPtr identityInstance, ConfirmForgotPasswordRequest request);
        [DllImport(IMPORT)] private static extern uint GameKitGetFederatedLoginUrl(IntPtr identityInstance, FederatedIdentityProvider identityProvider, IntPtr dispatchReceiver, FuncKeyValueStringCallback responseCallback);
        [DllImport(IMPORT)] private static extern uint GameKitPollAndRetrieveFederatedTokens(IntPtr identityInstance, FederatedIdentityProvider identityProvider, string requestId, int timeout);
        [DllImport(IMPORT)] private static extern uint GameKitGetFederatedIdToken(IntPtr identityInstance, FederatedIdentityProvider identityProvider, IntPtr dispatchReceiver, FuncStringCallback responseCallback);

        [AOT.MonoPInvokeCallback(typeof(FuncIdentityGetUserResponseCallback))]
        protected static void IdentityGetUserResponseCallback(IntPtr dispatchReceiver, IntPtr getUserResponse)
        {
            // recover object reference from dispatchReceiver
            GetUserResult result = Marshaller.GetDispatchObject<GetUserResult>(dispatchReceiver);

            // handle assignments to the result object
            result.Response = Marshal.PtrToStructure<GetUserResponse>(getUserResponse);
        }

        public uint IdentityRegister(UserRegistration userRegistration)
        {
            return DllLoader.TryDll(() => GameKitIdentityRegister(GetInstance(), userRegistration), nameof(GameKitIdentityRegister), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint IdentityConfirmRegistration(ConfirmRegistrationRequest request)
        {
            return DllLoader.TryDll(() => GameKitIdentityConfirmRegistration(GetInstance(), request), nameof(GameKitIdentityConfirmRegistration), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint IdentityResendConfirmationCode(ResendConfirmationCodeRequest request)
        {
            return DllLoader.TryDll(() => GameKitIdentityResendConfirmationCode(GetInstance(), request), nameof(GameKitIdentityResendConfirmationCode), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint IdentityLogin(UserLogin userLogin)
        {
            return DllLoader.TryDll(() => GameKitIdentityLogin(GetInstance(), userLogin), nameof(GameKitIdentityLogin), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint IdentityLogout()
        {
            return DllLoader.TryDll(() => GameKitIdentityLogout(GetInstance()), nameof(GameKitIdentityLogout), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public GetUserResult IdentityGetUser()
        {
            GetUserResult result = new GetUserResult();

            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitIdentityGetUser(GetInstance(), dispatchReceiver, IdentityGetUserResponseCallback), nameof(GameKitIdentityGetUser), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            result.ResultCode = status;

            return result;
        }

        public uint IdentityForgotPassword(ForgotPasswordRequest request)
        {
            return DllLoader.TryDll(() => GameKitIdentityForgotPassword(GetInstance(), request), nameof(GameKitIdentityForgotPassword), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public uint IdentityConfirmForgotPassword(ConfirmForgotPasswordRequest request)
        {
            return DllLoader.TryDll(() => GameKitIdentityConfirmForgotPassword(GetInstance(), request), nameof(GameKitIdentityConfirmForgotPassword), GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public MultiKeyValueStringCallbackResult GetFederatedLoginUrl(FederatedIdentityProvider identityProvider)
        {
            MultiKeyValueStringCallbackResult result = new MultiKeyValueStringCallbackResult();

            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetFederatedLoginUrl(GetInstance(), identityProvider, dispatchReceiver, GameKitCallbacks.MultiKeyValueStringCallback), nameof(GameKitGetFederatedLoginUrl), GameKitErrors.GAMEKIT_ERROR_GENERAL);
            
            result.ResultCode = status;
            return result;
        }

        public uint PollAndRetrieveFederatedTokens(PollAndRetrieveFederatedTokensDesc pollAndRetrieveFederatedTokensDesc)
        {
            return DllLoader.TryDll(() => GameKitPollAndRetrieveFederatedTokens(
                GetInstance(),
                pollAndRetrieveFederatedTokensDesc.IdentityProvider,
                pollAndRetrieveFederatedTokensDesc.RequestId,
                pollAndRetrieveFederatedTokensDesc.Timeout), nameof(GameKitPollAndRetrieveFederatedTokens),
                GameKitErrors.GAMEKIT_ERROR_GENERAL);
        }

        public StringCallbackResult GetFederatedIdToken(FederatedIdentityProvider identityProvider)
        {
            StringCallbackResult result = new StringCallbackResult();
            
            uint status = DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetFederatedIdToken(GetInstance(), identityProvider, dispatchReceiver, GameKitCallbacks.StringCallback), nameof(GameKitGetFederatedIdToken), GameKitErrors.GAMEKIT_ERROR_GENERAL);

            result.ResultCode = status;

            return result;
        }

        protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitIdentityInstanceCreateWithSessionManager(sessionManager, logCb), nameof(GameKitIdentityInstanceCreateWithSessionManager), IntPtr.Zero);
        }

        protected override void Release(IntPtr instance)
        {
            DllLoader.TryDll(() => GameKitIdentityInstanceRelease(instance), nameof(GameKitIdentityInstanceRelease));
        }
    }
}
