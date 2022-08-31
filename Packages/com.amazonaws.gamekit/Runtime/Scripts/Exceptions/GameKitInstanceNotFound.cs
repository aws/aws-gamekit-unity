// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Runtime.Exceptions
{
    public class GameKitInstanceNotFound : Exception
    {
        public GameKitInstanceNotFound(string message)
            : base(message)
        {
        }
    }
}
