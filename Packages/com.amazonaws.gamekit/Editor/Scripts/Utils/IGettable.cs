// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// Exposes a <c>Get()</c> method which returns an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of objects that may be returned.</typeparam>
    public interface IGettable<out T>
    {
        /// <summary>
        /// Get an object of the specified type. See the implementing class for details on the returned object.
        /// </summary>
        public T Get();
    }
}