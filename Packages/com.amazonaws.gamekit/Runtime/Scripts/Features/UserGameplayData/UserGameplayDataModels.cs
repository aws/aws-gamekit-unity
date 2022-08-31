// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Features.GameKitUserGameplayData
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncNetworkStatusChangeCallback(IntPtr dispatchReceiver, bool isConnectionOk, string connectionClient);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncCacheProcessedCallback(IntPtr dispatchReceiver, bool isCacheProcessed);

    [StructLayout(LayoutKind.Sequential)]
    public struct UserGameplayDataBundle
    {
        public string BundleName;
        public IntPtr BundleItemKeys;
        public IntPtr BundleItemValues;   
        public IntPtr NumKeys;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UserGameplayDataBundleItem
    {
        public string BundleName;
        public string BundleItemKey;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UserGameplayDataBundleItemValue
    {
        public string BundleName;
        public string BundleItemKey;
        public string BundleItemValue;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UserGameplayDataDeleteItemsRequest
    {
        public string BundleName;
        public IntPtr BundleItemKeys;
        public IntPtr NumKeys;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct UserGameplayDataClientSettings
    {
        public uint ClientTimeoutSeconds;
        public uint RetryIntervalSeconds;
        public uint MaxRetryQueueSize;
        public uint MaxRetries;
        public uint RetryStrategy;
        public uint MaxExponentialRetryThreshold;
        public uint PaginationSize;
    };

    public class AddUserGameplayDataDesc
    {
        public string BundleName;
        public Dictionary<string, string> BundleItems = new Dictionary<string, string>();
    };

    public class AddUserGameplayDataResults
    {
        public uint ResultCode;
        public Dictionary<string, string> BundleItems = new Dictionary<string, string>();
    }

    public class GetUserGameplayDataBundleResults
    {
        public uint ResultCode;
        public Dictionary<string, string> Bundles = new Dictionary<string, string>();
    }

    public class DeleteUserGameplayDataBundleItemsDesc
    {
        public string BundleName;
        public string[] BundleItemKeys = new string[0];
        public ulong NumKeys = 0;
    };

    public class NetworkStatusChangeResults
    {
        public uint ResultCode;
        public bool IsConnectionOk;
        public string ConnectionClient;
    }

    public class CacheProcessedResults
    {
        public uint ResultCode;
        public bool IsCacheProcessed;
    }
}
