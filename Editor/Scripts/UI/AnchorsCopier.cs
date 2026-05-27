using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI
{
    internal static class AnchorsCopier
    {
        private const string RectTransformContextMenuPath = "CONTEXT/RectTransform/";

        [MenuItem(RectTransformContextMenuPath + nameof(CopyAnchorsAsVector4))]
        private static void CopyAnchorsAsVector4(MenuCommand command)
        {
            var rectTransform = (RectTransform)command.context;
            var min = rectTransform.anchorMin;
            var max = rectTransform.anchorMax;
            EditorGUIUtility.systemCopyBuffer = $"Vector4({min.x}, {min.y}, {max.x}, {max.y})";
        }
    }
}