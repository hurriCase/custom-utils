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

        // Asset bundles carry their own copy of a shader, so on device the same shader can be a different object.
        internal Shader GetThemeShader(Shader baseShader)
        {
            foreach (var mapping in ThemeShaders)
            {
                if (mapping.BaseShader && mapping.BaseShader.name == baseShader.name)
                    return mapping.ThemeShader;
            }

            return null;
        }
    }
}