// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Runtime.Utils
{
    /// <summary>
    /// Class to help with calling exported DLL functions.
    /// </summary>
    public static class DllLoader
    {

        /// <summary>
        /// Tries to call imported DLL method. Logs Dll Exception if unsuccessful.
        /// </summary>
        /// <param name="importedMethod">The imported method to call.</param>
        /// <param name="dllName">Name of the dll the method is in.</param>
        public static void TryDll(Action importedMethod, string dllName)
        {
            try
            {
                importedMethod();
            }
            catch (Exception e)
            {
                LogDllException(e, dllName);
            }
        }

        /// <summary>
        /// Tries to call imported DLL method. Logs Dll Exception if unsuccessful.
        /// </summary>
        /// <param name="importedMethod">The imported method to call.</param>
        /// <param name="dllName">Name of the dll the method is in.</param>
        public static T TryDll<T>(Func<T> importedMethod, string dllName, T returnOnError)
        {
            try
            {
                return importedMethod();
            }
            catch (Exception e)
            {
                LogDllException(e, dllName);

                return returnOnError;
            }
        }

        /// <summary>
        /// Tries to call imported DLL method. Logs Dll Exception if unsuccessful.
        /// </summary>
        /// <param name="importedMethod">The imported method to call.</param>
        /// <param name="dllName">Name of the dll the method is in.</param>
        /// <param name="dispatchReceiver">The object this method is called on.</param> 
        public static void TryDll(object dispatchReceiver, Action<IntPtr> importedMethod, string dllName)
        {
            Marshaller.Dispatch(dispatchReceiver, (IntPtr handle) =>
            {
                try
                {
                    importedMethod(handle);
                }
                catch (Exception e)
                {
                    LogDllException(e, dllName);
                }
            });
        }

        /// <summary>
        /// Tries to call imported DLL method. Logs Dll Exception if unsuccessful.
        /// </summary>
        /// <param name="importedMethod">The imported method to call.</param>
        /// <param name="dllName">Name of the dll the method is in.</param>
        /// <param name="dispatchReceiver">The object this method is called on.</param> 
        public static T TryDll<T>(object dispatchReceiver, Func<IntPtr, T> importedMethod, string dllName, T returnOnError)
        {
            return Marshaller.Dispatch(dispatchReceiver, (IntPtr handle) =>
            {
                try
                {
                    return importedMethod(handle);
                }
                catch (Exception e)
                {
                    LogDllException(e, dllName);

                    return returnOnError;
                }
            });
        }

        private static void LogDllException(Exception e, string dllName)
        {
            if (e is DllNotFoundException || e is EntryPointNotFoundException)
            {
                Logging.LogException($"{dllName} DLL not linked", e);
            }
            else
            {
                Logging.LogException("Unable to make call to DLL imported method, {}, ", e);
            }
        }
    }
}
