using System;
using System.Collections.Generic;
using System.Linq;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Attributes;
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

        private const string RelativeOffsetTooltip =
            "Shown in pixels at the current rect size. Stored as a fraction of the size, " +
            "so the offset scales with the rect (x by width, y by height).";

        private static readonly GUIContent _cornerOffsetModeContent = new("Corner Offset Mode",
            "Absolute: offsets are pixels and don't change on resize.\n" +
            "Relative: offsets scale with the rect size. Switching keeps the current shape.");

        private SerializedProperty _useCustomMaterial;
        private SerializedProperty _borderWidth;
        private SerializedProperty _falloffDistance;
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

            _borderWidth = serializedObject.FindProperty(ProceduralImage.BorderWidthFieldName);
            _falloffDistance = serializedObject.FindProperty(ProceduralImage.FalloffDistanceFieldName);

            _cornerOffsetTopLeft = serializedObject.FindField(nameof(ProceduralImage.CornerOffsetTopLeft));
            _cornerOffsetTopRight = serializedObject.FindField(nameof(ProceduralImage.CornerOffsetTopRight));
            _cornerOffsetBottomRight = serializedObject.FindField(nameof(ProceduralImage.CornerOffsetBottomRight));
            _cornerOffsetBottomLeft = serializedObject.FindField(nameof(ProceduralImage.CornerOffsetBottomLeft));

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

            if (_useCustomMaterial.boolValue)
                _editorStateControls.PropertyField(m_Material);

            RaycastControlsGUI();

            EditorGUILayout.Space();

            ModifierGUI();

            _editorStateControls.PropertyField(_borderWidth);
            _editorStateControls.PropertyField(_falloffDistance);

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
            => targets.All(static item => ((ProceduralImage)item).CornerOffsetMode == CornerOffsetMode.Absolute);

        private void CornerOffsetModeGUI()
        {
            var mode = _proceduralImage.CornerOffsetMode;

            EditorGUI.showMixedValue = targets.Any(item => ((ProceduralImage)item).CornerOffsetMode != mode);
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
            RelativeCornerOffsetField(
                "Corner Offset Top Left",
                static image => image.CornerOffsetTopLeft,
                static (image, value) => image.CornerOffsetTopLeft = value);

            RelativeCornerOffsetField(
                "Corner Offset Top Right",
                static image => image.CornerOffsetTopRight,
                static (image, value) => image.CornerOffsetTopRight = value);

            RelativeCornerOffsetField(
                "Corner Offset Bottom Right",
                static image => image.CornerOffsetBottomRight,
                static (image, value) => image.CornerOffsetBottomRight = value);

            RelativeCornerOffsetField(
                "Corner Offset Bottom Left",
                static image => image.CornerOffsetBottomLeft,
                static (image, value) => image.CornerOffsetBottomLeft = value);
        }

        private void RelativeCornerOffsetField(
            string label,
            Func<ProceduralImage, Vector2> getOffset,
            Action<ProceduralImage, Vector2> setOffset)
        {
            var pixels = _proceduralImage.StoredOffsetToPixels(getOffset(_proceduralImage));

            EditorGUI.showMixedValue = targets.Any(item =>
            {
                var proceduralImage = (ProceduralImage)item;
                return proceduralImage.StoredOffsetToPixels(getOffset(proceduralImage)) != pixels;
            });
            EditorGUI.BeginChangeCheck();

            var content = new GUIContent($"{label} (px)", RelativeOffsetTooltip);
            var newPixels = EditorGUILayout.Vector2Field(content, pixels);

            EditorGUI.showMixedValue = false;

            if (!EditorGUI.EndChangeCheck())
                return;

            Undo.RecordObjects(targets, $"Change {label}");
            foreach (var item in targets)
            {
                var proceduralImage = (ProceduralImage)item;
                var currentPixels = proceduralImage.StoredOffsetToPixels(getOffset(proceduralImage));

                var resultPixels = new Vector2(
                    Mathf.Approximately(newPixels.x, pixels.x) ? currentPixels.x : newPixels.x,
                    Mathf.Approximately(newPixels.y, pixels.y) ? currentPixels.y : newPixels.y);

                setOffset(proceduralImage, proceduralImage.PixelsToStoredOffset(resultPixels));
                proceduralImage.SetVerticesDirty();
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

            if ((_canvas.additionalShaderChannels
                 | AdditionalCanvasShaderChannels.TexCoord1
                 | AdditionalCanvasShaderChannels.TexCoord2
                 | AdditionalCanvasShaderChannels.TexCoord3) == _canvas.additionalShaderChannels)
                return;

            EditorVisualControls.WarningBox(
                "TexCoord1,2,3 are not enabled as an additional shader channel in parent canvas." +
                " Procedural Image will not work properly");

            if (!EditorVisualControls.Button("Fix: Enable TexCoord1,2,3 in Canvas: " + _canvas.name))
                return;

            Undo.RecordObject(_canvas, "enable TexCoord1,2,3 as additional shader channels");
            _canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 |
                                                AdditionalCanvasShaderChannels.TexCoord2 |
                                                AdditionalCanvasShaderChannels.TexCoord3;
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