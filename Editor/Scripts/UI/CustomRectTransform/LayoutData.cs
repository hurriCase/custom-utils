#if IS_RECTTRANSFORM_EXTENDED_ENABLED
using UnityEngine;
#if MEMORY_PACK_INSTALLED
using MemoryPack;
#endif

namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
#if MEMORY_PACK_INSTALLED
    [MemoryPackable]
#endif

    // ReSharper disable once PartialTypeWithSinglePart | required by MemoryPack
    internal partial struct LayoutData
    {
        public float ParentWidth { get; set; }
        public float ParentHeight { get; set; }
        public float LeftMargin { get; set; }
        public float RightMargin { get; set; }
        public float TopMargin { get; set; }
        public float BottomMargin { get; set; }

        internal readonly LayoutField GetDifferentFields(in LayoutData other) =>
            FieldIfDifferent(LayoutField.ParentWidth, ParentWidth, other.ParentWidth)
            | FieldIfDifferent(LayoutField.ParentHeight, ParentHeight, other.ParentHeight)
            | FieldIfDifferent(LayoutField.LeftMargin, LeftMargin, other.LeftMargin)
            | FieldIfDifferent(LayoutField.RightMargin, RightMargin, other.RightMargin)
            | FieldIfDifferent(LayoutField.TopMargin, TopMargin, other.TopMargin)
            | FieldIfDifferent(LayoutField.BottomMargin, BottomMargin, other.BottomMargin);

        /// <summary>
        /// Returns a copy of this layout with the given fields taken from <paramref name="source"/>.
        /// </summary>
        internal readonly LayoutData WithFieldsFrom(in LayoutData source, LayoutField fields) => new()
        {
            ParentWidth = fields.HasFlag(LayoutField.ParentWidth) ? source.ParentWidth : ParentWidth,
            ParentHeight = fields.HasFlag(LayoutField.ParentHeight) ? source.ParentHeight : ParentHeight,
            LeftMargin = fields.HasFlag(LayoutField.LeftMargin) ? source.LeftMargin : LeftMargin,
            RightMargin = fields.HasFlag(LayoutField.RightMargin) ? source.RightMargin : RightMargin,
            TopMargin = fields.HasFlag(LayoutField.TopMargin) ? source.TopMargin : TopMargin,
            BottomMargin = fields.HasFlag(LayoutField.BottomMargin) ? source.BottomMargin : BottomMargin
        };

        private static LayoutField FieldIfDifferent(LayoutField field, float value, float otherValue) =>
            Mathf.Approximately(value, otherValue) ? LayoutField.None : field;
    }
}

#endif
