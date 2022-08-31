// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Unity
using UnityEditor;
using UnityEngine;

// GameKit 
using AWS.GameKit.Editor.GUILayoutExtensions;
using AWS.GameKit.Editor.Utils;

namespace AWS.GameKit.Editor.Windows.Settings
{
    /// <summary>
    /// GUIStyles for the AWS GameKit Settings window.
    /// </summary>
    public static class SettingsGUIStyles
    {
        public static class Window
        {
            private const float _MIN_WIDTH = 800f;
            private const float _MIN_HEIGHT = 200f;
            public static readonly Vector2 MinSize = new Vector2(_MIN_WIDTH, _MIN_HEIGHT);

            private const float _INITIAL_WIDTH = _MIN_WIDTH;
            private const float _INITIAL_HEIGHT = 600f;
            public static readonly Vector2 InitialSize = new Vector2(_INITIAL_WIDTH, _INITIAL_HEIGHT);
        }

        public static class NavigationTree
        {
            public const float FIXED_WIDTH = 220f;

            public static readonly GUIStyle VerticalLayout = new GUIStyle()
            {
                fixedWidth = FIXED_WIDTH,
                margin = new RectOffset(0, 0, 8, 8)
            };

            public static readonly float RowHeight = EditorGUIUtility.singleLineHeight;

            public static readonly GUIStyle RowText = new GUIStyle(EditorStyles.label)
            {
                // empty - alias for GUI.skin.label
            };
        }

        public static class PageContainer
        {
            public static readonly GUIStyle VerticalLayout = new GUIStyle()
            {
                margin = new RectOffset(8, 8, 6, 6)
            };
        }

        public static class Page
        {
            public static readonly GUIStyle FoldoutTitle = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };

            public static readonly GUIStyle FoldoutBox = new GUIStyle()
            {
                margin = new RectOffset(10, 0, 0, 0)
            };

            public static readonly GUIStyle VerticalLayout = new GUIStyle()
            {
                fixedWidth = 158
            };

            public static readonly GUIStyle PrefixLabelSubtext = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                padding = new RectOffset(18, 0, 0, 0),
                wordWrap = true
            };

