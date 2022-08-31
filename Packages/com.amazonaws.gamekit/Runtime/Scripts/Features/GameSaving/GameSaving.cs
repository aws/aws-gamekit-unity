// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Runtime.FeatureUtils;

namespace AWS.GameKit.Runtime.Features.GameKitGameSaving
{
    /// <summary>
    /// This class provides APIs for storing game save files in the cloud and synchronizing them with local devices.
    /// </summary>
    /// <remarks>
    /// 
    /// [Initialization]
    /// <para>
    /// The SetFileActions() can be called at the start of your program to provide custom file I/O to the Game Saving singleton. This is an optional call and can also be called more than once, however
    /// it is recommended to only call one time at the start of your program if needed. 
    ///
    /// Each time a user logs in, GameKitClearSyncedSlots(), GameKitAddLocalSlots(), and GameKitGetAllSlotSyncStatuses() must be called in that order to ensure all local and cloud slots are up to date.
    /// The GameKitClearSyncedSlots() can optionally be called after a user logs out instead of being called before GameKitAddLocalSlots().
    /// <list type="bullet">
    ///   <item>SetFileActions() is optional. It lets you provide custom file I/O in case the default I/O functions provided by Unity don't support your target platform(s). </item>
    ///   <item>ClearSyncedSlots() can be called during the initialization step, before AddLocalSlots() is called. Alternatively it can be called after a user logs out to ensure no cached slots remain.</item>
    ///   <item>AddLocalSlots() ensures Game Saving knows about local saves on the device that exist from previous times the game was played. </item>
    ///   <item>GetAllSlotSyncStatuses() ensures Game Saving has the latest information about the cloud saves, knows which local saves are synchronized
    ///   with the cloud, and which saves should be uploaded, downloaded, or need manual conflict resolution. </item>
    /// </list>
    /// </para>
    /// 
    /// [Offline Mode]
    /// <para>
    /// If your game is being played without internet, you must still call SaveSlot() and DeleteSlot() each time you would normally call these methods.
    /// Otherwise, there is a risk that the progress made while playing offline will be overwritten the next time the game is played on this device with
    /// an internet connection if a newer save has since been uploaded from another device.
    /// </para>
    /// 
    /// [Save Slots]
    /// <para>
    /// Save files that are uploaded/downloaded/tracked through this API are each associated with a named "save slot" for the player.
    ///
    /// When you deploy the Game Saving feature, you can configure the maximum number of cloud saves slots to provide each player. This limit can
    /// prevent malicious players from storing too much data in the cloud. You can change this limit by doing another deployment through the Plugin UI.
    /// </para>
    /// 
    /// [Slot Information]
    /// <para>
    /// The local and cloud attributes for a save slot are collectively known as "slot information" and are stored in the Slot class.
    /// </para>
    /// 
    /// [Cached Slots]
    /// <para>
    /// This library maintains a cache of slot information for all slots it interacts with (both locally and in the cloud).
    /// The cached slots are updated on every API call, and are also returned in the delegate of most API calls.
    /// </para>
    /// 
    /// [SaveInfo.json Files]
    /// <para>
    /// This library creates "SaveInfo.json" files on the device every time save files are uploaded/downloaded through the SaveSlot() and LoadSlot() APIs.
    /// 
    /// The exact filenames and locations are provided by you. We highly recommended you store the SaveInfo.json files alongside their corresponding
    /// save file to help developers and curious players to understand these files go together.
    ///
    /// The SaveInfo.json files are loaded during game startup by calling AddLocalSlots(). This informs the library about any save files that exist on the
    /// device from previous game sessions.
    /// </para>
    /// </remarks>
    public class GameSaving : GameKitFeatureBase<GameSavingWrapper>, IGameSavingProvider
    {
        public override FeatureType FeatureType => FeatureType.GameStateCloudSaving;

        /// <summary>
        /// Call to get an instance of the GameKit GameSaving feature.
        /// </summary>
        /// <returns>An instance of the GameSaving feature that can be used to call GameSaving related methods.</returns>
        public static GameSaving Get()
        {
            return GameKitFeature<GameSaving>.Get();
        }

