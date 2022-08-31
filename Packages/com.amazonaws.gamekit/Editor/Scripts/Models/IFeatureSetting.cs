// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

namespace AWS.GameKit.Editor.Models
{
    /// <summary>
    /// A string-based feature setting.<br/><br/>
    ///
    /// This is intended for use with the AWS GameKit C++ SDK, which treats all feature settings as strings.
    /// </summary>
    public interface IFeatureSetting
    {
        /// <summary>
        /// The setting's variable name in the feature's CloudFormation parameters.yml file.<br/><br/>
        ///
        /// Feature settings are marked in parameters.yml files by a placeholder that looks like: <c>{{AWSGAMEKIT::VARS::my_variable}}</c>,
        /// where <c>my_variable</c> is specified by <c>VariableName</c>.<br/><br/>
        ///
        /// <c>VariableName</c> should be used when calling:<br/>
        /// - <c>FeatureResourceManager.SetFeatureVariable(..., varName=MySetting.VariableName, ...)</c><br/>
        /// - <c>FeatureResourceManager.SetFeatureVariableIfUnset(..., varName=MySetting.VariableName, ...)</c><br/>
        /// - As keys in the dictionary response from <c>FeatureResourceManager.GetFeatureVariables()</c>
        /// </summary>
        public string VariableName { get; }

        /// <summary>
        /// The current value of this setting represented as a string.
        /// </summary>
        public string CurrentValueString { get; }

        /// <summary>
        /// The default value of this setting represented as a string.
        /// </summary>
        public string DefaultValueString { get; }

        /// <summary>
        /// Set the current value of this setting to the provided string.
        /// </summary>
        /// <param name="value">The new current value.</param>
        public void SetCurrentValueFromString(string value);
    }
}
