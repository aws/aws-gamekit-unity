// Copyright Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: Apache-2.0

// Standard Library
using System;
using System.Collections.Generic;

// Unity
using UnityEditor;
using UnityEngine;

// GameKit
using AWS.GameKit.Editor.FileStructure;
using AWS.GameKit.Editor.Windows.Settings;
using AWS.GameKit.Runtime.Models;

namespace AWS.GameKit.Editor.GUILayoutExtensions
{
    /// <summary>
    /// Provides new types of GUI elements for Unity's EditorGUILayout class.
    ///
    /// See: https://docs.unity3d.com/ScriptReference/EditorGUILayout.html
    /// </summary>
    public static class EditorGUILayoutElements
    {
        private const string INFO_ICON_ID = "console.infoicon"; // Icon ids provided by Unity that can be retrieved with EditorGUIUtility.IconContent
        private const string EMPTY_PREFIX_SPACE = " "; // If we use string.Empty, the prefix label won't take up any space in the layout - use a single space instead

        private const int HELP_ICON_HEIGHT = 35;
        private const int OVERRIDE_SLIDER_TEXT_WIDTH = 50;

        private static string SELECT_FILE = L10n.Tr("Select file");

        /// <summary>
        /// Make a vertical divider.<br/>
        ///
        /// A vertical divider is a thin line used to separate two sections inside an <c>EditorGUILayout.BeginHorizontal()</c> and <c>EditorGUILayout.EndHorizontal()</c> block.<br/><br/>
        ///
        /// The divider color changes automatically when the user changes their Editor Theme between dark and light mode.
        /// </summary>
        /// <param name="width">The width of the divider line.</param>
        /// <param name="verticalPadding">The amount of padding around the top and bottom of the divider line.</param>
        public static void VerticalDivider(float width = 1f, int verticalPadding = 0)
        {
            GUIStyle lineStyle = new GUIStyle
            {
                fixedWidth = width,
                stretchHeight = true,
                stretchWidth = false,
                margin = new RectOffset(0, 0, verticalPadding, verticalPadding),
                normal =
                {
                    background = EditorResources.Textures.Colors.GUILayoutDivider.Get()
                }
            };

            GUILayout.Box(GUIContent.none, lineStyle);
        }

        /// <summary>
        /// Make a horizontal divider.<br/>
        ///
        /// A horizontal divider is a thin line used to separate two sections inside an <c>EditorGUILayout.BeginVertical()</c> and <c>EditorGUILayout.EndVertical()</c> block.<br/><br/>
        ///
        /// The divider color changes automatically when the user changes their Editor Theme between dark and light mode.
        /// </summary>
        /// <param name="height">The height of the divider line.</param>
        /// <param name="verticalPadding">The amount of padding around the top and bottom of the divider line.</param>
        public static void HorizontalDivider(float height = 1f, int verticalPadding = 0)
        {
            GUIStyle lineStyle = new GUIStyle
            {
                fixedHeight = height,
                stretchHeight = false,
                stretchWidth = true,
                margin = new RectOffset(0, 0, verticalPadding, verticalPadding),
                normal =
                {
                    background = EditorResources.Textures.Colors.GUILayoutDivider.Get()
                }
            };

            GUILayout.Box(GUIContent.none, lineStyle);
        }

        /// <summary>
        /// Creates a toolbar that persists which tab of the toolbar is selected across sessions.
        /// </summary>
        /// <param name="selectedTabId">A reference to the selectedTabId that is used to determine which tab should be draw in DrawTabContent.</param>
        /// <param name="tabNames">The names of the tabs that should be added to the toolbar.</param>
        /// <param name="tabSelectorButtonSize">Determines how the toolbar buttons will be sized.</param>
        /// <param name="tabSelectorStyle">The style of the toolbar.</param>
        public static int CreateFeatureToolbar(int selectedTabId, string[] tabNames, GUI.ToolbarButtonSize tabSelectorButtonSize, GUIStyle tabSelectorStyle)
        {
            return GUILayout.Toolbar(selectedTabId, tabNames, tabSelectorStyle, tabSelectorButtonSize);
        }

