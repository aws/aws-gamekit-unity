// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Libraries
using System;

namespace AWS.GameKit.Editor.Models
{
    /// <summary>
    /// A generic feature setting.<br/><br/>
    ///
    /// Feature settings are configured by the user through the AWS GameKit Settings window. Each feature has it's own settings to configure.<br/><br/>
    ///
    /// Feature settings are applied during deployment in the form of CloudFormation template parameters.<br/><br/>
    /// </summary>
    /// <typeparam name="T">The data type of this feature setting. This type is used to convert to/from a string when communicating with the AWS GameKit C++ SDK.</typeparam>
    [Serializable]
    public abstract class FeatureSetting<T> : IFeatureSetting
    {
        public string VariableName { get; }

        /// <summary>
        /// The current value of this setting as configured in the UI.
        /// </summary>
        public T CurrentValue;

        /// <summary>
        /// The default value that will be used for this setting if the user does not change it.
        /// </summary>
        public T DefaultValue { get; }

        public string CurrentValueString => ValueToString(CurrentValue);

        public string DefaultValueString => ValueToString(DefaultValue);

        /// <summary>
        /// Create a new instance of FeatureSetting which has it's current value set to the default value.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown when any constructor argument is null.</exception>
        protected FeatureSetting(string variableName, T defaultValue)
        {
            VariableName = variableName ?? throw new ArgumentNullException(nameof(variableName));
            DefaultValue = defaultValue ?? throw new ArgumentNullException(nameof(defaultValue));
            CurrentValue = defaultValue;
        }

        /// <summary>
        /// Set the <c>CurrentValue</c> to a new value specified by a string.
        /// </summary>
        /// <param name="value">The new current value.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <c>value</c> argument is null.</exception>
        public void SetCurrentValueFromString(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            CurrentValue = ValueFromString(value);
        }

        /// <summary>
        /// Convert the provided value to its string representation.
        /// </summary>
        /// <param name="value">The value to convert to a string.</param>
        /// <returns>A string representation of the provided value.</returns>
        protected abstract string ValueToString(T value);

        /// <summary>
        /// Convert the provided string to a type <c>T</c> object.
        /// </summary>
        /// <param name="value">The string to convert to type <c>T</c>.</param>
        /// <returns>A type <c>T</c> object created from the provided string.</returns>
        protected abstract T ValueFromString(string value);
    }
}
