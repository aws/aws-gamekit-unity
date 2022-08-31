// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;
using UnityEngine;

namespace AWS.GameKit.Editor.GUILayoutExtensions
{
    /// <summary>
    /// Common GUIStyles used throughout AWS GameKit.
    /// </summary>
    public static class CommonGUIStyles
    {
        public const int PIXELS_PER_INDENTATION_LEVEL = 20;
        public const int DEFAULT_PIXELS_LEFT_MARGIN = 4;

        public const float INLINE_ICON_SIZE = 12;

        /// <summary>
        /// Create a copy of the provided GUIStyle which has the requested level of indentation.
        /// </summary>
        /// <param name="style">The GUIStyle to copy.</param>
        /// <param name="indentationLevel">The level of indentation to set.</param>
        /// <param name="pixelsPerIndentationLevel">The number of pixels in each indentation level.</param>
        /// <returns>A copy of the provided GUIStyle.</returns>
        public static GUIStyle SetIndentationLevel(GUIStyle style, int indentationLevel, int pixelsPerIndentationLevel = PIXELS_PER_INDENTATION_LEVEL)
        {
            // When provided with a left margin of 0, the following element will be misaligned.
            // Use the default pixel value instead, which will ensure alignment of all fields.
            int leftMargin = indentationLevel == 0
                ? DEFAULT_PIXELS_LEFT_MARGIN
                : indentationLevel * pixelsPerIndentationLevel;

            return new GUIStyle(style)
            {
                margin = new RectOffset(leftMargin, style.margin.right, style.margin.top, style.margin.bottom)
            };
        }

        /// <summary>
        /// Create a copy of the provided GUIStyle which has the requested content offset added to the existing content offset.
        /// </summary>
        /// <param name="originalStyle">The GUIStyle to copy.</param>
        /// <param name="addedContentOffset">The content offset to add to the existing content offset.</param>
        /// <returns>A copy of the provided GUIStyle.</returns>
        public static GUIStyle AddContentOffset(GUIStyle originalStyle, Vector2 addedContentOffset)
        {
            return new GUIStyle(originalStyle)
            {
                contentOffset = new Vector2(
                    originalStyle.contentOffset.x + addedContentOffset.x,
                    originalStyle.contentOffset.y + addedContentOffset.y
                )
            };
        }

        public static readonly GUIStyle SectionHeader = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };

        public static readonly GUIStyle SectionHeaderWithDescription = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            richText = true
        };

        public static readonly GUIStyle Description = new GUIStyle(EditorStyles.label)
        {
            wordWrap = true,
            richText = true
        };

        public static readonly GUIStyle PlaceholderLabel = new GUIStyle(EditorStyles.label)
        {
            padding = new RectOffset(4, 0, 0, 0)
        };

        public static readonly GUIStyle PlaceholderTextArea = new GUIStyle(EditorStyles.textArea)
        {
            padding = new RectOffset(4, 0, 0, 0)
        };

        public static readonly GUIStyle InputLabel = new GUIStyle(EditorStyles.label)
        {
            margin = new RectOffset(PIXELS_PER_INDENTATION_LEVEL, 10, EditorStyles.label.margin.top, EditorStyles.label.margin.bottom),
            wordWrap = true,
        };
        
        public static readonly GUIStyle DeploymentStatus = new GUIStyle(GUI.skin.label)
        {
            imagePosition = ImagePosition.ImageLeft,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Overflow,
            stretchHeight = true,
            fontSize = 16,
            margin = new RectOffset(0, 11, 0, 0),
            padding = new RectOffset(0, 0, 0, 0),
        };

        public static readonly GUIStyle DeploymentStatusIcon = new GUIStyle(DeploymentStatus)
        {
            fixedWidth = INLINE_ICON_SIZE,
            fixedHeight = INLINE_ICON_SIZE,
            margin = new RectOffset(0, 0, 0, 0),
            contentOffset = new Vector2(0, 3),
        };
        
        public static readonly GUIStyle DeploymentStatusText = new GUIStyle(DeploymentStatus)
        {
            fontSize = GUI.skin.label.fontSize,
            fontStyle = GUI.skin.label.fontStyle,
            margin = new RectOffset(5, 5, 0, 0),
        };
        
        public static readonly GUIStyle RefreshIcon = new GUIStyle(DeploymentStatus)
        {
            fixedWidth = INLINE_ICON_SIZE,
            fixedHeight = INLINE_ICON_SIZE,
            contentOffset = new Vector2(0, 3),
            margin = new RectOffset(10, 0, 0, 0),
        };

        public static readonly float SpaceBetweenSections = 20f;

        public static readonly GUIStyle UserLoginButtonStyle = new GUIStyle()
        {
            margin = new RectOffset(0, 0, 5, 5)
        };
        
        public static readonly Color ErrorRed = new Color(0.851f, 0.082f, 0.082f, 1f);
    }
}
