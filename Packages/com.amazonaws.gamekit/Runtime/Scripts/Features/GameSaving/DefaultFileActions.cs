// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

// Standard Library
using System;
using System.IO;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Features.GameKitGameSaving
{
    /// <summary>
    /// Describes default file actions to be called by the GameKit SDK
    /// </summary>
    public class DefaultFileActions
    {
        /// <summary>
        /// Create a FileActions struct.
        /// </summary>
        /// <returns>The created FileActions struct.</returns>
        public static FileActions Make()
        {
            FileActions actions;
            actions.FileWriteCallback = WriteCallback;
            actions.FileReadCallback = ReadCallback;
            actions.FileSizeCallback = GetFileSizeCallback;
            actions.FileWriteDispatchReceiver = IntPtr.Zero;
            actions.FileReadDispatchReceiver = IntPtr.Zero;
            actions.FileSizeDispatchReceiver = IntPtr.Zero;
            return actions;
        }

        private static bool WriteDesktopFile(string filePath, byte[] data)
        {
            try
            {
                // Ensure the parent directory exists - if not, create it now
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }

                File.WriteAllBytes(filePath, data);
            }
            catch (Exception exception)
            {
                Logging.LogException($"Failed to write data to {filePath}", exception);
                return false;
            }
            return true;
        }
        
        private static bool ReadDesktopFile(string filePath, IntPtr data, uint size)
        {
            try
            {
                byte[] byteData = File.ReadAllBytes(filePath);
                Marshal.Copy(byteData, 0, data, (int) size);
            }
            catch (Exception exception)
            {
                Logging.LogException($"Failed to read data from {filePath}", exception);
                return false;
            }
            return true;
        }

        private static uint GetDesktopFileSize(string filePath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                return (uint)fileInfo.Length;
            }
            catch (Exception exception)
            {
                Logging.LogException($"Failed to determine file size of {filePath}", exception);
                return 0;
            }
        }

        [AOT.MonoPInvokeCallback(typeof(FuncFileWriteCallback))]
        private static bool WriteCallback(IntPtr dispatchReceiver, string filePath, byte[] data, uint size)
        {
            return WriteDesktopFile(filePath, data);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncFileReadCallback))]
        private static bool ReadCallback(IntPtr dispatchReceiver, string filePath, IntPtr data, uint size)
        {
            return ReadDesktopFile(filePath, data, size);
        }

        [AOT.MonoPInvokeCallback(typeof(FuncFileGetSizeCallback))]
        private static uint GetFileSizeCallback(IntPtr dispatchReceiver, string filePath)
        {
            return GetDesktopFileSize(filePath);
        }
    }
}
