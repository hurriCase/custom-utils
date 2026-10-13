using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using CustomUtils.Runtime.UI.Theme.ThemeColors;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme.Databases
{
    internal abstract class ThemeDatabaseEditorBase<TDatabase, TTheme, TColor> : ThemeDatabaseEditorBase
        where TDatabase : ThemeColorDatabaseBase<TDatabase, TTheme, TColor>
        where TTheme : class, IThemeColor<TColor>
    {
        [SerializeField] private TDatabase _database;

        private const string AddColorUndoName = "Add Theme Color";
        private const string DefaultColorName = "New Color";
        private const int FirstNameSuffix = 2;
        private const string PreviewLabel = "Preview";

        internal override void DrawPreview(Rect rect, string guid)
        {
            if (_database.TryGetColorByGuid(guid, out var color))
                DrawColorField(rect, PreviewLabel, color);
        }

        internal string GetOrAdd(TColor color, string preferredName)
            => FindGuid(color) ?? AddColor(color, preferredName);

        protected abstract TTheme CreateThemeColor(string colorName, TColor color);
        protected abstract bool IsSameColor(TColor first, TColor second);
        protected abstract void DrawColorField(Rect rect, string label, TColor color);

        private string FindGuid(TColor color)
        {
            foreach (var themeColor in _database.Colors)
            {
                var existingColor = _database.GetColor(themeColor);
                var isSameColor = IsSameColor(existingColor, color);
                if (isSameColor)
                    return themeColor.Guid;
            }

            return null;
        }

        private string AddColor(TColor color, string preferredName)
        {
            var colorName = GetFreeName(preferredName);

            Undo.RecordObject(_database, AddColorUndoName);

            var themeColor = CreateThemeColor(colorName, color);
            themeColor.TrySetGuid();
            _database.Colors.Add(themeColor);

            _database.MarkAsDirty();
            AssetDatabase.SaveAssetIfDirty(_database);

            Debug.Log($"[{GetType().Name}::{nameof(AddColor)}] Added color '{colorName}' to {_database.name}");
            return themeColor.Guid;
        }

        private string GetFreeName(string preferredName)
        {
            var trimmedName = preferredName?.Trim();
            var baseName = string.IsNullOrEmpty(trimmedName) ? DefaultColorName : trimmedName;
            var colorName = baseName;
            var suffix = FirstNameSuffix;
            var isNameTaken = HasColorNamed(colorName);

            while (isNameTaken)
            {
                colorName = $"{baseName} {suffix}";
                suffix++;
                isNameTaken = HasColorNamed(colorName);
            }

            return colorName;
        }

        private bool HasColorNamed(string colorName)
        {
            foreach (var themeColor in _database.Colors)
            {
                var existingName = themeColor.Name.Trim();
                if (existingName == colorName)
                    return true;
            }

            return false;
        }
    }
}
