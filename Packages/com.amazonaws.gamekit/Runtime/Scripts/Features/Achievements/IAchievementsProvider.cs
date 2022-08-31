// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Runtime.Features.GameKitAchievements
{
    /// <summary>
    /// Interface for the AWS GameKit Achievements feature.
    /// </summary>
    public interface IAchievementsProvider
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
        public void ListAchievementsForPlayer(ListAchievementsDesc listAchievementsDesc, Action<AchievementListResult> callback, Action<uint> onCompleteCallback);

        /// <summary>
        /// Gets the specified achievement for currently logged in user, and passes it to ResultDelegate<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed. Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload. This should not happen. If it does, it indicates there is a bug in the backend code.<br/>
        /// - GAMEKIT_ERROR_ACHIEVEMENTS_INVALID_ID: The Achievement ID given is empty or malformed.
        /// </summary>
        /// <param name="achievementId">Identifier of the achievement to return</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void GetAchievementForPlayer(string achievementId, Action<AchievementResult> callback);

        /// <summary>
        /// Increments the currently logged in user's progress on a specific achievement.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature (AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed. Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload. This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="updateAchievementDesc">Object containing the identifier of the achievement and how much to increment by</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void UpdateAchievementForPlayer(UpdateAchievementDesc updateAchievementDesc, Action<AchievementResult> callback);

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
