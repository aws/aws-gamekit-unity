// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// AWS GameKit
using AWS.GameKit.Common.Models;

namespace AWS.GameKit.Common
{
    /// <summary>
    /// Manages common file access patterns for AWS GameKit samples.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Gets the directory used by GameKit to store any metadata, should be broken down further according to your use-case. 
        /// </summary>
        /// <returns>The file path for the GameKit save directory.</returns>
        public string GetGameKitSaveDirectory();

        /// <summary>
        /// Enumerates all files in a directory with a name that matches the provided pattern.
        /// </summary>
        /// <param name="filePath">The relative or absolute path to the directory to search.</param>
        /// <param name="pattern">The search pattern to match against file names in the specified path. Allows literal characters, as well as * and ? wildcards.</param>
        /// <returns>An array of matching file names within the specified directory.</returns>
        public string[] ListFiles(string filePath, string pattern);

        /// <summary>
        /// Reads the contents of the specified file into a byte array.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <returns>The raw contents of the file, in the form of a byte array.</returns>
        public byte[] ReadAllBytes(string filePath);

        /// <summary>
        /// Writes the contents of the provided byte array to the specified file.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <param name="data">The raw data to be written to the file.</param>
        public void WriteAllBytes(string filePath, byte[] data);

        /// <summary>
        /// Gets the Linux epoch time that the file was last modified in milliseconds.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <returns>Epoch time when the file was last modified, in milliseconds</returns>
        public long GetFileLastModifiedMilliseconds(string filePath);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        public void DeleteFile(string filePath);
        
        /// <summary>
        /// Checks if the specified file exists.
        /// </summary>
        /// <param name="filePath">The absolute or relative path to the file.</param>
        /// <returns>True if the file specified exists, false otherwise.</returns>
        public bool FileExists(string filePath);
    }
}
