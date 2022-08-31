// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit 
using AWS.GameKit.Editor.FileStructure;

namespace AWS.GameKit.Editor.GUILayoutExtensions
{
    /// <summary>
    /// A GUI element which displays a link followed by an external icon, optionally prepended with a label.
    /// </summary>
    public class LinkWidget : IDrawable
    {
        // Data
        private readonly string _prependedText;
        private readonly float _spaceAfterPrependedText;
        private readonly string _linkLabel;
        private readonly Options _options;
        private readonly Action _onClick;

        // Styles
        private GUIStyle _linkTextStyle;
        private GUIStyle _prependedTextStyle;
        private GUIStyle _iconStyle;
        private GUIStyle _iconStyleWithoutOffset;

        /// <summary>
        /// Create a new LinkWidget.
        /// </summary>
        /// <param name="linkLabel">The link text to display.</param>
        /// <param name="onClick">The action to execute when clicking on the link.</param>
        /// <param name="options">Styling options to apply.</param>
        public LinkWidget(string linkLabel, Action onClick, Options options = null)
        {
            _linkLabel = linkLabel;
            _onClick = onClick;
            _options = options ?? new Options();
        }

        /// <summary>
        /// Create a new LinkWidget with a label placed before it.
        /// </summary>
        /// <param name="prependedText">The text to place directly before the <c>linkLabel</c>.</param>
        /// <param name="linkLabel">The link text to display.</param>
        /// <param name="onClick">The action to execute when clicking on the link.</param>
        /// <param name="options">Styling options to apply.</param>
        /// <param name="spaceAfterPrependedText">The amount of space to place between the <c>prependedText</c> and <c>linkLabel</c>.</param>
        public LinkWidget(string prependedText, string linkLabel, Action onClick, Options options = null, float spaceAfterPrependedText = 0f)
            : this(linkLabel, onClick, options)
        {
            _prependedText = prependedText;
            _spaceAfterPrependedText = spaceAfterPrependedText;
        }

        /// <summary>
        /// Create a new LinkWidget.
        /// </summary>
        /// <param name="linkLabel">The text to display in place of the URL.</param>
        /// <param name="url">The link that clicking the label text will route to.</param>
        /// <param name="options">Styling options to apply.</param>
        public LinkWidget(string linkLabel, string url, Options options = null)
            : this(linkLabel, () => Application.OpenURL(url), options)
        {
        }

        /// <summary>
        /// Create a new LinkWidget with a label placed before it.
        /// </summary>
        /// <param name="prependedText">The text to place directly before the <c>linkLabel</c>.</param>
        /// <param name="linkLabel">The text to display in place of the URL.</param>
        /// <param name="url">The link that clicking the label text will route to.</param>
        /// <param name="options">Styling options to apply.</param>
        /// <param name="spaceAfterPrependedText">The amount of space to place between the <c>prependedText</c> and <c>linkLabel</c>.</param>
        public LinkWidget(string prependedText, string linkLabel, string url, Options options = null, float spaceAfterPrependedText = 0f)
            : this(linkLabel, url, options)
        {
            _prependedText = prependedText;
            _spaceAfterPrependedText = spaceAfterPrependedText;
        }

        /// <summary>
        /// Styling options for the LinkWidget class.
        /// </summary>
        public class Options
        {
            /// <summary>
            /// A GUIStyle to apply to the horizontal layout section which contains the entire link widget (the prepended text, link label, and icon).
            /// </summary>
            public GUIStyle OverallStyle = GUIStyle.none;

            /// <summary>
            /// Whether to enable/disable word wrapping for the <c>linkLabel</c> parameter's text.
            /// </summary>
            public bool ShouldWordWrapLinkLabel = true;

            /// <summary>
            /// Number of pixels to shift the entire link widget (prepended text, link label, and icon).
            /// </summary>
            public Vector2 ContentOffset = Vector2.zero;

            /// <summary>
            /// Alignment of the entire link widget (prepended text, link label, and icon).
            /// </summary>
            public Alignment Alignment = Alignment.Left;

            /// <summary>
            /// The tooltip to show when the mouse is hovering over the link. By default no tooltip is shown.
            /// </summary>
            public string Tooltip = string.Empty;

            /// <summary>
            /// Whether to draw the external icon after the link label.
            /// </summary>
            public bool ShouldDrawExternalIcon = true;
        }

        /// <summary>
        /// The alignment of the entire link widget (prepended text, link label, and icon).
        /// </summary>
        public enum Alignment
        {
            /// <summary>
            /// The LinkWidget is aligned to the left, by placing a GUILayout.FlexibleSpace() at the end of it's horizontal layout group.
            /// </summary>
            Left,

            /// <summary>
            /// The LinkWidget is aligned to the right by placing a GUILayout.FlexibleSpace() at the beginning of it's horizontal layout group.
            /// </summary>
            Right,

            /// <summary>
            /// The LinkWidget is placed in the default manner for a horizontal layout group.
            /// It does not use a GUILayout.FlexibleSpace() to position itself.
            /// </summary>
            None,
        }

