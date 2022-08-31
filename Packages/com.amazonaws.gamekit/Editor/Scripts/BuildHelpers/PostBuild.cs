// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.IO;

// Unity
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS   // Unity iOS is for Editor mode
using UnityEditor.iOS.Xcode;
#endif

// GameKit
using AWS.GameKit.Common;

namespace AWS.GameKit.Editor
{
    /// <summary>
    /// This class describes steps to be taken after the player is built.
    /// </summary>
    public class PostBuild
    {
        private const string CERTIFICATES_BUILD_PATH = "Data/Security/Certs/";
        
        /// <summary>
        /// Post build step for a specific target.
        /// </summary>
        /// <param name="target">The target to perform the post build step for.</param>
        /// <param name="pathToBuiltProject">Path to the built project.</param>
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.iOS)
                OnPostprocessBuildIOS(pathToBuiltProject);
        }

        private static void OnPostprocessBuildIOS(string pathToBuiltProject)
        {
#if UNITY_IOS   // Unity iOS is for Editor mode
            string xcodeProjectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            
            PBXProject xcodeProject = new PBXProject();
            xcodeProject.ReadFromString(File.ReadAllText(xcodeProjectPath));
            string unityFrameworkTarget = xcodeProject.GetUnityFrameworkTargetGuid();
            
            Directory.CreateDirectory(Path.Combine(pathToBuiltProject, CERTIFICATES_BUILD_PATH));

            string[] certificatesToCopy = new string[]
            {
                "cacert.pem"
            };

            for(int i = 0 ; i < certificatesToCopy.Length ; ++i)
            {
                string srcPath = Path.Combine(GameKitPaths.Get().PACKAGES_CERTIFICATES_FULL_PATH, certificatesToCopy[i]);
                string dstLocalPath = CERTIFICATES_BUILD_PATH + certificatesToCopy[i];
                string dstPath = Path.Combine(pathToBuiltProject, dstLocalPath);
                File.Copy(srcPath, dstPath, true);
            }
            
            // Compile with the newest version of libz xcode has available
            xcodeProject.AddBuildProperty(unityFrameworkTarget, "OTHER_LDFLAGS", "-lz");
            
            File.WriteAllText(xcodeProjectPath, xcodeProject.WriteToString());
#endif
        }
    }
}
