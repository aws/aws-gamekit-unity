// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Libraries
using System;
using System.Globalization;

namespace AWS.GameKit.Editor.Models.FeatureSettings
{
    /// <summary>
    /// A feature setting that is a <c>bool</c>.
    /// </summary>
    [Serializable]
    public class FeatureSettingBool : FeatureSetting<bool>
    {
        public FeatureSettingBool(string variableName, bool defaultValue) : base(variableName, defaultValue)
        {
        }

        protected override string ValueToString(bool value) => value.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();

        protected override bool ValueFromString(string value) => bool.Parse(value);
    }
}