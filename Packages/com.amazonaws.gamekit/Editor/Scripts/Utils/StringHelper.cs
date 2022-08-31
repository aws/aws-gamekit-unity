// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;
using System.Linq;

namespace AWS.GameKit.Editor.Utils
{
    /// <summary>
    /// Helper methods for working with strings.
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// Remove all whitespace from a string.
        /// </summary>
        /// <param name="text">The string to remove whitespace from.</param>
        /// <returns>A copy of the input string with all the whitespace removed.</returns>
        public static string RemoveWhitespace(string text)
        {
            return string.Concat(text.Where(character => !Char.IsWhiteSpace(character)));
        }

        /// <summary>
        /// Join a collection of strings into a single string, which is a comma separated list of quoted elements. Example: <c>"\"First\", \"Second\", \"Third\""</c>
        /// </summary>
        /// <param name="values">The collection of strings to join.</param>
        /// <returns>The joined string, or <see cref="string.Empty"/> if <paramref name="values"/> was empty.</returns>
        public static string MakeCommaSeparatedList(IEnumerable<string> values)
        {
            return string.Join(", ", values.Select(value => $"\"{value}\""));
        }
    }
}