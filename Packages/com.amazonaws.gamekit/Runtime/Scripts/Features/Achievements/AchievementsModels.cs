// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Third Party
using Newtonsoft.Json;

namespace AWS.GameKit.Runtime.Features.GameKitAchievements
{
    public class ListAchievementsDesc
    {
        public uint PageSize;
        public bool WaitForAllPages;

        public override string ToString() => $"ListAchievementsDesc(PageSize={PageSize}, WaitForAllPages={WaitForAllPages})";
    }

    public class UpdateAchievementDesc
    {
        public string AchievementId;
        public uint IncrementBy;

        public override string ToString() => $"UpdateAchievementDesc(AchievementId={AchievementId}, IncrementBy={IncrementBy})";
    }

    [Serializable]
    public class Achievement
    {
        [JsonProperty("achievement_id")]
        public string AchievementId = string.Empty;

        [JsonProperty("title")]
        public string Title = string.Empty;

        [JsonProperty("locked_description")]
        public string LockedDescription = string.Empty;

        [JsonProperty("unlocked_description")]
        public string UnlockedDescription = string.Empty;

        [JsonProperty("locked_icon_url")]
        public string LockedIcon = string.Empty;

        [JsonProperty("unlocked_icon_url")]
        public string UnlockedIcon = string.Empty;

        [JsonProperty("current_value")]
        public int CurrentValue = 0;

        [JsonProperty("max_value")]
        public int RequiredAmount = 0;

        [JsonProperty("points")]
        public int Points = 0;

        [JsonProperty("order_number")]
        public int OrderNumber = 0;

        [JsonProperty("is_stateful")]
        public bool IsStateful => RequiredAmount > 1;

        [JsonProperty("is_secret")]
        public bool IsSecret = false;

        [JsonProperty("is_hidden")]
        public bool IsHidden = false;

        [JsonProperty("earned")]
        public bool IsEarned = false;

        [JsonProperty("newly_earned")]
        public bool IsNewlyEarned = false;

        [JsonProperty("earned_at")]
        public string EarnedAt = string.Empty;

        [JsonProperty("updated_at")]
        public string UpdatedAt = string.Empty;
    }

    [Serializable]
    public class AchievementResult
    {
        /// <summary>
        /// Will be <see cref="Core.GameKitErrors.GAMEKIT_SUCCESS"/> if the API call was successful, otherwise will be a specific value which indicates the kind of error that occurred.
        /// Consult the API's documentation to find which error codes are possible and their meaning.
        /// </summary>
        public uint ResultCode;

        /// <summary>
        /// The Achievement that was retrieved or acted upon. Will be a default <see cref="Achievement"/> object when the <see cref="ResultCode"/> is unsuccessful (i.e. not <see cref="Core.GameKitErrors.GAMEKIT_SUCCESS"/>).
        /// </summary>
        public Achievement Achievement;
    }

    [Serializable]
    public class AchievementListResult
    {
        [JsonProperty("achievements")]
        public Achievement[] Achievements;
    }
}
