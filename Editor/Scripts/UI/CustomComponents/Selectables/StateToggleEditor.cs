using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.UI.CustomComponents.Selectables.Toggles;
using UnityEditor;
using UnityEditor.UI;

namespace CustomUtils.Editor.Scripts.UI.CustomComponents.Selectables
{
    [CustomEditor(typeof(StateToggle), true)]
    internal sealed class StateToggleEditor : ToggleEditor
    {
        private EditorStateControls _editorStateControls;

        protected override void OnEnable()
        {
            base.OnEnable();

            _editorStateControls = new EditorStateControls(target, serializedObject);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorVisualControls.LabelField("Custom Settings");

            _editorStateControls.PropertyField(nameof(StateToggle.Text), true);
            _editorStateControls.PropertyField(nameof(StateToggle.Image), true);
            _editorStateControls.PropertyField(nameof(StateToggle.AdditionalGraphics), true);
            _editorStateControls.PropertyField(nameof(StateToggle.CheckedObjects), true);
            _editorStateControls.PropertyField(nameof(StateToggle.UncheckedObjects), true);
            _editorStateControls.PropertyField(nameof(StateToggle.Lock), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}