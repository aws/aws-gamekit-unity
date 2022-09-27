// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Utils;
using AWS.GameKit.Runtime.Features.GameKitIdentity;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// A GUI element which allows a user to log in to or log out from their GameKit Identity feature within the editor.
    /// </summary>
    [Serializable]
    public class UserLoginWidget : IDrawable
    {
        private Action _logoutDelegate;

        private IIdentityProvider _identity;
        private UserInfo _userInfo;
        private LinkWidget _createUserLink;
        private SerializedProperty _serializedProperty;

        [SerializeField] private string _userName;

        private string _password;
        private string _errorMessage;

        private bool _isRequestInProgress;

        public void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty, Action logoutDelegateMethod)
        {
            _identity = dependencies.Identity;
            _userInfo = dependencies.UserInfo;
            _serializedProperty = serializedProperty;
            _logoutDelegate = logoutDelegateMethod;

            _createUserLink = new LinkWidget(L10n.Tr("Register a new player in the Identity testing tab."), OpenIdentityTestingTab, new LinkWidget.Options { Alignment = LinkWidget.Alignment.Left, ShouldDrawExternalIcon = false });
        }

        public void OnGUI()
        {
            if (_userInfo.IsLoggedIn)
            {
                DrawLogout();
            }
            else
            {
                DrawLogin();
            }
        }

        private void DrawLogout()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayoutElements.Description(L10n.Tr("Logged in as ") + $"<b>{_userInfo.UserName}</b>.", 0, TextAnchor.LowerLeft);

                GUILayout.FlexibleSpace();

                if (EditorGUILayoutElements.Button(L10n.Tr("Log out"), isEnabled: !_isRequestInProgress))
                {
                    CallLogout();
                }
            }
        }

        private void DrawLogin()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleContainer))
            {
                EditorGUILayoutElements.Description(L10n.Tr("<b>Log in as a player:</b>"), 0);

                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleFoldoutContainer))
                {
                    EditorGUILayoutElements.PropertyField("User Name", _serializedProperty.FindPropertyRelative(nameof(_userName)), 0);
                    _password = EditorGUILayoutElements.PasswordField("Password", _password, 0);

                    GUILayout.Space(2f);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayoutElements.EmptyPrefixLabel();

                        string buttonText = _isRequestInProgress ? L10n.Tr("Loading...") : L10n.Tr("Log in");

                        if (EditorGUILayoutElements.Button(buttonText, isEnabled: !_isRequestInProgress))
                        {
                            CallLogin();
                        }

                        _createUserLink.OnGUI();
                    }

                    GUILayout.Space(2f);

                    DrawErrorText();
                }
            }
        }

        private void DrawErrorText()
        {
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayoutElements.EmptyPrefixLabel();

                    EditorGUILayoutElements.ErrorText(_errorMessage, 0);
                }
            }
        }

        private void OpenIdentityTestingTab()
        {
            SettingsWindow.OpenPageToTab(PageType.IdentityAndAuthenticationPage, FeaturePage.TestingTabName);
        }

        private void CallLogin()
        {
            UserLogin userLogin = new UserLogin
            {
                UserName = _userName,
                Password = _password
            };

            _errorMessage = string.Empty;
            _isRequestInProgress = true;

            _identity.Login(userLogin, (uint resultCode) =>
            {
                _isRequestInProgress = false;

                if (resultCode != GameKitErrors.GAMEKIT_SUCCESS)
                {
                    _errorMessage = $"Failed to log in - {GameKitErrorConverter.GetErrorName(resultCode)}";
                    Debug.LogError(_errorMessage);
                }
                else
                {
                    _userInfo.UserName = _userName;
                    _identity.GetUser((GetUserResult result) =>
                    {
                        _userInfo.UserId = result.Response.UserId;

                        if (result.ResultCode != GameKitErrors.GAMEKIT_SUCCESS)
                        {
                            Debug.LogError($"Identity.GetUser() completed with result code {result.ResultCode} and response {result.Response}. Calls made that require the logged in user's Id will fail.");
                            return;
                        }

                        Debug.Log($"Identity.GetUser() completed successfully with the following response: {result.Response}");
                    });
                }
            });
        }

        private void CallLogout()
        {
            _isRequestInProgress = true;
            _identity.Logout((uint resultCode) =>
            {
                _isRequestInProgress = false;
                if (resultCode != GameKitErrors.GAMEKIT_SUCCESS)
                {
                    Debug.Log($"Failed to log out; removing session anyways - {GameKitErrorConverter.GetErrorName(resultCode)}");
                }

                _userName = string.Empty;
                _password = string.Empty;

                _userInfo.UserName = string.Empty;
                _userInfo.UserId = string.Empty;

                // Used to clean up a feature after logout
                _logoutDelegate.Invoke();
            });
        }
    }
}