        /// <summary>
        /// Make a help box with a similar style to Unity's default help box, except this help box has a read more link.
        /// </summary>
        /// <param name="message">The message that should be displayed in the help box.</param>
        /// <param name="url">The url that the read more link should redirect to.</param>
        public static void HelpBoxWithReadMore(string message, string url)
        {
            LinkWidget.Options options = new LinkWidget.Options()
            {
                OverallStyle = SettingsGUIStyles.Page.CustomHelpBoxText,
                ContentOffset = new Vector2(
                    x: -4,
                    y: -4 
                ),
                Alignment = LinkWidget.Alignment.Right
            };

            LinkWidget _readMoreLinkWidget = new LinkWidget(L10n.Tr("Read more"), url, options);

            using (EditorGUILayout.HorizontalScope horizontalScope = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                Rect helpBoxRect = horizontalScope.rect;
                using (new EditorGUILayout.VerticalScope())
                {
                    GUIStyle style = new GUIStyle();
                    float centeredContentOffset = (helpBoxRect.height - HELP_ICON_HEIGHT)/2 - 1;
                    style.contentOffset = new Vector2(0, centeredContentOffset);
                    
                    GUILayout.Box(EditorGUIUtility.IconContent(INFO_ICON_ID).image, style, GUILayout.Height(HELP_ICON_HEIGHT));
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(message, SettingsGUIStyles.Page.CustomHelpBoxText);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        _readMoreLinkWidget.OnGUI();
                    }
                }
            }
        }

        /// <summary>
        /// Make a GUI element that is tinted with a color.<br/><br/>
        ///
        /// Use this when you need to change the color of a GUI element without changing it's <c>GUIStyle</c>.
        /// </summary>
        /// <param name="color">The color to tint the GUI element.</param>
        /// <param name="createElement">A callback function which creates the GUI element.</param>
        public static void TintedElement(Color color, Action createElement)
        {
            Color originalBackgroundColor = GUI.backgroundColor;

            GUI.backgroundColor = color;
            createElement();
            GUI.backgroundColor = originalBackgroundColor;
        }

        /// <summary>
        /// Make a GUI element that is tinted with a color.<br/><br/>
        ///
        /// Use this when you need to change the color of a GUI element without changing it's <c>GUIStyle</c>.
        /// </summary>
        /// <typeparam name="T">The return type of the <c>createElement()</c> callback function.</typeparam>
        /// <param name="color">The color to tint the GUI element.</param>
        /// <param name="createElement">A callback function which creates the GUI element.</param>
        /// <returns>The return value of the <c>createElement()</c> callback function. This should be the new value entered by the user in the GUI element.</returns>
        public static T TintedElement<T>(Color color, Func<T> createElement)
        {
            Color originalBackgroundColor = GUI.backgroundColor;

            GUI.backgroundColor = color;
            T result = createElement();
            GUI.backgroundColor = originalBackgroundColor;

            return result;
        }

        /// <summary>
        /// Make a stylized single press button that can be colored and disabled.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="isEnabled">Whether the button is enabled or not. By default the button is enabled.</param>
        /// <param name="tooltip">The tooltip to display on the button. By default no tooltip is displayed.</param>
        /// <param name="colorWhenEnabled">The color to make the button when it is enabled. By default the button is Unity's default button color (grey). The button is grey when disabled.</param>
        /// <param name="image">An image to display before the text. By default no image is displayed.</param>
        /// <param name="imageSize">The size to make the image. By default the image is be drawn at it's full size (i.e. not scaled).</param>
        /// <param name="minWidth">Custom value for the minimum width of the button, if not provided then a common default is used.</param>
        /// <returns>True when the user clicks the button.</returns>
        public static bool Button(
            string text, 
            bool isEnabled = true, 
            string tooltip = "", 
            Nullable<Color> colorWhenEnabled = null, 
            Texture image = null, 
            Nullable<Vector2> imageSize = null, 
            float minWidth = SettingsGUIStyles.Buttons.MIN_WIDTH_NORMAL)
        {
            Vector2 finalImageSize = imageSize ?? EditorGUIUtility.GetIconSize();
            GUIContent buttonContent = new GUIContent(text, image, tooltip);

            using (new EditorGUIUtility.IconSizeScope(finalImageSize))
            {
                using (new EditorGUI.DisabledScope(!isEnabled))
                {
                    if (isEnabled && colorWhenEnabled.HasValue)
                    {
                        return TintedElement(colorWhenEnabled.Value, () =>
                            GUILayout.Button(buttonContent, SettingsGUIStyles.Buttons.ColoredButtonNormal, GUILayout.MinWidth(minWidth)));
                    }
                    else
                    {
                        return GUILayout.Button(buttonContent, SettingsGUIStyles.Buttons.GreyButtonNormal, GUILayout.MinWidth(minWidth));
                    }
                }
            }
        }
        
