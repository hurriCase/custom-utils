using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Other;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Halftone
{
    /// <inheritdoc />
    /// <summary>
    /// Draws a halftone pattern over a Graphic, blended like a Figma image fill.
    /// </summary>
    [PublicAPI]
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public sealed class HalftoneOverlay : MonoBehaviour
    {
        /// <summary>
        /// Gets the halftone pattern settings.
        /// </summary>
        [field: SerializeField] public HalftoneProperties HalftoneProperties { get; private set; }

        [SerializeField, Self] private Graphic _graphic;

        private Material _instanceMaterial;

        private void OnEnable()
        {
            ApplyProperties();
        }

        private void ApplyProperties()
        {
            if (!_graphic)
                return;

            if (!_instanceMaterial)
                _instanceMaterial = new Material(ResourceReferences.Instance.HalftoneMaterial);

            if (_graphic.material != _instanceMaterial)
                _graphic.material = _instanceMaterial;

            HalftoneProperties.ApplyProperties(_instanceMaterial);
        }

        private void OnDestroy()
        {
            if (_instanceMaterial)
                DestroyImmediate(_instanceMaterial);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ApplyProperties();
        }
#endif
    }
}