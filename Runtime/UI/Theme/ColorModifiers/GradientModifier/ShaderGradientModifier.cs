using System;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Attributes.ShowIf;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.Halftone;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier.ShaderGradient;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.GradientModifier
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.ShaderGraphicGradient)]
    internal sealed class ShaderGradientModifier : GenericColorModifierBase<Gradient>
    {
        [SerializeField] private GradientType _gradientType;

        [SerializeField] private bool _useHalftone;
        [SerializeField, ShowIf(nameof(_useHalftone))] private HalftoneProperties _halftoneProperties;

        [SerializeField, ShowIf(nameof(_gradientType), GradientType.Diamond)]
        private DiamondGradientDirection _gradientDirection;

        [SerializeField,
         ShowIf(nameof(_gradientType), GradientType.Linear, GradientType.Angular)]
        private float _gradientRotation;

        [SerializeField, InspectorReadOnly] private Graphic _graphic;

        protected override IThemeDatabase<Gradient> ThemeDatabase => GradientColorDatabase.Instance;

        private const string HalftoneKeywordName = "HALFTONE_ON";

        private static readonly int _gradientTextureId = Shader.PropertyToID("_GradientTex");
        private static readonly int _directionId = Shader.PropertyToID("_Direction");
        private static readonly int _rotationId = Shader.PropertyToID("_Rotation");

        private const int GradientTextureResolution = 64;

        private Material _material;
        private Texture2D _gradientTexture;

        protected override void Awake()
        {
            base.Awake();

            _graphic = _graphic.AsNullable() ?? GetComponent<Graphic>();

            _gradientTexture = new Texture2D(GradientTextureResolution, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };

            _material = new Material(ResourceReferences.Instance.GradientMaterial);
            _graphic.material = _material;

            this.MarkAsDirty();
        }

        protected override void OnUpdateColor(Gradient gradient)
        {
            BakeGradient(gradient);

            _material.SetTexture(_gradientTextureId, _gradientTexture);

            _material.shaderKeywords = Array.Empty<string>();
            var gradientKeyword = GradientConfig.Instance.GradientKeywords[_gradientType];
            _material.EnableKeyword(gradientKeyword);
            _material.SetInt(_directionId, (int)_gradientDirection);
            _material.SetFloat(_rotationId, _gradientRotation);

            if (!_useHalftone)
                return;

            _material.EnableKeyword(HalftoneKeywordName);
            _halftoneProperties.ApplyProperties(_material);
        }

        private void BakeGradient(Gradient gradient)
        {
            for (var i = 0; i < GradientTextureResolution; i++)
                _gradientTexture.SetPixel(i, 0, gradient.Evaluate((float)i / (GradientTextureResolution - 1)));

            _gradientTexture.Apply();
        }

        public override void Dispose()
        {
            _graphic.material = null;
            _material.Destroy();
            _gradientTexture.Destroy();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateColor(currentColorName);
        }
#endif
    }
}