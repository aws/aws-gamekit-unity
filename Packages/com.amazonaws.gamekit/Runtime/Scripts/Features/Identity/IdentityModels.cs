// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Features.GameKitIdentity
{
    public enum FederatedIdentityProvider
    {
        FACEBOOK = 0,
    };

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncIdentityGetUserResponseCallback(IntPtr dispatchReceiver, IntPtr getUserResponse);

    [StructLayout(LayoutKind.Sequential)]
    public struct IdentityInfo
    {
        public string CognitoAppClientId;
        public string Region;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UserRegistration
    {
        public string UserName;
        public string Password;
        public string Email;
        public string UserId;
        public string UserIdHash;

        public readonly override string ToString() => $"UserRegistration(UserName={UserName}, Password=<Hidden>, Email={Email}, UserId={UserId}, UserIdHash={UserIdHash})";
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ConfirmRegistrationRequest
    {
        public string UserName;
        public string ConfirmationCode;

        public readonly override string ToString() => $"ConfirmRegistrationRequest(UserName={UserName}, ConfirmationCode={ConfirmationCode})";
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ResendConfirmationCodeRequest
    {
        public string UserName;

        public readonly override string ToString() => $"ResendConfirmationCodeRequest(UserName={UserName})";
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UserLogin
    {
        public string UserName;
        public string Password;

        public readonly override string ToString() => $"UserLogin(UserName={UserName}, Password=<Hidden>)";
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ForgotPasswordRequest
    {
        public string UserName;

        public readonly override string ToString() => $"ForgotPasswordRequest(UserName={UserName})";
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ConfirmForgotPasswordRequest
    {
        public string UserName;
        public string NewPassword;
        public string ConfirmationCode;

        public readonly override string ToString() => $"ConfirmForgotPasswordRequest(UserName={UserName}, NewPassword=<Hidden>, ConfirmationCode={ConfirmationCode})";
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct GetUserResponse
    {
        public string UserId;
        public string UpdatedAt;
        public string CreatedAt;
        public string FacebookExternalId;
        public string FacebookRefId;
        public string UserName;
        public string Email;

        public readonly override string ToString() => $"GetUserResponse(UserId={UserId}, UserName={UserName}, Email={Email}, UpdatedAt={UpdatedAt}, CreatedAt={CreatedAt}, FacebookExternalId={FacebookExternalId}, FacebookRefId={FacebookRefId})";
    };

    public class PollAndRetrieveFederatedTokensDesc
    {
        public FederatedIdentityProvider IdentityProvider;
        public string RequestId;
        public int Timeout;
    }

    public class GetUserResult
    {
        public uint ResultCode;
        public GetUserResponse Response;
    }
}
