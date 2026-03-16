using System;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using CustomUtils.Unsafe;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.Base
{
    [ExecuteAlways]
    internal abstract class ShaderGradientModifierBase<TDirection> : GenericColorModifierBase<Gradient>
        where TDirection : unmanaged, Enum
    {
        [field: SerializeField]
        internal SerializableReactiveProperty<TDirection> CurrentGradientDirection { get; private set; } = new();

        [SerializeField, InspectorReadOnly] private Graphic _graphic;

        protected override IThemeDatabase<Gradient> ThemeDatabase => GradientColorDatabase.Instance;

        protected abstract Shader GradientShader { get; }

        private static readonly int _gradientTexId = Shader.PropertyToID("_GradientTex");
        private static readonly int _directionId = Shader.PropertyToID("_Direction");

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

            _material = new Material(GradientShader);
            _graphic.material = _material;

            this.MarkAsDirty();

            CurrentGradientDirection.SubscribeUntilDestroy(this, static self => self.UpdateColor(self.currentColorName));
        }

        protected override void OnUpdateColor(Gradient gradient)
        {
            BakeGradient(gradient);

            _material.SetTexture(_gradientTexId, _gradientTexture);
            var directionValue = UnsafeEnumConverter<TDirection>.ToInt32(CurrentGradientDirection.Value);
            _material.SetInt(_directionId, directionValue);
        }

        public override void Dispose()
        {
            _graphic.material = null;
            _material.Destroy();
            _gradientTexture.Destroy();
        }

        private void BakeGradient(Gradient gradient)
        {
            for (var i = 0; i < GradientTextureResolution; i++)
                _gradientTexture.SetPixel(i, 0, gradient.Evaluate((float)i / (GradientTextureResolution - 1)));

            _gradientTexture.Apply();
        }
    }
}