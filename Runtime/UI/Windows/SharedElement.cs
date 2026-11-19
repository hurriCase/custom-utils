using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows
{
    internal sealed class SharedElement : MonoBehaviour
    {
        [field: SerializeField] internal TweenAnimationComponent Animation { get; private set; }
        [field: SerializeField, SharedElementKey] internal string Key { get; private set; }
    }
}
