#if PRIMETWEEN_PRO_INSTALLED
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.SequentialShow
{
    internal static class SequentialShowSpaceExtensions
    {
        internal static RectTransform GetViewport(this ScrollRect scrollRect)
            => scrollRect.viewport ? scrollRect.viewport : (RectTransform)scrollRect.transform;

        // Cascades measure when a show starts, often while a window ancestor is still scaled to zero; world matrices
        // collapse then and rows merge, so the position is accumulated through local transforms instead.
        internal static Vector2 GetLocalPositionIn(this Transform target, Transform space)
        {
            var position = target.localPosition;
            var parent = target.parent;
            while (parent != space)
            {
                var scaledPosition = Vector3.Scale(parent.localScale, position);
                position = parent.localPosition + parent.localRotation * scaledPosition;
                parent = parent.parent;
            }

            return position;
        }
    }
}
#endif
