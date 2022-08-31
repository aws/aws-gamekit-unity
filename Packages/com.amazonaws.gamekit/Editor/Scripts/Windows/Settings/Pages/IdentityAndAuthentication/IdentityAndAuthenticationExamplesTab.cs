// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// System
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Common.Models;
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.Core;
using AWS.GameKit.Runtime.Features.GameKitIdentity;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.Windows.Settings.Pages.IdentityAndAuthentication
{
    [Serializable]
    public class IdentityAndAuthenticationExamplesTab : FeatureExamplesTab
    {
        public override FeatureType FeatureType => FeatureType.Identity;

        protected override bool RequiresLogin => false;

        // Dependencies
        private IIdentityProvider _identity;

        // Examples
        [SerializeField] private RegisterExampleUI _registerExampleUI;
        [SerializeField] private ResendConfirmationExampleUI _resendConfirmationExampleUI;
        [SerializeField] private ConfirmEmailExampleUI _confirmEmailExampleUI;
        [SerializeField] private LoginExampleUI _loginExampleUI;
        [SerializeField] private GetUserExampleUI _getUserExampleUI;
        [SerializeField] private ForgotPasswordExampleUI _forgotPasswordExampleUI;
        [SerializeField] private ConfirmForgotPasswordExampleUI _confirmForgotPasswordExampleUI;
        [SerializeField] private OpenFacebookLoginExampleUI _openFacebookLoginExampleUI;
        [SerializeField] private LogoutExampleUI _logoutExampleUI;

        public override void Initialize(SettingsDependencyContainer dependencies, SerializedProperty serializedProperty)
        {
            _identity = dependencies.Identity;

            _registerExampleUI.Initialize(CallRegister, serializedProperty.FindPropertyRelative(nameof(_registerExampleUI)));
            _resendConfirmationExampleUI.Initialize(CallResendConfirmation, serializedProperty.FindPropertyRelative(nameof(_resendConfirmationExampleUI)));
            _confirmEmailExampleUI.Initialize(CallConfirmEmail, serializedProperty.FindPropertyRelative(nameof(_confirmEmailExampleUI)));
            _loginExampleUI.Initialize(CallLogin, serializedProperty.FindPropertyRelative(nameof(_loginExampleUI)));
            _getUserExampleUI.Initialize(CallGetUser, serializedProperty.FindPropertyRelative(nameof(_getUserExampleUI)));
            _forgotPasswordExampleUI.Initialize(CallForgotPassword, serializedProperty.FindPropertyRelative(nameof(_forgotPasswordExampleUI)));
            _confirmForgotPasswordExampleUI.Initialize(CallConfirmForgotPassword, serializedProperty.FindPropertyRelative(nameof(_confirmForgotPasswordExampleUI)));
            _openFacebookLoginExampleUI.Initialize(CallOpenFacebookLogin, serializedProperty.FindPropertyRelative(nameof(_openFacebookLoginExampleUI)));
            _logoutExampleUI.Initialize(CallLogout, serializedProperty.FindPropertyRelative(nameof(_logoutExampleUI)));

            _displayLoginWidget = false;

            base.Initialize(dependencies, serializedProperty);
        }

        #region Draw Examples
        protected override void DrawExamples()
        {
            _registerExampleUI.OnGUI();
            _resendConfirmationExampleUI.OnGUI();
            _confirmEmailExampleUI.OnGUI();
            _loginExampleUI.OnGUI();
            _getUserExampleUI.OnGUI();
            _forgotPasswordExampleUI.OnGUI();
            _confirmForgotPasswordExampleUI.OnGUI();
            _openFacebookLoginExampleUI.OnGUI();
            _logoutExampleUI.OnGUI();
        }
        #endregion

        #region Identity API Calls 
        private void CallRegister()
        {
            UserRegistration userRegistration = new UserRegistration
            {
                UserName = _registerExampleUI.UserName,
                Password = _registerExampleUI.Password,
                Email = _registerExampleUI.Email
            };

            Debug.Log($"Calling Identity.Register() with {userRegistration}");

            _identity.Register(userRegistration, (uint resultCode) =>
            {
                Debug.Log($"Identity.Register() completed with result code {resultCode}");
                _registerExampleUI.ResultCode = resultCode;
            });
        }

        private void CallResendConfirmation()
        {
            ResendConfirmationCodeRequest resendConfirmationCodeRequest = new ResendConfirmationCodeRequest
            {
                UserName = _resendConfirmationExampleUI.UserName
            };

            Debug.Log($"Calling Identity.ResendConfirmationCode() with {resendConfirmationCodeRequest}");

            _identity.ResendConfirmationCode(resendConfirmationCodeRequest, (uint resultCode) =>
            {
                Debug.Log($"Identity.ResendConfirmationCode() completed with result code {resultCode}");
                _resendConfirmationExampleUI.ResultCode = resultCode;
            });
        }

        private void CallConfirmEmail()
        {
            ConfirmRegistrationRequest confirmRegistrationRequest = new ConfirmRegistrationRequest
            {
                UserName = _confirmEmailExampleUI.UserName,
                ConfirmationCode = _confirmEmailExampleUI.ConfirmationCode
            };

            Debug.Log($"Calling Identity.ConfirmRegistration() with {confirmRegistrationRequest}");

            _identity.ConfirmRegistration(confirmRegistrationRequest, (uint resultCode) =>
            {
                Debug.Log($"Identity.ConfirmRegistration() completed with result code {resultCode}");
                _confirmEmailExampleUI.ResultCode = resultCode;
            });
        }

        private void CallLogin()
        {
            UserLogin userLogin = new UserLogin
            {
                UserName = _loginExampleUI.UserName,
                Password = _loginExampleUI.Password
            };

            Debug.Log($"Calling Identity.Login() with {userLogin}");

            _identity.Login(userLogin, (uint resultCode) =>
            {
                Debug.Log($"Identity.Login() completed with result code {resultCode}");
                _loginExampleUI.ResultCode = resultCode;

                _userInfo.UserName = userLogin.UserName;
            });
        }

        private void CallGetUser()
        {
            Debug.Log("Calling Identity.GetUser()");

            _identity.GetUser((GetUserResult result) =>
            {
                Debug.Log($"Identity.GetUser() completed with result code {result.ResultCode} and response {result.Response}");
                _getUserExampleUI.ResultCode = result.ResultCode;
                _getUserExampleUI.Response = result.Response;
            });
        }

        private void CallForgotPassword()
        {
            ForgotPasswordRequest forgotPasswordRequest = new ForgotPasswordRequest
            {
                UserName = _forgotPasswordExampleUI.UserName
            };

            Debug.Log($"Calling Identity.ForgotPassword() with {forgotPasswordRequest}");

            _identity.ForgotPassword(forgotPasswordRequest, (uint resultCode) =>
            {
                Debug.Log($"Identity.ForgotPassword() completed with result code {resultCode}");
                _forgotPasswordExampleUI.ResultCode = resultCode;
            });
        }

        private void CallConfirmForgotPassword()
        {
            ConfirmForgotPasswordRequest confirmForgotPasswordRequest = new ConfirmForgotPasswordRequest
            {
                UserName = _confirmForgotPasswordExampleUI.UserName,
                NewPassword = _confirmForgotPasswordExampleUI.NewPassword,
                ConfirmationCode = _confirmForgotPasswordExampleUI.ConfirmationCode
            };

            Debug.Log($"Calling Identity.ConfirmForgotPassword() with {confirmForgotPasswordRequest}");

            _identity.ConfirmForgotPassword(confirmForgotPasswordRequest, (uint resultCode) =>
            {
                Debug.Log($"Identity.ConfirmForgotPassword() completed with result code {resultCode}");
                _confirmForgotPasswordExampleUI.ResultCode = resultCode;
            });
        }

        private void CallOpenFacebookLogin()
        {
            const string FAILED_LOGIN = "FAILED_LOGIN";
            _openFacebookLoginExampleUI.Response.Clear();
            Debug.Log("Calling Identity.GetFederatedLoginUrl() with FederatedIdentityProvider.FACEBOOK");

            _openFacebookLoginExampleUI.Response["status"] = "POLLING FOR LOGIN COMPLETION ...";

            Action<uint> completionCallback = (uint result) =>
            {
                if (result != GameKitErrors.GAMEKIT_SUCCESS)
                {
                    _openFacebookLoginExampleUI.Response["status"] = FAILED_LOGIN;
                    _openFacebookLoginExampleUI.ResultCode = result;
                    return;
                }
                _openFacebookLoginExampleUI.Response["status"] = "LOGIN COMPLETE";
                // No username gets configured when logging in with facebook, put in a filler that will unlock rest of the API examples
                _userInfo.UserName = "facebook_user";

                _identity.GetUser((GetUserResult result) =>
                {
                    Debug.Log($"Identity.GetUser() completed with result code {result.ResultCode} and response {result.Response}");
                    _userInfo.UserId = result.Response.UserId;
                });
            };

            _identity.FederatedLogin(FederatedIdentityProvider.FACEBOOK, (MultiKeyValueStringCallbackResult result) =>
            {
                Debug.Log($"Identity.GetFederatedLoginUrl() completed with result code {result.ResultCode}");
                _openFacebookLoginExampleUI.ResultCode = result.ResultCode;

                if (result.ResultCode == GameKitErrors.GAMEKIT_SUCCESS)
                {
                    for (int i = 0; i < result.ResponseKeys.Length; i++)
                    {
                        _openFacebookLoginExampleUI.Response[result.ResponseKeys[i]] = result.ResponseValues[i];
                    }
                }
                else
                {
                    _openFacebookLoginExampleUI.Response["status"] = FAILED_LOGIN;
                }
            },
            completionCallback);
        }

        private void CallLogout()
        {
            Debug.Log("Calling Identity.Logout()");

            _identity.Logout((uint resultCode) =>
            {
                Debug.Log($"Identity.Logout() completed with result code {resultCode}");
                _logoutExampleUI.ResultCode = resultCode;

                _userInfo.UserName = string.Empty;
                _userInfo.UserId = string.Empty;
            });
        }
        #endregion
    }

    #region Example Classes
    [Serializable]
    public class RegisterExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Register";
        public string UserName;
        [NonSerialized] public string Password = string.Empty;  // Since NonSerialized, must initialize to prevent from being null instead of empty
        public string Email;

        protected override void DrawInput()
        {
            PropertyField(nameof(UserName), "User Name");
            PropertyField(nameof(Email), "Email");
            Password = EditorGUILayoutElements.PasswordField("Password", Password, 0);
        }
    }

    [Serializable]
    public class ResendConfirmationExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Resend Confirmation Code";
        public string UserName;

        protected override void DrawInput()
        {
            PropertyField(nameof(UserName), "User Name");
        }
    }

    [Serializable]
    public class ConfirmEmailExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Confirm Email";
        public string UserName;
        public string ConfirmationCode;

        protected override void DrawInput()
        {
            PropertyField(nameof(UserName), "User Name");
            PropertyField(nameof(ConfirmationCode), "Confirmation Code");
        }
    }

    [Serializable]
    public class LoginExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Login";
        public string UserName;
        [NonSerialized] public string Password = string.Empty;  // Since NonSerialized, must initialize to prevent from being null instead of empty

        protected override void DrawInput()
        {
            PropertyField(nameof(UserName), "User Name");
            Password = EditorGUILayoutElements.PasswordField("Password", Password, 0);
        }
    }

    [Serializable]
    public class GetUserExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Get User";
        protected override bool _shouldDisplayResponse => true;

        public GetUserResponse Response = new GetUserResponse
        {
            UserId = "",
            UserName = "",
            Email = "",
            CreatedAt = "",
            UpdatedAt = "",
            FacebookExternalId = "",
            FacebookRefId = ""
        };

        protected override void DrawOutput()
        {
            EditorGUILayoutElements.TextField("User Name", Response.UserName, isEnabled: false);
            EditorGUILayoutElements.TextField("User Id", Response.UserId, isEnabled: false);
            EditorGUILayoutElements.TextField("Email", Response.Email, isEnabled: false);
            EditorGUILayoutElements.TextField("Created At", Response.CreatedAt, isEnabled: false);
            EditorGUILayoutElements.TextField("Updated At", Response.UpdatedAt, isEnabled: false);
            EditorGUILayoutElements.TextField("Facebook External Id", Response.FacebookExternalId, isEnabled: false);
            EditorGUILayoutElements.TextField("Facebook Ref Id", Response.FacebookRefId, isEnabled: false);
        }
    }

    [Serializable]
    public class OpenFacebookLoginExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Open Facebook Login";
        public readonly IDictionary<string, string> Response = new Dictionary<string, string>();
        protected override bool _shouldDisplayResponse => true;

        protected override void DrawOutput()
        {
            foreach (KeyValuePair<string, string> entry in Response)
            {
                EditorGUILayoutElements.LabelField(entry.Key, entry.Value);
            }
        }
    }

    [Serializable]
    public class LogoutExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Logout";
    }

    [Serializable]
    public class ForgotPasswordExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Forgot Password";
        public string UserName;

        protected override void DrawInput()
        {
            PropertyField(nameof(UserName), "User Name");
        }
    }

    [Serializable]
    public class ConfirmForgotPasswordExampleUI : GameKitExampleUI
    {
        public override string ApiName => "Confirm Forgot Password";
        public string UserName;
        [NonSerialized] public string NewPassword = string.Empty;   // Since NonSerialized, must initialize to prevent from being null instead of empty
        public string ConfirmationCode;

        protected override void DrawInput()
        {
            PropertyField(nameof(UserName), "User Name");
            NewPassword = EditorGUILayoutElements.PasswordField("New Password", NewPassword, 0);
            PropertyField(nameof(ConfirmationCode), "Confirmation Code");
        }
    }
    #endregion
}