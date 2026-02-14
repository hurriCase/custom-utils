using System;
using System.Collections.Generic;
using CustomUtils.Editor.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
namespace CustomUtils.Editor.Scripts.CustomEditorUtilities
{
    /// <summary>
    /// Enhanced GUI system for editor inspectors with automatic undo support.
    /// Uses Unity's standard editor styling for consistency with native Unity editors.
    /// </summary>
    [PublicAPI]
    public sealed class EditorStateControls
    {
        private readonly Object _target;
        private readonly SerializedObject _serializedObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorStateControls"/> class with a target object for undo support.
        /// </summary>
        /// <param name="target">The Unity object to track for undo operations. Must not be null.</param>
        /// <param name="serializedObject">The serialized object which is used for operation where it is required (Like FindField() (Optional)</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null.</exception>
        public EditorStateControls([NotNull] Object target, SerializedObject serializedObject = null)
        {
            _target = target;
            _serializedObject = serializedObject;
        }

        /// <summary>
        /// Creates a color field with Unity's standard styling and undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current color value.</param>
        /// <param name="useConsistentHeight">Whether to use Unity's single line height. Default is true.</param>
        /// <returns>The modified color value.</returns>
        [MustUseReturnValue]
        public Color ColorField(string label, Color value, bool useConsistentHeight = true) =>
            HandleValueChange(label, value, () =>
                useConsistentHeight
                    ? EditorGUILayout.ColorField(label, value, GUILayout.Height(EditorGUIUtility.singleLineHeight))
                    : EditorGUILayout.ColorField(label, value));

        /// <summary>
        /// Creates a gradient field with Unity's standard styling and undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current gradient value.</param>
        /// <param name="useConsistentHeight">Whether to use Unity's single line height. Default is true.</param>
        /// <returns>The modified gradient value.</returns>
        [MustUseReturnValue]
        public Gradient GradientField(string label, Gradient value, bool useConsistentHeight = true) =>
            HandleValueChange(label, value, () =>
                useConsistentHeight
                    ? EditorGUILayout.GradientField(label, value, GUILayout.Height(EditorGUIUtility.singleLineHeight))
                    : EditorGUILayout.GradientField(label, value));

        /// <summary>
        /// Creates a sprite field with Unity's standard styling and undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current sprite reference.</param>
        /// <param name="allowSceneObjects">Whether to allow scene objects to be assigned. Default is false.</param>
        /// <returns>The modified sprite reference.</returns>
        [MustUseReturnValue]
        public Sprite SpriteField(string label, Sprite value, bool allowSceneObjects = false) =>
            HandleValueChange(label, value, () =>
                (Sprite)EditorGUILayout.ObjectField(label, value, typeof(Sprite), allowSceneObjects,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f))); // Standard object field height

        /// <summary>
        /// Creates an object field with Unity's standard styling and undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current object reference.</param>
        /// <param name="type">The type of object that can be assigned to the field.</param>
        /// <param name="allowSceneObjects">Whether to allow scene objects to be assigned. Default is false.</param>
        /// <param name="expandWidth">Whether the field should expand to fill available width. Default is true.</param>
        /// <returns>The modified object reference.</returns>
        [MustUseReturnValue]
        public Object ObjectField(string label, Object value, Type type, bool allowSceneObjects = false,
            bool expandWidth = true) =>
            HandleValueChange(label, value, () =>
                EditorGUILayout.ObjectField(label, value, type, allowSceneObjects,
                    GUILayout.ExpandWidth(expandWidth)));

        /// <summary>
        /// Creates an object field with Unity's standard styling and undo support using the object's name as the label.
        /// </summary>
        /// <param name="value">The current object reference whose name will be used as the label.</param>
        /// <param name="type">The type of object that can be assigned to the field.</param>
        /// <param name="allowSceneObjects">Whether to allow scene objects to be assigned. Default is false.</param>
        /// <param name="expandWidth">Whether the field should expand to fill available width. Default is true.</param>
        /// <returns>The modified object reference.</returns>
        [MustUseReturnValue]
        public Object ObjectField(Object value, Type type, bool allowSceneObjects = false,
            bool expandWidth = true) =>
            HandleValueChange(value.name, value, () =>
                EditorGUILayout.ObjectField(value.name, value, type, allowSceneObjects,
                    GUILayout.ExpandWidth(expandWidth)));

        /// <summary>
        /// Creates a float field with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current float value.</param>
        /// <returns>The modified float value.</returns>
        [MustUseReturnValue]
        public float FloatField(string label, float value) =>
            HandleValueChange(label, value, () => EditorGUILayout.FloatField(label, value));

