using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Theme.Materials
{
    // Gradient settings travel in vertex data, so every graphic with the same base material shares one variant.
    internal static class SharedGradientMaterials
    {
        private const string GradientKeywordName = "THEME_GRADIENT";

        private static readonly Dictionary<Material, Material> _variants = new();
        private static readonly List<Material> _destroyedBaseMaterials = new();

        internal static Material Get(Material baseMaterial)
        {
            if (!_variants.TryGetValue(baseMaterial, out var variant))
                variant = CreateVariant(baseMaterial);

            variant.CopyMatchingPropertiesFromMaterial(baseMaterial);
            variant.shaderKeywords = baseMaterial.shaderKeywords;
            variant.EnableKeyword(GradientKeywordName);
            return variant;
        }

        private static Material CreateVariant(Material baseMaterial)
        {
            RemoveDestroyedVariants();

            var themeShader = ResourceReferences.Instance.GetThemeShader(baseMaterial.shader);
            var variant = new Material(themeShader) { hideFlags = HideFlags.HideAndDontSave };
            _variants.Add(baseMaterial, variant);
            return variant;
        }

        private static void RemoveDestroyedVariants()
        {
            foreach (var baseMaterial in _variants.Keys)
            {
                if (!baseMaterial)
                    _destroyedBaseMaterials.Add(baseMaterial);
            }

            foreach (var baseMaterial in _destroyedBaseMaterials)
            {
                _variants[baseMaterial].TryDestroy();
                _variants.Remove(baseMaterial);
            }

            _destroyedBaseMaterials.Clear();
        }
    }
}