        /// <summary>
        /// Asynchronously load slot information for all of the player's local saves on the device.<br/><br/>
        /// 
        /// This should be the first method you call on the Game Saving library (except optionally SetFileActions() and ClearSyncedSlots()) and
        /// you should call it exactly once per user that is logged in. Afterwards, you should call GetAllSlotSyncStatuses().
        /// See the class level documentation for more details on initialization.<br/><br/>
        /// 
        /// This method loads the SaveInfo.json files that were created on the device during previous game sessions when calling SaveSlot() and LoadSlot().
        /// This overwrites any cached slots in memory which have the same slot name as the slots loaded from the SaveInfo.json files.
        /// </summary>
        /// <param name="addSlotsDesc">Object containing a list of files to load when the game starts. 
        /// Each file should contain the slot information for the locally saved slot and should be the file generated automatically by the SaveSlot()/LoadSlot() methods.</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void AddLocalSlots(AddLocalSlotsDesc addSlotsDesc, Action callback)
        {
            Call(Feature.AddLocalSlots, addSlotsDesc, callback);
        }

        /// <summary>
        /// This method will immediately clear the slots that are cached in the GameKit backend. 
        ///
        /// ClearSyncedSlots() should be called immediately after a user logs out or immediately before AddLocalSlots() is called. This
        /// will ensure that any slots that were synced for a previous user are cleared out. 
        /// </summary>
        public void ClearSyncedSlots()
        {
            Feature.ClearSyncedSlots();
        }

        /// <summary>
        /// Asynchronously change the file I/O callbacks used by this library.<br/><br/>
        ///
        /// If you call this method, it should be the first method called on the library(even before AddLocalSlots()).<br/><br/>
        ///
        /// By default, this library uses the DefaultFileActions documented in AwsGameKitGameSavingWrapper.h
        /// These use Unity-provided file I/O methods and may not work on all platforms.
        /// Call this method to provide your own file I/O methods which support the necessary platform(s).
        /// </summary>
        /// <param name="fileActions">Object containing delegates for file IO</param>
        /// <param name="callback">Delegate that is called once the function has finished executing</param>
        public void SetFileActions(FileActions fileActions, Action callback)
        {
            Call(Feature.SetFileActions, fileActions, callback);
        }

        /// <summary>
        /// Asynchronously get a complete and updated view of the player's save slots (both local and cloud).<br/><br/>
        ///
        /// Call this method during initialization (see class level documentation) and any time you suspect the cloud saves may have
        /// been updated from another device.<br/><br/>
        ///
        /// This method adds cached slots for all cloud saves not currently on the device, updates all cached slots with accurate cloud attributes,
        /// and marks the SlotSyncStatus member of all cached slots with the recommended syncing action you should take.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature(AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed.Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload.This should not happen.If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="callback">Delegate called once this method completes. It contains information about each cloud save slot and known local slot. 
        /// Additionally it contains the result status code for the call.</param>
        public void GetAllSlotSyncStatuses(Action<SlotListResult> callback)
        {
            Call(Feature.GetAllSlotSyncStatuses, callback);
        }

        /// <summary>
        /// Asynchronously get an updated view and recommended syncing action for the player's specific save slot.<br/><br/>
        ///
        /// This method updates the specific save slot's cloud attributes and marks the SlotSyncStatus member with the recommended syncing action you should take.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature(AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_MALFORMED_SLOT_NAME: The provided slot name is malformed.Check the logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SLOT_NOT_FOUND: The provided slot name was not found in the cached slots.This either means you have a typo in the slot name,
        ///                                             or the slot only exists in the cloud and you need to call GetAllSlotSyncStatuses() first before calling this method.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed.Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload.This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="slotName">String identifier for the save slot</param>
        /// <param name="callback">Delegate called once this method completes. It contains information about the slot in question. Additionally it contains the result status code for the call.</param>
        public void GetSlotSyncStatus(string slotName, Action<SlotActionResult> callback)
        {
            Call(Feature.GetSlotSyncStatus, slotName, callback);
        }

