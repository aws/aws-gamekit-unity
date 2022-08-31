// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Runtime.Utils
{
    public static class TimeUtils
    {
        /// <summary>
        /// Converts an epoch timestamp into a human readable string.
        /// </summary>
        /// <remarks>
        /// Converts the timestamp into the format "dd/mm/yyyy hh:mm:ss [AM|PM] [+|-]hh:mm".
        /// Uses the platform's local time zone.<br/>
        /// If the provided epoch timestamp is 0, this function assumes it to be an empty value
        /// and returns an empty string.
        /// </remarks>
        /// <param name="epochTime">The epoch time, in millseconds.</param>
        /// <returns>The epoch time as a date string.</returns>
        public static string EpochTimeToString(long epochTime)
        {
            if (epochTime == 0)
            {
                return string.Empty;
            }

            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(epochTime).ToLocalTime();
            return dateTimeOffset.ToString();
        }

        /// <summary>
        /// Convert an ISO-8601 formatted timestamp into a human readable string.
        /// </summary>
        /// <remarks>
        /// Converts the timestamp into the format "dd/mm/yyyy hh:mm:ss [AM|PM] [+|-]hh:mm".
        /// Uses the platform's local time zone.<br/>
        /// If the provided epoch timestamp is 0, this function assumes it to be an empty value
        /// and returns an empty string.
        /// </remarks>
        /// <param name="timestamp">The ISO-8601 formatted timestamp to convert.</param>
        /// <returns>The ISO-8601 timestamp as a human readable string.</returns>
        public static string ISO8601StringToLocalFormattedString(string timestamp)
        {
            if (string.IsNullOrEmpty(timestamp))
            {
                return string.Empty;
            }

            DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(timestamp).ToLocalTime();
            return dateTimeOffset.ToString();
        }
    }
}