using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.Databases
{
    internal sealed class GradientAtlas
    {
        private const int Width = 64;

        private static readonly int _atlasId = Shader.PropertyToID("_GradientAtlas");
        private static readonly int _atlasHeightId = Shader.PropertyToID("_GradientAtlasHeight");

        private Texture2D _texture;

        internal void Build(IReadOnlyList<Gradient> gradients)
        {
            var height = Mathf.Max(gradients.Count, 1);
            if (!_texture || _texture.height != height)
            {
                _texture.TryDestroy();
                _texture = new Texture2D(Width, height, TextureFormat.RGBA32, false)
                {
                    wrapMode = TextureWrapMode.Clamp,
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            for (var row = 0; row < gradients.Count; row++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var time = x / (Width - 1f);
                    var color = gradients[row].Evaluate(time);
                    _texture.SetPixel(x, row, color);
                }
            }

            _texture.Apply();
            Shader.SetGlobalTexture(_atlasId, _texture);
            Shader.SetGlobalFloat(_atlasHeightId, height);
        }
    }
}
