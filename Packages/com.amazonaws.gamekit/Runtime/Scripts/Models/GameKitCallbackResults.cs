// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Models
{ 
    [StructLayout(LayoutKind.Sequential)] 
    public class StringCallbackResult
    {
        public uint ResultCode;
        public string ResponseValue = string.Empty;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class KeyValueStringCallbackResult
    {
        public uint ResultCode;
        public string ResponseKey = string.Empty;
        public string ResponseValue = string.Empty;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class ResourceInfoCallbackResult
    {
        public uint ResultCode;
        public string LogicalResourceId = string.Empty;
        public string ResourceType = string.Empty;
        public string ResourceStatus = string.Empty;
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class MultiResourceInfoCallbackResult
    {
        public uint ResultCode;
        public string[] LogicalResourceId = new string[0];
        public string[] ResourceType = new string[0];
        public string[] ResourceStatus = new string[0];
    }
    
    [StructLayout(LayoutKind.Sequential)]
    public class MultiKeyValueStringCallbackResult
    {
        public uint ResultCode;
        public string[] ResponseKeys = new string[0];
        public string[] ResponseValues = new string[0];
    }

    [StructLayout(LayoutKind.Sequential)]
    public class MultiStringCallbackResult
    {
        public uint ResultCode;
        public string[] ResponseValues = new string[0];
    }
}
