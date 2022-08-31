// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.FeatureUtils
{
    public abstract class GameKitFeatureWrapperBase
    {
        // Saved instance
        private IntPtr _instance = IntPtr.Zero;

        private readonly object _instanceCreationLock = new object();

        public virtual void Release()
        {
            if (_instance != IntPtr.Zero)
            {
                Release(_instance);
                _instance = IntPtr.Zero;
            }
        }

        protected abstract IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb);
        protected abstract void Release(IntPtr instance);

        // used to lazy instantiate the feature, and prevents its instantiation if the feature is not used
        protected IntPtr GetInstance()
        {
            if (_instance != IntPtr.Zero)
            {
                return _instance;
            }

            lock (_instanceCreationLock)
            {
                // If a call was waiting on this lock, this null check will be safeguard in case the pointer has already been created.
                if (_instance == IntPtr.Zero)
                {
                    _instance = Create(SessionManagerWrapper.Get().GetInstance(), Logging.LogCb);
                }
            }

            return _instance;
        }
    }
}
