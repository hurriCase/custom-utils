using CustomUtils.Runtime.Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.ImagePixelPerUnit
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    internal sealed class ImagePixelsPerUnitAdjuster : MonoBehaviour
    {
        [field: SerializeField, PixelPerUnitPopup] internal PixelPerUnitData PixelPerUnitData { get; set; }

        [SerializeField, Self] private Image _image;
        [SerializeField, Self] private RectTransform _rectTransform;

        private void OnEnable()
        {
            _image.type = Image.Type.Sliced;

            UpdateImagePixelPerUnit();
        }

        private void OnValidate()
        {
            UpdateImagePixelPerUnit();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateImagePixelPerUnit();
        }

        private void UpdateImagePixelPerUnit()
        {
            if (!PixelPerUnitData.IsCorrectData || !_image.sprite)
                return;

            var (spriteCornerSize, rectSize) = PixelPerUnitData.DimensionType switch
            {
                DimensionType.Width => (_image.sprite.border.x, _rectTransform.rect.size.x),
                DimensionType.Height => (_image.sprite.border.y, _rectTransform.rect.size.y),
                _ => (1f, 1f)
            };

            var desiredCornerSize = rectSize / PixelPerUnitData.CornerRatio;
            _image.pixelsPerUnitMultiplier = spriteCornerSize / desiredCornerSize;
        }
    }
}