        /// <summary>
        /// Asynchronously delete the player's cloud save slot and remove it from the cached slots.<br/><br/>
        ///
        /// No local files are deleted from the device. Data is only deleted from the cloud and from memory(the cached slot).<br/><br/>
        ///
        /// After calling DeleteSlot(), you'll probably want to delete the local save file and corresponding SaveInfo.json file from the device.
        /// If you keep the SaveInfo.json file, then next time the game boots up this library will recommend re-uploading the save file to the cloud when
        /// you call GetAllSlotSyncStatuses() or GetSlotSyncStatus().<br/><br/>
        ///
        /// If your game is being played without internet, you must still call this method and delete the SaveInfo.json file as normal to avoid the risk
        /// of having the offline progress be overwritten when internet connectivity is restored.See the "Offline Mode" section in the file level documentation for more details.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature(AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_MALFORMED_SLOT_NAME: The provided slot name is malformed.Check the logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SLOT_NOT_FOUND: The provided slot name was not found in the cached slots.This either means you have a typo in the slot name,
        ///                                             or the slot only exists in the cloud and you need to call GetAllSlotSyncStatuses() first before calling this method.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed.Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload.This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="slotName">String identifier for the save slot</param>
        /// <param name="callback">Delegate called once this method completes. It contains information about the slot that was deleted. Additionally it contains the result status code for the call.</param>
        public void DeleteSlot(string slotName, Action<SlotActionResult> callback)
        {
            Call(Feature.DeleteSlot, slotName, callback);
        }

        /// <summary>
        /// Asynchronously upload a data buffer to the cloud, overwriting the player's cloud slot if it already exists.<br/><br/>
        ///
        /// Also write the slot's information to a SaveInfo.json file on the device, and add the slot to the cached slots if it doesn't already exist.
        /// This SaveInfo.json file should be passed into AddLocalSlots() when you initialize the Game Saving library in the future.<br/><br/>
        ///
        /// If your game is being played without internet, you must still call this method as normal to avoid the risk of having the offline progress be
        /// overwritten when internet connectivity is restored.See the "Offline Mode" section in the file level documentation for more details.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature(AwsGameKitIdentity) before calling this method. <br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_MALFORMED_SLOT_NAME: The provided slot name is malformed.Check the logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_FILE_WRITE_FAILED: The SaveInfo.json file was unable to be written to the device.If using the default file I/O callbacks,
        ///                                    check the logs to see the root cause.If the platform is not supported by the default file I/O callbacks,
        ///                                    use SetFileActions() to provide your own callbacks.See SetFileActions() for more details.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_MAX_CLOUD_SLOTS_EXCEEDED: The upload was cancelled because it would have caused the player to exceed their "maximum cloud save slots limit". This limit
        ///                                                       was configured when you deployed the Game Saving feature and can be changed by doing another deployment through the Plugin UI.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_EXCEEDED_MAX_SIZE: The Metadata member of your Request object is too large. Please see the documentation on FGameSavingSaveSlotRequest::Metadata for details.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SYNC_CONFLICT: The upload was cancelled to prevent overwriting the player's progress. This most likely indicates the player has played on multiple
        ///                                            devices without having their progress properly synced with the cloud at the start and end of their play sessions.We recommend you inform
        ///                                            the player of this conflict and present them with a choice - keep the cloud save or keep the local save.Then call SaveSlot() or LoadSlot()
        ///                                            with OverrideSync= true to override the cloud/local file.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_CLOUD_SLOT_IS_NEWER: The upload was cancelled because the cloud save file is newer than the file you attempted to upload. Treat this like a
        ///                                                  GAMEKIT_ERROR_GAME_SAVING_SYNC_CONFLICT because the local and cloud save might have non-overlapping game progress.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed.Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload.This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="saveSlotDesc">Object containing the required information about the slot that needs to be saved(uploaded) to cloud storage</param>
        /// <param name="callback">Delegate called once this method completes. It contains information about the newly saved slot. Additionally it contains the result status code for the call.</param>
        public void SaveSlot(SaveSlotDesc saveSlotDesc, Action<SlotActionResult> callback)
        {
            Call(Feature.SaveSlot, saveSlotDesc, callback);
        }

