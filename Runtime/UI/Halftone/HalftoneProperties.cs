using System;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Halftone
{
    [Serializable]
    internal sealed class HalftoneProperties
    {
        [SerializeField] private Vector2 _patternOffset;
        [SerializeField] private Vector2 _patternScale = Vector2.one;
        [SerializeField] private float _patternOpacity = 0.5f;
        [SerializeField] private Color _dotColor = Color.black;
        [SerializeField, Range(0, 360)] private float _patternRotation;

        private static readonly int _patternOffsetId = Shader.PropertyToID("_PatternOffset");
        private static readonly int _patternScaleId = Shader.PropertyToID("_PatternScale");
        private static readonly int _patternOpacityId = Shader.PropertyToID("_PatternOpacity");
        private static readonly int _dotColorId = Shader.PropertyToID("_DotColor");
        private static readonly int _patternRotationId = Shader.PropertyToID("_PatternRotation");

        internal void ApplyProperties(Material material)
        {
            material.SetVector(_patternOffsetId, new Vector4(_patternOffset.x, _patternOffset.y, 0f, 0f));
            material.SetVector(_patternScaleId, new Vector4(_patternScale.x, _patternScale.y, 0f, 0f));
            material.SetFloat(_patternOpacityId, _patternOpacity);
            material.SetColor(_dotColorId, _dotColor);
            material.SetFloat(_patternRotationId, _patternRotation);
        }
    }
}