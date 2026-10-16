using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Theme.Databases;
using UnityEngine;
#if MULTI_THEME
using R3;
#endif

namespace CustomUtils.Runtime.UI.Theme.StyleData
{
    // One row per distinct style, shared through a global texture so every procedural image can use one material.
    // Row layout (texels): 0 gradient (atlas row, type, rotation, diamond direction),
    // 1 halftone (enabled, opacity, blend mode, rotation), 2 halftone (offset xy, scale xy), 3 color.
    internal static class StyleDataTexture
    {
        internal const int NoRow = -1;

        private const int TexelsPerRow = 4;
        private const int InitialCapacity = 16;

        private static readonly int _textureId = Shader.PropertyToID("_ThemeStyleData");
        private static readonly int _halftoneTextureId = Shader.PropertyToID("_HalftoneTex");

        private static readonly Dictionary<StyleDataKey, int> _rows = new();
        private static readonly List<StyleDataKey> _keys = new();
        private static readonly List<int> _referenceCounts = new();
        private static readonly Stack<int> _freeRows = new();

        private static Texture2D _texture;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            _rows.Clear();
            _keys.Clear();
            _referenceCounts.Clear();
            _freeRows.Clear();
            _texture.TryDestroy();
            _texture = null;
        }

        internal static int Acquire(StyleDataKey key)
        {
            if (_rows.TryGetValue(key, out var existingRow))
            {
                _referenceCounts[existingRow]++;
                return existingRow;
            }

            EnsureTexture();

            var row = _freeRows.Count > 0 ? _freeRows.Pop() : AddRow();
            _rows[key] = row;
            _keys[row] = key;
            _referenceCounts[row] = 1;

            WriteRow(row);
            _texture.Apply(false);
            return row;
        }

        internal static void Release(int row)
        {
            if (row == NoRow || row >= _referenceCounts.Count)
                return;

            _referenceCounts[row]--;
            if (_referenceCounts[row] > 0)
                return;

            _rows.Remove(_keys[row]);
            _freeRows.Push(row);
        }

        // Theme colors and gradient atlas rows can change under existing rows, so they're resolved again.
        private static void RewriteAll()
        {
            if (!_texture)
                return;

            foreach (var row in _rows.Values)
                WriteRow(row);

            _texture.Apply(false);
        }

        private static void EnsureTexture()
        {
            if (_texture)
                return;

            _texture = CreateTexture(InitialCapacity);
            Subscribe();
        }

        private static int AddRow()
        {
            var row = _keys.Count;
            _keys.Add(default);
            _referenceCounts.Add(0);

            if (row < _texture.height)
                return row;

            var previousTexture = _texture;
            _texture = CreateTexture(_texture.height * 2);
            previousTexture.TryDestroy();

            foreach (var existingRow in _rows.Values)
                WriteRow(existingRow);

            return row;
        }

        private static Texture2D CreateTexture(int height)
        {
            var texture = new Texture2D(TexelsPerRow, height, TextureFormat.RGBAFloat, false, true)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

            Shader.SetGlobalTexture(_textureId, texture);
            Shader.SetGlobalTexture(_halftoneTextureId, ResourceReferences.Instance.HalftoneTexture);
            return texture;
        }

        private static void WriteRow(int row)
        {
            var key = _keys[row];
            var gradientRow = key.HasGradient
                ? ThemeDatabaseConfig.Instance.Gradient.GetAtlasRow(key.ColorGuid)
                : NoRow;

            var gradient = new Color(gradientRow, (float)key.GradientType, key.GradientRotation,
                (float)key.DiamondDirection);
            var halftone = new Color(key.UseHalftone ? 1f : 0f, key.PatternOpacity, (float)key.BlendMode,
                key.PatternRotation);
            var pattern = new Color(key.PatternOffset.x, key.PatternOffset.y, key.PatternScale.x, key.PatternScale.y);
            var color = ResolveColor(key);

            _texture.SetPixel(0, row, gradient);
            _texture.SetPixel(1, row, halftone);
            _texture.SetPixel(2, row, pattern);
            _texture.SetPixel(3, row, color);
        }

        private static Color ResolveColor(StyleDataKey key)
        {
            if (!key.IsColorWritten)
                return Color.white;

            var color = key.RawColor;
            var isThemeSolid = key.ColorType == ColorType.Solid && !string.IsNullOrEmpty(key.ColorGuid);
            if (isThemeSolid)
                ThemeDatabaseConfig.Instance.Solid.TryGetColorByGuid(key.ColorGuid, out color);

            // In a Linear project UI colors reach the shader linearized, so the stored color matches the tint.
            return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.linear : color;
        }

        private static void Subscribe()
        {
#if MULTI_THEME
            ThemeHandler.CurrentThemeType.Subscribe(static _ => RewriteAll());
#endif
#if UNITY_EDITOR
            ThemeDatabaseConfig.Instance.Gradient.OnAtlasChanged -= RewriteAll;
            ThemeDatabaseConfig.Instance.Gradient.OnAtlasChanged += RewriteAll;
#endif
        }
    }
}
