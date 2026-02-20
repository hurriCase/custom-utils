using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Modifiers.Base
{
    [PublicAPI]
    public abstract class ModifierBase : MonoBehaviour
    {
        public abstract Vector4 CalculateRadius(Rect imageRect);
    }
}