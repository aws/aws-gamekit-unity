// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEngine;

// Standard Library
using System;
using System.IO;
using System.Linq;

namespace AWS.GameKit.Common
{
    /// <summary>
    /// This class provides a basic implementation of IFileManager.
    /// </summary>
    public class FileManager : IFileManager
    {
        public string GetGameKitSaveDirectory()
        {
            return Path.GetFullPath($"{Application.persistentDataPath}/AWSGameKit");
        }

        public string[] ListFiles(string filePath, string searchPattern)
        {
            if (!Directory.Exists(filePath))
            {
                return Array.Empty<string>();
            }

            return Directory.EnumerateFiles(filePath, searchPattern).Select(file => Path.GetFullPath(file)).ToArray<string>();
        }

        public byte[] ReadAllBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public void WriteAllBytes(string filePath, byte[] data)
        {
            // Ensure the parent directory exists - if not, create it now
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            File.WriteAllBytes(filePath, data);
        }

        public long GetFileLastModifiedMilliseconds(string filePath)
        {
            FileInfo info = new FileInfo(filePath);
            DateTimeOffset offset = new DateTimeOffset(info.LastWriteTimeUtc);
            return offset.ToUnixTimeMilliseconds();
        }
        
        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }
}
