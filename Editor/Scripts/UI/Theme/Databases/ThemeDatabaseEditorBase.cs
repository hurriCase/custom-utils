using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Theme.Databases
{
    internal abstract class ThemeDatabaseEditorBase : ScriptableObject
    {
        internal const string MenuPath = "CustomUtils/Theme/";

        internal abstract void DrawPreview(Rect rect, string guid);
    }
}
