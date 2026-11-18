using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.ColorModifiers.Base;
using CustomUtils.Runtime.UI.Theme.Databases;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Graphic))]
    [ColorModifier(ColorType.Solid)]
    internal sealed class SolidColorModifier : GenericColorModifierBase<Color>
    {
        protected override IThemeDatabase<Color> ThemeDatabase => ThemeDatabaseConfig.Instance.Solid;

        protected override void OnUpdateColor(Color color)
        {
            if (Graphic.color == color)
                return;

            Graphic.color = color;
            this.MarkAsDirty();
        }

        public override void Dispose()
        {
            Graphic.color = Color.white;
            this.MarkAsDirty();
        }
    }
}