using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Other;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Halftone
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    internal sealed class HalftoneOverlay : MonoBehaviour
    {
        [SerializeField] private HalftoneProperties _halftoneProperties;

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
            {
                _instanceMaterial = new Material(ResourceReferences.Instance.HalftoneMaterial);
                _graphic.material = _instanceMaterial;
            }

            _halftoneProperties.ApplyProperties(_instanceMaterial);
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