// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// Third Party
using Newtonsoft.Json;

namespace AWS.GameKit.Editor.AchievementsAdmin
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AccountCredentials
    {
        public string Region;
        public string AccessKey;
        public string AccessSecret;
        public string AccountId;

        public static implicit operator AccountCredentials(Runtime.Models.AccountCredentials accountCredentials)
        {
            return new AccountCredentials()
            {
                Region = accountCredentials.Region,
                AccessKey = accountCredentials.AccessKey,
                AccessSecret = accountCredentials.AccessSecret,
                AccountId = accountCredentials.AccountId
            };
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct AccountInfo
    {
        public string Environment;
        public string AccountId;
        public string CompanyName;
        public string GameName;

        public static implicit operator AccountInfo(Runtime.Models.AccountInfo accountInfo)
        {
            return new AccountInfo()
            {
                Environment = accountInfo.Environment,
                AccountId = accountInfo.AccountId,
                CompanyName = accountInfo.CompanyName,
                GameName = accountInfo.GameName
            };
        }
    };

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct AdminAchievement
    {
        [JsonProperty("achievement_id")]
        public string AchievementId;
        
        [JsonProperty("title")]
        public string Title;

        [JsonProperty("locked_description")]
        public string LockedDescription;

        [JsonProperty("unlocked_description")]
        public string UnlockedDescription;

        [JsonProperty("locked_icon_url")]
        public string LockedIcon;

        [JsonProperty("unlocked_icon_url")]
        public string UnlockedIcon;

        [JsonProperty("max_value")]
        public uint RequiredAmount;

        [JsonProperty("points")]
        public uint Points;

        [JsonProperty("order_number")]
        public uint OrderNumber;

        [JsonProperty("is_stateful")]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsStateful;
        
        [JsonProperty("is_secret")]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsSecret;

        [JsonProperty("is_hidden")]
        [MarshalAs(UnmanagedType.U1)]
        public bool IsHidden;
    };

    public struct AccountDesc
    {
        public AccountCredentials Credentials;
        public AccountInfo Info;
    }
    
    public struct ListAchievementsDesc
    {
        public uint PageSize;
        public bool WaitForAllPages;
    }

    public struct AddAchievementDesc
    {
        public AdminAchievement[] Achievements;
        public uint BatchSize;
    }

    public struct DeleteAchievementsDesc
    {
        public string[] AchievementIdentifiers;
        public uint BatchSize;
    }

    public struct ChangeCredentialsDesc
    {
        public AccountCredentials AccountCredentials;
        public AccountInfo AccountInfo;
    }

    [Serializable]
    public class AchievementListResult
    {
        [JsonProperty("achievements")]
        public AdminAchievement[] Achievements;
    }
}