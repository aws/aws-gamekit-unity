// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// GameKit
using AWS.GameKit.Runtime.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Runtime.UnitTests
{
    public class MarshallerTests : GameKitTestBase
    {
        public struct TestStruct
        {
            public string Result;
        }

        const string TEST_MARSHAL_VALUE = "test marshal value";

        [Test]
        public void IntPtrToArray_StandardCase_CallsSuccessfully()
        {
            // arrange
            byte[] testArray = new byte[] { 0, 1, 2, 3, 4, 5 };
            IntPtr pointerToArray = Marshal.AllocHGlobal(testArray.Length);
            Marshal.Copy(testArray, 0, pointerToArray, testArray.Length);

            // act
            byte[] result = Marshaller.IntPtrToArray<byte>(pointerToArray, (uint)testArray.Length);

            // assert
            Assert.AreEqual(testArray, result);

            // cleanup
            Marshal.FreeHGlobal(pointerToArray);
        }

        [Test]
        public void ArrayToIntPtr_StandardCase_CallsSuccessfully()
        {
            // arrange
            TestStruct[] testStructs = new TestStruct[]
            {
                new TestStruct(),
                new TestStruct(),
                new TestStruct()
            };

            testStructs[0].Result = "result 0";
            testStructs[1].Result = "result 1";
            testStructs[2].Result = "result 2";

            // act
            IntPtr result = Marshaller.ArrayToIntPtr(testStructs);

            // assert
            Assert.AreNotEqual(IntPtr.Zero, result);

            // cleanup
            Marshal.FreeHGlobal(result);
        }

        [Test]
        public void ArrayToIntPtr_WhenArrayHasZeroElements_ZeroPointerIsReturned()
        {
            // arrange
            TestStruct[] testStructs = new TestStruct[0];

            // act
            IntPtr result = Marshaller.ArrayToIntPtr(testStructs);

            // assert
            Assert.AreEqual(IntPtr.Zero, result);
        }

        [Test]
        public void ArrayOfStringsToArrayOfPointers_StandardCase_CallsSuccessfully()
        {
            // arrange
            string[] testArray = new string[] { "1", "22", "333", "4444" };

            // act
            IntPtr[] result = Marshaller.ArrayOfStringsToArrayOfIntPtr(testArray);

            // assert
            Assert.AreEqual(testArray.Length, result.Length, "Expected that the size of the IntPtr array will be equal to the size of the string array.");
            for (uint i = 0; i < result.Length; ++i)
            {
                Assert.NotNull(result[i], "Expected that the IntPtr is allocated");
            }

            // cleanup
            for (uint i = 0; i < result.Length; ++i)
            {
                Marshal.FreeHGlobal(result[i]);
            }
        }

        [Test]
        public void FreeArrayOfPointers_StandardCase_CallsSuccessfully()
        {
            // arrange
            string[] originalArray = new string[] { "1", "22", "333", "4444" };
            IntPtr[] testArray = Marshaller.ArrayOfStringsToArrayOfIntPtr(originalArray);

            // act
            Marshaller.FreeArrayOfIntPtr(testArray);

            // assert
            for (uint i = 0; i < testArray.Length; ++i)
            {
                Assert.AreEqual(IntPtr.Zero, testArray[i]);
            }
        }
    }
}
