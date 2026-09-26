#if IS_RECTTRANSFORM_EXTENDED_ENABLED
using System;

namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
    [Flags]
    internal enum LayoutField
    {
        None = 0,
        ParentWidth = 1 << 0,
        ParentHeight = 1 << 1,
        LeftMargin = 1 << 2,
        RightMargin = 1 << 3,
        TopMargin = 1 << 4,
        BottomMargin = 1 << 5
    }
}
#endif
