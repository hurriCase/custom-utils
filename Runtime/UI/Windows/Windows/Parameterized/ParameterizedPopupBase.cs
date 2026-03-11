using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Parameterized
{
    [PublicAPI]
    public abstract class ParameterizedPopupBase<TParameters> : PopupBase, IParameterizedWindow<TParameters>
    {
        public abstract void SetParameters(TParameters parameters);
    }
}