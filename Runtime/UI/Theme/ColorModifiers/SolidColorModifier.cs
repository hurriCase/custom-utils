using CustomUtils.Runtime.Attributes;
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
        [SerializeField, InspectorReadOnly] private Graphic _graphic;

        protected override IThemeDatabase<Color> ThemeDatabase => SolidColorDatabase.Instance;

        protected override void Awake()
        {
            base.Awake();

            _graphic = _graphic.AsNullable() ?? GetComponent<Graphic>();
            this.MarkAsDirty();
        }

        protected override void OnUpdateColor(Color gradient)
        {
            _graphic.color = gradient;
            this.MarkAsDirty();
        }

        public override void Dispose()
        {
            _graphic.color = Color.white;
            this.MarkAsDirty();
        }
    }
}