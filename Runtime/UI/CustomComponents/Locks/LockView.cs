using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.Locks
{
    /// <summary>
    /// Locked look of an element: theme colors of its graphics, desaturated images
    /// and objects that appear or disappear while it is locked.
    /// </summary>
    [Serializable]
    [PublicAPI]
    public sealed class LockView
    {
        [SerializeField] private List<LockedColorMapping> _lockedColors;
        [SerializeField] private List<GameObject> _lockedObjects;
        [SerializeField] private List<GameObject> _unlockedObjects;
        [SerializeField] private List<Image> _desaturatedImages;
        [SerializeField] private Material _desaturatedMaterial;

        private readonly Dictionary<Image, Material> _originalMaterials = new();

        private bool? _isLocked;

        /// <summary>
        /// Shows or hides the locked look.
        /// </summary>
        /// <param name="isLocked">Whether the element is locked.</param>
        public void SetLocked(bool isLocked)
        {
            if (!Application.isPlaying || _isLocked == isLocked)
                return;

            _isLocked = isLocked;

            foreach (var lockedColor in _lockedColors)
                lockedColor.SetLocked(isLocked);

            foreach (var lockedObject in _lockedObjects)
                lockedObject.SetActive(isLocked);

            foreach (var unlockedObject in _unlockedObjects)
                unlockedObject.SetActive(!isLocked);

            foreach (var desaturatedImage in _desaturatedImages)
            {
                if (!_originalMaterials.ContainsKey(desaturatedImage))
                    _originalMaterials[desaturatedImage] = GetCustomMaterialOrNull(desaturatedImage);

                desaturatedImage.material = isLocked ? _desaturatedMaterial : _originalMaterials[desaturatedImage];
            }
        }

        private Material GetCustomMaterialOrNull(Graphic image)
        {
            var material = image.material;
            var isSharedMaterial = material == image.defaultMaterial || material == _desaturatedMaterial;
            return isSharedMaterial ? null : material;
        }
    }
}
