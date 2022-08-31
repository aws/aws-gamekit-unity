// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Libraries
using System;
using System.Globalization;

namespace AWS.GameKit.Editor.Models.FeatureSettings
{
    /// <summary>
    /// A feature setting that is an <c>int</c>.
    /// </summary>
    [Serializable]
    public class FeatureSettingInt : FeatureSetting<int>
    {
        public FeatureSettingInt(string variableName, int defaultValue) : base(variableName, defaultValue)
        {
        }

        protected override string ValueToString(int value) => value.ToString(CultureInfo.InvariantCulture);

        protected override int ValueFromString(string value) => int.Parse(value, CultureInfo.InvariantCulture);
    }
}
