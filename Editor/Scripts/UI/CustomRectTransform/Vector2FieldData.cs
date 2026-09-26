#if IS_RECTTRANSFORM_EXTENDED_ENABLED
namespace CustomUtils.Editor.Scripts.UI.CustomRectTransform
{
    internal readonly struct Vector2FieldData
    {
        internal string Label { get; }
        internal string XLabel { get; }
        internal string YLabel { get; }
        internal bool IsXMixed { get; }
        internal bool IsYMixed { get; }

        internal Vector2FieldData(string label, string xLabel, string yLabel, bool isXMixed, bool isYMixed)
        {
            Label = label;
            XLabel = xLabel;
            YLabel = yLabel;
            IsXMixed = isXMixed;
            IsYMixed = isYMixed;
        }
    }
}
#endif
