using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Editor.Scripts.Extensions;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Glow;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.CustomComponents.ProceduralUIImage
{
    [CustomEditor(typeof(ProceduralImageGlow), true)]
    [CanEditMultipleObjects]
    public sealed class ProceduralImageGlowEditor : GraphicEditor
    {
        private SerializedProperty _source;
        private SerializedProperty _blur;
        private SerializedProperty _spread;
        private SerializedProperty _showBehindTransparentAreas;

        private EditorStateControls _editorStateControls;
        private Canvas _canvas;

        protected override void OnEnable()
        {
            base.OnEnable();

            _editorStateControls = new EditorStateControls(target, serializedObject);

            _source = serializedObject.FindField(nameof(ProceduralImageGlow.Source));
            _blur = serializedObject.FindField(nameof(ProceduralImageGlow.Blur));
            _spread = serializedObject.FindField(nameof(ProceduralImageGlow.Spread));
            _showBehindTransparentAreas = serializedObject.FindField(nameof(ProceduralImageGlow.ShowBehindTransparentAreas));

            _canvas = ((Component)target).GetComponentInParent<Canvas>();
        }

        public override void OnInspectorGUI()
        {
            CheckForShaderChannelsGUI();

            serializedObject.Update();

            _editorStateControls.PropertyField(_source);

            if (!_source.objectReferenceValue && !_source.hasMultipleDifferentValues)
                EditorVisualControls.WarningBox("Assign a Procedural Image as a source to draw the glow.");

            EditorGUILayout.Space();

            _editorStateControls.PropertyField(m_Color);
            _editorStateControls.PropertyField(_blur);
            _editorStateControls.PropertyField(_spread);
            _editorStateControls.PropertyField(_showBehindTransparentAreas);

            EditorGUILayout.Space();

            _editorStateControls.PropertyField(m_Material);
            _editorStateControls.PropertyField(m_Maskable);
            RaycastControlsGUI();

            serializedObject.ApplyModifiedProperties();
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
                " Procedural Image Glow will not work properly");

            if (!EditorVisualControls.Button("Fix: Enable TexCoord1,2,3 in Canvas: " + _canvas.name))
                return;

            Undo.RecordObject(_canvas, "enable TexCoord1,2,3 as additional shader channels");
            _canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 |
                                                AdditionalCanvasShaderChannels.TexCoord2 |
                                                AdditionalCanvasShaderChannels.TexCoord3;
        }
    }
}
