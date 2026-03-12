using System;
using System.Reflection;
using CustomUtils.Runtime.Animations.Base;
using CustomUtils.Runtime.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CustomUtils.Editor.Scripts.AttributeDrawers
{
    [CustomPropertyDrawer(typeof(AnimationBase<,,>), useForChildren: true)]
    internal sealed class AnimationBaseDrawer : PropertyDrawer
    {
        private const string FieldAlignedClassName = "unity-base-field__aligned";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var stateType = fieldInfo.FieldType.GenericTypeArguments[0];
            var previewState = (Enum)Enum.GetValues(stateType).GetValue(0);

            var rootVisualElement = new VisualElement();
            rootVisualElement.Add(new PropertyField(property));

            var enumField = new EnumField("Preview State", previewState) { style = { flexGrow = 1f } };
            enumField.RegisterValueChangedCallback(changeEvent => previewState = changeEvent.newValue);
            enumField.AddToClassList(FieldAlignedClassName);

            var buttonName = nameof(IAnimationPreview.PreviewAnimation).ToSpacedWords();
            var button = new Button(() => InvokePreview(property, previewState)) { text = buttonName };

            rootVisualElement.Add(enumField);
            rootVisualElement.Add(button);

            return rootVisualElement;
        }

        private static void InvokePreview(SerializedProperty property, Enum state)
        {
            var target = property.serializedObject.targetObject;
            var field = target.GetType().GetField(property.propertyPath,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (field?.GetValue(target) is IAnimationPreview animation)
                animation.PreviewAnimation(state);
        }
    }
}