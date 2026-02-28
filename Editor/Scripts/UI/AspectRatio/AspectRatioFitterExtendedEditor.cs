using System.Reflection;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Editor.Scripts.UI.AspectRatio
{
    [CustomEditor(typeof(AspectRatioFitter), true)]
    [CanEditMultipleObjects]
    internal sealed class AspectRatioFitterExtendedEditor : EditorBase
    {
        private UnityEditor.Editor _defaultEditor;
        private float _desiredWidth;
        private float _desiredHeight;

        public override void OnInspectorGUI()
        {
            DrawMainInspector();

            serializedObject.Update();

            DrawCalculatorSection();

            serializedObject.ApplyModifiedProperties();
        }

        protected override void InitializeEditor()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var aspectRatioFitterEditorType = assembly.GetType("UnityEditor.UI.AspectRatioFitterEditor");

            if (aspectRatioFitterEditorType != null)
                _defaultEditor = CreateEditor(targets, aspectRatioFitterEditorType);
        }

        protected override void CleanupEditor()
        {
            if (!_defaultEditor)
                return;

            DestroyImmediate(_defaultEditor);
            _defaultEditor = null;
        }

        private void DrawMainInspector()
        {
            if (_defaultEditor)
                _defaultEditor.OnInspectorGUI();
            else
                DrawDefaultInspector();
        }

        private void DrawCalculatorSection()
        {
            EditorGUILayout.Space();

            DrawSizeFields();

            EditorGUILayout.Space();

            DrawButtonsSection();
        }

        private void DrawSizeFields()
        {
            _desiredWidth = EditorStateControls.FloatField("Desired Width", _desiredWidth);
            _desiredHeight = EditorStateControls.FloatField("Desired Height", _desiredHeight);
        }

        private void DrawButtonsSection()
        {
            using var horizontalScope = EditorVisualControls.CreateHorizontalGroup();

            EditorVisualControls.Button("Apply Ratio", ApplyCalculatedRatio);
            EditorVisualControls.Button("Get Current Size", GetCurrentRectSize);
        }

        private void ApplyCalculatedRatio()
        {
            var calculatedRatio = CalculateAspectRatio(_desiredWidth, _desiredHeight);

            if (calculatedRatio <= 0)
                return;

            foreach (var selectedTarget in targets)
            {
                if (!selectedTarget || selectedTarget is not AspectRatioFitter component)
                    continue;

                Undo.RecordObject(component, "Set Aspect Ratio");
                component.aspectRatio = calculatedRatio;
                EditorUtility.SetDirty(component);
            }
        }

        private void GetCurrentRectSize()
        {
            if (targets.Length != 1 || !targets[0] || targets[0] is not AspectRatioFitter component)
                return;

            var rectTransform = component.GetComponent<RectTransform>();
            if (!rectTransform)
                return;

            var size = GetRectTransformSize(rectTransform);
            _desiredWidth = size.x;
            _desiredHeight = size.y;
        }

        private static float CalculateAspectRatio(float width, float height)
        {
            if (height > 0)
                return width / height;

            Debug.LogWarning("[AspectRatioFitterExtendedEditor::CalculateAspectRatio] " +
                             "Desired height must be greater than 0 to calculate aspect ratio.");
            return 1f;
        }

        private static Vector2 GetRectTransformSize(RectTransform rectTransform)
        {
            if (!rectTransform)
                return Vector2.zero;

            var rect = rectTransform.rect;
            return new Vector2(Mathf.Abs(rect.width), Mathf.Abs(rect.height));
        }
    }
}