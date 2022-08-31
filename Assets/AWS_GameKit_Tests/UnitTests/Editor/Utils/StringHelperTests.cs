// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System.Collections.Generic;

// GameKit
using AWS.GameKit.Runtime.UnitTests;
using AWS.GameKit.Editor.Utils;

// Third Party
using NUnit.Framework;

namespace AWS.GameKit.Editor.UnitTests
{
    public class StringHelperTests : GameKitTestBase
    {
        [Test]
        public void RemoveWhitespace_WhenInputHasWhitespace_RemovesAllWhitespace()
        {
            // arrange
            string input = " \t\n this \t\n is \t\n a \t\n string \t\n ";
            string expectedOutput = "thisisastring";

            // act
            string actualOutput = StringHelper.RemoveWhitespace(input);

            // assert
            Assert.AreEqual(expectedOutput, actualOutput);
        }

        [Test]
        public void RemoveWhitespace_WhenInputHasNoWhitespace_ReturnsUnmodifiedString()
        {
            // arrange
            string input = "StringWithNoWhitespace";

            // act
            string output = StringHelper.RemoveWhitespace(input);

            // assert
            Assert.AreEqual(input, output);
        }

        [Test]
        public void MakeCommaSeparatedList_WhenInputListIsEmpty_ReturnsEmptyString()
        {
            // arrange
            List<string> emptyInputList = new List<string>();
            string expectedValue = string.Empty;

            // act
            string actualValue = StringHelper.MakeCommaSeparatedList(emptyInputList);

            // assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void MakeCommaSeparatedList_WhenInputListIsNonEmpty_ReturnsJoinedString()
        {
            // arrange
            string first = "first";
            string second = "second";
            string third = "third";

            List<string> nonEmptyInputList = new List<string>()
            {
                first, second, third
            };

            string expectedValue = $"\"{first}\", \"{second}\", \"{third}\"";

            // act
            string actualValue = StringHelper.MakeCommaSeparatedList(nonEmptyInputList);

            // assert
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void MakeCommaSeparatedList_WhenInputListContainsQuotes_ReturnValueProperlyEscapesTheQuotes()
        {
            // arrange
            string withoutQuotes = "foo";
            string withQuotes = "\"bar\"";

            List<string> nonEmptyInputList = new List<string>()
            {
                withoutQuotes, withQuotes
            };

            string expectedValue = $"\"{withoutQuotes}\", \"{withQuotes}\"";

            // act
            string actualValue = StringHelper.MakeCommaSeparatedList(nonEmptyInputList);

            // assert
            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}