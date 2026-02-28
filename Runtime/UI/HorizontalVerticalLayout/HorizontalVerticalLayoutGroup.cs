using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.HorizontalVerticalLayout
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class HorizontalVerticalLayoutGroup : LayoutGroup
    {
        [field: SerializeField] public float Spacing { get; private set; }
        [field: SerializeField] public LayoutDirection Direction { get; private set; } = LayoutDirection.Horizontal;
        [field: SerializeField] public bool ReverseArrangement { get; private set; }

        [field: SerializeField] public bool ChildForceExpandWidth { get; private set; } = true;
        [field: SerializeField] public bool ChildForceExpandHeight { get; private set; } = true;

        [field: SerializeField] public bool ChildControlWidth { get; private set; }
        [field: SerializeField] public bool ChildControlHeight { get; private set; }

        [field: SerializeField] public bool ChildScaleWidth { get; private set; }
        [field: SerializeField] public bool ChildScaleHeight { get; private set; }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            CalcAlongAxis(0, Direction == LayoutDirection.Vertical);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, Direction == LayoutDirection.Vertical);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, Direction == LayoutDirection.Vertical);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, Direction == LayoutDirection.Vertical);
        }

        /// <summary>
        /// Calculate the layout element properties for this layout element along the given axis.
        /// </summary>
        /// <param name="axis">The axis to calculate for. 0 is horizontal and 1 is vertical.</param>
        /// <param name="isVertical">Is this group a vertical group?</param>
        private void CalcAlongAxis(int axis, bool isVertical)
        {
            float combinedPadding = axis == 0 ? padding.horizontal : padding.vertical;
            var controlSize = axis == 0 ? ChildControlWidth : ChildControlHeight;
            var useScale = axis == 0 ? ChildScaleWidth : ChildScaleHeight;
            var childForceExpandSize = axis == 0 ? ChildForceExpandWidth : ChildForceExpandHeight;

            var totalMin = combinedPadding;
            var totalPreferred = combinedPadding;
            float totalFlexible = 0;

            var alongOtherAxis = isVertical ^ (axis == 1);
            var rectChildrenCount = rectChildren.Count;
            for (var i = 0; i < rectChildrenCount; i++)
            {
                var child = rectChildren[i];
                GetChildSizes(child, axis, controlSize, childForceExpandSize,
                    out var min, out var preferred, out var flexible);

                if (useScale)
                {
                    var scaleFactor = child.localScale[axis];
                    min *= scaleFactor;
                    preferred *= scaleFactor;
                    flexible *= scaleFactor;
                }

                if (alongOtherAxis)
                {
                    totalMin = Mathf.Max(min + combinedPadding, totalMin);
                    totalPreferred = Mathf.Max(preferred + combinedPadding, totalPreferred);
                    totalFlexible = Mathf.Max(flexible, totalFlexible);
                }
                else
                {
                    totalMin += min + Spacing;
                    totalPreferred += preferred + Spacing;

                    // Increment flexible size with element's flexible size.
                    totalFlexible += flexible;
                }
            }

            if (!alongOtherAxis && rectChildren.Count > 0)
            {
                totalMin -= Spacing;
                totalPreferred -= Spacing;
            }

            totalPreferred = Mathf.Max(totalMin, totalPreferred);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        /// <summary>
        /// Set the positions and sizes of the child layout elements for the given axis.
        /// </summary>
        /// <param name="axis">The axis to handle. 0 is horizontal and 1 is vertical.</param>
        /// <param name="isVertical">Is this group a vertical group?</param>
        private void SetChildrenAlongAxis(int axis, bool isVertical)
        {
            var size = rectTransform.rect.size[axis];
            var controlSize = axis == 0 ? ChildControlWidth : ChildControlHeight;
            var useScale = axis == 0 ? ChildScaleWidth : ChildScaleHeight;
            var childForceExpandSize = axis == 0 ? ChildForceExpandWidth : ChildForceExpandHeight;
            var alignmentOnAxis = GetAlignmentOnAxis(axis);

            var alongOtherAxis = isVertical ^ (axis == 1);
            var startIndex = ReverseArrangement ? rectChildren.Count - 1 : 0;
            var endIndex = ReverseArrangement ? 0 : rectChildren.Count;
            var increment = ReverseArrangement ? -1 : 1;
            if (alongOtherAxis)
            {
                var innerSize = size - (axis == 0 ? padding.horizontal : padding.vertical);

                for (var i = startIndex; ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
                {
                    var child = rectChildren[i];
                    GetChildSizes(child, axis, controlSize, childForceExpandSize,
                        out var min, out var preferred, out var flexible);

                    var scaleFactor = useScale ? child.localScale[axis] : 1f;

                    var requiredSpace = Mathf.Clamp(innerSize, min, flexible > 0 ? size : preferred);
                    var startOffset = GetStartOffset(axis, requiredSpace * scaleFactor);
                    if (controlSize)
                        SetChildAlongAxisWithScale(child, axis, startOffset, requiredSpace, scaleFactor);
                    else
                    {
                        var offsetInCell = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxisWithScale(child, axis, startOffset + offsetInCell, scaleFactor);
                    }
                }
            }
            else
            {
                float pos = axis == 0 ? padding.left : padding.top;
                float itemFlexibleMultiplier = 0;
                var surplusSpace = size - GetTotalPreferredSize(axis);

                if (surplusSpace > 0)
                {
                    if (GetTotalFlexibleSize(axis) == 0)
                        pos = GetStartOffset(axis,
                            GetTotalPreferredSize(axis) - (axis == 0 ? padding.horizontal : padding.vertical));
                    else if (GetTotalFlexibleSize(axis) > 0)
                    {
                        var totalFlexibleSize = childForceExpandSize
                            ? GetTotalFlexibleSize(axis) - 1
                            : GetTotalFlexibleSize(axis);

                        if (totalFlexibleSize > 0)
                            itemFlexibleMultiplier = surplusSpace / totalFlexibleSize;
                    }
                }

                float minMaxLerp = 0;
                if (!Mathf.Approximately(GetTotalMinSize(axis), GetTotalPreferredSize(axis)))
                    minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) /
                                               (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));

                for (var i = startIndex; ReverseArrangement ? i >= endIndex : i < endIndex; i += increment)
                {
                    var child = rectChildren[i];
                    GetChildSizes(child, axis, controlSize, childForceExpandSize, out var min, out var preferred, out var flexible);
                    var scaleFactor = useScale ? child.localScale[axis] : 1f;

                    var childSize = Mathf.Lerp(min, preferred, minMaxLerp);
                    childSize += flexible * itemFlexibleMultiplier;
                    if (controlSize)
                        SetChildAlongAxisWithScale(child, axis, pos, childSize, scaleFactor);
                    else
                    {
                        float offsetInCell = 0;
                        if (!childForceExpandSize)
                            offsetInCell = (childSize - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxisWithScale(child, axis, pos + offsetInCell, scaleFactor);
                    }

                    pos += childSize * scaleFactor + Spacing;
                }
            }
        }

        private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand,
            out float min, out float preferred, out float flexible)
        {
            if (!controlSize)
            {
                min = child.sizeDelta[axis];
                preferred = min;
                flexible = 0;
            }
            else
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }

            if (childForceExpand)
                flexible = Mathf.Max(flexible, 1);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();

            m_ChildAlignment = TextAnchor.MiddleCenter;

            // For new added components we want these to be set to false,
            // so that the user's sizes won't be overwritten before they
            // have a chance to turn these settings off.
            // However, for existing components that were added before this
            // feature was introduced, we want it to be on be default for
            // backwardds compatibility.
            // Hence their default value is on, but we set to off in reset.
            ChildControlWidth = false;
            ChildControlHeight = false;
        }

        private int _capacity = 10;
        private Vector2[] _sizes = new Vector2[10];

        private void Update()
        {
            if (Application.isPlaying)
                return;

            var count = transform.childCount;

            if (count > _capacity)
            {
                if (count > _capacity * 2)
                    _capacity = count;
                else
                    _capacity *= 2;

                _sizes = new Vector2[_capacity];
            }

            // If children size change in editor, update layout (case 945680 - Child GameObjects in a Horizontal/Vertical Layout Group don't display their correct position in the Editor)
            var dirty = false;
            for (var i = 0; i < count; i++)
            {
                var child = transform.GetChild(i) as RectTransform;
                if (child == null || child.sizeDelta == _sizes[i])
                    continue;

                dirty = true;
                _sizes[i] = child.sizeDelta;
            }

            if (dirty)
                LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

#endif
    }
}