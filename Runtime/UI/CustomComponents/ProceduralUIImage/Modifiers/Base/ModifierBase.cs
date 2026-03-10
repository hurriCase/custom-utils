using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base
{
    [PublicAPI]
    public abstract class ModifierBase : MonoBehaviour
    {
#if UNITY_EDITOR
        private ProceduralImage _proceduralImage;
#endif

        public abstract Vector4 CalculateRadius(Rect imageRect);

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (_proceduralImage || TryGetComponent(out _proceduralImage))
                _proceduralImage.SetVerticesDirty();
        }
#endif
    }
}