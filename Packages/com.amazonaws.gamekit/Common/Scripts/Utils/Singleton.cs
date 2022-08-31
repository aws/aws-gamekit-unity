// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Common
{
    /// <summary>
    /// Utility class that wraps class T in a singleton pattern. Use Get() to initialize and return an instance of the wrapped class.
    /// </summary>
    public abstract class Singleton<T> where T : new()
    {
        private static Lazy<T> _instance = new Lazy<T>();

        /// <summary>
        /// Instantiates a class of type T if it has not already been instantiated, then returns the pointer to that instance.
        /// </summary>
        /// <return>The instance of class T</return>
        public static T Get()
        {
            return _instance.Value;
        }

        /// <summary>
        /// Gets the current initialized state of the held singleton.
        /// </summary>
        /// <remarks>To initialize, call Get().</remarks>
        /// <return>True if the instance is initialized.</return>
        public static bool IsInitialized()
        {
            return _instance.IsValueCreated;
        }
    }
}
