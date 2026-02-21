using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.InputDialog
{
    /// <summary>
    /// Provides utility methods for displaying modal input dialogs in the Unity Editor.
    /// </summary>
    /// <remarks>
    /// This static class offers a convenient way to show password input dialogs
    /// and retrieve user input without having to directly manage window instances.
    /// </remarks>
    [PublicAPI]
    public static class EditorInputDialog
    {
        /// <summary>
        /// Shows a modal input dialog and returns the text entered by the user.
        /// </summary>
        /// <param name="title">The title of the dialog window.</param>
        /// <param name="message">The message to display to the user.</param>
        /// <param name="inputText">The initial text to populate the input field.</param>
        /// <returns>
        /// The text entered by the user if OK was clicked, or an empty string if Cancel was clicked.
        /// If the dialog is closed without clicking either button, returns the initial inputText value.
        /// </returns>
        /// <remarks>
        /// This method creates and shows a modal InputDialogWindow, blocking the editor
        /// until the user completes the input process.
        /// </remarks>
        public static string Show(string title, string message, string inputText)
        {
            var result = inputText;

            var window = EditorWindow.GetWindow<InputDialogWindow>(true, title, true);
            window.titleContent = new GUIContent(title);
            window.Message = message;
            window.InputText = inputText;
            window.OnComplete += text => result = text;
            window.ShowModalUtility();

            return result;
        }
    }
}