using System.Collections.Generic;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.CustomMenu.MenuItems.Helpers
{
    internal static class MenuValidationHelper
    {
        private static readonly Dictionary<string, string> _unityShortcuts = new()
        {
            { "_%n", "New Scene (Ctrl+N)" },
            { "_%o", "Open Scene (Ctrl+O)" },
            { "_%s", "Save Scene (Ctrl+S)" },
            { "_%#s", "Save Scene As (Ctrl+Shift+S)" },
            { "_%z", "Undo (Ctrl+Z)" },
            { "_%y", "Redo (Ctrl+Y)" },
            { "_%#z", "Redo (Ctrl+Shift+Z)" },
            { "_%x", "Cut (Ctrl+X)" },
            { "_%c", "Copy (Ctrl+C)" },
            { "_%v", "Paste (Ctrl+V)" },
            { "_%d", "Duplicate (Ctrl+D)" },
            { "_DELETE", "Delete" },
            { "_%a", "Select All (Ctrl+A)" },
            { "_%f", "Find (Ctrl+F)" },
            { "_%g", "Find Next (Ctrl+G)" },
            { "_%#g", "Find Previous (Ctrl+Shift+G)" },
            { "_%h", "Replace (Ctrl+H)" },
            { "_%p", "Play Mode (Ctrl+P)" },
            { "_%#p", "Pause (Ctrl+Shift+P)" },
            { "_%&p", "Step Frame (Ctrl+Alt+P)" },
            { "_F5", "Refresh (F5)" },
            { "_%r", "Run (Ctrl+R)" },
            { "_%b", "Build (Ctrl+B)" },
            { "_%#b", "Build and Run (Ctrl+Shift+B)" },
            { "_%0", "Services (Ctrl+0)" }
        };

        private static readonly string[] _validModifiers = { "%", "&", "#" };

        internal static bool Validate<T>(T menuTarget, string menuPath, string shortcut, string itemName,
            string itemType,
            HashSet<string> usedMenuPaths, HashSet<string> usedShortcuts) =>
            ValidateMenuTarget(menuTarget)
            && ValidateMenuPath(menuPath)
            && ValidateShortcut(shortcut, itemName)
            && ValidateAndAddUniqueMenuPath(menuPath, itemName, itemType, usedMenuPaths)
            && ValidateAndAddUniqueShortcut(shortcut, itemName, itemType, usedShortcuts);

        private static bool ValidateMenuTarget<T>(T menuTarget)
        {
            if (menuTarget != null && (menuTarget is not Object @object || @object))
                return true;

            Debug.LogError(
                "[MenuController::GenerateSceneMenuItems] Scene Asset must be assigned to create Menu Items");
            return false;
        }

        private static bool ValidateMenuPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[MenuValidationHelper::ValidateMenuPath] Menu Path cannot be empty");
                return false;
            }

            if (!path.Contains('/'))
            {
                Debug.LogError(
                    $"[MenuValidationHelper::ValidateMenuPath] Menu path '{path}' should contain a submenu " +
                    "(using forward slash to specify it, e.g. 'Tools/Custom')");
                return false;
            }

            if (path.EndsWith('/'))
            {
                Debug.LogError(
                    $"[MenuValidationHelper::ValidateMenuPath] Menu path '{path}' cannot end with a forward slash");
                return false;
            }

            if (path.StartsWith('/'))
            {
                Debug.LogError(
                    $"[MenuValidationHelper::ValidateMenuPath] Menu path '{path}' cannot start with a forward slash");
                return false;
            }

            if (!path.Contains("//"))
                return true;

            Debug.LogError(
                $"[MenuValidationHelper::ValidateMenuPath] Menu path '{path}' contains double slashes which would create empty menu items");
            return false;
        }

        private static bool ValidateShortcut(string shortcut, string itemName)
        {
            if (string.IsNullOrEmpty(shortcut))
                return true;

            if (!shortcut.StartsWith("_"))
            {
                Debug.LogError($"[MenuValidationHelper::ValidateShortcut] Shortcut '{shortcut}' for '{itemName}' " +
                               "must start with underscore (_). Example: '_h' for H key, '_%h' for Ctrl+H");
                return false;
            }

            var shortcutPart = shortcut[1..];

            while (_validModifiers.Any(modifier => shortcutPart.StartsWith(modifier)))
            {
                shortcutPart = shortcutPart[1..];
            }

            if (shortcutPart.Length != 1 && !IsValidFunctionKey(shortcutPart))
            {
                Debug.LogError($"[MenuValidationHelper::ValidateShortcut] Shortcut '{shortcut}' for '{itemName}' " +
                               "must end with a single key (a-z) or function key (F1-F12). " +
                               "Examples: '_h', '_%h' (Ctrl+H), '_&h' (Alt+H), '_#h' (Shift+H), '_#&g' (Shift+Alt+G)");
                return false;
            }

            if (!_unityShortcuts.TryGetValue(shortcut.ToLower(), out var conflictingAction))
                return true;

            Debug.LogError("[MenuValidationHelper::CheckForUnityShortcutConflicts] " +
                           $"Shortcut '{shortcut}' for '{itemName}' conflicts with Unity's built-in " +
                           $"'{conflictingAction}'. This may override Unity's default behavior.");
            return false;
        }

        private static bool ValidateAndAddUniqueMenuPath(string menuPath, string itemName, string itemType,
            HashSet<string> usedMenuPaths)
        {
            if (usedMenuPaths.Add(menuPath))
                return true;

            Debug.LogError($"[MenuValidationHelper] Duplicate menu path '{menuPath}' " +
                           $"for {itemType} '{itemName}'.");
            return false;
        }

        private static bool ValidateAndAddUniqueShortcut(string shortcut, string itemName, string itemType,
            HashSet<string> usedShortcuts)
        {
            if (string.IsNullOrEmpty(shortcut))
                return true;

            if (usedShortcuts.Add(shortcut))
                return true;

            Debug.LogError($"[MenuValidationHelper] Duplicate shortcut '{shortcut}' " +
                           $"for {itemType} '{itemName}'. Each shortcut can only be used once across all menu items. " +
                           $"Menu generation aborted.");
            return false;
        }

        internal static string GetUniqueMethodName(string baseName, HashSet<string> usedNames)
        {
            var methodName = baseName;
            var suffix = 1;

            while (usedNames.Contains(methodName))
            {
                methodName = $"{baseName}_{suffix}";
                suffix++;
            }

            usedNames.Add(methodName);
            return methodName;
        }

        internal static string SanitizeMethodName(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "Empty";

            var sanitized = input
                .Replace(" ", "_")
                .Replace(".", "_")
                .Replace("-", "_")
                .Replace("+", "Plus")
                .Replace("&", "And")
                .Replace("@", "At")
                .Replace("#", "Hash")
                .Replace("$", "Dollar")
                .Replace("%", "Percent")
                .Replace("^", "Caret")
                .Replace("*", "Star")
                .Replace("(", "_")
                .Replace(")", "_")
                .Replace("[", "_")
                .Replace("]", "_")
                .Replace("{", "_")
                .Replace("}", "_")
                .Replace("|", "_")
                .Replace("\\", "_")
                .Replace("/", "_")
                .Replace("?", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace(",", "_")
                .Replace(";", "_")
                .Replace(":", "_")
                .Replace("'", "_")
                .Replace("\"", "_")
                .Replace("!", "_")
                .Replace("~", "_")
                .Replace("`", "_")
                .Replace("=", "_");

            while (sanitized.Contains("__"))
            {
                sanitized = sanitized.Replace("__", "_");
            }

            sanitized = sanitized.Trim('_');

            if (!string.IsNullOrEmpty(sanitized) && char.IsDigit(sanitized[0]))
                sanitized = "_" + sanitized;

            return string.IsNullOrEmpty(sanitized) ? "Method" : sanitized;
        }

        private static bool IsValidFunctionKey(string key)
        {
            if (key.Length < 2 || !key.StartsWith("F"))
                return false;

            if (int.TryParse(key[1..], out var number))
                return number is >= 1 and <= 12;

            return false;
        }
    }
}