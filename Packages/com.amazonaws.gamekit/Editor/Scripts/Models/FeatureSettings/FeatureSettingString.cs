// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

namespace AWS.GameKit.Editor.Models.FeatureSettings
{
    /// <summary>
    /// A feature setting that is a <c>string</c>.
    /// </summary>
    [Serializable]
    public class FeatureSettingString : FeatureSetting<string>
    {
        public FeatureSettingString(string variableName, string defaultValue) : base(variableName, defaultValue)
        {
        }

        protected override string ValueToString(string value) => value;

        protected override string ValueFromString(string value) => value;
    }
}