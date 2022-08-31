// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Models
{
    public class AccountDetails
    {
        public string GameName { get; set; }
        public string Environment { get; set; }
        public string AccountId { get; set; }
        public string Region { get; set; }
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }

        public AccountCredentials CreateAccountCredentials()
        {
            AccountCredentials accountCredentials = new AccountCredentials();
            accountCredentials.AccountId = AccountId;
            accountCredentials.AccessKey = AccessKey;
            accountCredentials.AccessSecret = AccessSecret;
            accountCredentials.Region = Region;

            return accountCredentials;
        }

        public AccountInfo CreateAccountInfo()
        {
            AccountInfo accountInfo = new AccountInfo();
            accountInfo.AccountId = AccountId;
            accountInfo.GameName = GameName;
            accountInfo.Environment = String.IsNullOrEmpty(Environment) ? Constants.EnvironmentCodes.DEVELOPMENT : Environment;
            accountInfo.CompanyName = "";

            return accountInfo;
        }
    }
}
