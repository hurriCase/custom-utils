using System;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes
{
    /// <inheritdoc />
    /// <summary>
    /// Disposable scope for boxed sections that can be used with 'using' statements.
    /// Uses Unity's standard help box styling for consistency with native Unity editors.
    /// </summary>
    public sealed class SectionScope : IDisposable
    {
        private readonly bool _endVertical;

        internal SectionScope(string title, bool withBox = true)
        {
            if (withBox)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                _endVertical = true;

                if (!string.IsNullOrEmpty(title))
                {
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
                    EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
                }

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            }
            else
                _endVertical = false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes the section scope, properly closing the vertical layout if applicable and adding appropriate spacing.
        /// </summary>
        public void Dispose()
        {
            if (!_endVertical)
                return;

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }
    }
}