        /// <summary>
        /// Make a stylized single press button, that is half the size of normal buttons, that can be colored and disabled.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="isEnabled">Whether the button is enabled or not. By default the button is enabled.</param>
        /// <param name="tooltip">The tooltip to display on the button. By default no tooltip is displayed.</param>
        /// <param name="colorWhenEnabled">The color to make the button when it is enabled. By default the button is Unity's default button color (grey). The button is grey when disabled.</param>
        /// <param name="image">An image to display before the text. By default no image is displayed.</param>
        /// <param name="imageSize">The size to make the image. By default the image is be drawn at it's full size (i.e. not scaled).</param>
        /// <param name="minWidth">Custom value for the minimum width of the button, if not provided then a common default is used.</param>
        /// <returns>True when the user clicks the button.</returns>
        public static bool SmallButton(
            string text, 
            bool isEnabled = true, 
            string tooltip = "", 
            Nullable<Color> colorWhenEnabled = null, 
            Texture image = null, 
            Nullable<Vector2> imageSize = null, 
            float minWidth = SettingsGUIStyles.Buttons.MIN_WIDTH_SMALL)
        {
            Vector2 finalImageSize = imageSize ?? EditorGUIUtility.GetIconSize();
            GUIContent buttonContent = new GUIContent(text, image, tooltip);

            using (new EditorGUIUtility.IconSizeScope(finalImageSize))
            {
                using (new EditorGUI.DisabledScope(!isEnabled))
                {
                    if (isEnabled && colorWhenEnabled.HasValue)
                    {
                        return TintedElement(colorWhenEnabled.Value, () =>
                            GUILayout.Button(buttonContent, SettingsGUIStyles.Buttons.ColoredButtonSmall, GUILayout.MinWidth(minWidth)));
                    }
                    else
                    {
                        return GUILayout.Button(buttonContent, SettingsGUIStyles.Buttons.GreyButtonSmall, GUILayout.MinWidth(minWidth));
                    }
                }
            }
        }

