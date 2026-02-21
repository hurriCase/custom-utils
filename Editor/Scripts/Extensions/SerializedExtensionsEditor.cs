using System;
using System.Reflection;
using CustomUtils.Runtime.Extensions;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.Extensions
{
    /// <summary>
    /// Provides Editor time extension methods for serialized-based operations.
    /// </summary>
    [PublicAPI]
    public static class SerializedExtensionsEditor
    {
        private const string ValuePropertyName = "value";

        /// <summary>
        /// Retrieves the reactive serialized property associated with the specified property name
        /// within the given serialized object. The returned property represents the "value" field
        /// of the reactive property.
        /// </summary>
        /// <param name="serializedObject">The serialized object containing the reactive property.</param>
        /// <param name="propertyName">The name of the reactive property to access.</param>
        /// <returns>The serialized property representing the "value" field of the specified reactive property.</returns>
        public static SerializedProperty GetReactiveSerializedProperty(
            this SerializedObject serializedObject,
            string propertyName)
        {
            var reactivePropertyField = serializedObject.FindField(propertyName);
            var valueProperty = reactivePropertyField.FindPropertyRelative(ValuePropertyName);
            return valueProperty;
        }

        /// <summary>
        /// Binds a reactive property from a serialized object to a UI element,
        /// enabling two-way binding and change notifications.
        /// </summary>
        /// <typeparam name="T">The type of the property being bound.</typeparam>
        /// <param name="element">The UI element to bind the property to.</param>
        /// <param name="serializedObject">The serialized object containing the property to bind.</param>
        /// <param name="propertyName">The name of the reactive property to bind.</param>
        public static void BindReactiveProperty<T>(
            this SerializedObject serializedObject,
            BaseField<T> element,
            string propertyName)
        {
            var reactivePropertyField = serializedObject.FindField(propertyName);
            var valueProperty = reactivePropertyField.FindPropertyRelative(ValuePropertyName);

            element.BindProperty(valueProperty);

            element.RegisterValueChangedCallback(_ =>
            {
                serializedObject.ApplyModifiedProperties();

                TriggerReactivePropertyNotification(serializedObject, reactivePropertyField);
            });
        }

        /// <summary>
        /// Finds a serialized property using its backing field name format.
        /// </summary>
        /// <param name="serializedObject">The serialized object to search in.</param>
        /// <param name="name">The property name to find (will be converted to backing field format).</param>
        /// <returns>The found serialized property or null if not found.</returns>
        public static SerializedProperty FindField(this SerializedObject serializedObject, string name) =>
            serializedObject.FindProperty(name.ConvertToBackingField());

        /// <summary>
        /// Finds a relative serialized property using its backing field name format.
        /// </summary>
        /// <param name="serializedProperty">The serialized property to search in.</param>
        /// <param name="name">The property name to find (will be converted to backing field format).</param>
        /// <returns>The found relative serialized property or null if not found.</returns>
        public static SerializedProperty FindFieldRelative(this SerializedProperty serializedProperty, string name) =>
            serializedProperty.FindPropertyRelative(name.ConvertToBackingField());

        /// <summary>
        /// Tries to get a component from the target object.
        /// </summary>
        /// <param name="serializedProperty">The serialized property to get the component from.</param>
        /// <param name="requestedComponent">The requested component.</param>
        /// <returns>True if the component was found; otherwise, false.</returns>
        public static bool TryGetComponent<TComponent>(
            this SerializedProperty serializedProperty,
            out TComponent requestedComponent)
            where TComponent : Component
        {
            requestedComponent = null;

            return serializedProperty.serializedObject.targetObject is Component component
                   && component.TryGetComponent(out requestedComponent);
        }

        /// <summary>
        /// Attempts to retrieve a component of the specified type from the target object of the serialized property.
        /// </summary>
        /// <param name="serializedProperty">The serialized property associated with the object to inspect.</param>
        /// <param name="componentType">The type of the component to retrieve.</param>
        /// <param name="requestedComponent">When this method returns, contains the component if found, otherwise null.</param>
        /// <returns>True if the component of the specified type is found; otherwise, false.</returns>
        public static bool TryGetComponent(
            this SerializedProperty serializedProperty,
            Type componentType,
            out Component requestedComponent)
        {
            requestedComponent = null;

            return serializedProperty.serializedObject.targetObject is Component component
                   && component.TryGetComponent(componentType, out requestedComponent);
        }

        /// <summary>
        /// Tries to retrieve a component of the specified type from the target object of a serialized property.
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to retrieve.</typeparam>
        /// <param name="serializedProperty">The serialized property whose target object is searched for the component.</param>
        /// <param name="requestedComponent">The output parameter that will hold the retrieved component if found, or null if not found.</param>
        /// <returns>True if the component of the specified type is found; otherwise, false.</returns>
        public static bool TryGetComponent<TComponent>(
            this SerializedObject serializedProperty,
            out TComponent requestedComponent)
            where TComponent : Component
        {
            requestedComponent = null;

            return serializedProperty.targetObject is Component component
                   && component.TryGetComponent(out requestedComponent);
        }

        /// <summary>
        /// Retrieves a value of a specified type from the parent of the serialized property
        /// by examining a specific field in the parent.
        /// </summary>
        /// <typeparam name="T">The expected type of the value to retrieve.</typeparam>
        /// <param name="property">The serialized property whose parent is to be examined.</param>
        /// <param name="propertyName">The name of the field in the parent to retrieve the value from.</param>
        /// <returns>The value of the specified type retrieved from the parent's property,
        /// or the default value of the type if retrieval fails.</returns>
        public static T GetPropertyFromParent<T>(this SerializedProperty property, string propertyName)
        {
            var parentPath = property.propertyPath[..property.propertyPath.LastIndexOf('.')];
            var parentProperty = property.serializedObject.FindProperty(parentPath);
            var targetProperty = parentProperty.FindFieldRelative(propertyName);

            return targetProperty != null ? (T)(object)targetProperty.enumValueIndex : default;
        }

        /// <summary>
        /// Creates a UI property field associated with the specified property name in the serialized object
        /// and adds it to the provided container.
        /// </summary>
        /// <param name="serializedObject">The serialized object containing the property to be displayed.</param>
        /// <param name="propertyName">The name of the property within the serialized object to create the field for.</param>
        /// <param name="container">The container to which the UI property field will be added.</param>
        public static void CreateProperty(
            this SerializedObject serializedObject,
            string propertyName,
            VisualElement container)
        {
            var property = serializedObject.FindField(propertyName);

            var propertyField = new PropertyField(property);
            propertyField.BindProperty(property);

            container.Add(propertyField);
        }

        private static void TriggerReactivePropertyNotification(
            SerializedObject serializedObject,
            SerializedProperty reactivePropertyField)
        {
            try
            {
                var targetObject = serializedObject.targetObject;
                var reactivePropertyInstance =
                    GetReactivePropertyInstance(targetObject, reactivePropertyField.propertyPath);

                if (reactivePropertyInstance == null)
                    return;

                var forceNotifyMethod = reactivePropertyInstance.GetType().GetMethod("ForceNotify",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                forceNotifyMethod?.Invoke(reactivePropertyInstance, Array.Empty<object>());
            }
            catch (Exception ex)
            {
                Debug.LogError("[NotifyValueChangedExtensionsEditor::TriggerReactivePropertyNotification] '" +
                               $"Failed to trigger reactive property notification: {ex.Message}");

                Debug.LogException(ex);
            }
        }

        private static object GetReactivePropertyInstance(object targetObject, string propertyPath)
        {
            var pathParts = propertyPath.Split('.');
            var currentObject = targetObject;

            foreach (var pathPart in pathParts)
            {
                if (currentObject == null)
                    break;

                var fieldInfo = currentObject.GetType().GetFieldInfo(pathPart);

                if (fieldInfo != null)
                    currentObject = fieldInfo.GetValue(currentObject);
                else
                    return null;
            }

            return currentObject;
        }
    }
}