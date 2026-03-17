using System.Collections.Generic;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.UI.Theme.ThemeColors;
using UnityEngine;
using ZLinq;

namespace CustomUtils.Runtime.UI.Theme.Databases.Base
{
    internal abstract class ThemeColorDatabaseBase<TDatabase, TTheme, TColor> :
        SingletonScriptableObject<TDatabase>,
        IThemeDatabase<TColor>
        where TDatabase : ThemeColorDatabaseBase<TDatabase, TTheme, TColor>
        where TTheme : class, IThemeColor<TColor>
    {
        [field: SerializeField] public List<TTheme> Colors { get; protected set; }

        public List<string> GetColorNames()
        {
            if (Colors == null || Colors.Count == 0)
                return null;

            return Colors.Select(static color => color.Name).ToList();
        }

        public List<string> GetColorGuids()
        {
            if (Colors == null || Colors.Count == 0)
                return null;

            return Colors.Select(static color => color.Guid).ToList();
        }

        public bool TryGetColorByGuid(string guid, out TColor color)
        {
            color = default;
            if (Colors == null || Colors.Count == 0 || guid == null)
                return false;

            foreach (var colorItem in Colors)
            {
                if (colorItem.Guid != guid)
                    continue;

#if MULTI_THEME
                color = colorItem.Colors[ThemeHandler.CurrentThemeType.Value];
#else
                color = colorItem.Color;
#endif
                return true;
            }

            return false;
        }

        public bool TryGetColorByName(string name, out TColor color)
        {
            color = default;

            var guid = Colors?.Find(color => color.Name == name)?.Guid;
            TryGetColorByGuid(guid, out color);
            return guid != null;
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            foreach (var color in Colors)
                color.EnsureGuid();
        }
#endif
    }
}