        /// <summary>
        /// Asynchronously download the player's cloud slot into a local data buffer.
        ///
        /// Also write the slot's information to a SaveInfo.json file on the device.
        /// This SaveInfo.json file should be passed into AddLocalSlots() when you initialize the Game Saving library in the future.<br/><br/>
        /// 
        /// Result status codes returned in the callback function (from GameKitErrors.cs):<br/>
        /// - GAMEKIT_SUCCESS: The API call was successful.<br/>
        /// - GAMEKIT_ERROR_NO_ID_TOKEN: The player is not logged in. You must login the player through the Identity and Authentication feature(AwsGameKitIdentity) before calling this method.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_MALFORMED_SLOT_NAME: The provided slot name is malformed.Check the logs to see what the required format is.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SLOT_NOT_FOUND: The provided slot name was not found in the cached slots.This either means you have a typo in the slot name,
        ///                                             or the slot only exists in the cloud and you need to call GetAllSlotSyncStatuses() first before calling this method.<br/>
        /// - GAMEKIT_ERROR_FILE_WRITE_FAILED: The SaveInfo.json file was unable to be written to the device. If using the default file I/O callbacks,
        ///                                    check the logs to see the root cause.If the platform is not supported by the default file I/O callbacks,
        ///                                    use SetFileActions() to provide your own callbacks.See SetFileActions() for more details.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SYNC_CONFLICT: The download was cancelled to prevent overwriting the player's progress. This most likely indicates the player has played on multiple
        ///                                            devices without having their progress properly synced with the cloud at the start and end of their play sessions.We recommend you inform
        ///                                            the player of this conflict and present them with a choice - keep the cloud save or keep the local save.Then call SaveSlot() or LoadSlot()
        ///                                            with OverrideSync= true to override the cloud/local file.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_LOCAL_SLOT_IS_NEWER: The download was cancelled because the local save file is newer than the cloud file you attempted to download.Treat this like a
        ///                                                  GAMEKIT_ERROR_GAME_SAVING_SYNC_CONFLICT because the local and cloud save might have non-overlapping game progress.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SLOT_UNKNOWN_SYNC_STATUS: The download was cancelled because the sync status could not be determined. Treat this like a GAMEKIT_ERROR_GAME_SAVING_SYNC_CONFLICT.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_MISSING_SHA: The S3 file is missing a SHA-256 metadata attribute and therefore the validity of the file could not be determined.This should not happen. If it does,
        ///                                          this indicates there is a bug in the backend code.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_SLOT_TAMPERED: The SHA-256 hash of the downloaded file does not match the SHA-256 hash of the original file that was uploaded to S3. This indicates the downloaded
        ///                                            file was corrupted or tampered with. You should try downloading again to rule out the possibility of random corruption.<br/>
        /// - GAMEKIT_ERROR_GAME_SAVING_BUFFER_TOO_SMALL: The data buffer you provided in the Request object is not large enough to hold the downloaded S3 file.This likely means a newer version of the
        ///                                               cloud file was uploaded from another device since the last time you called GetAllSlotSyncStatuses() or GetSlotSyncStatus() on this device.To resolve,
        ///                                               call GetSlotSyncStatus() to get the up-to-date size of the cloud file.<br/>
        /// - GAMEKIT_ERROR_HTTP_REQUEST_FAILED: The backend HTTP request failed.Check the logs to see what the HTTP response code was.<br/>
        /// - GAMEKIT_ERROR_PARSE_JSON_FAILED: The backend returned a malformed JSON payload.This should not happen. If it does, it indicates there is a bug in the backend code.
        /// </summary>
        /// <param name="loadSlotDesc">Object containing the required information about the slot that needs to be loaded(downloaded) from cloud storage</param>
        /// <param name="callback">Delegate called once this method completes. It contains information about the slot loaded and a buffer to the data downloaded from cloud storage for the slot. 
        /// Additionally it contains the result status code for the call.</param>
        public void LoadSlot(LoadSlotDesc loadSlotDesc, Action<SlotDataResult> callback)
        {
            Call(Feature.LoadSlot, loadSlotDesc, callback);
        }
    }
}
