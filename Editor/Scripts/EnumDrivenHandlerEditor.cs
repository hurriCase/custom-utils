using System;
using System.Collections.Generic;
using System.Reflection;
using CustomUtils.Editor.Scripts.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace CustomUtils.Editor.Scripts
{
    /// <inheritdoc />
    /// <summary>
    /// A generic base editor for enum-based database ScriptableObjects.
    /// </summary>
    /// <typeparam name="TDatabase">The database type</typeparam>
    /// <typeparam name="TData">The data item type</typeparam>
    /// <typeparam name="TEnum">The enum type used for identifying data items</typeparam>
    [PublicAPI]
    public abstract class EnumDrivenHandlerEditor<TDatabase, TData, TEnum> : UnityEditor.Editor
        where TDatabase : ScriptableObject
        where TEnum : Enum
    {
        private SerializedProperty _dataListProperty;
        private TEnum _selectedEnumValue;
        private List<TEnum> _unusedValues = new();

        protected abstract string EnumFieldName { get; }

        protected abstract List<TEnum> GetUsedEnumValues(TDatabase database);

        private void AddNewItem(TEnum enumValue)
        {
            var sequentialIndex = GetSequentialEnumIndex(enumValue);

            _dataListProperty.arraySize++;
            var newElement = _dataListProperty.GetArrayElementAtIndex(_dataListProperty.arraySize - 1);

            var typeProperty = newElement.FindFieldRelative(EnumFieldName);
            typeProperty.enumValueIndex = sequentialIndex;

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void OnEnable()
        {
            _dataListProperty = serializedObject.FindProperty(GetListFieldName());

            UpdateUnusedEnumValues();

            if (_unusedValues.Count > 0)
                _selectedEnumValue = _unusedValues[0];
        }

        private void UpdateUnusedEnumValues()
        {
            var database = target as TDatabase;
            var usedValues = new HashSet<TEnum>(GetUsedEnumValues(database));

            _unusedValues = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .Where(value => !usedValues.Contains(value))
                .OrderBy(static value => Convert.ToInt32(value))
                .ToList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawCustomListProperty(_dataListProperty);

            EditorGUILayout.Space(10);

            DrawAddSpecificTypeSection();
            EditorGUILayout.Space(5);
            DrawAddAllMissingTypesButton();

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCustomListProperty(SerializedProperty listProperty)
        {
            EditorGUILayout.PropertyField(listProperty, new GUIContent(listProperty.displayName), false);

            if (!listProperty.isExpanded)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(listProperty.FindPropertyRelative("Array.size"));

            for (var i = 0; i < listProperty.arraySize; i++)
            {
                var elementProperty = listProperty.GetArrayElementAtIndex(i);
                var enumProperty = elementProperty.FindFieldRelative(EnumFieldName);

                var displayName = "Element " + i;

                if (enumProperty != null)
                {
                    var enumValue = GetEnumValueFromProperty(enumProperty);
                    if (enumValue != null)
                        displayName = enumValue.ToString();
                }

                EditorGUILayout.PropertyField(elementProperty, new GUIContent(displayName), true);
            }

            EditorGUI.indentLevel--;
        }

        private TEnum GetEnumValueFromProperty(SerializedProperty enumProperty)
        {
            try
            {
                var enumIntValue = enumProperty.enumValueIndex;
                var enumValues = Enum.GetValues(typeof(TEnum));

                if (enumIntValue >= 0 && enumIntValue < enumValues.Length)
                    return (TEnum)enumValues.GetValue(enumIntValue);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error getting enum value: {ex.Message}");
            }

            return default;
        }

        protected virtual void DrawAddSpecificTypeSection()
        {
            UpdateUnusedEnumValues();

            if (_unusedValues.Count == 0)
            {
                EditorGUILayout.HelpBox($"All {typeof(TEnum).Name} values are already added!", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            var selectedIndex = _unusedValues.IndexOf(_selectedEnumValue);
            if (selectedIndex < 0 && _unusedValues.Count > 0)
            {
                selectedIndex = 0;
                _selectedEnumValue = _unusedValues[0];
            }

            var displayOptions = _unusedValues.Select(static @enum => @enum.ToString())
                .ToArray();
            var newSelectedIndex = EditorGUILayout.Popup("Add Specific Type", selectedIndex, displayOptions);

            if (newSelectedIndex >= 0 && newSelectedIndex < _unusedValues.Count)
                _selectedEnumValue = _unusedValues[newSelectedIndex];

            if (GUILayout.Button("Add", GUILayout.Width(80)))
                if (_selectedEnumValue != null && _unusedValues.Contains(_selectedEnumValue))
                {
                    AddNewItem(_selectedEnumValue);
                    UpdateUnusedEnumValues();
                    if (_unusedValues.Count > 0)
                        _selectedEnumValue = _unusedValues[0];
                }

            EditorGUILayout.EndHorizontal();
        }

        protected virtual void DrawAddAllMissingTypesButton()
        {
            UpdateUnusedEnumValues();

            if (_unusedValues.Count == 0)
                return;

            var typeName = typeof(TEnum).Name.Replace("Type", "");
            if (!GUILayout.Button($"Add All Missing {typeName} Types ({_unusedValues.Count})"))
                return;

            foreach (var value in _unusedValues) AddNewItem(value);

            Debug.Log($"Added {_unusedValues.Count} missing {typeName} types");

            UpdateUnusedEnumValues();
        }

        private string GetListFieldName()
        {
            var currentType = typeof(TDatabase);

            while (currentType != null && currentType != typeof(object))
            {
                foreach (var fieldInfo in currentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var fieldType = fieldInfo.FieldType;
                    if (fieldType.IsGenericType &&
                        fieldType.GetGenericTypeDefinition() == typeof(List<>) &&
                        fieldType.GetGenericArguments()[0].IsAssignableFrom(typeof(TData)))
                        return fieldInfo.Name;
                }

                currentType = currentType.BaseType;
            }

            Debug.LogError("[EnumDrivenHandlerEditor::GetListFieldName] " +
                           $"Could not find List<{typeof(TData).Name}> field in {typeof(TDatabase).Name}");
            return null;
        }

        private int GetSequentialEnumIndex(TEnum enumValue)
        {
            var enumValues = Enum.GetValues(typeof(TEnum));
            return Array.IndexOf(enumValues, enumValue);
        }
    }
}