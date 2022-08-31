// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Libraries
using System;
using System.Globalization;

namespace AWS.GameKit.Editor.Models.FeatureSettings
{
    /// <summary>
    /// A feature setting that is a <c>float</c>.
    /// </summary>
    [Serializable]
    public class FeatureSettingFloat : FeatureSetting<float>
    {
        public FeatureSettingFloat(string variableName, float defaultValue) : base(variableName, defaultValue)
        {
        }

        protected override string ValueToString(float value) => value.ToString(CultureInfo.InvariantCulture);

        protected override float ValueFromString(string value) => float.Parse(value, CultureInfo.InvariantCulture);
    }
}