// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Common
{
    /// <summary>
    /// This class lists all the paths used by GameKit
    /// </summary>
    public class GameKitPaths : Singleton<GameKitPaths>, IGameKitPathsProvider
    {
        #region Files
        public virtual string README_FILE_NAME => "README.md";
        public virtual string SETTINGS_WINDOW_STATE_FILE_NAME => "SettingsWindowState.asset";
        public virtual string GIT_IGNORE_FILE_NAME => ".gitignore";
        public virtual string SAVE_INFO_FILE_NAME => "saveInfo.yml";
        #endregion

        #region Folders
        public virtual string ASSETS_DATA_FOLDER_NAME => "Assets";
        public virtual string PACKAGES_FOLDER_NAME => "Packages";
        public virtual string GAME_KIT_FOLDER_NAME => "com.amazonaws.gamekit";
        public virtual string EDITOR_FOLDER_NAME => "Editor";
        public virtual string SECURITY_FOLDER_NAME => "Security";
        public virtual string CERTIFICATES_FOLDER_NAME => "Certs";
        public virtual string RESOURCE_FOLDER_NAME => "Resources";
        public virtual string CLOUD_RESOURCES_FOLDER_NAME => "CloudResources";
        public virtual string BASE_TEMPLATES_FOLDER_NAME => ".BaseTemplates";
        public virtual string INSTANCE_FILES_FOLDER_NAME => "InstanceFiles";
        public virtual string WINDOWS_STATE_FOLDER_NAME => "WindowState";
        public virtual string GAMEKIT_ART_FOLDER => "Art";
        public virtual string PLUGINS_FOLDER_NAME => "Plugins";
        public virtual string ANDROID_FOLDER_NAME => "Android";
        public virtual string GAMEKIT_CONFIG_ANDROID_LIB_FOLDER_NAME => "GameKitConfig.androidlib";
        public virtual string GAMEKIT_CONFIG_ASSETS_FOLDER_NAME => "assets";
        public virtual string RAW_FOLDER_NAME => "raw";
        public virtual string CERT_FILE_NAME => "cacert.pem";
        public virtual string ANDROID_MANIFEST_FILE_NAME => "AndroidManifest.xml";
        #endregion

        #region ASSETS Paths
        public virtual string ASSETS_RELATIVE_PATH => Path(ASSETS_DATA_FOLDER_NAME, GAME_KIT_FOLDER_NAME);
        public virtual string ASSETS_FULL_PATH => CleanPath(System.IO.Path.GetFullPath(ASSETS_RELATIVE_PATH));
        public virtual string ASSETS_EDITOR_RELATIVE_PATH => Path(ASSETS_RELATIVE_PATH, EDITOR_FOLDER_NAME);
        public virtual string ASSETS_EDITOR_FULL_PATH => Path(ASSETS_FULL_PATH, EDITOR_FOLDER_NAME);
        public virtual string ASSETS_EDITOR_RESOURCES_RELATIVE_PATH => Path(ASSETS_EDITOR_RELATIVE_PATH, RESOURCE_FOLDER_NAME);
        public virtual string ASSETS_EDITOR_RESOURCES_FULL_PATH => Path(ASSETS_EDITOR_FULL_PATH, RESOURCE_FOLDER_NAME);
        public virtual string ASSETS_RESOURCES_RELATIVE_PATH => Path(ASSETS_RELATIVE_PATH, RESOURCE_FOLDER_NAME);
        public virtual string ASSETS_RESOURCES_FULL_PATH => Path(ASSETS_FULL_PATH, RESOURCE_FOLDER_NAME);
        public virtual string ASSETS_README_RELATIVE_PATH => Path(ASSETS_RESOURCES_RELATIVE_PATH, README_FILE_NAME);
        public virtual string ASSETS_EDITOR_RESOURCES_README_RELATIVE_PATH => Path(ASSETS_EDITOR_RESOURCES_RELATIVE_PATH, README_FILE_NAME);
        public virtual string ASSETS_CLOUD_RESOURCES_RELATIVE_PATH => Path(ASSETS_EDITOR_RELATIVE_PATH, CLOUD_RESOURCES_FOLDER_NAME);
        public virtual string ASSETS_CLOUD_RESOURCES_FULL_PATH => Path(ASSETS_EDITOR_FULL_PATH, CLOUD_RESOURCES_FOLDER_NAME);
        public virtual string ASSETS_CLOUD_RESOURCES_README_RELATIVE_PATH => Path(ASSETS_CLOUD_RESOURCES_RELATIVE_PATH, README_FILE_NAME);
        public virtual string ASSETS_INSTANCE_FILES_RELATIVE_PATH => Path(ASSETS_CLOUD_RESOURCES_RELATIVE_PATH, INSTANCE_FILES_FOLDER_NAME);
        public virtual string ASSETS_INSTANCE_FILES_FULL_PATH => Path(ASSETS_CLOUD_RESOURCES_FULL_PATH, INSTANCE_FILES_FOLDER_NAME);
        public virtual string ASSETS_INSTANCE_FILES_README_RELATIVE_PATH => Path(ASSETS_INSTANCE_FILES_RELATIVE_PATH, README_FILE_NAME);
        public virtual string ASSETS_WINDOW_STATE_RELATIVE_PATH => Path(ASSETS_EDITOR_RELATIVE_PATH, WINDOWS_STATE_FOLDER_NAME);
        public virtual string ASSETS_WINDOW_STATE_README_RELATIVE_PATH => Path(ASSETS_WINDOW_STATE_RELATIVE_PATH, README_FILE_NAME);
        public virtual string ASSETS_SETTINGS_WINDOW_STATE_RELATIVE_PATH => Path(ASSETS_WINDOW_STATE_RELATIVE_PATH, SETTINGS_WINDOW_STATE_FILE_NAME);
        public virtual string ASSETS_GIT_IGNORE_RELATIVE_PATH => Path(ASSETS_RELATIVE_PATH, GIT_IGNORE_FILE_NAME);
        public virtual string ASSETS_GAMEKIT_ART_PATH => Path(ASSETS_EDITOR_RELATIVE_PATH, GAMEKIT_ART_FOLDER);
        public virtual string ASSETS_GAMEKIT_ANDROID_PATH => Path(new string[] { ASSETS_RELATIVE_PATH, PLUGINS_FOLDER_NAME, ANDROID_FOLDER_NAME});
        public virtual string ASSETS_GAMEKIT_RAW_RELATIVE_PATH => Path(new string[] { ASSETS_GAMEKIT_ANDROID_PATH, GAMEKIT_CONFIG_ANDROID_LIB_FOLDER_NAME, GAMEKIT_CONFIG_ASSETS_FOLDER_NAME, RAW_FOLDER_NAME });
        public virtual string ASSETS_GAMEKIT_RAW_CERT_RELATIVE_PATH => Path(new string[] { ASSETS_GAMEKIT_ANDROID_PATH, GAMEKIT_CONFIG_ANDROID_LIB_FOLDER_NAME, GAMEKIT_CONFIG_ASSETS_FOLDER_NAME, RAW_FOLDER_NAME, CERT_FILE_NAME });
        public virtual string ASSETS_GAMEKIT_ANDROID_MANIFEST_RELATIVE_PATH => Path(new string[] { ASSETS_GAMEKIT_ANDROID_PATH, GAMEKIT_CONFIG_ANDROID_LIB_FOLDER_NAME, ANDROID_MANIFEST_FILE_NAME });
        #endregion

        #region PACKAGES Paths
        public virtual string PACKAGES_RELATIVE_PATH => Path(PACKAGES_FOLDER_NAME, GAME_KIT_FOLDER_NAME);
        public virtual string PACKAGES_FULL_PATH => CleanPath(System.IO.Path.GetFullPath(PACKAGES_RELATIVE_PATH));
        public virtual string PACKAGES_EDITOR_RELATIVE_PATH => Path(PACKAGES_RELATIVE_PATH, EDITOR_FOLDER_NAME);
        public virtual string PACKAGES_EDITOR_FULL_PATH => Path(PACKAGES_FULL_PATH, EDITOR_FOLDER_NAME);
        public virtual string PACKAGES_CERTIFICATES_FULL_PATH => Path(PACKAGES_FULL_PATH, SECURITY_FOLDER_NAME, CERTIFICATES_FOLDER_NAME);
        public virtual string PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH => Path(PACKAGES_EDITOR_RELATIVE_PATH, RESOURCE_FOLDER_NAME);
        public virtual string PACKAGES_EDITOR_RESOURCES_FULL_PATH => Path(PACKAGES_EDITOR_FULL_PATH, RESOURCE_FOLDER_NAME);
        public virtual string PACKAGES_EDITOR_RESOURCES_README_RELATIVE_PATH => Path(PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH, README_FILE_NAME);
        public virtual string PACKAGES_RESOURCES_README_RELATIVE_PATH => Path(PACKAGES_RELATIVE_PATH, RESOURCE_FOLDER_NAME, README_FILE_NAME);
        public virtual string PACKAGES_CLOUD_RESOURCES_RELATIVE_PATH => Path(PACKAGES_EDITOR_RELATIVE_PATH, CLOUD_RESOURCES_FOLDER_NAME);
        public virtual string PACKAGES_CLOUD_RESOURCES_FULL_PATH => Path(PACKAGES_EDITOR_FULL_PATH, CLOUD_RESOURCES_FOLDER_NAME);
        public virtual string PACKAGES_CLOUD_RESOURCES_README_RELATIVE_PATH => Path(PACKAGES_CLOUD_RESOURCES_RELATIVE_PATH, README_FILE_NAME);
        public virtual string PACKAGES_BASE_TEMPLATES_RELATIVE_PATH => Path(PACKAGES_CLOUD_RESOURCES_RELATIVE_PATH, BASE_TEMPLATES_FOLDER_NAME);
        public virtual string PACKAGES_BASE_TEMPLATES_FULL_PATH => Path(PACKAGES_CLOUD_RESOURCES_FULL_PATH, BASE_TEMPLATES_FOLDER_NAME);
        public virtual string PACKAGES_WINDOW_STATE_README_RELATIVE_PATH => Path(PACKAGES_EDITOR_RELATIVE_PATH, WINDOWS_STATE_FOLDER_NAME, README_FILE_NAME);
        public virtual string PACKAGES_INSTANCE_FILES_README_RELATIVE_PATH => Path(PACKAGES_CLOUD_RESOURCES_RELATIVE_PATH, INSTANCE_FILES_FOLDER_NAME, README_FILE_NAME);

        public virtual string PACKAGES_CACERT_RELATIVE_PATH => Path(new string[] { PACKAGES_RELATIVE_PATH, SECURITY_FOLDER_NAME, CERTIFICATES_FOLDER_NAME, CERT_FILE_NAME});
        public virtual string PACKAGES_GIT_IGNORE_RELATIVE_PATH => Path(PACKAGES_RELATIVE_PATH, GIT_IGNORE_FILE_NAME);
        public virtual string PACKAGES_ANDROID_MANIFEST_RELATIVE_PATH => Path(new string[] { PACKAGES_RELATIVE_PATH, PLUGINS_FOLDER_NAME, ANDROID_FOLDER_NAME, GAMEKIT_CONFIG_ANDROID_LIB_FOLDER_NAME, ANDROID_MANIFEST_FILE_NAME });
        #endregion

        private static string Path(string path1, string path2)
        {
            return CleanPath(System.IO.Path.Combine(path1, path2));
        }

        private static string Path(string path1, string path2, string path3)
        {
            return CleanPath(System.IO.Path.Combine(path1, path2, path3));
        }

        private static string Path(string[] path)
        {
            return CleanPath(System.IO.Path.Combine(path));
        }

        private static string CleanPath(string path)
        {
            return path.Replace("\\", "/");
        }
    }
}