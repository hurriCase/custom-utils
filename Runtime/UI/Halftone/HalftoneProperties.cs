using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Halftone
{
    /// <summary>
    /// Halftone pattern settings, applied to a material that includes HalftoneUtils.hlsl.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public sealed class HalftoneProperties
    {
        /// <summary>
        /// Gets the pattern offset in UV space.
        /// </summary>
        [field: SerializeField] public Vector2 PatternOffset { get; private set; }

        /// <summary>
        /// Gets the pattern scale in UV space.
        /// </summary>
        [field: SerializeField] public Vector2 PatternScale { get; private set; } = Vector2.one;

        /// <summary>
        /// Gets the pattern opacity, matching the Figma image fill opacity.
        /// </summary>
        [field: SerializeField] public float PatternOpacity { get; private set; } = 0.5f;

        /// <summary>
        /// Gets how the pattern blends with the element color, matching the Figma fill blend mode.
        /// </summary>
        [field: SerializeField] public HalftoneBlendMode BlendMode { get; private set; }

        /// <summary>
        /// Gets the pattern rotation in degrees.
        /// </summary>
        [field: SerializeField, Range(0, 360)] public float PatternRotation { get; private set; }

        private static readonly int _patternOffsetId = Shader.PropertyToID("_PatternOffset");
        private static readonly int _patternScaleId = Shader.PropertyToID("_PatternScale");
        private static readonly int _patternOpacityId = Shader.PropertyToID("_PatternOpacity");
        private static readonly int _blendModeId = Shader.PropertyToID("_BlendMode");
        private static readonly int _patternRotationId = Shader.PropertyToID("_PatternRotation");

        internal void ApplyProperties(Material material)
        {
            material.SetVector(_patternOffsetId, new Vector4(PatternOffset.x, PatternOffset.y, 0f, 0f));
            material.SetVector(_patternScaleId, new Vector4(PatternScale.x, PatternScale.y, 0f, 0f));
            material.SetFloat(_patternOpacityId, PatternOpacity);
            material.SetFloat(_blendModeId, (float)BlendMode);
            material.SetFloat(_patternRotationId, PatternRotation);
        }
    }
}