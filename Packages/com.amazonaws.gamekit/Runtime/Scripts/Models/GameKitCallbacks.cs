// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Models
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncLoggingCallback(uint level, string message, int size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncStringCallback(IntPtr dispatchReceiver, string responseValue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncKeyValueStringCallback(IntPtr dispatchReceiver, string responseKey, string responseValue);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncResourceInfoCallback(IntPtr dispatchReceiver, string logicalResourceId, string resourceType, string resourceStatus);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncDeploymentResponseCallback(IntPtr dispatchReceiver, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] FeatureType[] features, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] FeatureStatus[] featureStatuses, uint featureCount, uint callStatus);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncCanExecuteDeploymentActionCallback(IntPtr dispatchReceiver, FeatureType targetFeature, [MarshalAs(UnmanagedType.U1)] bool canExecuteAction, DeploymentActionBlockedReason reason, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] FeatureType[] blockingFeatures, uint featureCount);

    public static class GameKitCallbacks
    {
        [AOT.MonoPInvokeCallback(typeof(FuncKeyValueStringCallback))]
        public static void KeyValueStringCallback(IntPtr dispatchReceiver, string responseKey, string responseValue)
        {
            // recover object reference from dispatchReceiver
            KeyValueStringCallbackResult result = Marshaller.GetDispatchObject<KeyValueStringCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            result.ResponseKey = responseKey;
            result.ResponseValue = responseValue;
        }

        [AOT.MonoPInvokeCallback(typeof(FuncKeyValueStringCallback))]
        public static void MultiKeyValueStringCallback(IntPtr dispatchReceiver, string responseKey, string responseValue)
        {
            // recover object reference from dispatchReceiver
            MultiKeyValueStringCallbackResult result = Marshaller.GetDispatchObject<MultiKeyValueStringCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            Marshaller.AppendToArray(ref result.ResponseKeys, responseKey);
            Marshaller.AppendToArray(ref result.ResponseValues, responseValue);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncStringCallback))]
        public static void RecurringCallStringCallback(IntPtr dispatchReceiver, string responseValue)
        {
            // recover callback delegate from dispatchReceiver
            Action<StringCallbackResult> resultCallback = Marshaller.GetDispatchObject<Action<StringCallbackResult>>(dispatchReceiver);

            // create a temporary struct to hold the result
            StringCallbackResult result = new StringCallbackResult();

            // handle assignments to the result object
            result.ResponseValue = responseValue;

            // call the callback and pass it the result
            resultCallback(result);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncStringCallback))]
        public static void StringCallback(IntPtr dispatchReceiver, string responseValue)
        {
            // recover object reference from dispatchReceiver
            StringCallbackResult result = Marshaller.GetDispatchObject<StringCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            result.ResponseValue = responseValue;
        }

        [AOT.MonoPInvokeCallback(typeof(FuncStringCallback))]
        public static void MultiStringCallback(IntPtr dispatchReceiver, string responseValue)
        {
            // recover object reference from dispatchReceiver
            MultiStringCallbackResult result = Marshaller.GetDispatchObject<MultiStringCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            Marshaller.AppendToArray(ref result.ResponseValues, responseValue);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncResourceInfoCallback))]
        public static void ResourceInfoCallback(IntPtr dispatchReceiver, string logicalResourceId, string resourceType, string resourceStatus)
        {
            // recover object reference from dispatchReceiver
            ResourceInfoCallbackResult result = Marshaller.GetDispatchObject<ResourceInfoCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            result.LogicalResourceId = logicalResourceId;
            result.ResourceType = resourceType;
            result.ResourceStatus = resourceStatus;
        }

        [AOT.MonoPInvokeCallback(typeof(FuncResourceInfoCallback))]
        public static void MultiResourceInfoCallback(IntPtr dispatchReceiver, string logicalResourceId, string resourceType, string resourceStatus)
        {
            // recover object reference from dispatchReceiver
            MultiResourceInfoCallbackResult result = Marshaller.GetDispatchObject<MultiResourceInfoCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            Marshaller.AppendToArray(ref result.LogicalResourceId, logicalResourceId);
            Marshaller.AppendToArray(ref result.ResourceType, resourceType);
            Marshaller.AppendToArray(ref result.ResourceStatus, resourceStatus);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncDeploymentResponseCallback))]
        public static void DeploymentResponseCallback(IntPtr dispatchReceiver, FeatureType[] features, FeatureStatus[] featureStatuses, uint featureCount, uint callStatus)
        {
            // recover object reference from dispatchReceiver
            DeploymentResponseCallbackResult result = Marshaller.GetDispatchObject<DeploymentResponseCallbackResult>(dispatchReceiver);

            // handle assignments to the result object
            result.Features = features;
            result.FeatureStatuses = featureStatuses;

            // copy the GK Error status
            result.ResultCode = callStatus;
        }

        [AOT.MonoPInvokeCallback(typeof(FuncCanExecuteDeploymentActionCallback))]
        public static void CanExecuteDeploymentActionCallback(IntPtr dispatchReceiver, FeatureType targetFeature, bool canExecuteAction, DeploymentActionBlockedReason reason, FeatureType[] blockingFeatures, uint featureCount)
        {
            // recover object reference from dispatchReceiver
            CanExecuteDeploymentActionResult result = Marshaller.GetDispatchObject<CanExecuteDeploymentActionResult>(dispatchReceiver);

            // handle assignments to the result object
            result.TargetFeature = targetFeature;
            result.CanExecuteAction = canExecuteAction;
            result.Reason = reason;
            result.BlockingFeatures = blockingFeatures;
        }
    }
}
