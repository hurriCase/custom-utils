using System;
using System.Collections.Generic;
using UnityEditor;

// ReSharper disable UnusedMember.Global
namespace CustomUtils.Editor.Scripts.CustomEditorUtilities
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for all custom editors with enhanced GUI capabilities
    /// </summary>
    public abstract class EditorBase : UnityEditor.Editor
    {
        /// <summary>
        /// Access to the enhanced GUI system with automatic undo support
        /// </summary>
        protected EditorStateControls EditorStateControls
            => _editorStateControls ??= new EditorStateControls(target, serializedObject);

        private EditorStateControls _editorStateControls;

#if PRIMETWEEN_INSTALLED
        /// <summary>
        /// Access to the progress tracker for handling long-running operations.
        /// </summary>
        protected EditorProgressTracker EditorProgressTracker => _editorProgressTracker ??= new EditorProgressTracker();
        private EditorProgressTracker _editorProgressTracker;
#endif

        private const string DefaultInspectorLabelName = "Default Inspector";
        private const string PrefPrefix = "AbstractEditor_";
        private const string DefaultInspectorPostfix = "_DefaultInspector";

        private readonly Dictionary<string, bool> _foldoutStates = new();
        private bool _showDefaultInspector;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

#if PRIMETWEEN_INSTALLED
            DrawProgressIfNeeded();
#endif

            DrawCustomSections();

            DrawDefaultInspectorSection();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Override this method in derived classes to draw custom inspector sections.
        /// </summary>
        /// <remarks>
        /// This is the primary extension point for custom editors. When overriding this method,
        /// use the <see cref="DrawFoldoutSection"/> and <see cref="DrawSection"/> methods to create
        /// consistently styled UI sections for your custom properties and controls.
        ///
        /// This method is called automatically during <see cref="OnInspectorGUI"/> before
        /// the default inspector section is drawn. This allows derived classes to implement
        /// their custom UI without needing to override the entire inspector drawing process.
        /// </remarks>
        protected virtual void DrawCustomSections() { }

        /// <summary>
        /// Initializes the editor. This method is called during OnEnable and should be overridden
        /// by derived classes to perform any custom initialization.
        /// </summary>
        protected virtual void InitializeEditor() { }

        /// <summary>
        /// Performs editor cleanup tasks. This method is called during OnDisable and should be overridden
        /// by derived classes to perform any custom cleanup operations.
        /// </summary>
        protected virtual void CleanupEditor() { }

#if PRIMETWEEN_INSTALLED
        /// <summary>
        /// Updates the progress information and triggers a repaint of the inspector
        /// </summary>
        /// <param name="operation">Name of the operation</param>
        /// <param name="info">Additional information</param>
        /// <param name="progress">Progress value between 0 and 1</param>
        protected void UpdateProgress(string operation, string info, float progress)
            => EditorProgressTracker.UpdateProgress(operation, info, progress);

        /// <summary>
        /// Completes the current operation and resets progress tracking
        /// </summary>
        protected void CompleteOperation(string completeInfo = null) => EditorProgressTracker.CompleteOperation(completeInfo);
#endif

        /// <summary>
        /// Draw a custom section with a foldout header.
        /// The foldout state is automatically saved between sessions.
        /// </summary>
        /// <param name="title">Title of the section</param>
        /// <param name="drawContent">Action to draw the section content</param>
        protected void DrawFoldoutSection(string title, Action drawContent)
        {
            _foldoutStates.TryAdd(title, true);

            var foldout = _foldoutStates[title];
            EditorVisualControls.DrawBoxWithFoldout(title, ref foldout, drawContent);
            _foldoutStates[title] = foldout;
        }

        /// <summary>
        /// Draw a custom section without a foldout header.
        /// </summary>
        /// <param name="title">Title of the section</param>
        /// <param name="drawContent">Action to draw the section content</param>
        protected void DrawSection(string title, Action drawContent)
        {
            EditorVisualControls.DrawBoxedSection(title, drawContent);
        }

        private void OnEnable()
        {
            _showDefaultInspector = EditorPrefs.GetBool($"{PrefPrefix}{target.GetType().Name}{DefaultInspectorPostfix}", false);

            _foldoutStates.Clear();

            InitializeEditor();
        }

        private void OnDisable()
        {
            CleanupEditor();

#if PRIMETWEEN_INSTALLED
            _editorProgressTracker?.Dispose();
#endif

            var targetTypeName = target.GetType().Name;
            EditorPrefs.SetBool($"{PrefPrefix}{targetTypeName}{DefaultInspectorPostfix}", _showDefaultInspector);

            foreach (var foldoutState in _foldoutStates)
            {
                EditorPrefs.SetBool(
                    $"{PrefPrefix}{targetTypeName}_{SanitizeKey(foldoutState.Key)}",
                    foldoutState.Value
                );
            }
        }

        private void DrawDefaultInspectorSection()
        {
            EditorVisualControls.DrawBoxWithFoldout(DefaultInspectorLabelName, ref _showDefaultInspector,
                () => DrawDefaultInspector());
        }

#if PRIMETWEEN_INSTALLED
        private void DrawProgressIfNeeded() => EditorProgressTracker.DrawProgressIfNeeded();
#endif

        private string SanitizeKey(string key, bool reverse = false)
            => reverse ? key.Replace("_", " ").Replace("__", ".") : key.Replace(" ", "_").Replace(".", "__");
    }
}