        /// <summary>
        /// Creates a float field with custom rect positioning and undo support.
        /// </summary>
        /// <param name="rect">The rect where the field should be drawn.</param>
        /// <param name="label">The label used for the undo operation.</param>
        /// <param name="value">The current float value.</param>
        /// <returns>The modified float value.</returns>
        [MustUseReturnValue]
        public float FloatField(Rect rect, string label, float value) =>
            HandleValueChangeWithRect(rect, label, value, EditorGUI.FloatField);

        /// <summary>
        /// Creates a float field with custom rect positioning, content label, and undo support.
        /// </summary>
        /// <param name="rect">The rect where the field should be drawn.</param>
        /// <param name="label">The label used for the undo operation.</param>
        /// <param name="content">The GUIContent to display with the field.</param>
        /// <param name="value">The current float value.</param>
        /// <returns>The modified float value.</returns>
        [MustUseReturnValue]
        public float FloatField(Rect rect, string label, GUIContent content, float value) =>
            HandleValueChangeWithRect(rect, label, value, (position, currentValue)
                => EditorGUI.FloatField(position, content, currentValue));

        /// <summary>
        /// Helper method that handles change detection and undo recording for GUI controls with custom rect positioning.
        /// </summary>
        /// <typeparam name="T">The type of value being modified.</typeparam>
        /// <param name="rect">The rect where the control should be drawn.</param>
        /// <param name="label">The label used for the undo operation.</param>
        /// <param name="currentValue">The current value before any changes.</param>
        /// <param name="guiMethod">The function that creates the GUI control with custom rect and returns its new value.</param>
        /// <returns>The current value if unchanged, or the new value if modified.</returns>
        [MustUseReturnValue]
        public T HandleValueChangeWithRect<T>(Rect rect, string label, T currentValue,
            Func<Rect, T, T> guiMethod)
        {
            EditorGUI.BeginChangeCheck();
            var newValue = guiMethod(rect, currentValue);
            if (EditorGUI.EndChangeCheck() is false)
                return currentValue;

            Undo.RecordObject(_target, $"Change {label}");
            return newValue;
        }

        /// <summary>
        /// Creates an int field with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current int value.</param>
        /// <returns>The modified int value.</returns>
        [MustUseReturnValue]
        public int IntField(string label, int value) =>
            HandleValueChange(label, value, () => EditorGUILayout.IntField(label, value));

        /// <summary>
        /// Creates an enum field with undo support.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current enum value.</param>
        /// <returns>The modified enum value.</returns>
        [MustUseReturnValue]
        public T EnumField<T>(string label, T value) where T : Enum =>
            HandleValueChange(label, value, () => (T)EditorGUILayout.EnumPopup(label, value));

        /// <summary>
        /// Creates an enum field with undo support.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The current enum value.</param>
        /// <returns>The modified enum value.</returns>
        [MustUseReturnValue]
        public T EnumField<T>(T value) where T : Enum =>
            HandleValueChange(nameof(T), value, () => (T)EditorGUILayout.EnumPopup(value));

        /// <summary>
        /// Creates a property field with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the property field.</param>
        /// <param name="property">The serialized property to modify.</param>
        /// <param name="includeChildren">Whether to include children of the property. Default is false.</param>
        /// <returns>True if the property was modified, false otherwise.</returns>
        public bool PropertyField(SerializedProperty property, string label, bool includeChildren = false)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(property, new GUIContent(label), includeChildren);

            if (EditorGUI.EndChangeCheck() is false)
                return false;

