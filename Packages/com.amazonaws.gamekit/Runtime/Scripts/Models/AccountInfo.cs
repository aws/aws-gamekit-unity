// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AccountInfo
    {
        public string Environment;
        public string AccountId;
        public string CompanyName;
        public string GameName;
    };
}
