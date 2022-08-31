// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Features.GameKitGameSaving
{
    public static class GameSavingConstants
    {
        public const uint S3_PRE_SIGNED_URL_DEFAULT_TIME_TO_LIVE_SECONDS = 120;
        public const uint GET_ALL_SLOT_SYNC_STATUSES_DEFAULT_PAGE_SIZE = 0;
        public const string SAVE_INFO_FILE_EXTENSION = ".SaveInfo.json";
    }

    public enum SlotSyncStatus : uint
    {
        UNKNOWN = 0,
        SYNCED = 1,
        SHOULD_DOWNLOAD_CLOUD = 2,
        SHOULD_UPLOAD_LOCAL = 3,
        IN_CONFLICT = 4
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncGameSavingResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, bool complete, uint callStatus);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncGameSavingSlotActionResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, IntPtr activeSlot, uint callStatus);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void FuncGameSavingDataResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, IntPtr slot, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] byte[] data, uint dataSize, uint callStatus);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool FuncFileWriteCallback(IntPtr dispatchReceiver, string filePath, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] byte[] data, uint size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool FuncFileReadCallback(IntPtr dispatchReceiver, string filePath, IntPtr data, uint size);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate uint FuncFileGetSizeCallback(IntPtr dispatchReceiver, string filePath);

    [StructLayout(LayoutKind.Sequential)]
    public struct FileActions
    {
        public FuncFileWriteCallback FileWriteCallback;
        public FuncFileReadCallback FileReadCallback;
        public FuncFileGetSizeCallback FileSizeCallback;
        public IntPtr FileWriteDispatchReceiver;
        public IntPtr FileReadDispatchReceiver;
        public IntPtr FileSizeDispatchReceiver;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GameSavingModel
    {
        public string SlotName;
        public string Metadata;
        public long EpochTime;

        [MarshalAs(UnmanagedType.U1)]
        public bool OverrideSync;

        public IntPtr Data;
        public uint DataSize;
        public string LocalSlotInformationFilePath;
        public uint UrlTimeToLive;

        [MarshalAs(UnmanagedType.U1)]
        public bool ConsistentRead;
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public class Slot
    {
        public string SlotName;
        public string MetadataLocal = string.Empty;
        public string MetadataCloud = string.Empty;
        public long SizeLocal = 0;
        public long SizeCloud = 0;
        public long LastModifiedLocal = 0;
        public long LastModifiedCloud = 0;
        public long LastSync = 0;
        public byte SlotSyncStatus = (byte)GameKitGameSaving.SlotSyncStatus.UNKNOWN;
    }

    public class SlotListResult
    {
        public uint ResultCode;
        public Slot[] CachedSlots = new Slot[0];
    }

    public class SlotActionResult
    {
        public uint ResultCode;
        public Slot[] CachedSlots = new Slot[0];
        public Slot ActionedSlot;
    }

    public class SlotDataResult
    {
        public uint ResultCode;
        public Slot[] CachedSlots = new Slot[0];
        public Slot ActionedSlot;
        public byte[] Data = new byte[0];
        public uint DataSize = 0;
    }

    public class AddLocalSlotsDesc
    {
        public string[] LocalSlotInformationFilePaths = new string[0];

        public override string ToString() => $"AddLocalSlotDesc(LocalSlotInformationFilePaths=[{string.Join(",", LocalSlotInformationFilePaths)}])";
    }

    public class SaveSlotDesc
    {
        public string SlotName;
        public string SaveInfoFilePath;
        public byte[] Data = new byte[0];
        public string Metadata;
        public long EpochTime = 0;
        public bool OverrideSync = false;

        public override string ToString() => $"SaveSlotDesc(SlotName={SlotName}, SaveInfoFilePath={SaveInfoFilePath}, Data=<{Data.Length} bytes>, Metadata={Metadata}, EpochTime={EpochTime}, OverrideSync={OverrideSync})";
    }

    public class LoadSlotDesc
    {
        public string SlotName;
        public string SaveInfoFilePath;
        public byte[] Data = new byte[0];
        public bool OverrideSync = false;

        public override string ToString() => $"LoadSlotDesc(SlotName={SlotName}, SaveInfoFilePath={SaveInfoFilePath}, Data=<{Data.Length} bytes>, OverrideSync={OverrideSync})";
    }
}
