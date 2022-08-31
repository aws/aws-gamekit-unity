// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Models;
using AWS.GameKit.Editor.Models.FeatureSettings;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.IdentityAndAuthentication
{
    [Serializable]
    public class IdentityAndAuthenticationSettingsTab : FeatureSettingsTab
    {
        private const string EMAIL_ENABLED_FEATURE_KEY = "is_email_enabled";
        private const string FACEBOOK_ENABLED_FEATURE_KEY = "is_facebook_enabled";
        private const string FACEBOOK_CLIENT_ID_FEATURE_KEY = "facebook_client_id";
        private const string FACEBOOK_SECRET_FEATURE_KEY = "facebook_client_secret";

        public override FeatureType FeatureType => FeatureType.Identity;

        protected override IEnumerable<IFeatureSetting> FeatureSpecificSettings => new List<IFeatureSetting>()
        {
            _isEnabledEmailAndPassword,
            _isEnabledFacebook,
            _facebookAppId
        };

        protected override IEnumerable<SecretSetting> FeatureSecrets => new List<SecretSetting>()
        {
            _facebookAppSecret
        };

        // Feature Settings
        [SerializeField] private FeatureSettingBool _isEnabledEmailAndPassword = new FeatureSettingBool(EMAIL_ENABLED_FEATURE_KEY, defaultValue: true);
        [SerializeField] private FeatureSettingBool _isEnabledFacebook = new FeatureSettingBool(FACEBOOK_ENABLED_FEATURE_KEY, defaultValue: false);
        [SerializeField] private FeatureSettingString _facebookAppId = new FeatureSettingString(FACEBOOK_CLIENT_ID_FEATURE_KEY, defaultValue: string.Empty);
        private SecretSetting _facebookAppSecret = new SecretSetting(FACEBOOK_SECRET_FEATURE_KEY, string.Empty, false);

        protected override void DrawSettings()
        {
            DrawLoginMechanisms();

            EditorGUILayoutElements.SectionSpacer();

            if (IsEnabledAnyIdentityProvider())
            {
                DrawIdentityProviderCredentials();
            }
        }

        private void DrawLoginMechanisms()
        {
            EditorGUILayoutElements.Description(L10n.Tr("Login mechanisms"), indentationLevel: 0);

            // Email & password is always enabled. It cannot be turned off.
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayoutElements.ToggleField(L10n.Tr("Email / password"), _isEnabledEmailAndPassword.CurrentValue);
            }
            
            SerializedProperty isEnabledFacebookProperty = GetFeatureSettingProperty(nameof(_isEnabledFacebook));
            isEnabledFacebookProperty.boolValue = EditorGUILayoutElements.ToggleField(L10n.Tr("Facebook"), isEnabledFacebookProperty.boolValue);
        }

        private bool IsEnabledAnyIdentityProvider()
        {
            return _isEnabledFacebook.CurrentValue;
        }

        private void DrawIdentityProviderCredentials()
        {
            EditorGUILayoutElements.Description(L10n.Tr("Identity provider credentials"), indentationLevel: 0);

            EditorGUILayoutElements.Description(L10n.Tr("To save credentials changes, deploy or update your Identity feature."));

            if (_isEnabledFacebook.CurrentValue)
            {
                EditorGUILayoutElements.Description(L10n.Tr("Facebook"), indentationLevel: 1);

                SerializedProperty facebookAppIdProperty = GetFeatureSettingProperty(nameof(_facebookAppId));
                facebookAppIdProperty.stringValue = EditorGUILayoutElements.TextField(L10n.Tr("App ID"), facebookAppIdProperty.stringValue, indentationLevel: 2);

                if (_facebookAppSecret.IsStoredInCloud)
                {
                    _facebookAppSecret.SecretValue = EditorGUILayoutElements.PasswordField(L10n.Tr("App Secret"), _facebookAppSecret.SecretValue, indentationLevel: 2, L10n.Tr("Secured in AWS Secrets Manager"));
                }
                else
                {
                    _facebookAppSecret.SecretValue = EditorGUILayoutElements.PasswordField(L10n.Tr("App Secret"), _facebookAppSecret.SecretValue, indentationLevel: 2);
                }
            }
        }
    }
}