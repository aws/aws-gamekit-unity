// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Runtime.Features.GameKitIdentity
{
    /// <summary>
    /// Interface for the AWS GameKit Identity feature.
    /// </summary>
    public interface IIdentityProvider
    {
        /// <summary>
        /// Register a new player for email and password based sign in.<br/><br/>
        /// 
        /// After calling this method, you must call ConfirmRegistration() to confirm the player's identity.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_USERNAME: The provided UserName is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_PASSWORD: The provided Password is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_METHOD_NOT_IMPLEMENTED: You attempted to register a guest, which is not yet supported. To fix, make sure the request's FUserRegistrationRequest::UserId field is empty.<br/>
        /// - GAMEKIT_ERROR_REGISTER_USER_FAILED: The backend web request failed. Check the output logs to see what the error was.
        /// </summary>
        /// <param name="userRegistration">Object containing the information about the user to register</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void Register(UserRegistration userRegistration, Action<uint> callback);

        /// <summary>
        /// Confirm registration of a new player that was registered through Register().<br/><br/>
        /// 
        /// The confirmation code is sent to the player's email and can be re-sent by calling ResendConfirmationCode().<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_USERNAME: The provided UserName is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_CONFIRM_REGISTRATION_FAILED: The backend web request failed. Check the output logs to see what the error was.
        /// </summary>
        /// <param name="request">Object containing the required username and code to confirm the registration</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void ConfirmRegistration(ConfirmRegistrationRequest request, Action<uint> callback);

        /// <summary>
        /// Resend the registration confirmation code to the player's email.<br/><br/>
        /// 
        /// This resends the confirmation code that was sent by calling Register() or ResendConfirmationCode().<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_USERNAME: The provided UserName is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_RESEND_CONFIRMATION_CODE_FAILED: The backend web request failed. Check the output logs to see what the error was.
        /// </summary>
        /// <param name="request">Object containing the username of the user requesting the resend</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void ResendConfirmationCode(ResendConfirmationCodeRequest request, Action<uint> callback);

        /// <summary>
        /// Send a password reset code to the player's email.<br/><br/>
        /// 
        /// After calling this method, you must call ConfirmForgotPassword() to complete the password reset.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_USERNAME: The provided UserName is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_FORGOT_PASSWORD_FAILED: The backend web request failed. Check the output logs to see what the error was.
        /// </summary>
        /// <param name="request">Object containing the username of the user requesting a password reset</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void ForgotPassword(ForgotPasswordRequest request, Action<uint> callback);

        /// <summary>
        /// Set the player's new password.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_USERNAME: The provided UserName is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_MALFORMED_PASSWORD: The provided Password is malformed. Check the output logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_CONFIRM_FORGOT_PASSWORD_FAILED: The backend web request failed. Check the output logs to see what the error was.
        /// </summary>
        /// <param name="request">Object containing the username and required new password information</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void ConfirmForgotPassword(ConfirmForgotPasswordRequest request, Action<uint> callback);

        /// <summary>
        /// Retrieves a login/signup URL for the specified federated identity provider.<br/><br/>
        /// 
        /// Players will be able to register and/or sign in when the URL opens in a web browser.<br/><br/>
        /// 
        /// This method will automatically start polling for login completion with PollAndRetrieveFederatedTokens()
        /// after the login url has been opened.
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_INVALID_FEDERATED_IDENTITY_PROVIDER: The specified federated identity provider is invalid or is not yet supported.
        /// </summary>
        /// <param name="identityProvider">enum for the identity provider</param>
        /// <param name="loginUrlRetrievedCallback">Delegate that is called once a login url for the federated provider is available.</param>
        /// <param name="loginCompletionCallback">Delegate that is called once the player has finished logging in, and the token has been cached.</param>
        /// <param name="loginTimeout">How long the player has to login before polling for completion stops.</param>
        /// <param name="loginTimeout">How long the player has to login and polling for completion stops.</param>
        public void FederatedLogin(FederatedIdentityProvider identityProvider, Action<MultiKeyValueStringCallbackResult> loginUrlRetrievedCallback, Action<uint> loginCompletionCallback = null, int loginTimeout = 90);

        /// <summary>
        /// Continually check if the player has completed signing in with the federated identity provider, then store their access tokens in the AwsGameKitSessionManager.<br/><br/>
        /// 
        /// After calling this method, the player will be signed in and you'll be able to call the other GameKit APIs.
        /// This method stores the player's authorized access tokens in the AWS GameKit Session Manager, which automatically refreshes them before they expire.<br/><br/>
        /// 
        /// To call this method, you must first call GetFederatedLoginUrl() to get a unique request ID.<br/><br/>
        /// 
        /// This method will timeout after the specified limit, in which case the player is not logged in.
        /// You can call GetFederatedIdToken() to check if the login was successful.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </summary>
        /// <param name="pollAndRetrieveFederatedTokensDesc">Object containing details for the request such as timeout and provider</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void PollAndRetrieveFederatedTokens(PollAndRetrieveFederatedTokensDesc pollAndRetrieveFederatedTokensDesc, Action<uint> callback);

        /// <summary>
        /// Get the player's authorized Id token for the specified federated identity provider.<br/><br/>
        /// 
        /// The returned access token will be empty if the player is not logged in with the federated identity provider.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_INVALID_FEDERATED_IDENTITY_PROVIDER: The specified federated identity provider is invalid or is not yet supported.
        /// </summary>
        /// <param name="identityProvider">enum for the identity provider</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void GetFederatedIdToken(FederatedIdentityProvider identityProvider, Action<StringCallbackResult> callback);

        /// <summary>
        /// Sign in the player through email and password.<br/><br/>
        /// 
        /// After calling this method, the player will be signed in and you'll be able to call the other GameKit APIs.
        /// This method stores the player's authorized access tokens in the AwsGameKitSessionManager, and automatically refreshes them before they expire.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </summary>
        /// <param name="userLogin">Object containing login information</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void Login(UserLogin userLogin, Action<uint> callback);

        /// <summary>
        /// Sign out the currently logged in player.<br/><br/>
        /// 
        /// This revokes the player's access tokens and clears them from the AwsGameKitSessionManager.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// - GAMEKIT_ERROR_LOGIN_FAILED: There was no user logged in or an exception occurred while revoking the user's tokens. For the second case, please check the logs for details.
        /// </summary>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void Logout(Action<uint> callback);

        /// <summary>
        /// Get information about the currently logged in player.<br/><br/>
        /// 
        /// The response is a JSON string containing the following information (or an empty string if the call failed):<br/>
        /// - The date time when the player was registered.<br/>
        /// - The date time of the last time the player's identity information was modified.<br/>
        /// - The player's GameKit ID.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed. Check the output logs to see what the HTTP response code was<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload. This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void GetUser(Action<GetUserResult> callback);
    }
}