        /// <summary>
        /// Make a section header.
        /// </summary>
        /// <param name="title">The string to display.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the title.</param>
        /// <param name="textAnchor">Where to anchor the text.</param>
        public static void SectionHeader(string title, int indentationLevel = 0, TextAnchor textAnchor = TextAnchor.MiddleLeft)
        {
            GUIStyle headerStyle = CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.SectionHeader, indentationLevel);
            headerStyle.alignment = textAnchor;
            EditorGUILayout.LabelField(title, headerStyle);
        }

        /// <summary>
        /// Make a section header with a description on the same line. The text is formatted as <c>Title: Description</c> with <c>Title</c> in bold.
        /// </summary>
        /// <param name="title">The title string to display in bold.</param>
        /// <param name="description">The description string to display after the title and colon.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the title.</param>
        /// <param name="textAnchor">Where to anchor the text.</param>
        public static void SectionHeaderWithDescription(string title, string description, int indentationLevel = 0, TextAnchor textAnchor = TextAnchor.MiddleLeft)
        {
            GUIStyle headerStyle = CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.SectionHeaderWithDescription, indentationLevel);
            headerStyle.alignment = textAnchor;

            string text = $"<b>{title}:</b> {description}";

            EditorGUILayout.LabelField(text, headerStyle);
        }


        /// <summary>
        /// Make a vertical gap between the previous and next GUI sections.
        /// </summary>
        /// <param name="extraPixels">Number of additional pixels to add/subtract from the gap.</param>
        public static void SectionSpacer(float extraPixels = 0)
        {
            EditorGUILayout.Space(CommonGUIStyles.SpaceBetweenSections + extraPixels);
        }

        /// <summary>
        /// Make a horizontal line to divide two sections.
        /// </summary>
        public static void SectionDivider()
        {
            SectionSpacer(extraPixels: -10);
            HorizontalDivider();
            SectionSpacer(extraPixels: -10);
        }

        /// <summary>
        /// Make a prefix label for a user input.
        /// </summary>
        /// <param name="inputLabel">The string to display.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the prefix label.</param>
        public static void PrefixLabel(string inputLabel, int indentationLevel = 1)
        {
            GUIStyle labelStyle = CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.InputLabel, indentationLevel);

            EditorGUILayout.PrefixLabel(inputLabel, labelStyle, labelStyle);
            KeepPreviousPrefixLabelEnabled();
        }

        /// <summary>
        /// Make a custom field.
        /// </summary>
        /// <param name="inputLabel">A string to display before the custom field.</param>
        /// <param name="createField">A callback function which creates the field.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <param name="guiStyle">Used to add a custom GUIStyle, else defaults to EditorStyles.textArea</param>
        public static void CustomField(string inputLabel, Action createField, int indentationLevel = 1, bool isEnabled = true, GUIStyle guiStyle = null)
        {
            GUIStyle labelStyle = CommonGUIStyles.SetIndentationLevel(guiStyle ?? CommonGUIStyles.InputLabel, indentationLevel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(inputLabel, labelStyle);
                KeepPreviousPrefixLabelEnabled();
                using (new EditorGUI.DisabledScope(!isEnabled))
                {
                    createField();
                }
            }
        }

        /// <summary>
        /// Make a custom field and temporarily override the labelWidth variable in the editor.
        /// </summary>
        /// <param name="inputLabel">A string to display before the custom field.</param>
        /// <param name="labelWidth">A float that will override EditorGUIUtility.labelWidth for the life of this method.</param>
        /// <param name="createField">A callback function which creates the field.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <param name="guiStyle">Used to add a custom GUIStyle, else defaults to EditorStyles.textArea</param>
        public static void CustomField(string inputLabel, float labelWidth, Action createField, int indentationLevel = 1, bool isEnabled = true, GUIStyle guiStyle = null)
        {
            float originalLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = labelWidth;

            CustomField(inputLabel, createField, indentationLevel, isEnabled, guiStyle);

            EditorGUIUtility.labelWidth = originalLabelWidth;
        }

        /// <summary>
        /// Make a custom field.
        /// </summary>
        /// <typeparam name="T">The type of data the field contains.</typeparam>
        /// <param name="inputLabel">A string to display before the custom field.</param>
        /// <param name="createField">A callback function which creates the field and returns the field's new value.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <param name="guiStyle">Used to add a custom GUIStyle, else defaults to EditorStyles.textArea</param>
        /// <returns>The field's new value entered by the user.</returns>
        public static T CustomField<T>(string inputLabel, Func<T> createField, int indentationLevel = 1, bool isEnabled = true, GUIStyle guiStyle = null)
        {
            GUIStyle labelStyle = CommonGUIStyles.SetIndentationLevel(guiStyle ?? CommonGUIStyles.InputLabel, indentationLevel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PrefixLabel(inputLabel, labelStyle, labelStyle);
                KeepPreviousPrefixLabelEnabled();

                using (new EditorGUI.DisabledScope(!isEnabled))
                {
                    return createField();
                }
            }
        }

        /// <summary>
        /// Make a text input field.
        /// </summary>
        /// <param name="inputLabel">A string to display before the text input area.</param>
        /// <param name="currentText">The current text in the text input area.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="placeholderText">Optional placeholder text to display when the input field is empty.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <returns>The text entered by the user.</returns>
        public static string TextField(string inputLabel, string currentText, int indentationLevel = 1, string placeholderText = "", bool isEnabled = true)
        {
            return CustomField(inputLabel, () => {
                string newText = EditorGUILayout.TextField(currentText);
                
                if (!String.IsNullOrEmpty(placeholderText) && String.IsNullOrEmpty(newText))
                {
                    DisplayPlaceholderTextOverLastField(placeholderText);
                }

                return newText;
            }, indentationLevel, isEnabled);
        }

        /// <summary>
        /// Make a text input area that handles multiple lines.
        /// </summary>
        /// <param name="inputLabel">A string to display before the text input area.</param>
        /// <param name="currentText">The current text in the text input area.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="placeholderText">Optional placeholder text to display when the input field is empty.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <param name="labelGuiStyle">Used to add a custom GUIStyle to the fields label, else defaults to CommonGUIStyles.InputLabel</param>
        /// <param name="textAreaGuiStyle">Used to add a custom GUIStyle to the text area, else defaults to EditorStyles.textArea</param>
        /// <param name="options">(Optional) Additional GUILayout params to apply to the text area.</param>
        /// <returns>The text entered by the user.</returns>
        public static string DescriptionField(
            string inputLabel, 
            string currentText, 
            int indentationLevel = 1, 
            string placeholderText = "", 
            bool isEnabled = true, 
            GUIStyle labelGuiStyle = null, 
            GUIStyle textAreaGuiStyle = null, 
            params GUILayoutOption[] options)
        {
            return CustomField(inputLabel, () => {
                string newText = EditorGUILayout.TextArea(currentText, textAreaGuiStyle ?? EditorStyles.textArea, options);

                if (!String.IsNullOrEmpty(placeholderText) && String.IsNullOrEmpty(newText))
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUI.TextArea(GUILayoutUtility.GetLastRect(), placeholderText, CommonGUIStyles.PlaceholderTextArea);
                    }
                }

                return newText;
            }, indentationLevel, isEnabled, labelGuiStyle);
        }

        /// <summary>
        /// Make a key-value dictionary field that will display a list of keys and a list of values.
        /// </summary>
        /// <typeparam name="K">The type of the key List. This type must be serializable.</typeparam>
        /// <typeparam name="V">The type of the value List. This type must be serializable.</typeparam>
        /// <param name="serializedKeysListProperty">The serialized property corresponding to the keys list.</param>
        /// <param name="serializedValuesListProperty">The serialized property corresponding to the values list.</param>
        /// <param name="keysList">The list of keys to be drawn.</param>
        /// <param name="valuesList">The list of values to be drawn.</param>
        /// <param name="keysLabel">The label that should be shown above the list of keys.</param>
        /// <param name="valuesLabel">The label that should be shown above the list of values.</param>
        /// <param name="isReadonly">Optional field is set to true if the GUI should be disabled for typing into and the options to add. Subtract fields will also be removed if isReadonly is true.</param>
        /// <param name="paddingBetweenKeyAndValue">When true, allows editing of the dictionary.</param>
        /// <param name="paddingBetweenPairs">Optional field for adding spacing between each pair in a vertical section.</param>
        public static void SerializableExamplesDictionary<K,V>(
            SerializedProperty serializedKeysListProperty, 
            SerializedProperty serializedValuesListProperty,
            List<K> keysList,
            List<V> valuesList, 
            string keysLabel, 
            string valuesLabel,
            bool isReadonly = false,
            int paddingBetweenKeyAndValue = 2,
            int paddingBetweenPairs = 4)
        {
            using (new EditorGUILayout.HorizontalScope(SettingsGUIStyles.FeatureExamplesTab.DictionaryKeyValues))
            {
                EditorGUILayout.LabelField(keysLabel, GUILayout.MinWidth(0));
                EditorGUILayout.LabelField(valuesLabel, GUILayout.MinWidth(0));

                if (!isReadonly)
                {
                    GUILayout.Space(CommonGUIStyles.INLINE_ICON_SIZE);
                }
            }

            using (new EditorGUI.DisabledScope(isReadonly))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    for (int i = 0; i < serializedKeysListProperty.arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(serializedKeysListProperty.GetArrayElementAtIndex(i),
                                GUIContent.none);
                            GUILayout.Space(paddingBetweenKeyAndValue);
                            EditorGUILayout.PropertyField(serializedValuesListProperty.GetArrayElementAtIndex(i),
                                GUIContent.none);

                            if (!isReadonly)
                            {
                                using (new EditorGUI.DisabledScope(serializedKeysListProperty.arraySize <= 1))
                                {
                                    GUILayout.Box(EditorResources.Textures.SettingsWindow.MinusIcon.Get(), SettingsGUIStyles.Icons.InlineIcons);

                                    Rect minusButtonRect = GUILayoutUtility.GetLastRect();

                                    // Create a button with no text but spans the length of both the icon created above
                                    if (GUI.Button(minusButtonRect, "", GUIStyle.none))
                                    {
                                        keysList.RemoveAt(i);
                                        valuesList.RemoveAt(i);
                                    }

                                    if (serializedKeysListProperty.arraySize > 1)
                                    {
                                        EditorGUIUtility.AddCursorRect(minusButtonRect, MouseCursor.Link);
                                    }
                                }
                            }
                        }
                        GUILayout.Space(paddingBetweenPairs);
                    }
                }

                if (!isReadonly)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Box(EditorResources.Textures.SettingsWindow.PlusIcon.Get(), SettingsGUIStyles.Icons.NormalSize);

                        Rect plusButtonRect = GUILayoutUtility.GetLastRect();

                        // Create a button with no text but spans the length of both the icon created above
                        if (GUI.Button(plusButtonRect, "", GUIStyle.none))
                        {
                            keysList.Add(default);
                            valuesList.Add(default);
                        }

                        EditorGUIUtility.AddCursorRect(plusButtonRect, MouseCursor.Link);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.Space(5);
                }
            }
        }

        /// <summary>
        /// Creates a UI element that will display a list in the same style as the SerializableExamplesDictionary
        /// </summary>
        /// <param name="serializedListProperty">The serialized property corresponding to the keys list.</param>
        /// <param name="list">The list of values to be drawn.</param>
        /// <param name="label">The label that should be shown above the list of keys.</param>
        /// <param name="isReadonly">When true, allows editing of the list.</param>
        /// <param name="paddingBetweenElements">Optional field for adding spacing between each element in a vertical section.</param>
        public static void SerializableExamplesList<T>(
            SerializedProperty serializedListProperty,
            List<T> list,
            string label,
            bool isReadonly = false,
            int paddingBetweenElements = 4)
        {
            using (new EditorGUILayout.HorizontalScope(SettingsGUIStyles.FeatureExamplesTab.DictionaryKeyValues))
            {
                EditorGUILayout.LabelField(label, GUILayout.MinWidth(0));

                if (!isReadonly)
                {
                    GUILayout.Space(CommonGUIStyles.INLINE_ICON_SIZE);
                }
            }

            using (new EditorGUI.DisabledScope(isReadonly))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    for (int i = 0; i < serializedListProperty.arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(serializedListProperty.GetArrayElementAtIndex(i), GUIContent.none);

                            if (!isReadonly)
                            {
                                using (new EditorGUI.DisabledScope(serializedListProperty.arraySize <= 1))
                                {
                                    GUILayout.Box(EditorResources.Textures.SettingsWindow.MinusIcon.Get(), SettingsGUIStyles.Icons.InlineIcons);

                                    Rect minusButtonRect = GUILayoutUtility.GetLastRect();

                                    // Create a button with no text but spans the length of both the icon created above
                                    if (GUI.Button(minusButtonRect, "", GUIStyle.none))
                                    {
                                        list.RemoveAt(i);
                                    }

                                    if (serializedListProperty.arraySize > 1)
                                    {
                                        EditorGUIUtility.AddCursorRect(minusButtonRect, MouseCursor.Link);
                                    }
                                }
                            }
                        }
                        GUILayout.Space(paddingBetweenElements);
                    }
                }

                if (!isReadonly)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.Box(EditorResources.Textures.SettingsWindow.PlusIcon.Get(), SettingsGUIStyles.Icons.NormalSize);

                        Rect plusButtonRect = GUILayoutUtility.GetLastRect();

                        // Create a button with no text but spans the length of both the text and the icon
                        if (GUI.Button(plusButtonRect, "", GUIStyle.none))
                        {
                            list.Add(default);
                        }

                        EditorGUIUtility.AddCursorRect(plusButtonRect, MouseCursor.Link);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.Space(5);
                }
            }
        }

        /// <summary>
        /// Make a label field. Useful for displaying read only information.
        /// </summary>
        /// <param name="labelKey">A string to display in the label key (left) field.</param>
        /// <param name="labelValue">A string to display in the label value (right) field.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label key.</param>
        public static void LabelField(string labelKey, string labelValue, int indentationLevel = 1)
        {
            CustomField(labelKey, () => EditorGUILayout.LabelField(labelValue), indentationLevel);
        }

        /// <summary>
        /// Make a password input field. All characters entered are displayed as '*'.
        /// </summary>
        /// <param name="inputLabel">A string to display before the password input area.</param>
        /// <param name="currentPassword">The current password in the password input area.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="placeholderText">Optional placeholder text to display when the input field is empty.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <returns>The password entered by the user.</returns>
        public static string PasswordField(string inputLabel, string currentPassword, int indentationLevel = 1, string placeholderText = "", bool isEnabled = true)
        {
            return CustomField(inputLabel, () => 
            {
                string newPassword = EditorGUILayout.PasswordField(currentPassword);
                
                if (!String.IsNullOrEmpty(placeholderText) && String.IsNullOrEmpty(newPassword))
                {
                    DisplayPlaceholderTextOverLastField(placeholderText);
                }

                return newPassword;
            }, indentationLevel, isEnabled);
        }

        /// <summary>
        /// Make a property field.
        /// </summary>
        /// <param name="inputLabel">A string to display before the property input area.</param>
        /// <param name="property">The serialized property to edit.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="placeholderText">Optional placeholder text to display when the input field is empty.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <param name="options">(Optional) Additional GUILayout params to apply to the field.</param>
        public static void PropertyField(string inputLabel, SerializedProperty property, int indentationLevel = 1, string placeholderText = "", bool isEnabled = true, params GUILayoutOption[] options)
        {
            CustomField(inputLabel, () =>
            {
                EditorGUILayout.PropertyField(property, GUIContent.none, options);

                if (!String.IsNullOrEmpty(placeholderText) && String.IsNullOrEmpty(property.stringValue))
                {
                    DisplayPlaceholderTextOverLastField(placeholderText);
                }
            }, indentationLevel, isEnabled);
        }

        /// <summary>
        /// Make a description field.
        /// </summary>
        /// <param name="description">The description to display.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the description.</param>
        /// <param name="textAnchor">Where to anchor the text.</param>
        public static void Description(string description, int indentationLevel = 1, TextAnchor textAnchor = TextAnchor.MiddleLeft)
        {
            GUIStyle descriptionStyle = CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.Description, indentationLevel);
            descriptionStyle.alignment = textAnchor;
            descriptionStyle.wordWrap = true;
            EditorGUILayout.LabelField(description, descriptionStyle);
        }

        public static void ErrorText(string error, int indentationLevel = 1)
        {
            GUIStyle errorStyle = CommonGUIStyles.SetIndentationLevel(CommonGUIStyles.Description, indentationLevel);
            errorStyle.normal.textColor = CommonGUIStyles.ErrorRed;

            EditorGUILayout.LabelField(error, errorStyle);
        }

        /// <summary>
        /// Make an on/off toggle field.
        /// </summary>
        /// <param name="inputLabel">A string to display before the toggle field.</param>
        /// <param name="currentValue">The current value of the toggle field.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <returns>The new value of the toggle field.</returns>
        public static bool ToggleField(string inputLabel, bool currentValue, int indentationLevel = 1, bool isEnabled = true)
        {
            return CustomField(inputLabel, () =>
                {
                    bool newValue = GUILayout.Toggle(currentValue, string.Empty);

                    // Make the toggle field only be selectable when the mouse is directly on it, rather than anywhere on it's row.
                    // Otherwise, the user can accidentally toggle this field without knowing it.
                    GUILayout.FlexibleSpace();

                    return newValue;
                },
                indentationLevel, isEnabled);
        }

        /// <summary>
        /// Make an on/off toggle with a label to the left of the checkbox.
        /// </summary>
        /// <param name="inputLabel">A string to display before after the checkbox.</param>
        /// <param name="currentValue">The current value of the toggle.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <returns>The new value of the toggle.</returns>
        public static bool ToggleLeft(string inputLabel, bool currentValue, bool isEnabled = true)
        {
            using (new EditorGUI.DisabledScope(!isEnabled))
            {
                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent(inputLabel)).x;
                bool result = EditorGUILayout.ToggleLeft(inputLabel, currentValue);
                EditorGUIUtility.labelWidth = originalLabelWidth;

                return result;
            }
        }

        /// <summary>
        /// Make an int slider field.
        /// </summary>
        /// <param name="inputLabel">A string to display before the int slider field.</param>
        /// <param name="currentValue">The current value of the slider.</param>
        /// <param name="minValue">The minimum value of the slider.</param>
        /// <param name="maxValue">The maximum value of the slider.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <returns>The new value of the int slider.</returns>
        public static int IntSlider(string inputLabel, int currentValue, int minValue, int maxValue, int indentationLevel = 1, bool isEnabled = true)
        {
            return CustomField(inputLabel, () =>
            {
                return EditorGUILayout.IntSlider(currentValue, minValue, maxValue);
            }, indentationLevel, isEnabled);
        }

        /// <summary>
        /// Make a slider that allows a user to override the min or max by using the text field value.
        /// </summary>
        /// <param name="inputLabel">A string to display before the int slider field.</param>
        /// <param name="currentValue">The current value of the slider.</param>
        /// <param name="minValue">The minimum value of the slider.</param>
        /// <param name="maxValue">The maximum value of the slider.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and and non-interactable.</param>
        /// <returns>The new value of the int slider.</returns>
        public static int OverrideSlider(string inputLabel, int currentValue, int minValue, int maxValue, int indentationLevel = 1, bool isEnabled = true)
        {
            return CustomField(inputLabel, () =>
            {
                float sliderValue = GUILayout.HorizontalSlider(currentValue, minValue, maxValue);

                float finalValue = sliderValue;

                string textValue = GUILayout.TextField(finalValue.ToString(), GUILayout.Width(OVERRIDE_SLIDER_TEXT_WIDTH));
                if (!float.TryParse(textValue, out finalValue))
                {
                    finalValue = string.IsNullOrEmpty(textValue) ? minValue : sliderValue;
                }

                return (int)finalValue;
            }, indentationLevel, isEnabled);
        }

        /// <summary>
        /// Make a text field that can also be populated by a file selector, accessible via an inline button.
        /// </summary>
        /// <param name="inputLabel">A string to display before the text field and file selector button.</param>
        /// <param name="currentValue">The current value of the text field.</param>
        /// <param name="filePanelTitle">The title of the file selection panel.</param>
        /// <param name="fileExtension">The file extension which can be selected.</param>
        /// <param name="indentationLevel">The level of indentation to place in front of the label.</param>
        /// <param name="isEnabled">When set to false, the input field will be greyed-out and non-interactable.</param>
        /// <param name="openingFile">Whether to open a file selection screen that opens existing files, or lets you enter the name of a non-existing file.</param>
        /// <returns>The new value of the text field.</returns>
        public static string FileSelection(string inputLabel, string currentValue, string filePanelTitle, string fileExtension = "*", int indentationLevel = 1, bool isEnabled = true, bool openingFile = true)
        {
            CustomField(inputLabel, () =>
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    currentValue = EditorGUILayout.TextField(currentValue);
                    if (GUILayout.Button(SELECT_FILE, GUILayout.Width(GUI.skin.button.CalcSize(new GUIContent(SELECT_FILE)).x + 16)))
                    {
                        if (openingFile)
                        {
                            currentValue = EditorUtility.OpenFilePanel(filePanelTitle, string.Empty, fileExtension);
                        }
                        else
                        {
                            currentValue = EditorUtility.SaveFilePanel(filePanelTitle, string.Empty, string.Empty, fileExtension);
                        }

                        // If the above TextField has focus before clicking the Select File button, it will not update after a file is selected. Forcing it to loose focus afterwards will force it to update.
                        GUI.FocusControl(null);
                    }
                };
            }, indentationLevel, isEnabled);

            return currentValue;
        }

        /// <summary>
        /// Make an empty prefix label.
        /// </summary>
        /// <remarks>
        /// Useful for aligning unlabeled fields with inputs.
        /// </remarks>
        public static void EmptyPrefixLabel()
        {
            PrefixLabel(EMPTY_PREFIX_SPACE);
        }

        /// <summary>
        /// Keep the previous <c>EditorGUILayout.PrefixLabel()</c> from getting disabled when the GUI element it labels is disabled.<br/><br/>
        ///
        /// This method must be called between <c>EditorGUILayout.PrefixLabel()</c> and the next <c>EditorGUILayout</c> or <c>GUILayout</c> function.
        /// See below for an example.<br/><br/>
        ///
        /// Explanation: PrefixLabels automatically get disabled when the GUI element they label is disabled.
        /// This happens even if the PrefixLabel itself is wrapped in a <c>using EditorGUI.DisabledScope(true){}</c> block.
        /// This method creates an invisible, zero-width label field between the PrefixLabel and the GUI element which it labels.
        /// The invisible label field is never disabled, so the PrefixLabel remains enabled.
        /// </summary>
        /// <example>
        /// This shows how to use this method to keep a prefix label from getting disabled.
        /// <code>
        ///     EditorGUILayout.PrefixLabel("Foo:");
        ///     EditorGUILayoutElements.KeepPreviousPrefixLabelEnabled();
        ///     using (new EditorGUI.DisabledScope(!_isFooInputFieldDisabled))
        ///     {
        ///         _currentFoo = EditorGUILayout.TextField(currentFoo);
        ///     }
        /// </code>
        /// </example>
        public static void KeepPreviousPrefixLabelEnabled()
        {
            // Surprisingly, Width(0f) is not actually zero width.
            EditorGUILayout.LabelField(string.Empty, new GUIStyle(), GUILayout.Width(-3f));
        }

        /// <summary>
        /// Draw an icon representing a feature's deployment status.
        /// </summary>
        /// <param name="deploymentStatus">The current deployment status of a feature.</param>
        public static void DeploymentStatusIcon(FeatureStatus deploymentStatus)
        {
            Texture icon = GetDeploymentStatusIcon(deploymentStatus);

            if (icon != null)
            {
                GUILayout.Box(icon, CommonGUIStyles.DeploymentStatusIcon);
            }
        }
        
        /// <summary>
        /// Draw an icon button for hard refreshing feature deployment statuses.
        /// </summary>
        /// <param name="isRefreshing">Indicates if feature refresh is in progress.</param>
        /// <remarks>A box will be rendered instead of a button if the refresh is in progress. During refresh the method will always return false.</remarks>
        /// <returns>True when the user clicks the button.</returns>
        public static bool DeploymentRefreshIconButton(bool isRefreshing)
        {
            Texture icon;

            if (isRefreshing)
            {
                icon = EditorResources.Textures.FeatureStatusWaiting.Get();
                GUILayout.Box(new GUIContent(icon, L10n.Tr("Refreshing deployment status of all features.")), CommonGUIStyles.RefreshIcon);
                return false;
            }

            icon = EditorResources.Textures.FeatureStatusRefresh.Get();
            return GUILayout.Button(new GUIContent(icon, L10n.Tr("Refresh deployment status of all features.")), CommonGUIStyles.RefreshIcon);
        }

        /// <summary>
        /// Displays a tooltip under the users mouse pointer when the mouse pointer is inside of the last element rendered before this call.
        /// </summary>
        /// <param name="message">Tooltip to display</param>
        public static void DrawToolTip(string message)
        {
            if (Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                Vector3 mousePos = Event.current.mousePosition;
                Vector2 size = GUI.skin.box.CalcSize(new GUIContent(message));
                GUI.Label(new Rect(mousePos.x - size.x, mousePos.y - size.y, size.x, size.y), message, SettingsGUIStyles.Tooltip.Text);
            }
        }

        internal static Texture GetDeploymentStatusIcon(FeatureStatus deploymentStatus)
        {
            switch (deploymentStatus)
            {
                case FeatureStatus.Undeployed:
                    // fall through
                case FeatureStatus.Unknown:
                    return null;

                case FeatureStatus.Deployed:
                    return EditorResources.Textures.FeatureStatusSuccess.Get();

                case FeatureStatus.Error:
                    // fall through
                case FeatureStatus.RollbackComplete:
                    return EditorResources.Textures.FeatureStatusError.Get();

                default:
                    return EditorResources.Textures.FeatureStatusWorking.Get();
            }
        }

        private static void DisplayPlaceholderTextOverLastField(string placeholderText)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), placeholderText, CommonGUIStyles.PlaceholderLabel);
            }
        }
    }
}