            public static readonly GUIStyle TextAreaSubtext = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                wordWrap = true
            };

            public static readonly GUIStyle Paragraph = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(0, 0, 10, 10),
                richText = true,
                wordWrap = true
            };

            public static readonly GUIStyle EnvDetails = new GUIStyle(EditorStyles.label)
            {
                margin = new RectOffset(0, 0, 0, 0),
                fontSize = 10,
                alignment = TextAnchor.UpperRight
            };

            public static readonly GUIStyle CustomHelpBoxText = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                richText = true,
                wordWrap = true,
                padding = new RectOffset(0,0, 3,0)
            };

            public static readonly GUIStyle Title = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = GUI.skin.label.fontSize + 4,
                wordWrap = true
            };
            
            public static readonly GUIStyle BannerBox = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(0, 0, 0, 5)
            };

            public static readonly GUIStyle BannerBoxLabel = new GUIStyle(Paragraph)
            {
                padding = new RectOffset(5, 0, 0, 0)
            };

            public static readonly float SpaceAfterTitle = 4f;
        }

        public class Buttons
        {
            /// <summary>
            /// This is the minimum width all normal sized buttons should have to maintain a consistent look in the UI.
            /// </summary>
            public const float MIN_WIDTH_NORMAL = 100;

            /// <summary>
            /// This is the minimum width all small sized buttons should have to maintain a consistent look in the UI.
            /// </summary>
            public const float MIN_WIDTH_SMALL = 50;

            /// <summary>
            /// This is the left and right padding all normal sized buttons should have so the text doesn't look crowded if it fills up the button's whole width.
            /// </summary>
            public const int HORIZONTAL_PADDING_NORMAL = 12;

            /// <summary>
            /// This is the left and right padding all small sized buttons should have so the text doesn't look crowded if it fills up the button's whole width.
            /// </summary>
            public const int HORIZONTAL_PADDING_SMALL = 8;

            public static readonly GUILayoutOption MinWidth = GUILayout.MinWidth(MIN_WIDTH_NORMAL);

            public static readonly IGettable<Color> GUIButtonGreen = new EditorThemeAware<Color>(
                new Color(0.25f, 0.85f, 0.25f),
                new Color(0, 0.50f, 0));

            public static readonly IGettable<Color> GUIButtonRed = new EditorThemeAware<Color>(
                new Color(1.0f, 0.25f, 0.25f),
                new Color(0.75f, 0, 0));

            public static readonly GUIStyleState WhiteTextButtonNormal = new GUIStyleState()
            {
                background = GUI.skin.button.normal.background,
                scaledBackgrounds = GUI.skin.button.normal.scaledBackgrounds,
                textColor = Color.white
            };

            public static readonly GUIStyleState WhiteTextButtonHovered = new GUIStyleState()
            {
                background = GUI.skin.button.hover.background,
                scaledBackgrounds = GUI.skin.button.hover.scaledBackgrounds,
                textColor = Color.white
            };

            public static readonly GUIStyleState WhiteTextButtonActive = new GUIStyleState()
            {
                background = GUI.skin.button.active.background,
                scaledBackgrounds = GUI.skin.button.active.scaledBackgrounds,
                textColor = Color.white
            };

            public static readonly GUIStyle WhiteTextButton = new GUIStyle(GUI.skin.button)
            {
                active = WhiteTextButtonActive,
                hover = WhiteTextButtonHovered,
                normal = WhiteTextButtonNormal,
                onFocused = WhiteTextButtonActive
            };

            public static readonly GUIStyle ColoredButtonNormal = new GUIStyle(WhiteTextButton)
            {
                padding = new RectOffset(HORIZONTAL_PADDING_NORMAL, HORIZONTAL_PADDING_NORMAL, WhiteTextButton.padding.top, WhiteTextButton.padding.bottom),
                stretchWidth = false
            };

            public static readonly GUIStyle GreyButtonNormal = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(HORIZONTAL_PADDING_NORMAL, HORIZONTAL_PADDING_NORMAL, GUI.skin.button.padding.top, GUI.skin.button.padding.bottom),
                stretchWidth = false
            };
            
            public static readonly GUIStyle ColoredButtonSmall = new GUIStyle(WhiteTextButton)
            {
                padding = new RectOffset(HORIZONTAL_PADDING_SMALL, HORIZONTAL_PADDING_SMALL, WhiteTextButton.padding.top, WhiteTextButton.padding.bottom),
                stretchWidth = false
            };

            public static readonly GUIStyle GreyButtonSmall = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(HORIZONTAL_PADDING_SMALL, HORIZONTAL_PADDING_SMALL, GUI.skin.button.padding.top, GUI.skin.button.padding.bottom),
                stretchWidth = false
            };

            public static readonly GUIStyle CreateAccountButton = new GUIStyle(WhiteTextButton)
            {
                margin = new RectOffset(0, 0, 10, 10),
                padding = new RectOffset(20, 20, 3, 3),
                stretchWidth = false
            };

            public static readonly GUIStyle SubmitCredentialsButton = new GUIStyle(GUI.skin.button)
            {
                stretchWidth = true,
                margin = new RectOffset(160, 10, GUI.skin.button.margin.top, GUI.skin.button.margin.bottom),
                fixedWidth = MIN_WIDTH_NORMAL
            };

            public static readonly GUIStyle LocateConfigurationButton = new GUIStyle(GUI.skin.button)
            {
                richText = true,
                padding = new RectOffset(8, 8, GUI.skin.button.margin.top, GUI.skin.button.margin.bottom),
                alignment = TextAnchor.MiddleCenter
            };

            public static readonly GUIStyle ChangeEnvironmentAndCredentialsButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(GUI.skin.button.margin.left, GUI.skin.button.margin.right, 10, GUI.skin.button.margin.bottom),
            };

            public static readonly GUIStyle CallExampleAPIButton = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(5, GUI.skin.button.margin.right, GUI.skin.button.margin.top, GUI.skin.button.margin.bottom),
                padding = new RectOffset(8, 8, GUI.skin.button.margin.top, GUI.skin.button.margin.bottom)
            };
        }

        public static class Tooltip
        {
            public static readonly GUIStyle Text = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft
            };
        }

        public static class EnvironmentAndCredentialsPage
        {
            public static readonly GUIStyle GetUserCredentialsLinkLayout = new GUIStyle()
            {
                margin = new RectOffset(160, 0, -5, 0)
            };

            public static readonly GUIStyle CustomEnvironmentVerticalLayout = new GUIStyle()
            {
                margin = new RectOffset(175, 0, 0, 0),
            };

            public static readonly GUIStyle CustomEnvironmentErrorVerticalLayout = new GUIStyle()
            {
                margin = new RectOffset(175, 0, 0, 10),
            };

            public static readonly GUIStyle AccountCredentialsHelpBoxesVerticalLayout = new GUIStyle()
            {
                margin = new RectOffset(EditorStyles.textField.margin.left, EditorStyles.textField.margin.right, 5, 10)
            };

            public static readonly float SpaceAfterAccountIdHelpBox = 10f;
        };

        public static class FeaturePage
        {
            public static readonly GUIStyle Description = new GUIStyle(GUI.skin.label)
            {
                fontSize = GUI.skin.label.fontSize + 2,
                wordWrap = true
            };

            public static readonly GUIStyle TabSelector = new GUIStyle("LargeButton")
            {
                margin = new RectOffset(0, 0, 8, 8)
            };
        }

        public static class FeatureExamplesTab
        {
            public static float ResponsePrefixLabelWidth = 145;

            public static readonly GUIStyle ExampleContainer = new GUIStyle(EditorStyles.helpBox)
            {
                margin = new RectOffset(0, 0, 0, 5)
            };

            public static readonly GUIStyle ExampleFoldoutContainer = new GUIStyle()
            {
                margin = new RectOffset(10, 0, 0, 0)
            };

            public static readonly GUIStyle ExampleResponseInputAligned = new GUIStyle()
            {
                margin = new RectOffset(157, 0, 0, 0)
            };

            public static readonly GUIStyle ExampleDictionaryInputAligned = new GUIStyle()
            {
                margin = new RectOffset(7, 0, 0, 0)
            };

            public static readonly GUIStyle DictionaryKeyValues = new GUIStyle()
            {
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = false,
                wordWrap = true

            };
        }

        public static class Icons
        {
            public static readonly Texture InfoIcon = EditorGUIUtility.IconContent("console.infoicon.sml").image;
            public static readonly Texture WarnIcon = EditorGUIUtility.IconContent("console.warnicon.sml").image;
            public static readonly Texture ErrorIcon = EditorGUIUtility.IconContent("console.erroricon.sml").image;

            public static readonly Vector2 InsideButtonIconSize = new Vector2( CommonGUIStyles.INLINE_ICON_SIZE, CommonGUIStyles.INLINE_ICON_SIZE);

            public static readonly GUIStyle InlineIcons = new GUIStyle(GUIStyle.none)
            {
                fixedWidth = CommonGUIStyles.INLINE_ICON_SIZE,
                fixedHeight = CommonGUIStyles.INLINE_ICON_SIZE,
                contentOffset = new Vector2(0 ,4)
            };

            public static readonly GUIStyle NormalSize = new GUIStyle()
            {
                fixedWidth = 18,
                fixedHeight = 18,
                margin = new RectOffset(5, 23, 5, 5)
            };
        }

        public static class LogPage
        {
            public static readonly IGettable<Color> LogDarkColor = new EditorThemeAware<Color>(
                new Color(0.22f, 0.22f, 0.22f),
                new Color(0.75f, 0.75f, 0.75f));

            public static readonly IGettable<Color> LogLightColor = new EditorThemeAware<Color>(
                new Color(0.25f, 0.25f, 0.25f),
                new Color(0.8f, 0.8f, 0.8f));

            public static readonly IGettable<Color> LogErrorTextColor = new EditorThemeAware<Color>(
                new Color(0.9f, 0.2f, 0.2f),
                new Color(1, 0.1f, 0.1f));

            public static readonly IGettable<Color> LogWarningTextColor = new EditorThemeAware<Color>(
                new Color(0.75f, 0.75f, 0),
                new Color(0.95f, 0.85f, 0.5f));

            public static readonly IGettable<Color> LogInfoTextColor = new EditorThemeAware<Color>(
                new Color(0.9f, 0.9f, 0.9f),
                new Color(0, 0, 0));

            public static readonly Texture2D DarkBackground = GUIEditorUtils.CreateBackground(SettingsGUIStyles.LogPage.LogDarkColor.Get());
            public static readonly Texture2D LightBackground = GUIEditorUtils.CreateBackground(SettingsGUIStyles.LogPage.LogLightColor.Get());

            public static readonly GUIStyle LogSection = new GUIStyle(EditorStyles.helpBox) 
            { 
                stretchHeight = true, 
                margin = new RectOffset(0, 0, 0, 2) 
            };

            public static readonly GUIStyle LogBox = new GUIStyle(Page.VerticalLayout)
            {
                margin = new RectOffset(10, 10, 10, 15),
                fixedWidth = 0
            };

            public static readonly GUIStyle LogEntry = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 0, 0),
                richText = true
            };

            public static readonly GUIStyle ButtonLeft = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(GUI.skin.button.margin.left, 10, GUI.skin.button.margin.top, GUI.skin.button.margin.bottom)
            };

            public static readonly GUIStyle ButtonRight = new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(10, GUI.skin.button.margin.right, GUI.skin.button.margin.top, GUI.skin.button.margin.bottom)
            };

            public static readonly GUIStyle GlobalSettingsSection = new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 10),
                fixedHeight = 65
            };

            public static readonly GUIStyle LoggingLevel = new GUIStyle(EditorStyles.radioButton)
            {
                padding = new RectOffset(20, 20, 0, 0),
                margin = new RectOffset(0, 0, 5, 0)
            };
        }

        public static class DeleteWindow
        {
            public const float MIN_SIZE_X = 750f;
            public const float MIN_SIZE_Y = 500f;

            const int LARGE_PADDING = 25;
            const int MEDIUM_PADDING = 15;
            const int SMALL_PADDING = 10;

            static RectOffset GENERIC_PADDING = new RectOffset(LARGE_PADDING, LARGE_PADDING, SMALL_PADDING, SMALL_PADDING);

            public static readonly GUIStyle ResourceDescriptionLine = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(SMALL_PADDING, SMALL_PADDING, SMALL_PADDING, 0),
                wordWrap = true,
                fontSize = 10,
                normal = new GUIStyleState()
                {
                    textColor = LogPage.LogInfoTextColor.Get(),
                    background = LogPage.LightBackground,
                }
            };

            public static readonly GUIStyle GeneralText = new GUIStyle(EditorStyles.label)
            {
                margin = GENERIC_PADDING,
                wordWrap = true,
            };

            public static readonly GUIStyle ResourceList = new GUIStyle()
            {
                margin = new RectOffset(LARGE_PADDING, MEDIUM_PADDING, MEDIUM_PADDING, 0)
            };

            public static readonly GUIStyle FloatRight = new GUIStyle(EditorStyles.label)
            {
                margin = GENERIC_PADDING,
                wordWrap = true,
                alignment = TextAnchor.MiddleRight,
            };

            public static readonly GUIStyle GeneralTextField = new GUIStyle(EditorStyles.textField)
            {
                margin = GENERIC_PADDING
            };

            public static readonly GUIStyle ButtonMargins = new GUIStyle()
            {
                margin = GENERIC_PADDING
            };
        }

        public static class Achievements
        {
            public const float SHORT_INPUT_WIDTH = 162;
            public const float DESCRIPTION_MIN_HEIGHT = 65;
            public const float DESCRIPTION_WIDTH = 400;
            public const int SPACING = 5;

            // These values are used for the size of each widget when collapsed or expanded
            public const int COLLAPSED_HEIGHT = 29;
            public const int EXPANDED_HEIGHT = 460;

            // Using a style with padding will not work for the header icon. The padding attribute does not account for tooltips
            public const int HEADER_ICON_HORIZONTAL_PADDING = 35;
            public const int HEADER_ICON_VERTICAL_PADDING = 3;
            public const int HEADER_ICON_WIDTH = 15;
            public const int HEADER_ICON_HEIGHT = 9;

            public static readonly GUIStyle BodyCollapsed = new GUIStyle(EditorStyles.helpBox)
            {
                stretchHeight = false,
                fixedHeight = COLLAPSED_HEIGHT,
                padding = new RectOffset(5, 0, 5, 5)
            };

            public static readonly GUIStyle BodyExpanded = new GUIStyle(BodyCollapsed)
            {
                fixedHeight = 0,
                stretchHeight = true,
            };

            public static readonly GUIStyle Header = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(-20, 0, 0, 0),
            };

            public static readonly GUIStyle DeleteButton = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(2, 2, 2, 2),
                margin = new RectOffset(0, 5, 0, 0)
            };

            public static readonly GUIStyle VisibilityLabel = new GUIStyle(CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.InputLabel, 1));

            public static readonly GUIStyle Expanded = new GUIStyle()
            {
                stretchHeight = true,
                padding = new RectOffset(0, 0, 10, 10)
            };

            public static readonly GUIStyle Description = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                fixedWidth = DESCRIPTION_WIDTH
            };

            public static readonly GUIStyle DescriptionLabel = new GUIStyle(CommonGUIStyles.InputLabel)
            {
                stretchHeight = true,
                alignment = TextAnchor.UpperLeft
            };

            public static readonly GUIStyle ImageBody = new GUIStyle()
            {
                fixedWidth = 306
            };

            public static readonly GUIStyle ImageLabel = new GUIStyle(CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.InputLabel, 1))
            {
                stretchHeight = true,
                alignment = TextAnchor.UpperLeft
            };

            public static readonly GUIStyle Image = new GUIStyle(EditorStyles.helpBox)
            {
                fixedHeight = 50,
                fixedWidth = 50,
                alignment = TextAnchor.MiddleCenter
            };

            public static class GetLatestPopupWindow
            {
                private const float _MIN_WIDTH = 400f;
                private const float _MIN_HEIGHT = 100f;
                public static readonly Vector2 MinSize = new Vector2(_MIN_WIDTH, _MIN_HEIGHT);
            }
        }
    }
}
