using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons;
using UnityEditor;
using UnityEditor.UI;

namespace CustomUtils.Editor.Scripts.UI.CustomComponents.Selectables
{
    [CustomEditor(typeof(LockableButton), true)]
    [CanEditMultipleObjects]
    internal sealed class LockableButtonEditor : ButtonEditor
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

            _editorStateControls.PropertyField(nameof(LockableButton.Lock), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
