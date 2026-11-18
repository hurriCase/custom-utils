using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
using CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage;
using CustomUtils.Runtime.UI.Halftone;
using CustomUtils.Runtime.UI.Theme.Base;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using CustomUtils.Runtime.UI.Theme.Materials;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme
{
    /// <inheritdoc />
    /// <summary>
    /// Component responsible for applying a theme style (color, gradient direction, halftone) to UI graphics.
    /// Automatically creates and manages appropriate color modifiers based on the color type.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [PublicAPI]
    public sealed class ThemeComponent : MonoBehaviour, IMaterialModifier, IMeshModifier
    {
        /// <summary>
        /// Gets the style applied to the graphic.
        /// </summary>
        [field: SerializeField] public ThemeStyle Style { get; private set; }

        [field: SerializeField, InspectorReadOnly]
        internal ColorModifierBase CurrentColorModifier { get; private set; }

        [SerializeField, Self] private Graphic _graphic;

#if UNITY_EDITOR // to prevent OnValidate from updating the color
        [SerializeField, HideInInspector] private ColorData _previousColorData;
#endif

        private ThemeStyle AppliedStyle
            => _overrideColorData is { } colorData ? Style.WithColorData(colorData) : Style;

        private readonly HalftoneMaterial _halftoneMaterial = new();
        private readonly ThemeVertexWriter _vertexWriter = new();

        private ColorData? _overrideColorData;

        private void OnEnable()
        {
            Refresh();
        }

        /// <summary>
        /// Applies the full style: theme color, gradient direction and halftone pattern.
        /// </summary>
        /// <param name="style">The style to apply.</param>
        public void UpdateStyle(ThemeStyle style)
        {
            var previousColorData = AppliedStyle.ColorData;
            Style = style;
#if UNITY_EDITOR
            _previousColorData = style.ColorData;
#endif
            if (previousColorData != AppliedStyle.ColorData)
                ApplyModifier(previousColorData.ColorType);

            Refresh();
        }

        /// <summary>
        /// Shows the given theme color instead of the style's own one, keeping the rest of the style.
        /// The override is runtime state: it is not serialized and survives <see cref="UpdateStyle"/>
        /// until <see cref="ClearColorOverride"/> is called.
        /// </summary>
        /// <param name="colorData">The color to show.</param>
        public void SetColorOverride(ColorData colorData)
        {
            ApplyColorOverride(colorData);
        }

        /// <summary>
        /// Shows the style's own color again after <see cref="SetColorOverride"/>.
        /// </summary>
        public void ClearColorOverride()
        {
            ApplyColorOverride(null);
        }

        private void ApplyColorOverride(ColorData? colorData)
        {
            var previousColorData = AppliedStyle.ColorData;
            _overrideColorData = colorData;
            if (previousColorData == AppliedStyle.ColorData)
                return;

            ApplyModifier(previousColorData.ColorType);
            Refresh();
        }

        internal GradientVertexData GetGradientVertexData()
            => CurrentColorModifier ? CurrentColorModifier.GetGradientVertexData(AppliedStyle) : default;

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            var themeShader = ResourceReferences.Instance.GetThemeShader(baseMaterial.shader);
            if (!isActiveAndEnabled || !themeShader)
                return baseMaterial;

            var colorMaterial = CurrentColorModifier
                ? CurrentColorModifier.ModifyMaterial(baseMaterial)
                : baseMaterial;

            var style = AppliedStyle;
            return style.UseHalftone
                ? _halftoneMaterial.Get(colorMaterial, style.HalftoneProperties, _graphic.rectTransform.rect.size)
                : colorMaterial;
        }

        void IMeshModifier.ModifyMesh(VertexHelper vertexHelper)
        {
            // A procedural image encodes its style itself, and its shape data uses the same vertex channels.
            if (!isActiveAndEnabled || _graphic is ProceduralImage)
                return;

            var gradientVertexData = GetGradientVertexData();
            _vertexWriter.Write(vertexHelper, _graphic, gradientVertexData);
        }

#if !UGUI_7_OR_NEWER
        void IMeshModifier.ModifyMesh(Mesh mesh) { }
#endif

        private void ApplyModifier(ColorType previousColorType)
        {
            var colorData = AppliedStyle.ColorData;
            if (!CurrentColorModifier || previousColorType != colorData.ColorType)
                CreateModifier(colorData.ColorType);

            CurrentColorModifier.AsNullable()?.UpdateColor(colorData.Guid);
        }

        private void CreateModifier(ColorType colorType)
        {
            var previousModifier = CurrentColorModifier;
            CurrentColorModifier = ColorModifierFactory.CreateModifier(colorType, gameObject);
            if (CurrentColorModifier)
                CurrentColorModifier.Init(_graphic);

            if (previousModifier && previousModifier != CurrentColorModifier)
            {
                previousModifier.Dispose();
                previousModifier.Destroy();
            }

            this.MarkAsDirty();
        }

        private void Refresh()
        {
            _graphic = _graphic.AsNullable() ?? GetComponent<Graphic>();

            if (_graphic is ProceduralImage proceduralImage)
                proceduralImage.SetFillStyle(AppliedStyle);

            _graphic.SetAllDirty();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Style.UseHalftone && _graphic)
                _graphic.SetMaterialDirty();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            // We can't destroy an object during OnValidate
            EditorApplication.delayCall += () =>
            {
                if (!this)
                    return;

                Refresh();

                var colorData = Style.ColorData;
                if (colorData == _previousColorData)
                    return;

                var previousColorType = _previousColorData.ColorType;
                _previousColorData = colorData;
                ApplyModifier(previousColorType);
            };
        }
#endif

        private void OnDisable()
        {
            if (_graphic is ProceduralImage proceduralImage)
                proceduralImage.ClearFillStyle();

            _graphic.SetAllDirty();
        }

        private void OnDestroy()
        {
            _halftoneMaterial.Release();
        }
    }
}