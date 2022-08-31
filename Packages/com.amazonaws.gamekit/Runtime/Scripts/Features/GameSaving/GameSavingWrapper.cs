// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.FeatureUtils;
using AWS.GameKit.Runtime.Models;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Runtime.Features.GameKitGameSaving
{
    /// <summary>
    /// Game Saving wrapper for GameKit C++ SDK calls
    /// </summary>
    public class GameSavingWrapper : GameKitFeatureWrapperBase
    {
        // Select the correct source path based on the platform
#if UNITY_IPHONE && !UNITY_EDITOR
        private const string IMPORT = "__Internal";
#else
        private const string IMPORT = "aws-gamekit-game-saving";
#endif

        // DLL loading   
        [DllImport(IMPORT)] private static extern IntPtr GameKitGameSavingInstanceCreateWithSessionManager(IntPtr sessionManager, FuncLoggingCallback logCb, string[] localSlotInformationFilePaths, uint arraySize, FileActions fileActions);
        [DllImport(IMPORT)] private static extern void GameKitGameSavingInstanceRelease(IntPtr gameSavingInstance);
        [DllImport(IMPORT)] private static extern void GameKitAddLocalSlots(IntPtr gameSavingInstance, string[] localSlotInformationFilePaths, uint arraySize);
        [DllImport(IMPORT)] private static extern void GameKitClearSyncedSlots(IntPtr gameSavingInstance);
        [DllImport(IMPORT)] private static extern void GameKitSetFileActions(IntPtr gameSavingInstance, FileActions fileActions);
        [DllImport(IMPORT)] private static extern uint GameKitGetAllSlotSyncStatuses(IntPtr gameSavingInstance, IntPtr dispatchReceiver, FuncGameSavingResponseCallback resultCb, bool waitForAllPages, uint pageSize);
        [DllImport(IMPORT)] private static extern uint GameKitGetSlotSyncStatus(IntPtr gameSavingInstance, IntPtr dispatchReceiver, FuncGameSavingSlotActionResponseCallback resultCb, string slotName);
        [DllImport(IMPORT)] private static extern uint GameKitDeleteSlot(IntPtr gameSavingInstance, IntPtr dispatchReceiver, FuncGameSavingSlotActionResponseCallback resultCb, string slotName);
        [DllImport(IMPORT)] private static extern uint GameKitSaveSlot(IntPtr gameSavingInstance, IntPtr dispatchReceiver, FuncGameSavingSlotActionResponseCallback resultCb, GameSavingModel model);
        [DllImport(IMPORT)] private static extern uint GameKitLoadSlot(IntPtr gameSavingInstance, IntPtr dispatchReceiver, FuncGameSavingDataResponseCallback resultCb, GameSavingModel model);

        [AOT.MonoPInvokeCallback(typeof(FuncGameSavingResponseCallback))]
        protected static void GameSavingResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, bool complete, uint callStatus)
        {
            // retrieve SlotListResult object reference from encoded IntPtr
            SlotListResult result = Marshaller.GetDispatchObject<SlotListResult>(dispatchReceiver);

            // handle assignments to the result object
            result.CachedSlots = Marshaller.IntPtrToArray<Slot>(cachedSlots, slotCount);

            // copy the GK Error status
            result.ResultCode = callStatus;
        }

        [AOT.MonoPInvokeCallback(typeof(FuncGameSavingSlotActionResponseCallback))]
        protected static void GameSavingSlotActionResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, IntPtr activeSlot, uint callStatus)
        {
            // retrieve SlotActionResult object reference from encoded IntPtr
            SlotActionResult result = Marshaller.GetDispatchObject<SlotActionResult>(dispatchReceiver);

            // handle assignments to the result object
            result.CachedSlots = Marshaller.IntPtrToArray<Slot>(cachedSlots, slotCount);
            result.ActionedSlot = Marshal.PtrToStructure<Slot>(activeSlot);

            // copy the GK Error status
            result.ResultCode = callStatus;
        }

        [AOT.MonoPInvokeCallback(typeof(FuncGameSavingDataResponseCallback))]
        protected static void GameSavingDataResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, IntPtr slot, byte[] data, uint dataSize, uint callStatus)
        {
            // retrieve SlotDataResult object reference from encoded IntPtr
            SlotDataResult result = Marshaller.GetDispatchObject<SlotDataResult>(dispatchReceiver);

            // handle assignments to the result object
            result.CachedSlots = Marshaller.IntPtrToArray<Slot>(cachedSlots, slotCount);
            result.ActionedSlot = Marshal.PtrToStructure<Slot>(slot);
            result.Data = data;
            result.DataSize = dataSize;

            // copy the GK Error status
            result.ResultCode = callStatus;
        }

        public void AddLocalSlots(AddLocalSlotsDesc addSlotDesc)
        {
            DllLoader.TryDll(() => GameKitAddLocalSlots(GetInstance(), addSlotDesc.LocalSlotInformationFilePaths, (uint)addSlotDesc.LocalSlotInformationFilePaths.Length), nameof(GameKitAddLocalSlots));
        }

        public void ClearSyncedSlots()
        {
            DllLoader.TryDll(() => GameKitClearSyncedSlots(GetInstance()), nameof(GameKitClearSyncedSlots));
        }

        public void SetFileActions(FileActions fileActions)
        {
            DllLoader.TryDll(() => GameKitSetFileActions(GetInstance(), fileActions), nameof(GameKitSetFileActions));
        }

        public SlotListResult GetAllSlotSyncStatuses()
        {
            SlotListResult result = new SlotListResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetAllSlotSyncStatuses(GetInstance(), dispatchReceiver, GameSavingResponseCallback, true, GameSavingConstants.GET_ALL_SLOT_SYNC_STATUSES_DEFAULT_PAGE_SIZE), nameof(GameKitGetAllSlotSyncStatuses));

            return result;
        }

        public SlotActionResult GetSlotSyncStatus(string slotName)
        {
            SlotActionResult result = new SlotActionResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitGetSlotSyncStatus(GetInstance(), dispatchReceiver, GameSavingSlotActionResponseCallback, slotName), nameof(GameKitGetSlotSyncStatus));

            return result;
        }

        public SlotActionResult DeleteSlot(string slotName)
        {
            SlotActionResult result = new SlotActionResult();

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitDeleteSlot(GetInstance(), dispatchReceiver, GameSavingSlotActionResponseCallback, slotName), nameof(GameKitDeleteSlot));

            return result;
        }

        public SlotActionResult SaveSlot(SaveSlotDesc saveSlotDesc)
        {
            SlotActionResult result = new SlotActionResult();

            GCHandle dataPinHandle = GCHandle.Alloc(saveSlotDesc.Data, GCHandleType.Pinned);
            
            GameSavingModel gameSavingModel = DefaultGameSavingModel.Make();
            gameSavingModel.SlotName = saveSlotDesc.SlotName;
            gameSavingModel.Metadata = saveSlotDesc.Metadata;
            gameSavingModel.EpochTime = saveSlotDesc.EpochTime;
            gameSavingModel.OverrideSync = saveSlotDesc.OverrideSync;
            gameSavingModel.Data = dataPinHandle.AddrOfPinnedObject();
            gameSavingModel.DataSize = (uint)saveSlotDesc.Data.Length;
            gameSavingModel.LocalSlotInformationFilePath = saveSlotDesc.SaveInfoFilePath;

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitSaveSlot(GetInstance(), dispatchReceiver, GameSavingSlotActionResponseCallback, gameSavingModel), nameof(GameKitSaveSlot));

            dataPinHandle.Free();
            
            return result;
        }

        public SlotDataResult LoadSlot(LoadSlotDesc loadSlotDesc)
        {
            SlotDataResult result = new SlotDataResult();

            GCHandle dataPinHandle = GCHandle.Alloc(loadSlotDesc.Data, GCHandleType.Pinned);
            
            GameSavingModel gameSavingModel = DefaultGameSavingModel.Make();
            gameSavingModel.SlotName = loadSlotDesc.SlotName;
            gameSavingModel.OverrideSync = loadSlotDesc.OverrideSync;
            gameSavingModel.Data = dataPinHandle.AddrOfPinnedObject();
            gameSavingModel.DataSize = (uint)loadSlotDesc.Data.Length;
            gameSavingModel.LocalSlotInformationFilePath = loadSlotDesc.SaveInfoFilePath;

            DllLoader.TryDll(result, (IntPtr dispatchReceiver) => GameKitLoadSlot(GetInstance(), dispatchReceiver, GameSavingDataResponseCallback, gameSavingModel), nameof(GameKitLoadSlot));

            dataPinHandle.Free();
            
            return result;
        }

        protected override IntPtr Create(IntPtr sessionManager, FuncLoggingCallback logCb)
        {
            return DllLoader.TryDll(() => GameKitGameSavingInstanceCreateWithSessionManager(sessionManager, logCb, new string[] { }, 0, DefaultFileActions.Make()), nameof(GameKitGameSavingInstanceCreateWithSessionManager), IntPtr.Zero);
        }

        protected override void Release(IntPtr instance)
        {
            DllLoader.TryDll(() => GameKitGameSavingInstanceRelease(instance), nameof(GameKitGameSavingInstanceRelease));
        }
    }
}
