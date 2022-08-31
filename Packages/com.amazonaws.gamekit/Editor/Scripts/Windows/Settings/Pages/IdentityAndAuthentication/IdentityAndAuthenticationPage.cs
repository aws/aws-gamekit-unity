// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.IdentityAndAuthentication
{
    [Serializable]
    public class IdentityAndAuthenticationPage : FeaturePage
    {
        public override FeatureType FeatureType => FeatureType.Identity;

        [SerializeField] private IdentityAndAuthenticationSettingsTab _identitySettingsTab;
        [SerializeField] private IdentityAndAuthenticationExamplesTab _identityExamplesTab;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _identitySettingsTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_identitySettingsTab)));
            _identityExamplesTab.Initialize(dependencies, serializedProperty.FindPropertyRelative(nameof(_identityExamplesTab)));

            base.Initialize(GetDefaultTabs(_identitySettingsTab, _identityExamplesTab), dependencies);
        }
    }
}