        /// <summary>
        /// Draw the LinkWidget.
        /// </summary>
        public void OnGUI()
        {
            SetGUIStyles();

            using (new EditorGUILayout.HorizontalScope(_options.OverallStyle))
            {
                if (_options.Alignment == Alignment.Right)
                {
                    GUILayout.FlexibleSpace();
                }

                DrawPrependedText();
                DrawLinkWithExternalIcon();

                if (_options.Alignment == Alignment.Left)
                {
                    GUILayout.FlexibleSpace();
                }
            }
        }

        // Avoid a null-reference exception by creating the GUIStyles during OnGUI instead of the constructor.
        // The exception happens because EditorStyles (ex: EditorStyles.label) are null until the first frame of the GUI.
        private void SetGUIStyles()
        {
            _linkTextStyle = new GUIStyle(LinkWidgetStyles.LinkLabelStyle)
            {
                wordWrap = _options.ShouldWordWrapLinkLabel
            };

            Vector2 totalContentOffset = _options.ContentOffset + LinkWidgetStyles.BaselineContentOffset;

            _linkTextStyle = CommonGUIStyles.AddContentOffset(_linkTextStyle, totalContentOffset);
            _linkTextStyle.fontSize = _options.OverallStyle.fontSize;
            _prependedTextStyle = CommonGUIStyles.AddContentOffset(LinkWidgetStyles.PrependedTextStyle, totalContentOffset);

            _iconStyleWithoutOffset = _linkTextStyle.fontSize == 10
                ? LinkWidgetStyles.IconStyleSmall
                : LinkWidgetStyles.IconStyleNormal;
            _iconStyle = CommonGUIStyles.AddContentOffset(_iconStyleWithoutOffset, totalContentOffset);
        }

        private void DrawPrependedText()
        {
            if (string.IsNullOrEmpty(_prependedText))
            {
                return;
            }

            EditorGUILayout.LabelField(_prependedText, _prependedTextStyle);
            if (_spaceAfterPrependedText != 0)
            {
                // We only create this Space() element if non-zero, because Space(0) actually creates a non zero-width space.
                EditorGUILayout.Space(_spaceAfterPrependedText);
            }
        }

        /// <summary>
        /// Draws a button, that is styled to look like a hyperlink, followed by an external link Icon.
        /// </summary>
        private void DrawLinkWithExternalIcon()
        {
            GUILayout.Label(_linkLabel, _linkTextStyle);

            Rect buttonRect = GUILayoutUtility.GetLastRect();
            buttonRect.x += _options.ContentOffset.x;
            buttonRect.y += _options.ContentOffset.y;
            buttonRect.width = _linkTextStyle.CalcSize(new GUIContent(_linkLabel)).x;

            if (_options.ShouldDrawExternalIcon)
            {
                // Draw icon
                GUILayout.Box(EditorResources.Textures.SettingsWindow.ExternalLinkIcon.Get(), _iconStyle);

                buttonRect.width += GUILayoutUtility.GetLastRect().width + _iconStyleWithoutOffset.fixedWidth / 3f;
            }

            // Create a button with no text but spans the length of both the text and the icon (if drawn)
            GUIContent buttonContent = new GUIContent(string.Empty, _options.Tooltip);
            if (GUI.Button(buttonRect, buttonContent, GUIStyle.none))
            {
                _onClick();
            }

            // Change mouse cursor to a "pointer" when hovering over the button
            EditorGUIUtility.AddCursorRect(buttonRect, MouseCursor.Link);
        }

        /// <summary>
        /// Subclass containing all specific styles for the LinkWidget.
        /// </summary>
        private static class LinkWidgetStyles
        {
            // The link widget is 2 pixels higher than other text on it's same line.
            // This offset brings it back down to where it should be.
            public static readonly Vector2 BaselineContentOffset = new Vector2(0, 2);

            private static readonly GUIStyleState LinkLabelStyleState = new GUIStyleState()
            {
                background = EditorStyles.label.normal.background,
                scaledBackgrounds = EditorStyles.label.normal.scaledBackgrounds,
                textColor = EditorStyles.linkLabel.normal.textColor
            };

            public static readonly GUIStyle LinkLabelStyle = new GUIStyle(EditorStyles.label)
            {
                active = EditorStyles.linkLabel.active,
                hover = EditorStyles.linkLabel.hover,
                normal = LinkLabelStyleState,
                stretchWidth = false,
            };

            public static readonly GUIStyle PrependedTextStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            public static readonly GUIStyle IconStyleNormal = new GUIStyle()
            {
                contentOffset = new Vector2(2, 3),
                fixedWidth = 14,
                fixedHeight = 14
            };

            public static readonly GUIStyle IconStyleSmall = new GUIStyle()
            {
                contentOffset = new Vector2(0, 2),
                fixedWidth = 10,
                fixedHeight = 10
            };
        }
    }
}