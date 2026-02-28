using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.UI.CustomComponents.FilledImage;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.UI.CustomComponents
{
    [CustomEditor(typeof(RoundedFilledImage))]
    [CanEditMultipleObjects]
    internal sealed class RoundedFilledImageEditor : EditorBase
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorStateControls.DrawScriptProperty();
            EditorStateControls.PropertyField(serializedObject.FindProperty("m_FillAmount"));

            EditorStateControls.PropertyField(nameof(RoundedFilledImage.CustomFillOrigin));
            EditorStateControls.PropertyField(nameof(RoundedFilledImage.ThicknessRatio));
            EditorStateControls.PropertyField(nameof(RoundedFilledImage.ArcResolutionPerRadian));

            var (_, roundedCapsProperty) = EditorStateControls
                .PropertyField(nameof(RoundedFilledImage.IsRoundedCaps));

            EditorStateControls.PropertyFieldIf(
                !roundedCapsProperty.boolValue,
                nameof(RoundedFilledImage.RoundedCapResolution));

            EditorStateControls.PropertyFieldIf(
                !roundedCapsProperty.boolValue,
                nameof(RoundedFilledImage.CapGeometryType));

            serializedObject.ApplyModifiedProperties();
        }
    }
}