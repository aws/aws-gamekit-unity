// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.Windows.Settings
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string UserId { get; set; }

        public bool IsLoggedIn => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(UserId);
    }
}
