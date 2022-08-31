// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Utils;
using AWS.GameKit.Runtime.Utils;


namespace AWS.GameKit.Editor.Windows.Settings.Pages.Log
{
    [Serializable]
    public class LogPage : Page
    {
        private const string COUNT_OVERFLOW_TEXT = "999+";
        private const uint COUNT_OVERFLOW_LIMIT = 1000;
        private const int MINIMUM_LOG_QUEUE_ENTRIES = 0;
        private const int MAXIMUM_LOG_QUEUE_ENTRIES = 10000;

        public override string DisplayName => L10n.Tr("Log");

        private SerializedProperty _serializedProperty;

        [SerializeField] private Vector2 _scrollPositionMain;
        [SerializeField] private Vector2 _scrollPositionLogs;

        // Global log settings
        [SerializeField] private int _verbosityLevel = (int)(Logging.MinimumUnityLoggingLevel - 1);
        [SerializeField] private int _maximumLogEntries = Logging.MaxLogQueueSize;

        // Toggles for log display
        [SerializeField] private bool _isWordWrapEnabled = false;
        [SerializeField] private bool _shouldFilterInfoLogs = false;
        [SerializeField] private bool _shouldFilterWarnLogs = false;
        [SerializeField] private bool _shouldFilterErrorLogs = false;

        public void Initialize(SerializedProperty serializedProperty)
        {
            _serializedProperty = serializedProperty;
        }

        protected override void DrawContent()
        {
            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPositionMain))
            {
                _scrollPositionMain = scrollView.scrollPosition;

                DrawGlobalSettings();

                using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.LogPage.LogSection))
                {
                    bool shouldCopyToClipboard = DrawLogBoxOptions();

                    EditorGUILayoutElements.HorizontalDivider();

                    DrawLogBox(shouldCopyToClipboard);
                }
            }
        }

        private void DrawGlobalSettings()
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.LogPage.GlobalSettingsSection))
            {
                EditorGUILayoutElements.SectionHeader(L10n.Tr("Global log settings"));

                EditorGUILayout.Space(0);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayoutElements.PrefixLabel(L10n.Tr(L10n.Tr("Console logging level")));

                    string[] options = { L10n.Tr("Verbose"), L10n.Tr("Info"), L10n.Tr("Warning"), L10n.Tr("Error"), L10n.Tr("Exception") };
                    _verbosityLevel = GUILayout.SelectionGrid(_verbosityLevel, options, options.Length, SettingsGUIStyles.LogPage.LoggingLevel, GUILayout.ExpandWidth(false), GUILayout.MinWidth(0));
                    Logging.MinimumUnityLoggingLevel = (Logging.Level)_verbosityLevel + 1;
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayoutElements.SectionSpacer(-17.5f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayoutElements.PrefixLabel(L10n.Tr("Maximum log entries"));
                    _maximumLogEntries = EditorGUILayoutElements.IntSlider(string.Empty, _maximumLogEntries, MINIMUM_LOG_QUEUE_ENTRIES, MAXIMUM_LOG_QUEUE_ENTRIES);
                    Logging.MaxLogQueueSize = _maximumLogEntries;
                }
            }
        }

        private bool DrawLogBoxOptions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(L10n.Tr("Clear"), SettingsGUIStyles.LogPage.ButtonLeft))
                {
                    Logging.ClearLogQueue();
                }

                bool shouldCopyToClipboard = GUILayout.Button(L10n.Tr("Copy"), SettingsGUIStyles.LogPage.ButtonLeft);

                _isWordWrapEnabled = EditorGUILayoutElements.ToggleLeft(L10n.Tr("Toggle word wrap"), _isWordWrapEnabled);

                GUILayout.FlexibleSpace();

                _shouldFilterInfoLogs = GUILayout.Toggle(_shouldFilterInfoLogs, CreateLogFilterToggleContent(Logging.InfoLogCount, SettingsGUIStyles.Icons.InfoIcon), SettingsGUIStyles.LogPage.ButtonRight);
                _shouldFilterWarnLogs = GUILayout.Toggle(_shouldFilterWarnLogs, CreateLogFilterToggleContent(Logging.WarningLogCount, SettingsGUIStyles.Icons.WarnIcon), SettingsGUIStyles.LogPage.ButtonRight);
                _shouldFilterErrorLogs = GUILayout.Toggle(_shouldFilterErrorLogs, CreateLogFilterToggleContent(Logging.ErrorLogCount, SettingsGUIStyles.Icons.ErrorIcon), SettingsGUIStyles.LogPage.ButtonRight);

                return shouldCopyToClipboard;
            }
        }

        private void DrawLogBox(bool shouldCopyToClipboard)
        {
            using (new EditorGUILayout.VerticalScope(SettingsGUIStyles.LogPage.LogBox))
            {
                using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(_scrollPositionLogs))
                {
                    _scrollPositionLogs = scrollView.scrollPosition;

                    string displayedLogs = string.Empty;

                    Texture entryIcon;

                    bool useDarkLine = true;
                    foreach (string entry in Logging.LogQueue)
                    {
                        if (entry.Contains(Logging.Level.ERROR.ToString()) || entry.Contains(Logging.Level.EXCEPTION.ToString()))
                        {
                            if (_shouldFilterErrorLogs)
                            {
                                continue;
                            }
                            else
                            {
                                entryIcon = SettingsGUIStyles.Icons.ErrorIcon;
                            }
                        }
                        else if (entry.Contains(Logging.Level.WARNING.ToString()))
                        {
                            if (_shouldFilterWarnLogs)
                            {
                                continue;
                            }
                            else
                            {
                                entryIcon = SettingsGUIStyles.Icons.WarnIcon;
                            }
                        }
                        else
                        {
                            if (_shouldFilterInfoLogs)
                            {
                                continue;
                            }
                            else
                            {
                                entryIcon = SettingsGUIStyles.Icons.InfoIcon;
                            }
                        }

                        GUIStyle style = new GUIStyle(SettingsGUIStyles.LogPage.LogEntry)
                        {
                            wordWrap = _isWordWrapEnabled,
                            normal = new GUIStyleState()
                            {
                                textColor = SettingsGUIStyles.LogPage.LogInfoTextColor.Get(),
                                background = useDarkLine ? SettingsGUIStyles.LogPage.DarkBackground : SettingsGUIStyles.LogPage.LightBackground,
                            }
                        };
                        useDarkLine = !useDarkLine;

                        GUIContent content = new GUIContent(entry, entryIcon);
                        Rect entrySize = GUILayoutUtility.GetRect(content, style);

                        EditorGUI.LabelField(entrySize, new GUIContent(entry, entryIcon), style);

                        displayedLogs += entry + "\n";
                    }

                    if (shouldCopyToClipboard)
                    {
                        GUIUtility.systemCopyBuffer = displayedLogs;
                    }
                }
            }
        }

        private GUIContent CreateLogFilterToggleContent(uint logCount, Texture icon)
        {
            if (logCount >= COUNT_OVERFLOW_LIMIT)
            {
                return new GUIContent(COUNT_OVERFLOW_TEXT, icon);
            }
            else
            {
                return new GUIContent(logCount.ToString(), icon);
            }
        }
    }
}
