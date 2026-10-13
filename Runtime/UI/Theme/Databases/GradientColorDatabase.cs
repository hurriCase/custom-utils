using System.Collections.Generic;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using CustomUtils.Runtime.UI.Theme.ThemeColors;
using UnityEngine;

#if UNITY_EDITOR
using System;
#endif

#if MULTI_THEME
using R3;
#endif

namespace CustomUtils.Runtime.UI.Theme.Databases
{
    [Resource(
        ResourcePaths.ThemeFullPath,
        ResourcePaths.GradientColorDatabaseAssetName,
        ResourcePaths.ThemeResourcePath
    )]
    internal sealed class GradientColorDatabase :
        ThemeColorDatabaseBase<GradientColorDatabase, ThemeGradientColor, Gradient>
    {
        private readonly GradientAtlas _atlas = new();
        private readonly Dictionary<string, int> _atlasRows = new();

#if MULTI_THEME
        private void OnEnable()
        {
            ThemeHandler.CurrentThemeType.Subscribe(this, static (_, self) => self.RebuildAtlas());
        }
#endif

        internal int GetAtlasRow(string guid)
        {
            if (!_atlasRows.ContainsKey(guid))
                RebuildAtlas();

            return _atlasRows[guid];
        }

        private void RebuildAtlas()
        {
            var gradients = new List<Gradient>(Colors.Count);
            _atlasRows.Clear();

            foreach (var themeColor in Colors)
            {
                _atlasRows[themeColor.Guid] = gradients.Count;
                var gradient = GetColor(themeColor);
                gradients.Add(gradient);
            }

            _atlas.Build(gradients);
        }

#if UNITY_EDITOR

        // Removing or reordering colors shifts atlas rows that graphics already wrote into their vertices.
        internal event Action OnAtlasChanged;

        protected override void OnValidate()
        {
            base.OnValidate();

            RebuildAtlas();
            OnAtlasChanged?.Invoke();
        }
#endif
    }
}