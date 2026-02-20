using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.Base;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
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
    public sealed class ThemeComponent : MonoBehaviour
    {
        [SerializeField] private ColorData _colorData;

        [SerializeField, InspectorReadOnly] private ColorModifierBase _currentColorModifier;

#if UNITY_EDITOR // to prevent OnValidate from updating the color
        [SerializeField, HideInInspector] private ColorData _previousColorData;
#endif

        /// <summary>
        /// Updates the color data for this theme component and applies the corresponding color modifier.
        /// </summary>
        /// <param name="colorData">The new color data to apply.</param>
        public void UpdateColorData(ColorData colorData)
        {
            if (_colorData == colorData)
                return;

            _colorData = colorData;
            this.MarkAsDirty();

            UpdateModifier(colorData);
        }

        private void UpdateModifier(ColorData colorData)
        {
            if (!_currentColorModifier || _colorData.ColorType != colorData.ColorType)
                CreateModifier(colorData.ColorType);

            _currentColorModifier.AsNullable()?.UpdateColor(colorData.ColorName);
        }

        private void CreateModifier(ColorType colorType)
        {
            _currentColorModifier.AsNullable()?.Dispose();
            _currentColorModifier.AsNullable()?.Destroy();
            _currentColorModifier = ColorModifierFactory.CreateModifier(colorType, gameObject);
            this.MarkAsDirty();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(this))
                return;

            // We can't destroy an object during OnValidate
            EditorApplication.delayCall += () =>
            {
                if (!this || _previousColorData == _colorData)
                    return;

                UpdateModifier(_colorData);
                _previousColorData = _colorData;
                this.MarkAsDirty();
            };
        }
#endif
    }
}