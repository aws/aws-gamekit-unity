// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Common
{
    /// <summary>
    /// Interface for providing GameKit paths
    /// </summary>
    public interface IGameKitPathsProvider
    {
        #region Files
        public string README_FILE_NAME { get; }
        public string SETTINGS_WINDOW_STATE_FILE_NAME { get; }
        public string GIT_IGNORE_FILE_NAME { get; }
        public string SAVE_INFO_FILE_NAME { get; }
        #endregion

        #region Folders
        public string ASSETS_DATA_FOLDER_NAME { get; }
        public string PACKAGES_FOLDER_NAME { get; }
        public string GAME_KIT_FOLDER_NAME { get; }
        public string EDITOR_FOLDER_NAME { get; }
        public string RESOURCE_FOLDER_NAME { get; }
        public string CLOUD_RESOURCES_FOLDER_NAME { get; }
        public string BASE_TEMPLATES_FOLDER_NAME { get; }
        public string INSTANCE_FILES_FOLDER_NAME { get; }
        public string WINDOWS_STATE_FOLDER_NAME { get; }
        #endregion

        #region ASSETS Paths
        public string ASSETS_RELATIVE_PATH { get; }
        public string ASSETS_FULL_PATH { get; }
        public string ASSETS_EDITOR_RELATIVE_PATH { get; }
        public string ASSETS_EDITOR_FULL_PATH { get; }
        public string ASSETS_EDITOR_RESOURCES_RELATIVE_PATH { get; }
        public string ASSETS_EDITOR_RESOURCES_FULL_PATH { get; }
        public string ASSETS_RESOURCES_RELATIVE_PATH { get; }
        public string ASSETS_RESOURCES_FULL_PATH { get; }
        public string ASSETS_README_RELATIVE_PATH { get; }
        public string ASSETS_EDITOR_RESOURCES_README_RELATIVE_PATH { get; }
        public string ASSETS_CLOUD_RESOURCES_RELATIVE_PATH { get; }
        public string ASSETS_CLOUD_RESOURCES_FULL_PATH { get; }
        public string ASSETS_CLOUD_RESOURCES_README_RELATIVE_PATH { get; }
        public string ASSETS_INSTANCE_FILES_RELATIVE_PATH { get; }
        public string ASSETS_INSTANCE_FILES_FULL_PATH { get; }
        public string ASSETS_INSTANCE_FILES_README_RELATIVE_PATH { get; }
        public string ASSETS_WINDOW_STATE_RELATIVE_PATH { get; }
        public string ASSETS_WINDOW_STATE_README_RELATIVE_PATH { get; }
        public string ASSETS_SETTINGS_WINDOW_STATE_RELATIVE_PATH { get; }
        public string ASSETS_GIT_IGNORE_RELATIVE_PATH { get; }
        #endregion

        #region PACKAGES Paths
        public string PACKAGES_RELATIVE_PATH { get; }
        public string PACKAGES_FULL_PATH { get; }
        public string PACKAGES_EDITOR_RELATIVE_PATH { get; }
        public string PACKAGES_EDITOR_FULL_PATH { get; }
        public string PACKAGES_EDITOR_RESOURCES_RELATIVE_PATH { get; }
        public string PACKAGES_EDITOR_RESOURCES_FULL_PATH { get; }
        public string PACKAGES_EDITOR_RESOURCES_README_RELATIVE_PATH { get; }
        public string PACKAGES_RESOURCES_README_RELATIVE_PATH { get; }
        public string PACKAGES_CLOUD_RESOURCES_RELATIVE_PATH { get; }
        public string PACKAGES_CLOUD_RESOURCES_FULL_PATH { get; }
        public string PACKAGES_CLOUD_RESOURCES_README_RELATIVE_PATH { get; }
        public string PACKAGES_BASE_TEMPLATES_RELATIVE_PATH { get; }
        public string PACKAGES_BASE_TEMPLATES_FULL_PATH { get; }
        public string PACKAGES_WINDOW_STATE_README_RELATIVE_PATH { get; }
        public string PACKAGES_INSTANCE_FILES_README_RELATIVE_PATH { get; }
        public string PACKAGES_GIT_IGNORE_RELATIVE_PATH { get; }
        #endregion
    }
}
