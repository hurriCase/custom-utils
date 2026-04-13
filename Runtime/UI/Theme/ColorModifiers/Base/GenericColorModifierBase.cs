using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.UI.Theme.Databases.Base;
using UnityEngine;

#if MULTI_THEME
using CustomUtils.Runtime.Extensions.Observables;
#endif

namespace CustomUtils.Runtime.UI.Theme.ColorModifiers.Base
{
    internal abstract class GenericColorModifierBase<TColor> : ColorModifierBase
    {
        protected abstract IThemeDatabase<TColor> ThemeDatabase { get; }

        [SerializeField, HideInInspector] protected string currentColorName;

        protected virtual void Awake()
        {
#if MULTI_THEME
            ThemeHandler.CurrentThemeType
                .SubscribeUntilDestroy(this, static self => self.UpdateColor(self.currentColorName));
#endif
        }

        internal override void UpdateColor(string guid)
        {
            if (!ThemeDatabase.TryGetColorByGuid(guid, out var color))
                return;

            OnUpdateColor(color);
            currentColorName = guid;
            this.MarkAsDirty();
        }

        protected abstract void OnUpdateColor(TColor gradient);
    }
}