// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AccountCredentials
    {
        public string Region;
        public string AccessKey;
        public string AccessSecret;
        public string AccountId;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct AccountCredentialsCopy
    {
        public string Region;
        public string AccessKey;
        public string AccessSecret;
        public string ShortRegionCode;
        public string AccountId;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct GetAWSAccountIdDescription
    {
        public string AccessKey;
        public string AccessSecret;
    };

}
