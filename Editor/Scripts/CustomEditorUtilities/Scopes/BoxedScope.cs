using System;
using UnityEditor;

namespace CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes
{
    /// <inheritdoc />
    /// <summary>
    /// Disposable scope for boxed sections with optional titles that can be used with 'using' statements.
    /// Provides consistent styling and spacing for creating visually distinct sections using Unity's standard styles.
    /// </summary>
    public sealed class BoxedScope : IDisposable
    {
        internal BoxedScope(string title = null)
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (!string.IsNullOrEmpty(title))
            {
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            }

            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes the boxed scope, properly closing the vertical layout and adding appropriate spacing.
        /// </summary>
        public void Dispose()
        {
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 0.5f);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
        }
    }
}