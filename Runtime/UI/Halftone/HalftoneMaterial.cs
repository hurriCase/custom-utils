using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Halftone
{
    internal sealed class HalftoneMaterial
    {
        private const string HalftoneKeywordName = "HALFTONE_ON";

        private static readonly int _halftoneTextureId = Shader.PropertyToID("_HalftoneTex");

        private Material _material;

        internal Material Get(Material baseMaterial, HalftoneProperties halftoneProperties, Vector2 elementSize)
        {
            var references = ResourceReferences.Instance;
            var themeShader = references.GetThemeShader(baseMaterial.shader);

            var isReusable = _material && _material.shader == themeShader;
            if (!isReusable)
            {
                Release();
                _material = new Material(themeShader) { hideFlags = HideFlags.HideAndDontSave };
            }

            _material.CopyMatchingPropertiesFromMaterial(baseMaterial);
            _material.shaderKeywords = baseMaterial.shaderKeywords;
            _material.EnableKeyword(HalftoneKeywordName);
            _material.SetTexture(_halftoneTextureId, references.HalftoneTexture);
            halftoneProperties.ApplyProperties(_material, elementSize);
            return _material;
        }

        internal void Release()
        {
            _material.TryDestroy();
            _material = null;
        }
    }
}
