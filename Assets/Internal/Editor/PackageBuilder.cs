// // Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// // SPDX-License-Identifier: Apache-2.0

// System
using System;
using System.Linq;
using System.Threading;

//Unity
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Internal.Editor
{
    // https://docs.unity3d.com/ScriptReference/BuildPipeline.BuildPlayer.html
    public sealed class PackageBuilder : MonoBehaviour
    {

        private static PackRequest CURRENT_PACK_REQUEST;
        
        public static void BuildPackage()
        {
            var path = EditorUtility.SaveFolderPanel("Package Destination", "", "");
            if (path != null)
            {
                ExportPackage(path);
            }
        }
        
        public static bool IsNoPackageBuildInProgress()
        {
            return CURRENT_PACK_REQUEST == null;
        }

        /// <summary>
        /// Exports Unity package, using the last command line arg as the destination path.
        /// </summary>
        public static void ExportPackage()
        {
            var path = Environment.GetCommandLineArgs().Last();
            ExportPackage(path);
        }

        private static void ExportPackage(string path)
        {
            Debug.LogFormat($"Exporting package to {path}");
            CURRENT_PACK_REQUEST = Client.Pack("Packages/com.amazonaws.gamekit", path);
            while (CURRENT_PACK_REQUEST.Status == StatusCode.InProgress)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            switch (CURRENT_PACK_REQUEST.Status)
            {
                case StatusCode.Success:
                    Debug.Log($"Finished exporting package to {CURRENT_PACK_REQUEST.Result.tarballPath}");
                    CURRENT_PACK_REQUEST = null;
                    return;
                case StatusCode.Failure:
                    CURRENT_PACK_REQUEST = null;
                    throw new Exception("Failed to create package");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}