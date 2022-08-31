// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// System
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Runtime.Utils;

namespace AWS.GameKit.Editor.Windows.Settings
{
    [Serializable]
    public abstract class GameKitExampleUI : IDrawable
    {
        public abstract string ApiName { get; }

        /// <summary>
        /// Set true by a feature examples when a response has been received for the first time. When true a response foldout will appear in the UI. Should be used in tandem with the <see cref="DrawOutput"/> method.
        /// </summary>
        public virtual bool ResponseReceived { get; set; } = false;

        /// <summary>
        /// Used to determine the state of the Response dropdown. Should be used in tandem with the <see cref="DrawOutput"/> method.
        /// </summary>
        public bool IsResponseDisplayed = false;

        public uint ResultCode
        {
            get => _resultCode;
            set
            {
                // External callers should be updating the result code after the API has been called;
                // use this opportunity to "complete" our request.
                _resultCode = value;
                _requestInProgress = false;

                ResponseReceived = true;
                IsResponseDisplayed = true;
            }
        }

        /// <summary>
        /// When true, a description of the example will be shown above the input section. 
        /// </summary>
        protected virtual bool _shouldDisplayDescription { get; } = false;

        /// <summary>
        /// When true, a response section will show under the result code. Uses IsResponseDisplayed to determine handling the dropdown.
        /// </summary>
        protected virtual bool _shouldDisplayResponse { get; } = false;

        /// <summary>
        /// When true, a result section will be shown until the Call API button.
        /// </summary>
        protected virtual bool _shouldDisplayResult { get; } = true;

        // Allows us to use property fields for 'free' data bindings with the Undo system
        protected SerializedProperty _serializedProperty;

        private Action _onCallApi;

        // Purposefully non-serialized - we do not want to snapshot request status, as it could lead to a soft lock
        // in case the user closes the editor or recompiles when a request is in progress and it is never completed
        private bool _requestInProgress = false;
        [SerializeField] private bool _isExampleDisplayed = true;
        private uint _resultCode;

        public void Initialize(Action onCallApi, SerializedProperty serializedProperty)
        {
            _onCallApi = onCallApi;
            _serializedProperty = serializedProperty;
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleContainer))
            {
                _isExampleDisplayed = EditorGUILayout.Foldout(_isExampleDisplayed, ApiName, SettingsGUIStyles.Page.FoldoutTitle);
                if (_isExampleDisplayed)
                {
                    using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.FeatureExamplesTab.ExampleFoldoutContainer))
                    {
                        if (_shouldDisplayDescription)
                        {
                            DrawDescription();
                            EditorGUILayout.Space(5);
                        }

                        using (new EditorGUI.DisabledScope(_requestInProgress))
                        {
                            DrawInput();

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayoutElements.PrefixLabel("Action", 0);

                                if (GUILayout.Button(ApiName, SettingsGUIStyles.Buttons.CallExampleAPIButton,
                                    GUILayout.Width(SettingsGUIStyles.Buttons.CallExampleAPIButton
                                        .CalcSize(new GUIContent(ApiName)).x)))
                                {
                                    // Mark request as "in progress"
                                    _resultCode = 0;
                                    _requestInProgress = true;

                                    // Hide the results from the last call
                                    ResponseReceived = false;
                                    IsResponseDisplayed = false;

                                    _onCallApi();
                                }
                            }
                        }

                        if (_shouldDisplayResult)
                        {
                            using (new EditorGUI.DisabledGroupScope(true))
                            {
                                EditorGUILayoutElements.LabelField(L10n.Tr("Result Code"), _requestInProgress
                                    ? L10n.Tr("In Progress...")
                                    : ResponseReceived ? GameKitErrorConverter.GetErrorName(_resultCode) : string.Empty, 0);
                            }
                        }

                        if (_shouldDisplayResponse)
                        {
                            if (ResponseReceived)
                            {
                                IsResponseDisplayed = EditorGUILayout.Foldout(IsResponseDisplayed, L10n.Tr("Response"), SettingsGUIStyles.Page.FoldoutTitle);

                                if (IsResponseDisplayed)
                                {
                                    using (new EditorGUI.DisabledScope(false))
                                    {
                                        using (new EditorGUILayout.VerticalScope())
                                        {
                                            DrawOutput();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                using (new EditorGUI.DisabledScope(true))
                                {
                                    EditorGUILayoutElements.LabelField(L10n.Tr("Response"), string.Empty, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void DrawDescription() { }

        protected virtual void DrawInput() { }

        protected virtual void DrawOutput() { }

        protected void PropertyField(string propertyPath, string label)
        {
            EditorGUILayoutElements.PropertyField(label, _serializedProperty.FindPropertyRelative(propertyPath), 0);
        }

        protected void PropertyField(string propertyPath)
        {
            EditorGUILayout.PropertyField(_serializedProperty.FindPropertyRelative(propertyPath), GUIContent.none);
        }
    }
}
