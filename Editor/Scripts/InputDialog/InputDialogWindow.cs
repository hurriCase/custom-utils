using System;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using JetBrains.Annotations;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.InputDialog
{
    /// <inheritdoc />
    /// <summary>
    /// A dialog window that prompts the user for password input.
    /// </summary>
    /// <remarks>
    /// This class inherits from WindowBase and provides a simple password input dialog
    /// with OK and Cancel buttons. It raises an event when the user completes the input.
    /// </remarks>
    [PublicAPI]
    public sealed class InputDialogWindow : WindowBase
    {
        /// <summary>
        /// Gets or sets the message displayed to the user.
        /// </summary>

        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the text entered by the user.
        /// </summary>

        public string InputText { get; set; }

        /// <summary>
        /// Event raised when the user completes the input process.
        /// </summary>
        /// <remarks>
        /// The string parameter contains the entered text, or an empty string if canceled.
        /// </remarks>
        public event Action<string> OnComplete;

        protected override void DrawWindowContent()
        {
            EditorVisualControls.LabelField(Message);

            InputText = EditorGUILayout.PasswordField(string.Empty, InputText);

            EditorGUILayout.BeginHorizontal();
            if (EditorVisualControls.Button("OK"))
            {
                OnComplete?.Invoke(InputText);
                Close();
            }

            if (EditorVisualControls.Button("Cancel"))
            {
                OnComplete?.Invoke(string.Empty);
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}