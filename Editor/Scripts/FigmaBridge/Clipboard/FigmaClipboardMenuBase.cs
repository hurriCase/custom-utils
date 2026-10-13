using System;
using CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Clipboard
{
    internal abstract class FigmaClipboardMenuBase<TTarget, TData>
        where TTarget : Object
        where TData : FigmaClipboardData
    {
        protected abstract string ExpectedSource { get; }
        protected abstract string UndoName { get; }

        private const string UnknownSource = "non-Figma";

        protected void Paste(MenuCommand menuCommand)
        {
            var target = GetTarget(menuCommand.context);

            if (!TryReadClipboard(target.name, out var clipboardData))
                return;

            using (new RecordUndoScope(target, UndoName))
                Apply(target, clipboardData);
        }

        protected virtual TTarget GetTarget(Object context) => (TTarget)context;

        protected abstract void Apply(TTarget target, TData clipboardData);

        private bool TryReadClipboard(string targetName, out TData clipboardData)
        {
            clipboardData = null;
            var clipboardText = EditorGUIUtility.systemCopyBuffer;
            var isClipboardEmpty = string.IsNullOrWhiteSpace(clipboardText);

            if (isClipboardEmpty)
            {
                Debug.LogWarning($"[{GetType().Name}::{nameof(TryReadClipboard)}] Clipboard is empty, " +
                                 $"nothing to paste into '{targetName}'");
                return false;
            }

            try
            {
                clipboardData = JsonConvert.DeserializeObject<TData>(clipboardText);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{GetType().Name}::{nameof(TryReadClipboard)}] " +
                                 $"Failed to parse clipboard JSON for '{targetName}': {exception.Message}");
                return false;
            }

            if (clipboardData?.Source == ExpectedSource)
                return true;

            var actualSource = clipboardData?.Source ?? UnknownSource;
            Debug.LogWarning($"[{GetType().Name}::{nameof(TryReadClipboard)}] " +
                             $"Clipboard contains {actualSource} data, but '{targetName}' expects {ExpectedSource}");
            clipboardData = null;
            return false;
        }
    }
}
