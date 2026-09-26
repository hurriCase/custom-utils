#if IS_RECTTRANSFORM_EXTENDED_ENABLED
using System;
using System.Reflection;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
    [CustomEditor(typeof(RectTransform), true)]
    [CanEditMultipleObjects]
    internal sealed class RectTransformExtendedEditor : EditorBase
    {
        private UnityEditor.Editor _defaultEditor;
        private LayoutGUIDrawer _guiDrawer;
        private LayoutCalculator _layoutCalculator;

        private float _parentWidth;
        private float _parentHeight;
        private float _leftMargin;
        private float _rightMargin;
        private float _topMargin;
        private float _bottomMargin;

        private LayoutField _editedFields;
        private LayoutField _mixedFields;

        private bool _showFoldout;

        protected override void InitializeEditor()
        {
            _guiDrawer = new LayoutGUIDrawer(EditorStateControls);
            _layoutCalculator = new LayoutCalculator();
            _editedFields = LayoutField.None;

            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var rectTransformEditorType = assembly.GetType("UnityEditor.RectTransformEditor");
            _defaultEditor = CreateEditor(targets, rectTransformEditorType);
        }

        protected override void CleanupEditor()
        {
            if (!_defaultEditor)
                return;

            DestroyImmediate(_defaultEditor);
            _defaultEditor = null;
        }

        public override void OnInspectorGUI()
        {
            _defaultEditor.OnInspectorGUI();

            serializedObject.Update();

            RefreshFromTargets();
            DrawLayoutHelper();

            serializedObject.ApplyModifiedProperties();
        }

        private void RefreshFromTargets()
        {
            var current = _layoutCalculator.Calculate(target);

            _mixedFields = LayoutField.None;
            for (var i = 1; i < targets.Length; i++)
                _mixedFields |= current.GetDifferentFields(_layoutCalculator.Calculate(targets[i]));

            SetDisplayedLayout(current.WithFieldsFrom(GetDisplayedLayout(), _editedFields));
        }

        private LayoutData GetDisplayedLayout() => new()
        {
            ParentWidth = _parentWidth,
            ParentHeight = _parentHeight,
            LeftMargin = _leftMargin,
            RightMargin = _rightMargin,
            TopMargin = _topMargin,
            BottomMargin = _bottomMargin
        };

        private void SetDisplayedLayout(LayoutData layoutData)
        {
            _parentWidth = layoutData.ParentWidth;
            _parentHeight = layoutData.ParentHeight;
            _leftMargin = layoutData.LeftMargin;
            _rightMargin = layoutData.RightMargin;
            _topMargin = layoutData.TopMargin;
            _bottomMargin = layoutData.BottomMargin;
        }

        private bool IsMixed(LayoutField field)
        {
            var layoutField = _mixedFields & ~_editedFields;
            return layoutField.HasFlag(field);
        }

        private void DrawLayoutHelper()
        {
            EditorGUILayout.Space();

            var foldoutRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);

            HandleLayoutContextMenu(foldoutRect);

            EditorVisualControls.Foldout(foldoutRect, "Layout Helper", ref _showFoldout, DrawFoldoutContent);
        }

        private void DrawFoldoutContent()
        {
            EditorGUI.indentLevel++;

            var layoutBeforeEdit = GetDisplayedLayout();

            DrawSizeFields();
            EditorGUILayout.Space();

            DrawMarginFields();
            EditorGUILayout.Space();

            _editedFields |= layoutBeforeEdit.GetDifferentFields(GetDisplayedLayout());

            DrawCalculatedContentSize();
            EditorGUILayout.Space();

            DrawButtonsSection();

            EditorGUI.indentLevel--;
        }

        private void HandleLayoutContextMenu(Rect rect)
        {
            var currentEvent = Event.current;

            if (currentEvent.type != EventType.ContextClick || !rect.Contains(currentEvent.mousePosition))
                return;

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy Layout"), false, CopyLayout);

            if (LayoutClipboard.HasData)
                menu.AddItem(new GUIContent("Paste Layout"), false, PasteLayout);
            else
                menu.AddDisabledItem(new GUIContent("Paste Layout"));

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Reset Layout"), false, ResetLayout);

            if (_editedFields != LayoutField.None)
                menu.AddItem(new GUIContent("Discard Edits"), false, DiscardEdits);
            else
                menu.AddDisabledItem(new GUIContent("Discard Edits"));

            menu.ShowAsContext();
            currentEvent.Use();
        }

        private void DiscardEdits() => _editedFields = LayoutField.None;

        private void ResetLayout()
        {
            ApplyLayout(static layoutData =>
            {
                layoutData.LeftMargin = 0f;
                layoutData.RightMargin = 0f;
                layoutData.TopMargin = 0f;
                layoutData.BottomMargin = 0f;
                return layoutData;
            });
        }

        private void DrawButtonsSection()
        {
            EditorVisualControls.Button("Apply Anchors", ApplyAnchors);
        }

        private void DrawSizeFields()
        {
            var parentField = new Vector2FieldData("Parent", "Width", "Height",
                IsMixed(LayoutField.ParentWidth), IsMixed(LayoutField.ParentHeight));

            _guiDrawer.DrawVector2FieldStacked(parentField, ref _parentWidth, ref _parentHeight);
        }

        private void DrawMarginFields()
        {
            var horizontalField = new Vector2FieldData("Left/Right Margin", "L", "R",
                IsMixed(LayoutField.LeftMargin), IsMixed(LayoutField.RightMargin));
            var verticalField = new Vector2FieldData("Top/Bottom Margin", "T", "B",
                IsMixed(LayoutField.TopMargin), IsMixed(LayoutField.BottomMargin));

            _guiDrawer.DrawVector2FieldHorizontal(horizontalField, ref _leftMargin, ref _rightMargin);
            _guiDrawer.DrawVector2FieldHorizontal(verticalField, ref _topMargin, ref _bottomMargin);
        }

        private void DrawCalculatedContentSize()
        {
            var contentWidth = _parentWidth - _leftMargin - _rightMargin;
            var contentHeight = _parentHeight - _topMargin - _bottomMargin;

            const LayoutField widthFields = LayoutField.ParentWidth | LayoutField.LeftMargin | LayoutField.RightMargin;
            const LayoutField heightFields = LayoutField.ParentHeight | LayoutField.TopMargin | LayoutField.BottomMargin;
            var visibleMixedFields = _mixedFields & ~_editedFields;

            var contentField = new Vector2FieldData("Content", "Width", "Height",
                (visibleMixedFields & widthFields) != 0,
                (visibleMixedFields & heightFields) != 0);

            _guiDrawer.DrawVector2ReadOnly(contentField, contentWidth, contentHeight);
        }

        private void CopyLayout()
        {
            LayoutClipboard.Copy(GetDisplayedLayout());
        }

        private void PasteLayout()
        {
            if (!LayoutClipboard.TryPaste(out var pastedLayout))
            {
                Debug.LogWarning("[RectTransformExtendedEditor::PasteLayout] " +
                                 "Failed to paste layout data from clipboard");
                return;
            }

            ApplyLayout(_ => pastedLayout);
        }

        private void ApplyAnchors()
        {
            var editedLayout = GetDisplayedLayout();
            var editedFields = _editedFields;

            ApplyLayout(layoutData => layoutData.WithFieldsFrom(editedLayout, editedFields));
        }

        private void ApplyLayout(Func<LayoutData, LayoutData> modifyLayout)
        {
            Undo.SetCurrentGroupName("Set RectTransform Anchors and Reset Offsets");

            foreach (var selectedTarget in targets)
            {
                if (!selectedTarget || selectedTarget is not RectTransform rectTransform)
                    continue;

                var layoutData = modifyLayout(_layoutCalculator.Calculate(rectTransform));

                if (layoutData.ParentWidth <= 0 || layoutData.ParentHeight <= 0)
                {
                    Debug.LogWarning("[RectTransformExtendedEditor::ApplyLayout] " +
                                     $"Cannot apply anchors to '{rectTransform.name}': Parent size is invalid.",
                        rectTransform);
                    continue;
                }

                Undo.RecordObject(rectTransform, "Set RectTransform Anchors and Reset Offsets");

                SetAnchors(rectTransform, layoutData);

                EditorUtility.SetDirty(rectTransform);
            }

            _editedFields = LayoutField.None;
        }

        private static void SetAnchors(RectTransform rectTransform, LayoutData layoutData)
        {
            var widthRatio = 1f / layoutData.ParentWidth;
            var heightRatio = 1f / layoutData.ParentHeight;

            rectTransform.anchorMin = new Vector2(
                widthRatio * layoutData.LeftMargin,
                heightRatio * layoutData.BottomMargin);

            rectTransform.anchorMax = new Vector2(
                1 - widthRatio * layoutData.RightMargin,
                1 - heightRatio * layoutData.TopMargin);

            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
        }
    }
}
#endif
