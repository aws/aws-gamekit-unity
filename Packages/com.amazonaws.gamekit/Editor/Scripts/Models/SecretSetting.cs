// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.Models
{
    /// <summary>
    /// Class for saving secrets in settings
    /// </summary>
    public class SecretSetting
    {
        public string SecretIdentifier { get; set; }
        public string SecretValue { get; set; }
        public bool IsStoredInCloud { get; set; }

        public SecretSetting(string secretIdentifier, string secretValue, bool isStoredInCloud)
        {
            SecretIdentifier = secretIdentifier;
            SecretValue = secretValue;
            IsStoredInCloud = isStoredInCloud;
        }
    }
}