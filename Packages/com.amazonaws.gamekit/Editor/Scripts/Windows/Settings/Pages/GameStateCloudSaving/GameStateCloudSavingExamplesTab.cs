
// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common;
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Features.GameKitGameSaving;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.GameStateCloudSaving
{
    [Serializable]
    public class GameStateCloudSavingExamplesTab : FeatureExamplesTab
    {
        private const string GAME_SAVING_TESTING_DIRECTORY_NAME = "Game_Saving";

        public override FeatureType FeatureType => FeatureType.GameStateCloudSaving;
        
        private enum InitializationLevel
        {
            Uninitialized,
            InProgress,
            Initialized
        };

        // Dependencies
        private IGameSavingProvider _gameSaving;
        private IFileManager _fileManager;

        // Examples
        [SerializeField] private SaveSlotExampleUI _saveSlotExampleUI;
        [SerializeField] private LoadSlotExampleUI _loadSlotExampleUI;
        [SerializeField] private DeleteSlotExampleUI _deleteSlotExampleUI;
        [SerializeField] private GetSlotSyncStatusExampleUI _getSlotSyncStatusExampleUI;
        [SerializeField] private GetAllSlotSyncStatusesExampleUI _getAllSlotSyncStatusesExampleUI;

        // State
        [SerializeField] private bool _areCachedSlotsDisplayed;

        private InitializationLevel _initializationLevel = InitializationLevel.Uninitialized;
        private string _saveInfoDirectory;
        private IDictionary<string, Slot> _slots = new Dictionary<string, Slot>();

        public override void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _gameSaving = dependencies.GameSaving;
            _fileManager = dependencies.FileManager;

            _saveInfoDirectory = _fileManager.GetGameKitSaveDirectory();

            _saveSlotExampleUI.Initialize(CallSaveSlot, serializedProperty.FindPropertyRelative(nameof(_saveSlotExampleUI)));
            _loadSlotExampleUI.Initialize(CallLoadSlot, serializedProperty.FindPropertyRelative(nameof(_loadSlotExampleUI)));
            _deleteSlotExampleUI.Initialize(CallDeleteSlot, serializedProperty.FindPropertyRelative(nameof(_deleteSlotExampleUI)));
            _getSlotSyncStatusExampleUI.Initialize(CallGetSlotSyncStatus, serializedProperty.FindPropertyRelative(nameof(_getSlotSyncStatusExampleUI)));
            _getAllSlotSyncStatusesExampleUI.Initialize(CallGetAllSlotSyncStatuses, serializedProperty.FindPropertyRelative(nameof(_getAllSlotSyncStatusesExampleUI)));

            base.Initialize(dependencies, serializedProperty);
        }

        protected override void DrawExamples()
        {
            InitializeIfLoggedIn();
            UninitializeIfLoggedOut();

            _saveSlotExampleUI.OnGUI();
            _loadSlotExampleUI.OnGUI();
            _deleteSlotExampleUI.OnGUI();
            _getSlotSyncStatusExampleUI.OnGUI();
            _getAllSlotSyncStatusesExampleUI.OnGUI();

            EditorGUILayoutElements.SectionDivider();

            DrawCachedSlots();
        }


        #region Helpers
        public static void DrawSaveSlot(Slot slot, int indentationLevel = 1)
        {
            EditorGUILayoutElements.TextField("Slot Name", slot.SlotName, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Sync Status", Enum.GetName(typeof(SlotSyncStatus), slot.SlotSyncStatus), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Metadata Local", slot.MetadataLocal, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Metadata Cloud", slot.MetadataCloud, indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Size Local", slot.SizeLocal.ToString(), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Size Cloud", slot.SizeCloud.ToString(), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Last Modified Local", TimeUtils.EpochTimeToString(slot.LastModifiedLocal), indentationLevel, isEnabled: false);
            EditorGUILayoutElements.TextField("Last Modified Cloud", TimeUtils.EpochTimeToString(slot.LastModifiedCloud), indentationLevel, isEnabled: false);
        }

        public static void DrawSaveSlots(Slot[] slots, int indentationLevel = 1)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                DrawSaveSlot(slots[i], indentationLevel);
                if (i != slots.Length - 1)
                {
                    EditorGUILayoutElements.SectionDivider();
                }
            }
        }

        private void DrawCachedSlots()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleContainer))
            {
                _areCachedSlotsDisplayed = EditorGUILayout.Foldout(_areCachedSlotsDisplayed, L10n.Tr("Cached Slots"), SettingsGUIStyles.Page.FoldoutTitle);
                if (_areCachedSlotsDisplayed)
                {
                    using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleFoldoutContainer))
                    {
                        if (!_userInfo.IsLoggedIn)
                        {
                            EditorGUILayoutElements.Description(L10n.Tr("Log in and initialize <b>Game Saving</b> to view your cached slots."), 0);
                        }
                        else if (_initializationLevel != InitializationLevel.Initialized)
                        {
                            EditorGUILayoutElements.Description(L10n.Tr("Initializing cached slots..."), 0);
                        }
                        else if (_slots.Count == 0)
                        {
                            EditorGUILayoutElements.Description(L10n.Tr("No cached slots found. Try creating a new save slot by calling the <b>Save Slot</b> API."), 0);
                        }
                        else
                        {
                            DrawSaveSlots(_slots.Values.ToArray(), 0);
                        }
                    }
                }
            }
        }

        private string GetUserSaveInfoDirectory(string userName, string userId)
        {
            return $"{_saveInfoDirectory}/{userName}_{userId}/{GAME_SAVING_TESTING_DIRECTORY_NAME}";
        }

        private string GetSaveInfoFilePath(string userName, string userId, string slotName)
        {
            return $"{GetUserSaveInfoDirectory(userName, userId)}/{slotName}{GameSavingConstants.SAVE_INFO_FILE_EXTENSION}";
        }

        private void UpdateCachedSlots(Slot[] slots)
        {
            _slots.Clear();
            foreach (Slot slot in slots)
            {
                _slots.Add(slot.SlotName, slot);
            }
        }

        private void InitializeIfLoggedIn()
        {
            // If the user is logged in, ensure that we are initialized
            if (_userInfo.IsLoggedIn && _initializationLevel == InitializationLevel.Uninitialized)
            {
                // We can't initialize unless Game Saving has been deployed to the account in some fashion; check for Game Saving in the config file.
                // This check is only necessary for the editor examples. In a real game, you can rely on your Game Saving feature being deployed.
                // In that situation, simply initialize GameSaving with InitializeGameSavingForUser().
                if (_gameKitManager.AreFeatureSettingsLoaded(FeatureType.GameStateCloudSaving))
                {
                    _initializationLevel = InitializationLevel.InProgress;
                    InitializeGameSavingForUser();
                }
                else
                {
                    Debug.Log($"Cannot initialize Game Saving for user {_userInfo.UserName}, as the feature does not appear to be deployed.");
                }
            }
        }

        private void UninitializeIfLoggedOut()
        {
            // If the user just logged out, clear local state and uninitialize
            if (!_userInfo.IsLoggedIn && _initializationLevel == InitializationLevel.Initialized)
            {
                _slots.Clear();
                _initializationLevel = InitializationLevel.Uninitialized;
            }
        }
        #endregion

        #region GameSaving API Calls
        private void InitializeGameSavingForUser()
        {
            // Ensure that any synced slots for a previous user are cleared out
            _gameSaving.ClearSyncedSlots();

            string[] saveInfoFiles = _fileManager.ListFiles(GetUserSaveInfoDirectory(_userInfo.UserName, _userInfo.UserId), $"*{GameSavingConstants.SAVE_INFO_FILE_EXTENSION}");
            AddLocalSlotsDesc addLocalSlotsDesc = new AddLocalSlotsDesc
            {
                LocalSlotInformationFilePaths = saveInfoFiles
            };

            Debug.Log($"Calling GameSaving.AddLocalSlots() with {addLocalSlotsDesc}");

            _gameSaving.AddLocalSlots(addLocalSlotsDesc, () =>
            {
                Debug.Log("GameSaving.AddLocalSlots() completed");
                Debug.Log($"Calling GameSaving.GetAllSlotSyncStatuses() for user {_userInfo.UserName}");

                _gameSaving.GetAllSlotSyncStatuses((SlotListResult result) =>
                {
                    Debug.Log($"GameSaving.GetAllSlotSyncStatuses() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                    if (result.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                    {
                        /*
                        * Check for sync conflicts:
                        *
                        * In your own game, when you call GetAllSlotSyncStatuses() at the game's startup,
                        * you would want to loop through the "CachedSlots" parameter and look for any slots with SlotSyncStatus == IN_CONFLICT.
                        *
                        * If any slots are in conflict, you'd likely want to present this conflict to the player and let them decide which file to keep: the local save or the cloud save.
                        */
                        UpdateCachedSlots(result.CachedSlots);
                    }
                    _initializationLevel = InitializationLevel.Initialized;
                });
            });
        }

        private void CallSaveSlot()
        {
            if (!System.IO.File.Exists(_saveSlotExampleUI.FilePath))
            {
                _saveSlotExampleUI.ResultCode = GameKitErrors.GAMEKIT_ERROR_GAME_SAVING_FILE_FAILED_TO_OPEN;
            }

            SaveSlotDesc saveSlotDesc = new SaveSlotDesc
            {
                SaveInfoFilePath = GetSaveInfoFilePath(_userInfo.UserName, _userInfo.UserId, _saveSlotExampleUI.SlotName),
                Data = _fileManager.ReadAllBytes(_saveSlotExampleUI.FilePath),
                EpochTime = _fileManager.GetFileLastModifiedMilliseconds(_saveSlotExampleUI.FilePath),
                Metadata = _saveSlotExampleUI.Metadata,
                OverrideSync = _saveSlotExampleUI.OverrideSync,
                SlotName = _saveSlotExampleUI.SlotName
            };

            Debug.Log($"Calling GameSaving.SaveSlot() with {saveSlotDesc}");

            _gameSaving.SaveSlot(saveSlotDesc, (SlotActionResult result) =>
            {
                Debug.Log($"GameSaving.SaveSlot() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _saveSlotExampleUI.ResultCode = result.ResultCode;
                _saveSlotExampleUI.ActionedSlot = result.ActionedSlot;
                UpdateCachedSlots(result.CachedSlots);
            });
        }

        private void CallLoadSlot()
        {
            if (!_slots.ContainsKey(_loadSlotExampleUI.SlotName))
            {
                Debug.LogWarning($"Cannot load slot {_loadSlotExampleUI.SlotName}, as it doesn't exist. Please sync your slot status by calling GetAllSlotSyncStatuses().");
                _loadSlotExampleUI.ResultCode = GameKitErrors.GAMEKIT_ERROR_GAME_SAVING_SLOT_NOT_FOUND;
                _loadSlotExampleUI.ActionedSlot = new Slot();
                return;
            }

            /*
            * Before calling LoadSlot(), you need to pre-allocate enough bytes to hold the cloud save file.
            *
            * We recommend determining how many bytes are needed by caching the Slot object
            * from the most recent Game Saving API call before calling LoadSlot(). From this cached object, you
            * can get the SizeCloud of the slot you are going to download. Note: the SizeCloud will be incorrect
            * if the cloud save has been updated from another device since the last time this device cached the
            * Slot. In that case, call GetSlotSyncStatus() to get the accurate size.
            *
            * In this example, we cache the Slot object from *every* Game Saving API call because we
            * allow you to test out the Game Saving APIs in any order.
            *
            * Alternative to caching, you can call GetSlotSyncStatus(slotName) to get the size of the cloud file.
            * However, this has extra latency compared to caching the results of the previous Game Saving API call.
            */
            long size = _slots[_loadSlotExampleUI.SlotName].SizeCloud;
            byte[] data = new byte[size];

            LoadSlotDesc loadSlotDesc = new LoadSlotDesc
            {
                SaveInfoFilePath = GetSaveInfoFilePath(_userInfo.UserName, _userInfo.UserId, _loadSlotExampleUI.SlotName),
                Data = data,
                OverrideSync = _loadSlotExampleUI.OverrideSync,
                SlotName = _loadSlotExampleUI.SlotName,
            };

            Debug.Log($"Calling GameSaving.LoadSlot() with {loadSlotDesc}");

            _gameSaving.LoadSlot(loadSlotDesc, (SlotDataResult result) =>
            {
                Debug.Log($"GameSaving.LoadSlot() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _loadSlotExampleUI.ResultCode = result.ResultCode;

                if (result.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    try
                    {
                        _fileManager.WriteAllBytes(_loadSlotExampleUI.FilePath, result.Data);
                    }
                    catch (Exception e) when (e is ArgumentException || e is NullReferenceException || e is IOException)
                    {
                        _loadSlotExampleUI.ResultCode = GameKitErrors.GAMEKIT_ERROR_FILE_WRITE_FAILED;
                        Debug.LogException(e);
                    }
                }

                _loadSlotExampleUI.ActionedSlot = result.ActionedSlot;
                UpdateCachedSlots(result.CachedSlots);
            });
        }

        private void CallDeleteSlot()
        {
            if (!_slots.ContainsKey(_deleteSlotExampleUI.SlotName))
            {
                Debug.LogWarning($"Cannot delete slot {_deleteSlotExampleUI.SlotName}, as it doesn't exist.");
                _deleteSlotExampleUI.ResultCode = GameKitErrors.GAMEKIT_ERROR_GAME_SAVING_SLOT_NOT_FOUND;
                return;
            }

            Debug.Log($"Calling GameSaving.DeleteSlot() for slot {_deleteSlotExampleUI.SlotName}");

            _gameSaving.DeleteSlot(_deleteSlotExampleUI.SlotName, (SlotActionResult result) =>
            {
                Debug.Log($"GameSaving.DeleteSlot() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _deleteSlotExampleUI.ResultCode = result.ResultCode;

                if (result.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    _slots.Remove(result.ActionedSlot.SlotName);

                    /*
                    * Delete the local SaveInfo.json file corresponding to the deleted save slot:
                    *
                    * In your own game, you'll probably want to delete the local save file and corresponding SaveInfo.json file from the device after calling DeleteSlot().
                    * If you keep the SaveInfo.json file, then next time the game boots up this library will recommend re-uploading the save file to the cloud when
                    * you call GetAllSlotSyncStatuses() or GetSlotSyncStatus().
                    *
                    * Note: DeleteSlot() doesn't delete any local files from the device. It only deletes data from the cloud and from memory (i.e. the cached slot).
                    */
                    string saveInfoPath = GetSaveInfoFilePath(_userInfo.UserName, _userInfo.UserId, result.ActionedSlot.SlotName);
                    _fileManager.DeleteFile(saveInfoPath);
                }
            });
        }

        private void CallGetSlotSyncStatus()
        {

            if (!_slots.ContainsKey(_getSlotSyncStatusExampleUI.SlotName))
            {
                Debug.Log($"Cannot get sync status for slot {_getSlotSyncStatusExampleUI.SlotName}, as it isn't cached. Either create a new slot by calling SaveSlot(), or refresh the list of available slots with GetAllSlotSyncStatuses().");
            }

            Debug.Log($"Calling GameSaving.GetSlotSyncStatus() for slot {_getSlotSyncStatusExampleUI.SlotName}");

            _gameSaving.GetSlotSyncStatus(_getSlotSyncStatusExampleUI.SlotName, (SlotActionResult result) =>
            {
                Debug.Log($"GameSaving.GetSlotSyncStatus() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _getSlotSyncStatusExampleUI.ResultCode = result.ResultCode;
                _getSlotSyncStatusExampleUI.ActionedSlot = result.ActionedSlot;

                UpdateCachedSlots(result.CachedSlots);
            });
        }

        private void CallGetAllSlotSyncStatuses()
        {
            Debug.Log($"Calling GameSaving.GetAllSlotSyncStatuses()");

            _gameSaving.GetAllSlotSyncStatuses((SlotListResult result) =>
            {
                Debug.Log($"GameSaving.GetAllSlotSyncStatuses() completed with result code {GameKitErrorConverter.GetErrorName(result.ResultCode)}");
                _getAllSlotSyncStatusesExampleUI.ResultCode = result.ResultCode;
                _getAllSlotSyncStatusesExampleUI.Slots = result.CachedSlots;

                UpdateCachedSlots(result.CachedSlots);
            });
        }
        #endregion
    }

    #region Example 
    [Serializable]
    public class SaveSlotExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Save Slot";
        protected override bool _shouldDisplayResponse => true;

        public string SlotName;
        public string Metadata;
        public string FilePath;
        public bool OverrideSync;
        public Slot ActionedSlot = new Slot();

        protected override void DrawInput()
        {
            PropertyField(nameof(SlotName), "Slot Name");
            PropertyField(nameof(Metadata), "Metadata");
            SerializedProperty filePath = _serializedProperty.FindPropertyRelative(nameof(FilePath));
            filePath.stringValue = EditorGUILayoutElements.FileSelection("File Path", filePath.stringValue, "Select a file", "", 0);
            PropertyField(nameof(OverrideSync), "Override Sync");
        }

        protected override void DrawOutput()
        {
            GameStateCloudSavingExamplesTab.DrawSaveSlot(ActionedSlot);
        }
    }

    [Serializable]
    public class LoadSlotExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Load Slot";
        protected override bool _shouldDisplayResponse => true;

        public string SlotName;
        public string FilePath;
        public bool OverrideSync;
        public Slot ActionedSlot = new Slot();

        protected override void DrawInput()
        {
            PropertyField(nameof(SlotName), "Slot Name");
            SerializedProperty filePath = _serializedProperty.FindPropertyRelative(nameof(FilePath));
            filePath.stringValue = EditorGUILayoutElements.FileSelection("File Path", filePath.stringValue, "Select a file", "", 0, openingFile: false);
            PropertyField(nameof(OverrideSync), "Override Sync");
        }

        protected override void DrawOutput()
        {
            GameStateCloudSavingExamplesTab.DrawSaveSlot(ActionedSlot);
        }
    }

    [Serializable]
    public class DeleteSlotExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Delete Slot";
        public string SlotName;

        protected override void DrawInput()
        {
            PropertyField(nameof(SlotName), "Slot Name");
        }
    }

    [Serializable]
    public class GetSlotSyncStatusExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Get Slot Sync Status";
        protected override bool _shouldDisplayResponse => true;

        public string SlotName;
        public Slot ActionedSlot = new Slot();

        protected override void DrawInput()
        {
            PropertyField(nameof(SlotName), "Slot Name");
        }

        protected override void DrawOutput()
        {
            GameStateCloudSavingExamplesTab.DrawSaveSlot(ActionedSlot);
        }
    }


    [Serializable]
    public class GetAllSlotSyncStatusesExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Get All Slot Sync Statuses";
        protected override bool _shouldDisplayResponse => true;

        public Slot[] Slots = new Slot[0];

        protected override void DrawOutput()
        {
            GameStateCloudSavingExamplesTab.DrawSaveSlots(Slots);
        }
    }
    #endregion
}