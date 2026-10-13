using UnityEngine;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Margins
{
    internal readonly struct RectTransformLayout
    {
        internal Vector2 Anchor { get; }
        internal Vector2 Pivot { get; }
        internal Vector2 Position { get; }

        internal RectTransformLayout(Vector2 anchor, Vector2 pivot, Vector2 position)
        {
            Anchor = anchor;
            Pivot = pivot;
            Position = position;
        }
    }
}
