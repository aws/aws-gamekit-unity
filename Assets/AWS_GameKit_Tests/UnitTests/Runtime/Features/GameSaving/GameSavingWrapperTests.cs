// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Features.GameKitGameSaving;
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class GameSavingWrapperTests : GameKitTestBase
    {
        const uint TEST_CALL_STATUS = 42;

        Slot[] _testSlots;

        [SetUp]
        public void SetUp()
        {
            _testSlots = new Slot[]
            {
                new Slot()
                {
                    SlotName = "slot name 0",
                    MetadataLocal = "meta data local 0",
                    MetadataCloud = "meta data cloud 0",
                    LastModifiedCloud = 1,
                    LastModifiedLocal = 2,
                    LastSync = 3,
                    SizeCloud = 4,
                    SizeLocal = 5,
                    SlotSyncStatus = 8
                },
                new Slot()
                {
                    SlotName = "slot name 1",
                    MetadataLocal = "meta data local 1",
                    MetadataCloud = "meta data cloud 1",
                    LastModifiedCloud = 11,
                    LastModifiedLocal = 22,
                    LastSync = 33,
                    SizeCloud = 44,
                    SizeLocal = 55,
                    SlotSyncStatus = 16
                },
                new Slot()
                {
                    SlotName = "slot name 2",
                    MetadataLocal = "meta data local 2",
                    MetadataCloud = "meta data cloud 2",
                    LastModifiedCloud = 111,
                    LastModifiedLocal = 222,
                    LastSync = 333,
                    SizeCloud = 444,
                    SizeLocal = 555,
                    SlotSyncStatus = 32
                }
            };
        }

        [Test]
        public void GameSavingResponseCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            SlotListResult result = new SlotListResult();
            IntPtr pSlots = Marshaller.ArrayToIntPtr(_testSlots);

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameSavingWrapperTarget.GameSavingResponseCallback(dispatchReceiver, pSlots, (uint)_testSlots.Length, true, TEST_CALL_STATUS));
            
            // assert
            Assert.AreEqual(TEST_CALL_STATUS, result.ResultCode);
            Assert.AreEqual(_testSlots.Length, result.CachedSlots.Length);

            for (uint i = 0; i < _testSlots.Length; ++i)
            {
                AssertSlotsAreEqual(_testSlots[i], result.CachedSlots[i]);
            }

            // cleanup
            Marshal.FreeHGlobal(pSlots);
        }

        [Test]
        public void GameSavingSlotActionResponseCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            SlotActionResult result = new SlotActionResult();
            IntPtr pSlots = Marshaller.ArrayToIntPtr(_testSlots);

            IntPtr activeSlotPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_testSlots[0]));
            Marshal.StructureToPtr(_testSlots[0], activeSlotPtr, false);
            
            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameSavingWrapperTarget.GameSavingSlotActionResponseCallback(dispatchReceiver, pSlots, (uint)_testSlots.Length, activeSlotPtr, TEST_CALL_STATUS));
  
            // assert
            Assert.AreEqual(TEST_CALL_STATUS, result.ResultCode);
            Assert.AreEqual(_testSlots.Length, result.CachedSlots.Length);
  
            AssertSlotsAreEqual(_testSlots[0], result.ActionedSlot);
  
            for (uint i = 0; i < _testSlots.Length; ++i)
            {
                AssertSlotsAreEqual(_testSlots[i], result.CachedSlots[i]);
            }
  
            // cleanup
            Marshal.FreeHGlobal(pSlots);
            Marshal.FreeHGlobal(activeSlotPtr);
        }

        [Test]
        public void GameSavingDataResponseCallback_StandardCase_CallsSuccessfully()
        {
            // arrange
            byte[] data = new byte[] { 1, 1, 0, 1, 0, 1, 1 };

            SlotDataResult result = new SlotDataResult();
            IntPtr pSlots = Marshaller.ArrayToIntPtr(_testSlots);

            IntPtr slotPtr = Marshal.AllocHGlobal(Marshal.SizeOf(_testSlots[0]));
            Marshal.StructureToPtr(_testSlots[0], slotPtr, false);

            // act
            Marshaller.Dispatch(result, (IntPtr dispatchReceiver) => GameSavingWrapperTarget.GameSavingDataResponseCallback(dispatchReceiver, pSlots, (uint)_testSlots.Length, slotPtr, data, (uint)data.Length, TEST_CALL_STATUS));

            // assert
            Assert.AreEqual(TEST_CALL_STATUS, result.ResultCode);
            Assert.AreEqual(_testSlots.Length, result.CachedSlots.Length);
            Assert.AreEqual(data.Length, result.DataSize);
            Assert.AreEqual(data, result.Data);

            AssertSlotsAreEqual(_testSlots[0], result.ActionedSlot);

            for (uint i = 0; i < _testSlots.Length; ++i)
            {
                AssertSlotsAreEqual(_testSlots[i], result.CachedSlots[i]);
            }

            // cleanup
            Marshal.FreeHGlobal(pSlots);
            Marshal.FreeHGlobal(slotPtr);
        }

        void AssertSlotsAreEqual (Slot expected, Slot actual)
        {
            Assert.AreEqual(expected.SlotName, actual.SlotName);
            Assert.AreEqual(expected.MetadataLocal, actual.MetadataLocal);
            Assert.AreEqual(expected.MetadataCloud, actual.MetadataCloud);
            Assert.AreEqual(expected.LastModifiedCloud, actual.LastModifiedCloud);
            Assert.AreEqual(expected.LastModifiedLocal, actual.LastModifiedLocal);
            Assert.AreEqual(expected.LastSync, actual.LastSync);
            Assert.AreEqual(expected.SizeCloud, actual.SizeCloud);
            Assert.AreEqual(expected.SizeLocal, actual.SizeLocal);
            Assert.AreEqual(expected.SlotSyncStatus, actual.SlotSyncStatus);
        }
    }

    public class GameSavingWrapperTarget : GameSavingWrapper
    {
        public new static void GameSavingResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, bool complete, uint callStatus) => GameSavingWrapper.GameSavingResponseCallback(
            dispatchReceiver,
            cachedSlots,
            slotCount,
            complete,
            callStatus);

        public new static void GameSavingSlotActionResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, IntPtr activeSlot, uint callStatus) => GameSavingWrapper.GameSavingSlotActionResponseCallback(
            dispatchReceiver,
            cachedSlots,
            slotCount,
            activeSlot,
            callStatus);

        public new static void GameSavingDataResponseCallback(IntPtr dispatchReceiver, IntPtr cachedSlots, uint slotCount, IntPtr slot, byte[] data, uint dataSize, uint callStatus) => GameSavingWrapper.GameSavingDataResponseCallback(
            dispatchReceiver,
            cachedSlots,
            slotCount,
            slot,
            data,
            dataSize,
            callStatus);
    }
}