// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.AchievementsAdmin
{
    /// <summary>
    /// Interface for the AWS GameKit Achievements feature's admin APIs.
    /// </summary>
    public interface IAchievementsAdminProvider
    {
        /// <summary>
        /// Lists non-hidden achievements, and will call delegates after every page.<br/><br/>
        ///
        /// Secret achievements will be included, and developers will be responsible for processing those as they see fit.<br/><br/>
        ///
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed. Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload. This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="listAchievementsDesc">Object containing call preferences</param>
        /// <param name="callback">Delegate that is called while the function is executing, once for each page of achievements</param>
        /// <param name="onCompleteCallback">Delegate that is called once the function has finished executing</param>
        public void ListAchievementsForGame(ListAchievementsDesc listAchievementsDesc, Action<AchievementListResult> callback, Action<uint> onCompleteCallback);

        /// <summary>
        /// Add all achievements passed to the games dynamoDB achievements table.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_ACHIEVEMENTS_ICON_UPLOAD_FAILED: Was unable to take the local path given of an image and upload it to S3.<br/>
        /// - GAMEKIT_ERROR_REGION_CODE_CONVERSION_FAILED: The current region isn't in our template of shorthand region codes, unknown if the region is supported.<br/>
        /// - GAMEKIT_ERROR_SIGN_REQUEST_FAILED: Was unable to sign the internal http request with account credentials and info, possibly because they do not have sufficient permissions.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed. Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload. This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="addAchievementDesc">Object containing a list of achievements to add</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void AddAchievementsForGame(AddAchievementDesc addAchievementDesc, Action<uint> callback);

        /// <summary>
        /// Deletes the set of achievements metadata from the game's DynamoDB.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_REGION_CODE_CONVERSION_FAILED: The current region isn't in our template of shorthand region codes, unknown if the region is supported.<br/>
        /// - GAMEKIT_ERROR_SIGN_REQUEST_FAILED: Was unable to sign the internal http request with account credentials and info, possibly because they do not have sufficient permissions.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed. Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload. This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="deleteAchievementsDesc">Object containing a list of achievement identifiers to delete</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void DeleteAchievementsForGame(DeleteAchievementsDesc deleteAchievementsDesc, Action<uint> callback);
        
        /// <summary>
        /// Changes the credentials used to sign requests and retrieve session tokens for admin requests.<br/><br/>
        /// 
        /// No result delegate as this isn't a blocking method, only possible failure is an invalid region code which will be logged.
        /// </summary>
        /// <param name="changeCredentialsDesc">Object containing information about the new credentials and the account</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void ChangeCredentials(ChangeCredentialsDesc changeCredentialsDesc, Action<uint> callback);

        /// <summary>
        /// Checks whether the passed in ID has invalid characters or length.<br/><br/>
        /// 
        /// No result delegate as this isn't a blocking method.
        /// </summary>
        /// <param name="achievementId">achievement identifier to validate</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void IsAchievementIdValid(string achievementId, Action<bool> callback);

        /// <summary>
        /// Gets the AWS CloudFront url which all achievement icons for this game/environment can be accessed from.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.
        /// </summary>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void GetAchievementIconBaseUrl(Action<StringCallbackResult> callback);
    }
}