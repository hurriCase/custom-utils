using System;
using System.Collections.Generic;
using CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Formatter;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
namespace CustomUtils.Editor.Scripts.CustomEditorUtilities
{
    /// <summary>
    /// Static utility class for editor layout operations that don't require undo support.
    /// </summary>
    /// <remarks>
    /// Provides consistent styling and layout methods for creating uniform editor UI elements.
    /// All methods use Unity's default editor styles for consistency with native Unity editors.
    /// </remarks>
    [UsedImplicitly]
    public static class EditorVisualControls
    {
        /// <summary>
        /// Draws a section header with consistent styling.
        /// </summary>
        /// <param name="title">The title text to display in the header.</param>
        /// <remarks>
        /// Creates a header using Unity's default bold label style with standard spacing.
        /// </remarks>
        [UsedImplicitly]
        public static void DrawSectionHeader(string title)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>
        /// Creates a panel with consistent spacing around content.
        /// </summary>
        /// <param name="drawContent">Action to execute for drawing the panel content.</param>
        /// <remarks>
        /// Adds consistent spacing before and after the content using Unity's standard spacing.
        /// </remarks>
        [UsedImplicitly]
        public static void DrawPanel(Action drawContent)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            drawContent?.Invoke();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates a panel scope that can be used with the 'using' statement.
        /// </summary>
        /// <returns>A disposable panel scope.</returns>
        /// <remarks>
        /// Use this with the 'using' statement to create a panel with consistent spacing around content.
        /// Adds spacing before and after the content using Unity's standard spacing.
        /// </remarks>
        [UsedImplicitly]
        public static PanelScope BeginPanel() => new();

