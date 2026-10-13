using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using CustomUtils.Runtime.UI.Theme.Materials;
using UnityEngine;

namespace CustomUtils.Runtime.Other
{
    [Resource(name: nameof(ResourceReferences))]
    internal sealed class ResourceReferences : SingletonScriptableObject<ResourceReferences>
    {
        [field: SerializeField] internal Sprite SquareSprite { get; private set; }
        [field: SerializeField] internal Sprite EmptySprite { get; private set; }
        [field: SerializeField] internal Material ProceduralImageMaterial { get; private set; }
        [field: SerializeField] internal Material ProceduralImageGlowMaterial { get; private set; }
        [field: SerializeField] internal Texture2D HalftoneTexture { get; private set; }
        [field: SerializeField] internal ThemeShaderMapping[] ThemeShaders { get; private set; }

        internal Shader GetThemeShader(Shader baseShader)
        {
            foreach (var mapping in ThemeShaders)
            {
                if (mapping.BaseShader == baseShader)
                    return mapping.ThemeShader;
            }

            return null;
        }
    }
}