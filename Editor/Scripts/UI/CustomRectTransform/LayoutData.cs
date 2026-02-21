#if IS_RECTTRANSFORM_EXTENDED_ENABLED
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
        internal float ParentWidth { get; set; }
        internal float ParentHeight { get; set; }
        internal float LeftMargin { get; set; }
        internal float RightMargin { get; set; }
        internal float TopMargin { get; set; }
        internal float BottomMargin { get; set; }

        internal static LayoutData Empty => new();
    }
}

#endif