        /// <summary>
        /// Creates a property field with consistent styling.
        /// </summary>
        /// <param name="property">The serialized property to display.</param>
        /// <param name="label">The label text to display next to the property field.</param>
        /// <remarks>
        /// Uses Unity's standard single line height for property fields.
        /// </remarks>
        [UsedImplicitly]
        public static void DrawPropertyFieldWithLabel(SerializedProperty property, string label)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label),
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
        }

        /// <summary>
        /// Creates a group of property fields in a horizontal layout.
        /// </summary>
        /// <param name="properties">Array of tuples containing (property, label) pairs to display.</param>
        /// <remarks>
        /// Useful for displaying related properties side-by-side to save vertical space.
        /// </remarks>
        [UsedImplicitly]
        public static void HorizontalProperties(params (SerializedProperty property, string label)[] properties)
        {
            EditorGUILayout.BeginHorizontal();

            foreach (var (property, label) in properties)
                DrawPropertyFieldWithLabel(property, label);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a warning message box with consistent styling.
        /// </summary>
        /// <param name="message">The warning message to display.</param>
        /// <remarks>
        /// Adds consistent spacing before and after the warning box using Unity's standard spacing.
        /// </remarks>
        [UsedImplicitly]
        public static void WarningBox(string message)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates a warning message box with consistent styling at the specified position.
        /// </summary>
        /// <param name="position">The rect position where the warning box should be drawn.</param>
        /// <param name="message">The warning message to display.</param>
        /// <remarks>
        /// Uses EditorGUI for manual positioning instead of EditorGUILayout.
        /// Designed for use in PropertyDrawer contexts where position is manually controlled.
        /// </remarks>
        [UsedImplicitly]
        public static void WarningBox(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.Warning);
        }

        /// <summary>
        /// Creates an information message box with consistent styling.
        /// </summary>
        /// <param name="message">The information message to display.</param>
        /// <remarks>
        /// Adds consistent spacing before and after the info box using Unity's standard spacing.
        /// </remarks>
        [UsedImplicitly]
        public static void InfoBox(string message)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.HelpBox(message, MessageType.Info);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates an information message box with consistent styling at the specified position.
        /// </summary>
        /// <param name="position">The rect position where the info box should be drawn.</param>
        /// <param name="message">The information message to display.</param>
        /// <remarks>
        /// Uses EditorGUI for manual positioning instead of EditorGUILayout.
        /// Designed for use in PropertyDrawer contexts where position is manually controlled.
        /// </remarks>
        [UsedImplicitly]
        public static void InfoBox(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.Info);
        }

        /// <summary>
        /// Creates a plain help box with consistent styling without an icon.
        /// </summary>
        /// <param name="message">The help message to display.</param>
        /// <remarks>
        /// Adds consistent spacing before and after the help box using Unity's standard spacing.
        /// Uses MessageType.None to display a box without an icon.
        /// </remarks>
        [UsedImplicitly]
        public static void HelpBox(string message)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.HelpBox(message, MessageType.None);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates a plain help box with consistent styling at the specified position.
        /// </summary>
        /// <param name="position">The rect position where the help box should be drawn.</param>
        /// <param name="message">The help message to display.</param>
        /// <remarks>
        /// Uses EditorGUI for manual positioning instead of EditorGUILayout.
        /// Uses MessageType.None to display a box without an icon.
        /// Designed for use in PropertyDrawer contexts where position is manually controlled.
        /// </remarks>
        [UsedImplicitly]
        public static void HelpBox(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.None);
        }

        /// <summary>
        /// Creates an error message box with consistent styling.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        /// <remarks>
        /// Adds consistent spacing before and after the error box using Unity's standard spacing.
        /// </remarks>
        [UsedImplicitly]
        public static void ErrorBox(string message)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.HelpBox(message, MessageType.Error);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates an error message box with consistent styling at the specified position.
        /// </summary>
        /// <param name="position">The rect position where the error box should be drawn.</param>
        /// <param name="message">The error message to display.</param>
        /// <remarks>
        /// Uses EditorGUI for manual positioning instead of EditorGUILayout.
        /// Designed for use in PropertyDrawer contexts where position is manually controlled.
        /// </remarks>
        [UsedImplicitly]
        public static void ErrorBox(Rect position, string message)
        {
            EditorGUI.HelpBox(position, message, MessageType.Error);
        }

        /// <summary>
        /// Creates a boxed section with title and content.
        /// </summary>
        /// <param name="title">The title to display at the top of the box. Can be null or empty for no title.</param>
        /// <param name="drawContent">Action to execute for drawing the box content.</param>
        /// <remarks>
        /// Creates a visually distinct boxed area using Unity's help box style with standard padding and spacing.
        /// </remarks>
        [UsedImplicitly]
        public static void DrawBoxedSection(string title, Action drawContent)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            }

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            drawContent();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates a boxed scope that can be used with the 'using' statement.
        /// </summary>
        /// <param name="title">Optional title to display at the top of the box.</param>
        /// <returns>A disposable boxed scope.</returns>
        /// <remarks>
        /// Use this with the 'using' statement to create a boxed section with clearly defined boundaries.
        /// The box will use Unity's standard help box styling with consistent padding and spacing.
        /// </remarks>
        [UsedImplicitly]
        public static BoxedScope BeginBoxedSection(string title = null) => new(title);

        /// <summary>
        /// Creates a section scope that can be used with the 'using' statement.
        /// </summary>
        /// <param name="title">Title of the section to display in the header.</param>
        /// <returns>A disposable section scope.</returns>
        /// <remarks>
        /// Use this with the 'using' statement to create a section with clearly defined boundaries.
        /// </remarks>
        [UsedImplicitly]
        public static SectionScope BeginSection(string title)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            return new SectionScope(title);
        }

        /// <summary>
        /// Creates a foldout control with Unity's standard foldout styling.
        /// </summary>
        /// <param name="title">The title to display next to the foldout arrow.</param>
        /// <param name="showFoldout">Current expanded state of the foldout.</param>
        /// <param name="drawContent">Action to execute for drawing the content when the foldout is expanded.</param>
        /// <param name="toggleOnLabelClick">Whether clicking the label toggles the foldout state (true) or only the arrow does (false).</param>
        /// <param name="foldoutStyle">The GUIStyle to use for rendering the foldout control.</param>
        /// <param name="enableSaving">Whether to automatically save and restore the foldout state using EditorPrefs.</param>
        /// <returns>The new expanded state of the foldout.</returns>
        /// <remarks>
        /// This method applies Unity's standard foldout styling for consistency with native editors.
        /// When enableSaving is true, the foldout state is automatically saved to and restored from EditorPrefs using the title as a key.
        /// </remarks>
        [UsedImplicitly]
        public static bool Foldout(string title, ref bool showFoldout, Action drawContent,
            bool toggleOnLabelClick = true, GUIStyle foldoutStyle = null, bool enableSaving = true)
        {
            if (enableSaving)
                showFoldout = EditorPrefs.GetBool($"Foldout_{title}", showFoldout);

            showFoldout = EditorGUILayout.Foldout(showFoldout, title, toggleOnLabelClick,
                foldoutStyle ?? EditorStyles.foldout);
            if (showFoldout)
                drawContent.Invoke();

            if (enableSaving)
                EditorPrefs.SetBool($"Foldout_{title}", showFoldout);

            return showFoldout;
        }

        /// <summary>
        /// Creates a boxed section with a foldout header for collapsible content.
        /// </summary>
        /// <param name="title">The title to display in the foldout header.</param>
        /// <param name="showFoldout">Reference to a boolean that tracks the expanded/collapsed state.</param>
        /// <param name="drawContent">Action to execute for drawing the box content when expanded.</param>
        /// <param name="withFoldout">Whether to use foldout behavior (currently unused parameter).</param>
        /// <param name="enableSaving">Whether to automatically save and restore the foldout state using EditorPrefs.</param>
        /// <remarks>
        /// Creates a collapsible section using Unity's standard help box and foldout styles.
        /// When enableSaving is true, the foldout state is automatically saved to and restored from EditorPrefs using the title as a key.
        /// </remarks>
        [UsedImplicitly]
        public static void DrawBoxWithFoldout(string title, ref bool showFoldout, Action drawContent,
            bool withFoldout = true, bool enableSaving = true)
        {
            if (enableSaving)
                showFoldout = EditorPrefs.GetBool($"BoxFoldout_{title}", showFoldout);

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);

            EditorGUI.indentLevel++;
            Foldout(title, ref showFoldout, drawContent, enableSaving: false);
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);

            if (enableSaving)
                EditorPrefs.SetBool($"BoxFoldout_{title}", showFoldout);
        }

        /// <summary>
        /// Creates a foldout section with styled header and expandable content with automatic state saving.
        /// </summary>
        /// <param name="title">The title text to display in the foldout header.</param>
        /// <param name="showFoldouts">List of boolean values tracking the expanded state of multiple foldouts.</param>
        /// <param name="index">The index in the showFoldouts list for this specific foldout.</param>
        /// <param name="drawContent">Action to execute when the foldout is expanded.</param>
        /// <param name="toggleOnLabelClick">Whether clicking on the label toggles the foldout state.</param>
        /// <param name="enableSaving">Whether to automatically save and restore the foldout state using EditorPrefs.</param>
        /// <returns>The updated list of foldout states.</returns>
        /// <remarks>
        /// Uses Unity's standard foldout styling for consistency across the editor interface.
        /// When enableSaving is true, the foldout state is automatically saved to and restored from EditorPrefs using the title and index as a key.
        /// </remarks>
        [UsedImplicitly]
        public static List<bool> Foldout(string title, in List<bool> showFoldouts, int index, Action drawContent,
            bool toggleOnLabelClick = true, bool enableSaving = true)
        {
            var showDetail = showFoldouts.GetOrCreate(index);

            if (enableSaving)
                showDetail = EditorPrefs.GetBool($"ListFoldout_{title}_{index}", showDetail);

            showDetail = EditorGUILayout.Foldout(showDetail, title, toggleOnLabelClick, EditorStyles.foldout);
            if (showDetail)
                drawContent.Invoke();

            showFoldouts[index] = showDetail;

            if (enableSaving)
                EditorPrefs.SetBool($"ListFoldout_{title}_{index}", showDetail);

            return showFoldouts;
        }

        /// <summary>
        /// Creates a foldout control with Unity's standard foldout styling at a specific position.
        /// </summary>
        /// <param name="foldoutRect">The rect where the foldout control should be drawn.</param>
        /// <param name="title">The title to display next to the foldout arrow.</param>
        /// <param name="showFoldout">Reference to the current expanded state of the foldout.</param>
        /// <param name="drawContent">Action to execute for drawing the content when the foldout is expanded.</param>
        /// <param name="toggleOnLabelClick">Whether clicking the label toggles the foldout state (true) or only the arrow does (false).</param>
        /// <param name="foldoutStyle">The GUIStyle to use for rendering the foldout control.</param>
        /// <param name="enableSaving">Whether to automatically save and restore the foldout state using EditorPrefs.</param>
        /// <returns>The new expanded state of the foldout.</returns>
        /// <remarks>
        /// Uses EditorGUI for manual positioning instead of EditorGUILayout. When enableSaving is true,
        /// the foldout state is automatically saved to and restored from EditorPrefs using the title as a key.
        /// This method applies Unity's standard foldout styling for consistency with native editors.
        /// </remarks>
        [UsedImplicitly]
        public static bool Foldout(Rect foldoutRect, string title, ref bool showFoldout, Action drawContent,
            bool toggleOnLabelClick = true, GUIStyle foldoutStyle = null, bool enableSaving = true)
        {
            if (enableSaving)
                showFoldout = EditorPrefs.GetBool($"Foldout_{title}", showFoldout);

            showFoldout = EditorGUI.Foldout(foldoutRect, showFoldout, title, toggleOnLabelClick,
                foldoutStyle ?? EditorStyles.foldout);
            if (showFoldout)
                drawContent.Invoke();

            if (enableSaving)
                EditorPrefs.SetBool($"Foldout_{title}", showFoldout);

            return showFoldout;
        }

        /// <summary>
        /// Creates a standard button with consistent styling.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="options">Optional GUILayout options for customizing button appearance and behavior.</param>
        /// <returns>True if the button was clicked in this frame, false otherwise.</returns>
        [UsedImplicitly]
        public static bool Button(string text, params GUILayoutOption[] options) => GUILayout.Button(text, options);

        /// <summary>
        /// Creates a button that executes an action when clicked.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="drawContent">Action to execute when the button is clicked. Can be null.</param>
        /// <param name="options">Optional GUILayout options for customizing button appearance and behavior.</param>
        [UsedImplicitly]
        public static void Button(string text, Action drawContent, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(text, options))
                drawContent?.Invoke();
        }

        /// <summary>
        /// Creates a standard button with consistent styling.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="buttonStyle">The GUIStyle to use for rendering the button.</param>
        [UsedImplicitly]
        public static bool Button(string text, GUIStyle buttonStyle) => GUILayout.Button(text, buttonStyle);

        /// <summary>
        /// Creates a button that executes an action when clicked.
        /// </summary>
        /// <param name="text">The text to display on the button.</param>
        /// <param name="drawContent">Action to execute when the button is clicked. Can be null.</param>
        /// <param name="buttonStyle">The GUIStyle to use for rendering the button.</param>
        [UsedImplicitly]
        public static void Button(string text, Action drawContent, GUIStyle buttonStyle)
        {
            if (GUILayout.Button(text, buttonStyle))
                drawContent?.Invoke();
        }

        /// <summary>
        /// Creates a primary heading (H1) with consistent styling.
        /// </summary>
        /// <param name="text">The text to display as a heading.</param>
        /// <remarks>
        /// This method applies Unity's large label style for main section titles or page headers.
        /// </remarks>
        [UsedImplicitly]
        public static void H1Label(string text)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.5f);
            EditorGUILayout.LabelField(text, EditorStyles.largeLabel);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.25f);
        }

        /// <summary>
        /// Creates a secondary heading (H2) with consistent styling.
        /// </summary>
        /// <param name="text">The text to display as a heading.</param>
        /// <remarks>
        /// This method applies Unity's bold label style for section headers within a larger content area.
        /// </remarks>
        [UsedImplicitly]
        public static void H2Label(string text)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.25f);
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.125f);
        }

        /// <summary>
        /// Creates a tertiary heading (H3) with consistent styling.
        /// </summary>
        /// <param name="text">The text to display as a heading.</param>
        /// <remarks>
        /// This method applies Unity's mini bold label style for subsection headers or group titles.
        /// </remarks>
        [UsedImplicitly]
        public static void H3Label(string text)
        {
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.125f);
            EditorGUILayout.LabelField(text, EditorStyles.miniBoldLabel);
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight * 0.0625f);
        }

        /// <summary>
        /// Creates a standard label with Unity's default label styling.
        /// </summary>
        /// <param name="text">The text to display in the label.</param>
        /// <remarks>
        /// This method applies Unity's standard label styling for regular text content.
        /// </remarks>
        [UsedImplicitly]
        public static void LabelField(string text)
        {
            EditorGUILayout.LabelField(text, EditorStyles.label);
        }

        /// <summary>
        /// Creates a standard label with Unity's default label styling.
        /// </summary>
        /// <param name="image">The texture to display on the label.</param>
        /// <remarks>
        /// This method applies Unity's standard label styling for image content.
        /// </remarks>
        [UsedImplicitly]
        public static void Label(Texture image)
        {
            GUILayout.Label(image, EditorStyles.label);
        }

        /// <summary>
        /// Creates a standard label with Unity's default label styling and custom options.
        /// </summary>
        /// <param name="image">The texture to display on the label.</param>
        /// <param name="options">Additional layout options to apply to the label.</param>
        /// <remarks>
        /// This method applies Unity's standard label styling with custom layout options.
        /// </remarks>
        [UsedImplicitly]
        public static void Label(Texture image, params GUILayoutOption[] options)
        {
            GUILayout.Label(image, EditorStyles.label, options);
        }

        /// <summary>
        /// Creates a standard label with custom styling and layout options.
        /// </summary>
        /// <param name="image">The texture to display on the label.</param>
        /// <param name="style">Additional style to apply to the label</param>
        /// <param name="options">Additional layout options to apply to the label.</param>
        /// <remarks>
        /// This method applies custom styling with layout options for specialized use cases.
        /// </remarks>
        [UsedImplicitly]
        public static void Label(Texture image, GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.Label(image, style, options);
        }

        /// <summary>
        /// Creates a standard label with custom styling.
        /// </summary>
        /// <param name="text">The text to display in the label.</param>
        /// <param name="style">Style to apply to the label.</param>
        /// <remarks>
        /// This method applies custom styling for specialized use cases.
        /// </remarks>
        [UsedImplicitly]
        public static void LabelField(string text, GUIStyle style)
        {
            EditorGUILayout.LabelField(text, style);
        }

        /// <summary>
        /// Creates a label field with custom rect positioning.
        /// </summary>
        /// <param name="rect">The rect where the label should be drawn.</param>
        /// <param name="text">The text to display in the label.</param>
        /// <param name="style">Optional style for the label. If null, uses EditorStyles.label.</param>
        [UsedImplicitly]
        public static void LabelField(Rect rect, string text, GUIStyle style = null)
        {
            EditorGUI.LabelField(rect, text, style ?? EditorStyles.label);
        }

        /// <summary>
        /// Creates a standard label with Unity's default styling and layout options.
        /// </summary>
        /// <param name="text">The text to display in the label.</param>
        /// <param name="option">Layout option to apply to the label.</param>
        /// <remarks>
        /// This method applies Unity's standard label styling with custom layout options.
        /// </remarks>
        [UsedImplicitly]
        public static void LabelField(string text, GUILayoutOption option)
        {
            EditorGUILayout.LabelField(text, EditorStyles.label, option);
        }

        /// <summary>
        /// Creates a selectable label that wraps text and automatically adjusts its height based on content.
        /// </summary>
        /// <param name="text">The text to display in the selectable label.</param>
        /// <param name="style">The base style to use for the label. If null, EditorStyles.label will be used.</param>
        [UsedImplicitly]
        public static void DrawWrappedSelectableLabel(string text, GUIStyle style = null)
        {
            style = style ?? EditorStyles.label;

            var wrappedStyle = new GUIStyle(style)
            {
                wordWrap = true
            };

            var height = wrappedStyle.CalcHeight(new GUIContent(text), EditorGUIUtility.currentViewWidth);

            EditorGUILayout.SelectableLabel(text, wrappedStyle, GUILayout.Height(height));
        }

        /// <summary>
        /// Creates a read-only float field with Unity's standard styling.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The float value to display.</param>
        /// <remarks>
        /// This method temporarily disables GUI interaction to create a non-editable float field
        /// that maintains Unity's standard visual appearance. The field appears grayed out to
        /// indicate its read-only state.
        /// </remarks>
        [UsedImplicitly]
        public static void ReadOnlyFloatField(string label, float value)
        {
            var originalEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUILayout.FloatField(label, value);
            GUI.enabled = originalEnabled;
        }

        /// <summary>
        /// Creates a read-only float field with custom rect positioning and Unity's standard styling.
        /// </summary>
        /// <param name="rect">The rect where the field should be drawn.</param>
        /// <param name="value">The float value to display.</param>
        /// <remarks>
        /// This method temporarily disables GUI interaction to create a non-editable float field
        /// with precise positioning control. The field appears grayed out to indicate its read-only state.
        /// </remarks>
        [UsedImplicitly]
        public static void ReadOnlyFloatField(Rect rect, float value)
        {
            var originalEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.FloatField(rect, value);
            GUI.enabled = originalEnabled;
        }

        /// <summary>
        /// Creates a scrollable area for the content defined in the action.
        /// Uses the 'using' pattern for automatic cleanup.
        /// </summary>
        /// <param name="scrollPosition">Reference to the scroll position Vector2.</param>
        /// <returns>A disposable scroll scope that handles Begin/End scroll view pairs.</returns>
        [UsedImplicitly]
        public static ScrollScope CreateScrollView(ref Vector2 scrollPosition) => new(ref scrollPosition);

        /// <summary>
        /// Creates a horizontal group and executes the provided action within it.
        /// </summary>
        /// <param name="drawContent">Action to execute inside the horizontal group.</param>
        /// <param name="options">Optional GUILayout options.</param>
        [UsedImplicitly]
        public static void DrawHorizontalGroup(Action drawContent, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            drawContent?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a horizontal group with a specified style and executes the provided action within it.
        /// </summary>
        /// <param name="style">The GUIStyle to use for the horizontal group.</param>
        /// <param name="drawContent">Action to execute inside the horizontal group.</param>
        /// <param name="options">Optional GUILayout options.</param>
        [UsedImplicitly]
        public static void DrawHorizontalGroup(GUIStyle style, Action drawContent, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(style, options);
            drawContent?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a horizontal group within a box and executes the provided action within it.
        /// </summary>
        /// <param name="drawContent">Action to execute inside the horizontal group.</param>
        /// <param name="options">Optional GUILayout options.</param>
        [UsedImplicitly]
        public static void DrawHorizontalBoxGroup(Action drawContent, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox, options);
            drawContent?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a disposable horizontal group scope for use with 'using' statements.
        /// </summary>
        /// <param name="options">Optional GUILayout options.</param>
        /// <returns>A disposable scope object.</returns>
        [UsedImplicitly]
        public static HorizontalScope CreateHorizontalGroup(params GUILayoutOption[] options) => new(options);

        /// <summary>
        /// Creates a disposable horizontal group scope for use with 'using' statements.
        /// </summary>
        /// <param name="style">The GUIStyle to use for the horizontal group.</param>
        /// <param name="options">Optional GUILayout options.</param>
        /// <returns>A disposable scope object.</returns>
        [UsedImplicitly]
        public static HorizontalScope CreateHorizontalGroup(GUIStyle style, params GUILayoutOption[] options)
            => new(style, options);

        /// <summary>
        /// Creates a horizontal line (divider) with Unity's standard styling.
        /// </summary>
        /// <remarks>
        /// Adds a visual separator between UI elements using Unity's standard divider appearance.
        /// </remarks>
        [UsedImplicitly]
        public static void DrawHorizontalLine()
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            var rect = EditorGUILayout.GetControlRect(false, 1f);
            rect.height = 1f;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1f));

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }

        /// <summary>
        /// Creates a text style based on Unity's editor styles.
        /// </summary>
        /// <param name="baseStyle">The base GUIStyle to clone and modify.</param>
        /// <param name="fontSize">Optional custom font size. If null, uses the base style's size.</param>
        /// <param name="fontStyle">Optional custom font style (bold, italic, etc). If null, uses the base style.</param>
        /// <param name="alignment">Optional custom text alignment. If null, uses the base style's alignment.</param>
        /// <returns>A new GUIStyle instance with the specified modifications.</returns>
        /// <remarks>
        /// This utility method creates text styles based on Unity's built-in editor styles.
        /// </remarks>
        [UsedImplicitly]
        public static GUIStyle CreateTextStyle(GUIStyle baseStyle, int? fontSize = null, FontStyle? fontStyle = null,
            TextAnchor? alignment = null)
        {
            var style = new GUIStyle(baseStyle);

            if (fontSize.HasValue)
                style.fontSize = fontSize.Value;

            if (fontStyle.HasValue)
                style.fontStyle = fontStyle.Value;

            if (alignment.HasValue)
                style.alignment = alignment.Value;

            return style;
        }

        /// <summary>
        /// Creates a box style using Unity's help box style with custom padding.
        /// </summary>
        /// <param name="paddingLeft">Left padding in pixels.</param>
        /// <param name="paddingRight">Right padding in pixels.</param>
        /// <param name="paddingTop">Top padding in pixels.</param>
        /// <param name="paddingBottom">Bottom padding in pixels.</param>
        /// <returns>A new GUIStyle instance configured for box display.</returns>
        /// <remarks>
        /// This utility method creates box styles based on Unity's help box style.
        /// </remarks>
        [UsedImplicitly]
        public static GUIStyle CreateBoxStyle(int paddingLeft, int paddingRight, int paddingTop, int paddingBottom)
        {
            var style = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(
                    paddingLeft,
                    paddingRight,
                    paddingTop,
                    paddingBottom)
            };

            return style;
        }

        /// <summary>
        /// Creates a drag area that accepts Unity Objects.
        /// </summary>
        /// <param name="height">Height of the drop area in pixels.</param>
        /// <param name="message">Message to display in the drop area.</param>
        /// <param name="droppedObject"></param>
        /// <returns>True if objects were dropped in this frame, false otherwise.</returns>
        /// <remarks>
        /// Creates a visual drop area where users can drag Unity Objects.
        /// When objects are dropped, the provided callback is invoked with the list of objects.
        /// </remarks>
        [UsedImplicitly]
        public static bool DrawObjectDropArea(float height, string message, in List<Object> droppedObject)
        {
            var dropArea = GUILayoutUtility.GetRect(0.0f, height, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, message);

            var currentEditorEvent = Event.current;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (currentEditorEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (dropArea.Contains(currentEditorEvent.mousePosition) is false)
                        break;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (currentEditorEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        if (DragAndDrop.objectReferences.Length > 0)
                        {
                            foreach (var draggedObject in DragAndDrop.objectReferences)
                            {
                                if (draggedObject && droppedObject.Contains(draggedObject) is false)
                                    droppedObject.Add(draggedObject);
                            }

                            return true;
                        }
                    }

                    currentEditorEvent.Use();
                    break;
            }

            return false;
        }

        /// <summary>
        /// Draws a progress bar with title and progress value
        /// </summary>
        /// <param name="title">Title of the progress operation</param>
        /// <param name="info">Additional information to display</param>
        /// <param name="progress">Progress value between 0 and 1</param>
        [UsedImplicitly]
        public static void DrawProgressBar(string title, string info, float progress)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

            if (string.IsNullOrEmpty(info) is false)
                EditorGUILayout.LabelField(info, EditorStyles.miniLabel);

            var progressRect = EditorGUILayout.GetControlRect(false, 20);
            var progressText = StringFormatter.Concat(Mathf.RoundToInt(progress * 100), "%");
            EditorGUI.ProgressBar(progressRect, Mathf.Clamp01(progress), progressText);

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Creates a clickable text label with hover cursor indication that executes an action when clicked.
        /// Text automatically wraps to multiple lines if it exceeds the available width.
        /// </summary>
        /// <param name="text">The text to display as a clickable label.</param>
        /// <param name="onClick">Action to execute when the text is clicked. Can be null.</param>
        /// <returns>True if the text was clicked, false otherwise.</returns>
        /// <remarks>
        /// The text appears as a standard label with hand cursor on hover and executes the provided action when clicked.
        /// Uses Unity's standard label styling with enhanced user interaction feedback.
        /// Text automatically wraps to new lines when it exceeds the available width, and the control height adjusts accordingly.
        /// This is a wrapper around the URL-opening version that provides more flexibility for custom actions.
        /// </remarks>
        [UsedImplicitly]
        public static bool ClickableTextWithCursor(string text, Action onClick)
        {
            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                wordWrap = true
            };

            var content = new GUIContent(text);
            var availableWidth = EditorGUIUtility.currentViewWidth - GUI.skin.box.padding.horizontal;
            var height = labelStyle.CalcHeight(content, availableWidth);

            var rect = GUILayoutUtility.GetRect(content, labelStyle, GUILayout.Height(height));

            if (GUI.Button(rect, text, labelStyle))
            {
                onClick?.Invoke();
                return true;
            }

            if (rect.Contains(Event.current.mousePosition))
                EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            return false;
        }

        /// <summary>
        /// Creates a clickable text label with hover cursor indication that opens a URL when clicked.
        /// Text automatically wraps to multiple lines if it exceeds the available width.
        /// </summary>
        /// <param name="text">The text to display as a clickable label.</param>
        /// <param name="url">The URL to open when the text is clicked.</param>
        /// <returns>True if the text was clicked, false otherwise.</returns>
        /// <remarks>
        /// The text appears as a standard label with hand cursor on hover and opens the URL when clicked.
        /// Uses Unity's standard label styling with enhanced user interaction feedback.
        /// Text automatically wraps to new lines when it exceeds the available width, and the control height adjusts accordingly.
        /// This is a convenience wrapper around the action-based version for URL opening specifically.
        /// </remarks>
        [UsedImplicitly]
        public static bool ClickableTextWithCursor(string text, string url)
            => ClickableTextWithCursor(text, () => Application.OpenURL(url));

        /// <summary>
        /// Creates a color field with Unity's standard styling.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current color value.</param>
        /// <param name="useConsistentHeight">Whether to use Unity's single line height. Default is true.</param>
        [UsedImplicitly]
        public static void ColorField(string label, Color value, bool useConsistentHeight = true) =>
            EditorGUILayout.ColorField(label, value, useConsistentHeight
                ? GUILayout.Height(EditorGUIUtility.singleLineHeight)
                : null);

        /// <summary>
        /// Creates a color field with Unity's standard styling.
        /// </summary>
        /// <param name="rect">The rect where the field should be drawn.</param>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current color value.</param>
        /// <param name="useConsistentHeight">Whether to use Unity's single line height. Default is true.</param>
        [UsedImplicitly]
        public static void ColorField(Rect rect, string label, Color value, bool useConsistentHeight = true) =>
            EditorGUI.ColorField(rect, label, value);

        /// <summary>
        /// Creates a gradient field with Unity's standard styling.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current gradient value.</param>
        /// <param name="useConsistentHeight">Whether to use Unity's single line height. Default is true.</param>
        [UsedImplicitly]
        public static void GradientField(string label, Gradient value, bool useConsistentHeight = true) =>
            EditorGUILayout.GradientField(label, value, useConsistentHeight
                ? GUILayout.Height(EditorGUIUtility.singleLineHeight)
                : null);

        /// <summary>
        /// Creates a gradient field with Unity's standard styling.
        /// </summary>
        /// <param name="rect">The rect where the field should be drawn.</param>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current gradient value.</param>
        /// <param name="useConsistentHeight">Whether to use Unity's single line height. Default is true.</param>
        [UsedImplicitly]
        public static void GradientField(Rect rect, string label, Gradient value, bool useConsistentHeight = true) =>
            EditorGUI.GradientField(rect, label, value);
    }
}