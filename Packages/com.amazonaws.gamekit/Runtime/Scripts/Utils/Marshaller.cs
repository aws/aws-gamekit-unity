// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace AWS.GameKit.Runtime.Utils
{
    /// <summary>
    /// Class to help with marshalling for calling exported C methods (of the GameKit C++ SDK)
    /// </summary>
    public static class Marshaller
    {
        /// <summary>
        /// Call an Action with data referenced by an IntPtr.
        /// </summary>
        /// <param name="obj">The data to work on.</param>
        /// <param name="work">The action the data is called with.</param>
        public static void Dispatch(object obj, Action<IntPtr> work)
        {
            GCHandle gch = GCHandle.Alloc(obj);
            try
            {
                work(GCHandle.ToIntPtr(gch));
            }
            finally
            {
                gch.Free();
            }
        }

        /// <summary>
        /// Call an Action with data referenced by an IntPtr.
        /// </summary>
        /// <param name="obj">The data to work on.</param>
        /// <param name="work">The action the data is called with.</param>
        public static T Dispatch<T>(object obj, Func<IntPtr, T> work)
        {
            GCHandle gch = GCHandle.Alloc(obj);
            try
            {
                return work(GCHandle.ToIntPtr(gch));
            }
            finally
            {
                gch.Free();
            }
        }


        /// <summary>
        /// Get a GCHandle to data in unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="intptr">IntPtr pointing to object in unmanaged memory.</param>
        /// <returns>Marshalled object.</returns>
        public static T GetDispatchObject<T>(IntPtr intptr)
        {
            GCHandle gch = GCHandle.FromIntPtr(intptr);
            return (T)gch.Target;
        }

        /// <summary>
        /// Marshalls array in unmanaged memory to managed memory.
        /// </summary>
        /// <typeparam name="T">Array element type.</typeparam>
        /// <param name="pointerToArray">Pointer to array in unmanaged memory.</param>
        /// <param name="sizeOfArray">Size of the array (in size of type)</param>
        /// <returns>Marshalled array.</returns>
        public static T[] IntPtrToArray<T>(IntPtr pointerToArray, uint sizeOfArray) where T : new()
        {
            T[] array = new T[sizeOfArray];
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

            for (int i = 0; i < sizeOfArray; ++i)
            {
                try
                {
                    array[i] = Marshal.PtrToStructure<T>(pointerToArray + (size * i));
                }
                catch (Exception e)
                {
                    array[i] = new T();
                    Logging.LogException("Unable to construct an array from the provided pointer", e);
                }
            }

            return array;
        }

        /// <summary>
        /// Marshalls array from managed to unmanaged memory.
        /// </summary>
        /// <typeparam name="T">Type of array element.</typeparam>
        /// <param name="array">Array in managed memory.</param>
        /// <returns>IntPtr to array in unmanaged memory.</returns>
        public static IntPtr ArrayToIntPtr<T>(T[] array)
        {
            IntPtr head = IntPtr.Zero;

            if (array.Length > 0)
            {
                head = Marshal.AllocHGlobal(Marshal.SizeOf(array[0]) * array.Length);

                long nextAddr = head.ToInt64();
                for (uint i = 0; i < array.Length; ++i)
                {
                    IntPtr next = new IntPtr(nextAddr);
                    Marshal.StructureToPtr(array[i], next, false);
                    nextAddr += Marshal.SizeOf(typeof(T));
                }
            }

            return head;
        }

        /// <summary>
        /// Marshalls array of strings to array in unmanaged memory.
        /// </summary>
        /// <param name="array">Array of strings in managed memory.</param>
        /// <returns>Array of IntPtr.</returns>
        public static IntPtr[] ArrayOfStringsToArrayOfIntPtr(string[] array)
        {
            IntPtr[] arrayOfPointers = new IntPtr[array.Length];

            for (uint i = 0; i < array.Length; ++i)
            {
                arrayOfPointers[i] = Marshal.StringToHGlobalAnsi(array[i]);
            }

            return arrayOfPointers;
        }

        /// <summary>
        /// Frees an array of IntPtr.
        /// </summary>
        /// <param name="array">The IntPtr array.</param>
        public static void FreeArrayOfIntPtr(IntPtr[] array)
        {
            for (uint i = 0; i < array.Length; ++i)
            {
                Marshal.FreeHGlobal(array[i]);
                array[i] = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Append value to an array of type T
        /// </summary>
        /// <typeparam name="T">Type of array element.</typeparam>
        /// <param name="arrayOfValues">Data array.</param>
        /// <param name="value">Value to be appended.</param>
        public static void AppendToArray<T>(ref T[] arrayOfValues, T value)
        {
            List<T> buffer = new List<T>(arrayOfValues);
            buffer.Add(value);
            arrayOfValues = buffer.ToArray();
        }
    }
}
