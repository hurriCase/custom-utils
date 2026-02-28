using System;
using System.Collections.Generic;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.CustomTypes;
using CustomUtils.Runtime.CustomTypes.Collections;
using CustomUtils.Runtime.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts
{
    [CustomPropertyDrawer(typeof(EnumArray<,>))]
    public class EnumArrayDrawer : PropertyDrawer
    {
        private SerializedProperty _entriesProperty;
        private SerializedProperty _rootProperty;
        private VisualElement _container;

        private readonly List<PropertyField> _entries = new();
        private string[] _enumNames;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (!fieldInfo.FieldType.TryGetEnumType(out var enumType))
                return null;

            _rootProperty = property;
            _enumNames = enumType.GetDistinctEnumNames();
            _entriesProperty = property.FindFieldRelative(nameof(EnumArray<NoneEnum, object>.Entries));

            EnsureSize();

            _container = new Foldout { text = preferredLabel, viewDataKey = preferredLabel };

            _container.AddManipulator(new ContextualMenuManipulator(AddActions));
            _container.RegisterCallback<PointerDownEvent>(HandleAltKey);

            CreateEntries(_container);

            return _container;
        }

        private void AddActions(ContextualMenuPopulateEvent menuEvent)
        {
            menuEvent.menu.AppendAction("Copy", _ => _rootProperty.CopyTo());

            var pasteStatus = SerializedPropertyClipboard.CanPaste()
                ? DropdownMenuAction.Status.Normal
                : DropdownMenuAction.Status.Disabled;

            menuEvent.menu.AppendAction("Paste", _ => PasteValues(), pasteStatus);
            menuEvent.menu.AppendAction("Reset", _ => ResetValues());
        }

        private void ResetValues()
        {
            _rootProperty.serializedObject.Update();

            for (var i = 0; i < _entriesProperty.arraySize; i++)
            {
                var entryProperty = _entriesProperty.GetArrayElementAtIndex(i);
                entryProperty.boxedValue = Activator.CreateInstance(entryProperty.boxedValue.GetType());
            }

            _rootProperty.serializedObject.ApplyModifiedProperties();

            RebindEntries();
        }

        private void CreateEntries(VisualElement container)
        {
            for (var i = 0; i < _entriesProperty.arraySize; i++)
            {
                var entryProperty = _entriesProperty.GetArrayElementAtIndex(i);
                var valueProperty = entryProperty.FindFieldRelative(nameof(Entry<object>.Value));

                var propertyField = new PropertyField(valueProperty, _enumNames[i]);

                _entries.Add(propertyField);
                container.Add(propertyField);
            }
        }

        private void RebindEntries()
        {
            for (var i = 0; i < _entries.Count && i < _entriesProperty.arraySize; i++)
            {
                var entryProperty = _entriesProperty.GetArrayElementAtIndex(i);
                var valueProperty = entryProperty.FindFieldRelative(nameof(Entry<object>.Value));

                _entries[i].BindProperty(valueProperty);
            }
        }

        private void EnsureSize()
        {
            var namesSize = _enumNames.Length;
            if (_entriesProperty.arraySize == namesSize)
                return;

            _entriesProperty.arraySize = namesSize;

            _entriesProperty.serializedObject.ApplyModifiedProperties();
            _entriesProperty.serializedObject.Update();
        }

        private void PasteValues()
        {
            _rootProperty.Paste();
            _rootProperty.serializedObject.ApplyModifiedProperties();
        }

        private void HandleAltKey(PointerDownEvent pointerEvent)
        {
            if (!pointerEvent.altKey)
                return;

            if (!_container.TryQ<Foldout>(out var toggle))
                return;

            var shouldExpand = !toggle.value;
            toggle.value = shouldExpand;

            foreach (var entry in _entries)
                SetFoldoutState(entry, shouldExpand);
        }

        private void SetFoldoutState(VisualElement element, bool expanded)
        {
            var foldouts = element.Query<Foldout>().ToList();
            foreach (var foldout in foldouts)
                foldout.value = expanded;
        }
    }
}