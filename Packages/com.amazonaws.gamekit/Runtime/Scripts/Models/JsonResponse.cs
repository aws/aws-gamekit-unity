// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Runtime.Models
{
    [Serializable]
    public class JsonResponse<T>
    {
        public T data;
    }
}