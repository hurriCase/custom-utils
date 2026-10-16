using System.Collections.Generic;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Attributes;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Corners;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtils.Editor.Scripts.UI.CustomComponents.ProceduralUIImage
{
    [CustomEditor(typeof(ProceduralImage), true)]
    [CanEditMultipleObjects]
    public sealed class ProceduralImageEditor : GraphicEditor
    {
        private static List<ModifierIDAttribute> _attributes;

        private const string PreserveAspectPropertyName = "m_PreserveAspect";
        private const string RelativeOffsetTooltip =
            "Shown in pixels at the current rect size. Stored as a fraction of the size, " +
            "so the offset scales with the rect (x by width, y by height).";

        private static readonly GUIContent _cornerOffsetModeContent = new("Corner Offset Mode",
            "Absolute: offsets are pixels and don't change on resize.\n" +
            "Relative: offsets scale with the rect size. Switching keeps the current shape.");

        private static readonly Corner[] _corners =
            { Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft };

        private SerializedProperty _useCustomMaterial;
        private SerializedProperty _preserveAspect;
        private SerializedProperty _borderWidth;
        private SerializedProperty _shapeSource;
        private SerializedProperty _outline;
        private SerializedProperty _shadow;
        private SerializedProperty _cornerOffsetTopLeft;
        private SerializedProperty _cornerOffsetTopRight;
        private SerializedProperty _cornerOffsetBottomRight;
        private SerializedProperty _cornerOffsetBottomLeft;

        private int _selectedId;

        private EditorStateControls _editorStateControls;
        private ProceduralImage _proceduralImage;
        private Component _targetComponent;
        private Canvas _canvas;

        protected override void OnEnable()
        {
            base.OnEnable();

            _editorStateControls = new EditorStateControls(target, serializedObject);

            _attributes = ModifierUtility.GetAttributeList();

            _useCustomMaterial = serializedObject.FindField(nameof(ProceduralImage.UseCustomMaterial));
            _preserveAspect = serializedObject.FindProperty(PreserveAspectPropertyName);

            _borderWidth = serializedObject.FindField(nameof(ProceduralImage.BorderWidth));
            _shapeSource = serializedObject.FindField(nameof(ProceduralImage.ShapeSource));
            _outline = serializedObject.FindField(nameof(ProceduralImage.Outline));
            _shadow = serializedObject.FindField(nameof(ProceduralImage.Shadow));

            var cornerOffsets = serializedObject.FindField(nameof(ProceduralImage.CornerOffsets));
            _cornerOffsetTopLeft = cornerOffsets.FindFieldRelative(nameof(CornerOffsets.TopLeft));
            _cornerOffsetTopRight = cornerOffsets.FindFieldRelative(nameof(CornerOffsets.TopRight));
            _cornerOffsetBottomRight = cornerOffsets.FindFieldRelative(nameof(CornerOffsets.BottomRight));
            _cornerOffsetBottomLeft = cornerOffsets.FindFieldRelative(nameof(CornerOffsets.BottomLeft));

            _proceduralImage = (ProceduralImage)target;
            _targetComponent = (Component)target;

            _canvas = _targetComponent.GetComponentInParent<Canvas>();

            if (_proceduralImage.GetComponent<ModifierBase>())
                _selectedId = _attributes.IndexOf(((ModifierIDAttribute[])_proceduralImage
                    .GetComponent<ModifierBase>().GetType()
                    .GetCustomAttributes(typeof(ModifierIDAttribute), false))[0]);

            _selectedId = Mathf.Max(_selectedId, 0);
        }

        public override void OnInspectorGUI()
        {
            CheckForShaderChannelsGUI();

            serializedObject.Update();

            _editorStateControls.PropertyField(m_Color);
            _editorStateControls.PropertyField(_useCustomMaterial);
            _editorStateControls.PropertyField(m_Maskable);
            _editorStateControls.PropertyField(_preserveAspect);

            if (_useCustomMaterial.boolValue)
                _editorStateControls.PropertyField(m_Material);

            RaycastControlsGUI();

            EditorGUILayout.Space();

            ModifierGUI();

            _editorStateControls.PropertyField(_borderWidth);
            _editorStateControls.PropertyField(_shapeSource);
            _editorStateControls.PropertyField(_outline, true);
            _editorStateControls.PropertyField(_shadow, true);

            serializedObject.ApplyModifiedProperties();

            CornerOffsetModeGUI();

            if (!HasOnlyAbsoluteOffsets())
            {
                RelativeCornerOffsetsGUI();
                return;
            }

            serializedObject.Update();

            _editorStateControls.PropertyField(_cornerOffsetTopLeft);
            _editorStateControls.PropertyField(_cornerOffsetTopRight);
            _editorStateControls.PropertyField(_cornerOffsetBottomRight);
            _editorStateControls.PropertyField(_cornerOffsetBottomLeft);

            serializedObject.ApplyModifiedProperties();
        }

        private bool HasOnlyAbsoluteOffsets()
            => targets.All(static item => ((ProceduralImage)item).CornerOffsets.Mode == CornerOffsetMode.Absolute);

        private void CornerOffsetModeGUI()
        {
            var mode = _proceduralImage.CornerOffsets.Mode;

            EditorGUI.showMixedValue = targets.Any(item => ((ProceduralImage)item).CornerOffsets.Mode != mode);
            EditorGUI.BeginChangeCheck();

            var newMode = (CornerOffsetMode)EditorGUILayout.EnumPopup(_cornerOffsetModeContent, mode);

            EditorGUI.showMixedValue = false;

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObjects(targets, "Change Corner Offset Mode");
            foreach (var item in targets)
            {
                var proceduralImage = (ProceduralImage)item;
                proceduralImage.SetCornerOffsetMode(newMode);
                MarkModified(proceduralImage);
            }
        }

        private void RelativeCornerOffsetsGUI()
        {
            foreach (var corner in _corners)
                RelativeCornerOffsetField(corner);
        }

        private void RelativeCornerOffsetField(Corner corner)
        {
            var pixelOffsets = _proceduralImage.GetCornerOffsetsInPixels();
            var pixels = pixelOffsets[corner];

            EditorGUI.showMixedValue = targets.Any(item =>
            {
                var itemOffsets = ((ProceduralImage)item).GetCornerOffsetsInPixels();
                return itemOffsets[corner] != pixels;
            });
            EditorGUI.BeginChangeCheck();

            var label = $"Corner Offset {ObjectNames.NicifyVariableName(corner.ToString())} (px)";
            var content = new GUIContent(label, RelativeOffsetTooltip);
            var newPixels = EditorGUILayout.Vector2Field(content, pixels);

            EditorGUI.showMixedValue = false;

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObjects(targets, $"Change {label}");
            foreach (var item in targets)
            {
                var proceduralImage = (ProceduralImage)item;
                var currentOffsets = proceduralImage.GetCornerOffsetsInPixels();
                var currentPixels = currentOffsets[corner];

                var resultPixels = new Vector2(
                    Mathf.Approximately(newPixels.x, pixels.x) ? currentPixels.x : newPixels.x,
                    Mathf.Approximately(newPixels.y, pixels.y) ? currentPixels.y : newPixels.y);

                var resultOffsets = currentOffsets.With(corner, resultPixels);
                proceduralImage.SetCornerOffsets(resultOffsets);
                MarkModified(proceduralImage);
            }
        }

        private static void MarkModified(Object proceduralImage)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(proceduralImage);
            EditorUtility.SetDirty(proceduralImage);
        }

        private void CheckForShaderChannelsGUI()
        {
            if (!_canvas)
            {
                EditorVisualControls.WarningBox("There is no Canvas in parent of this object.");
                return;
            }

            var channels = _canvas.additionalShaderChannels;
            if ((channels | ProceduralImage.RequiredShaderChannels) == channels)
                return;

            EditorVisualControls.WarningBox(
                "TexCoord1,2,3, Normal and Tangent are not enabled as additional shader channels in parent canvas." +
                " Procedural Image will not work properly");

            if (!EditorVisualControls.Button("Fix: Enable the shader channels in Canvas: " + _canvas.name))
                return;

            Undo.RecordObject(_canvas, "enable Procedural Image shader channels");
            _canvas.additionalShaderChannels |= ProceduralImage.RequiredShaderChannels;
        }

        private void ModifierGUI()
        {
            var con = new GUIContent[_attributes.Count];
            for (var i = 0; i < con.Length; i++)
                con[i] = new GUIContent(_attributes[i].Name);

            var hasMultipleValues = CheckMultipleValues();
            var selectedIndex = hasMultipleValues ? -1 : _selectedId;
            var index = EditorGUILayout.Popup(new GUIContent("Modifier Type"), selectedIndex, con);

            if (index == selectedIndex)
                return;

            UpdateModifierType(index);
        }

        private void UpdateModifierType(int index)
        {
            _selectedId = index;
            foreach (var item in targets)
            {
                var proceduralImage = (ProceduralImage)item;
                var modifierType = ModifierUtility.GetTypeWithId(_attributes[_selectedId].Name);
                proceduralImage.SetModifierType(modifierType);

                var imageModifier = proceduralImage.GetComponent<ModifierBase>();
                MoveComponentBehind(proceduralImage, imageModifier);
            }

            GUIUtility.ExitGUI();
        }

        private bool CheckMultipleValues()
        {
            if (targets.Length <= 1)
                return false;

            var firstImage = (ProceduralImage)targets[0];
            var firstImageModifier = firstImage.GetComponent<ModifierBase>().GetType();
            foreach (var item in targets)
            {
                var proceduralImage = (ProceduralImage)item;
                if (proceduralImage.GetComponent<ModifierBase>().GetType() == firstImageModifier)
                    continue;

                return true;
            }

            return false;
        }

        public override string GetInfoString()
            => $"Modifier: {_attributes[_selectedId].Name}, Line-Weight: {_proceduralImage.BorderWidth}";

        private static void MoveComponentBehind(Component reference, Component componentToMove)
        {
            if (!reference || !componentToMove || reference.gameObject != componentToMove.gameObject)
                return;

            var components = reference.GetComponents<Component>();
            var list = new List<Component>();
            list.AddRange(components);
            var i = list.IndexOf(componentToMove) - list.IndexOf(reference);
            while (i != 1)
            {
                switch (i)
                {
                    case < 1:
                        ComponentUtility.MoveComponentDown(componentToMove);
                        i++;
                        break;
                    case > 1:
                        ComponentUtility.MoveComponentUp(componentToMove);
                        i--;
                        break;
                }
            }
        }
    }
}