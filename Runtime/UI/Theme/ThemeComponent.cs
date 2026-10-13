using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Attributes.ShowIf;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Other;
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
    /// Component responsible for applying theme colors to UI graphics.
    /// Automatically creates and manages appropriate color modifiers based on the color type.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [PublicAPI]
    public sealed class ThemeComponent : MonoBehaviour, IMaterialModifier, IMeshModifier
    {
        /// <summary>
        /// Gets the theme color type and guid applied to the graphic.
        /// </summary>
        [field: SerializeField] public ColorData ColorData { get; private set; }

        [field: SerializeField] internal bool UseHalftone { get; private set; }

        [field: SerializeField, ShowIf(nameof(UseHalftone))]
        internal HalftoneProperties HalftoneProperties { get; private set; }

        [field: SerializeField, InspectorReadOnly]
        internal ColorModifierBase CurrentColorModifier { get; private set; }

        [SerializeField, Self] private Graphic _graphic;

#if UNITY_EDITOR // to prevent OnValidate from updating the color
        [SerializeField, HideInInspector] private ColorData _previousColorData;
#endif

        private readonly HalftoneMaterial _halftoneMaterial = new();
        private readonly ThemeVertexWriter _vertexWriter = new();

        private void OnEnable()
        {
            _graphic = _graphic.AsNullable() ?? GetComponent<Graphic>();
            _graphic.SetAllDirty();
        }

        /// <summary>
        /// Updates the color data for this theme component and applies the corresponding color modifier.
        /// </summary>
        /// <param name="colorData">The new color data to apply.</param>
        public void UpdateColorData(ColorData colorData)
        {
            if (ColorData == colorData)
                return;

            var previousColorType = ColorData.ColorType;
            ColorData = colorData;
#if UNITY_EDITOR
            _previousColorData = colorData;
#endif
            ApplyModifier(previousColorType);
        }

        internal GradientVertexData GetGradientVertexData()
            => CurrentColorModifier ? CurrentColorModifier.GetGradientVertexData() : default;

        internal void SetHalftone(HalftoneProperties halftoneProperties)
        {
            UseHalftone = true;
            HalftoneProperties = halftoneProperties;
            RefreshHalftone();
        }

        internal void DisableHalftone()
        {
            UseHalftone = false;
            RefreshHalftone();
        }

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            var themeShader = ResourceReferences.Instance.GetThemeShader(baseMaterial.shader);
            if (!isActiveAndEnabled || !themeShader)
                return baseMaterial;

            var colorMaterial = CurrentColorModifier
                ? CurrentColorModifier.ModifyMaterial(baseMaterial)
                : baseMaterial;

            return UseHalftone
                ? _halftoneMaterial.Get(colorMaterial, HalftoneProperties, _graphic.rectTransform.rect.size)
                : colorMaterial;
        }

        void IMeshModifier.ModifyMesh(VertexHelper vertexHelper)
        {
            if (!isActiveAndEnabled)
                return;

            var gradientVertexData = GetGradientVertexData();
            _vertexWriter.Write(vertexHelper, _graphic, gradientVertexData);
        }

#if !UGUI_7_OR_NEWER
        void IMeshModifier.ModifyMesh(Mesh mesh) { }
#endif

        private void ApplyModifier(ColorType previousColorType)
        {
            if (!CurrentColorModifier || previousColorType != ColorData.ColorType)
                CreateModifier(ColorData.ColorType);

            CurrentColorModifier.AsNullable()?.UpdateColor(ColorData.Guid);
        }

        private void CreateModifier(ColorType colorType)
        {
            var previousModifier = CurrentColorModifier;
            CurrentColorModifier = ColorModifierFactory.CreateModifier(colorType, gameObject);

            if (previousModifier && previousModifier != CurrentColorModifier)
            {
                previousModifier.Dispose();
                previousModifier.Destroy();
            }

            this.MarkAsDirty();
        }

        private void RefreshHalftone()
        {
            if (_graphic)
                _graphic.SetMaterialDirty();
        }

        private void OnRectTransformDimensionsChange()
        {
            if (UseHalftone)
                RefreshHalftone();
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

                RefreshHalftone();

                if (ColorData == _previousColorData)
                    return;

                var previousColorType = _previousColorData.ColorType;
                _previousColorData = ColorData;
                ApplyModifier(previousColorType);
            };
        }
#endif

        private void OnDisable()
        {
            _graphic.SetAllDirty();
        }

        private void OnDestroy()
        {
            _halftoneMaterial.Release();
        }
    }
}