            Undo.RecordObject(_target, $"Change {label}");
            return true;
        }

        /// <summary>
        /// Creates a property field with undo support using the property's built-in label.
        /// </summary>
        /// <param name="property">The serialized property to modify.</param>
        /// <param name="includeChildren">Whether to include children of the property. Default is false.</param>
        /// <returns>True if the property was modified, false otherwise.</returns>
        public bool PropertyField(SerializedProperty property, bool includeChildren = false)
            => PropertyField(property, property.displayName, includeChildren);

        /// <summary>
        /// Creates a property field with undo support using property name
        /// </summary>
        /// <param name="propertyName">Name of the serialized property to modify</param>
        /// <param name="includeChildren">Whether to include children of the property</param>
        /// <returns>Tuple containing whether the property was modified and the property reference</returns>
        public (bool wasModified, SerializedProperty serializedProperty) PropertyField(
            string propertyName,
            bool includeChildren = false)
        {
            if (_serializedObject is null)
            {
                EditorVisualControls.WarningBox("Serialized Object isn't set.");
                return (false, null);
            }

            var property = _serializedObject.FindField(propertyName);
            var wasModified = PropertyField(property, property.displayName, includeChildren);
            return (wasModified, property);
        }

        /// <summary>
        /// Creates a property field with undo support that can be conditionally disabled
        /// </summary>
        /// <param name="isDisabled">Whether the property field should be disabled</param>
        /// <param name="propertyName">Name of the serialized property to modify</param>
        /// <param name="includeChildren">Whether to include children of the property</param>
        /// <returns>True if the property was modified, false otherwise</returns>
        public bool PropertyFieldIf(bool isDisabled, string propertyName, bool includeChildren = false)
        {
            if (_serializedObject is null)
            {
                EditorVisualControls.WarningBox("Serialized Object isn't set.");
                return false;
            }

            using var disabledScope = new EditorGUI.DisabledScope(isDisabled);

            var property = _serializedObject.FindField(propertyName);
            return PropertyField(property, property.displayName, includeChildren);
        }

        /// <summary>
        /// Draws the script property in the Inspector.
        /// This displays the "Script" field which shows the script file associated with the component.
        /// </summary>
        public void DrawScriptProperty()
        {
            if (_serializedObject is null)
            {
                EditorVisualControls.WarningBox("Serialized Object isn't set.");
                return;
            }

            GUI.enabled = false;
            PropertyField(_serializedObject.FindProperty("m_Script"));
            GUI.enabled = true;
        }

        /// <summary>
        /// Creates a dropdown with undo support using string value selection.
        /// </summary>
        /// <param name="label">The label to display next to the dropdown.</param>
        /// <param name="selectedValue">The currently selected string value.</param>
        /// <param name="options">List of option strings to display.</param>
        /// <param name="rect">The rect where the dropdown should be drawn.</param>
        /// <returns>The selected string value.</returns>
        [MustUseReturnValue]
        public string Dropdown(string label, string selectedValue, List<string> options, Rect rect = default)
        {
            var selectedIndex = options.IndexOf(selectedValue);
            if (selectedIndex == -1)
                selectedIndex = 0;

            var newIndex = Dropdown(label, selectedIndex, options, rect);
            return newIndex < options.Count ? options[newIndex] : options[0];
        }

        /// <summary>
        /// Creates a dropdown with undo support using string value selection.
        /// Automatically handles both SerializedProperty and regular value scenarios.
        /// </summary>
        /// <param name="property">The serialized property to modify.</param>
        /// <param name="options">List of option strings to display.</param>
        /// <param name="rect">The rect where the dropdown should be drawn.</param>
        /// <returns>The selected string value.</returns>
        public string Dropdown(SerializedProperty property, List<string> options, Rect rect = default)
        {
            var selectedIndex = options.IndexOf(property.stringValue);
            if (selectedIndex == -1)
                selectedIndex = 0;

            var newIndex = DrawDropdown(property.displayName, selectedIndex, options.ToArray(), rect);

            if (newIndex < 0)
                return string.Empty;

            if (newIndex >= options.Count)
                return options[newIndex];

            property.stringValue = options[newIndex];
            property.serializedObject.ApplyModifiedProperties();
            return options[newIndex];
        }

        /// <summary>
        /// Creates a dropdown with undo support using int value selection from an array of options.
        /// </summary>
        /// <param name="property">The serialized property to modify (must be int type).</param>
        /// <param name="options">Array of int options to display and select from.</param>
        /// <param name="optionNames">Display names for the options.</param>
        /// <param name="rect">The rect where the dropdown should be drawn.</param>
        /// <returns>The selected int value.</returns>
        public int Dropdown(SerializedProperty property, int[] options, string[] optionNames, Rect rect = default)
        {
            var currentIndex = Array.IndexOf(options, property.intValue);
            if (currentIndex == -1)
                currentIndex = 0;

            var newIndex = DrawDropdown(property.displayName, currentIndex, optionNames, rect);

            if (newIndex < 0 || newIndex >= options.Length)
                return property.intValue;

            property.intValue = options[newIndex];
            property.serializedObject.ApplyModifiedProperties();
            return options[newIndex];
        }

        /// <summary>
        /// Creates a dropdown with undo support using string value selection.
        /// </summary>
        /// <param name="label">The label to display next to the dropdown.</param>
        /// <param name="selectedValue">The currently selected string value.</param>
        /// <param name="options">Array of option strings to display.</param>
        /// <param name="rect">The rect where the dropdown should be drawn.</param>
        /// <returns>The selected string value.</returns>
        [MustUseReturnValue]
        public string Dropdown(string label, string selectedValue, string[] options, Rect rect = default)
        {
            var selectedIndex = Array.IndexOf(options, selectedValue);
            if (selectedIndex == -1)
                selectedIndex = 0;

            var newIndex = Dropdown(label, selectedIndex, options, rect);
            return newIndex < options.Length ? options[newIndex] : options[0];
        }

        /// <summary>
        /// Creates a dropdown with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the dropdown.</param>
        /// <param name="selectedIndex">The currently selected index.</param>
        /// <param name="options">List of option strings to display.</param>
        /// <param name="rect">The rect where the dropdown should be drawn.</param>
        /// <returns>The index of the selected option.</returns>
        [MustUseReturnValue]
        public int Dropdown(string label, int selectedIndex, List<string> options, Rect rect = default)
            => Dropdown(label, selectedIndex, options.ToArray(), rect);

        /// <summary>
        /// Creates a dropdown with undo support using Unity's standard popup styling.
        /// </summary>
        /// <param name="label">The label to display next to the dropdown.</param>
        /// <param name="selectedIndex">The currently selected index.</param>
        /// <param name="options">Array of option strings to display.</param>
        /// <param name="rect">The rect where the dropdown should be drawn.</param>
        /// <returns>The index of the selected option.</returns>
        [MustUseReturnValue]
        public int Dropdown(string label, int selectedIndex, string[] options, Rect rect = default)
        {
            var originalIndent = EditorGUI.indentLevel;

            var result = HandleValueChange(label, selectedIndex,
                () => DrawDropdown(label, selectedIndex, options, rect));

            EditorGUI.indentLevel = originalIndent;
            return result;
        }

        private int DrawDropdown(string label, int selectedIndex, string[] options, Rect rect = default)
        {
            if (rect == default)
                return EditorGUILayout.Popup(
                    label,
                    selectedIndex,
                    options,
                    EditorStyles.popup,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight)
                );

            return EditorGUI.Popup(
                rect,
                label,
                selectedIndex,
                options,
                EditorStyles.popup);
        }

        /// <summary>
        /// Creates a toggle button with undo support using Unity's standard button styling.
        /// </summary>
        /// <param name="label">The text to display on the button.</param>
        /// <param name="isSelected">Whether the button is currently selected.</param>
        /// <param name="highlightColor">Optional color to use when the button is selected. If null, use a default highlight color.</param>
        /// <returns>The newly selected state of the button.</returns>
        [MustUseReturnValue]
        public bool ToggleButton(string label, bool isSelected, Color? highlightColor = null)
        {
            var originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = isSelected
                ? highlightColor ?? Color.cyan
                : Color.white;

            EditorGUI.BeginChangeCheck();
            var clicked = GUILayout.Button(label, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));

            GUI.backgroundColor = originalBackgroundColor;

            if (EditorGUI.EndChangeCheck() is false || clicked is false)
                return isSelected;

            Undo.RecordObject(_target, $"Toggle {label}");
            return isSelected is false;
        }

        /// <summary>
        /// Creates a group of toggle buttons where one can be selected at a time.
        /// </summary>
        /// <param name="labels">Array of button labels to display.</param>
        /// <param name="selectedIndex">The currently selected button index.</param>
        /// <returns>The index of the newly selected button.</returns>
        [MustUseReturnValue]
        public int ToggleButtonGroup(string[] labels, int selectedIndex)
        {
            EditorGUILayout.BeginHorizontal();

            var newIndex = selectedIndex;

            for (var i = 0; i < labels.Length; i++)
            {
                EditorGUI.BeginChangeCheck();
                var isSelected = selectedIndex == i;
                var newIsSelected = ToggleButton(labels[i], isSelected);

                if (EditorGUI.EndChangeCheck() && newIsSelected && isSelected is false)
                    newIndex = i;
            }

            EditorGUILayout.EndHorizontal();

            return newIndex;
        }

        /// <summary>
        /// Creates a toggle with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the toggle.</param>
        /// <param name="value">The current toggle state.</param>
        /// <returns>The new toggle state.</returns>
        [MustUseReturnValue]
        public bool Toggle(string label, bool value) =>
            HandleValueChange(label, value, () => EditorGUILayout.Toggle(label, value));

        /// <summary>
        /// Creates two mutually exclusive toggles with undo support.
        /// </summary>
        /// <remarks>
        /// When one toggle is enabled, the other is automatically disabled.
        /// At least one toggle will always remain enabled.
        /// </remarks>
        /// <param name="toggle1">Reference to the first toggle's state.</param>
        /// <param name="toggle2">Reference to the second toggle's state.</param>
        /// <param name="label1">The label for the first toggle.</param>
        /// <param name="label2">The label for the second toggle.</param>
        public void ExclusiveToggles(ref bool toggle1, ref bool toggle2, string label1, string label2)
        {
            EditorGUI.BeginChangeCheck();

            var oldToggle1 = toggle1;
            var oldToggle2 = toggle2;

            EditorGUILayout.BeginHorizontal();

            var newToggle1 = EditorGUILayout.Toggle(new GUIContent(label1), toggle1);
            var newToggle2 = EditorGUILayout.Toggle(new GUIContent(label2), toggle2);

            EditorGUILayout.EndHorizontal();

            if (newToggle1 == oldToggle1 && newToggle2 == oldToggle2)
                return;

            if (EditorGUI.EndChangeCheck() is false)
                return;

            Undo.RecordObject(_target, $"Change {label1}/{label2} Selection");

            toggle1 = newToggle1;
            toggle2 = newToggle2;

            switch (newToggle1)
            {
                case true when newToggle2:
                    if (oldToggle1 == false)
                        toggle2 = false;
                    else
                        toggle1 = false;
                    break;
                case false when newToggle2 == false:
                    if (oldToggle1)
                        toggle1 = true;
                    else
                        toggle2 = true;
                    break;
            }
        }

        /// <summary>
        /// Creates a multi-line text area with undo support.
        /// </summary>
        /// <param name="label">The label to display above the text area.</param>
        /// <param name="value">The current text content.</param>
        /// <returns>The modified text content.</returns>
        [MustUseReturnValue]
        public string TextArea(string label, string value)
        {
            EditorGUILayout.LabelField(label);
            return HandleValueChange(label, value, () => EditorGUILayout.TextArea(value));
        }

        /// <summary>
        /// Creates a single-line text field with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">The current text content.</param>
        /// <returns>The modified text content.</returns>
        [MustUseReturnValue]
        public string TextField(string label, string value) =>
            HandleValueChange(label, value, () => EditorGUILayout.TextField(label, value));

        /// <summary>
        /// Creates a single-line text field with undo support using a reference parameter.
        /// </summary>
        /// <param name="label">The label to display next to the field.</param>
        /// <param name="value">Reference to the text content that will be modified directly.</param>
        public void TextField(string label, ref string value)
        {
            var cashedValue = value;
            var newValue =
                HandleValueChange(label, value, () => EditorGUILayout.TextField(label, cashedValue));

            value = newValue;
        }

        /// <summary>
        /// Creates a float slider with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the slider.</param>
        /// <param name="value">The current float value.</param>
        /// <param name="leftValue">The minimum value (left end of slider).</param>
        /// <param name="rightValue">The maximum value (right end of slider).</param>
        /// <returns>The modified float value.</returns>
        [MustUseReturnValue]
        public float Slider(string label, float value, float leftValue, float rightValue) =>
            HandleValueChange(label, value, () => EditorGUILayout.Slider(label, value, leftValue, rightValue));

        /// <summary>
        /// Creates an int slider with undo support.
        /// </summary>
        /// <param name="label">The label to display next to the slider.</param>
        /// <param name="value">The current int value.</param>
        /// <param name="leftValue">The minimum value (left end of slider).</param>
        /// <param name="rightValue">The maximum value (right end of slider).</param>
        /// <returns>The modified int value.</returns>
        [MustUseReturnValue]
        public int IntSlider(string label, int value, int leftValue, int rightValue) =>
            HandleValueChange(label, value, () => EditorGUILayout.IntSlider(label, value, leftValue, rightValue));

        /// <summary>
        /// Helper method that handles change detection and undo recording for GUI controls.
        /// </summary>
        /// <typeparam name="T">The type of value being modified.</typeparam>
        /// <param name="label">The label used for the undo operation.</param>
        /// <param name="currentValue">The current value before any changes.</param>
        /// <param name="guiMethod">The function that creates the GUI control and returns its new value.</param>
        /// <returns>The current value if unchanged, or the new value if modified.</returns>
        private T HandleValueChange<T>(string label, T currentValue, Func<T> guiMethod)
        {
            EditorGUI.BeginChangeCheck();
            var newValue = guiMethod();
            if (EditorGUI.EndChangeCheck() is false)
                return currentValue;

            Undo.RecordObject(_target, $"Change {label}");
            return newValue;
        }
    }
}