using System;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Clipboard
{
    internal static class FigmaClipboardReader
    {
        private const string UnknownSource = "non-Figma";

        internal static bool TryRead<TData>(string expectedSource, string targetName, out TData clipboardData)
            where TData : FigmaClipboardData
        {
            clipboardData = null;
            var clipboardText = EditorGUIUtility.systemCopyBuffer;
            var isClipboardEmpty = string.IsNullOrWhiteSpace(clipboardText);

            if (isClipboardEmpty)
            {
                Debug.LogWarning($"[{nameof(FigmaClipboardReader)}::{nameof(TryRead)}] Clipboard is empty, " +
                                 $"nothing to paste into '{targetName}'");
                return false;
            }

            try
            {
                clipboardData = JsonConvert.DeserializeObject<TData>(clipboardText);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[{nameof(FigmaClipboardReader)}::{nameof(TryRead)}] " +
                                 $"Failed to parse clipboard JSON for '{targetName}': {exception.Message}");
                return false;
            }

            if (clipboardData?.Source == expectedSource)
                return true;

            var actualSource = clipboardData?.Source ?? UnknownSource;
            Debug.LogWarning($"[{nameof(FigmaClipboardReader)}::{nameof(TryRead)}] " +
                             $"Clipboard contains {actualSource} data, but '{targetName}' expects {expectedSource}");
            clipboardData = null;
            return false;
        }
    }
}
