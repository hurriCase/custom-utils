using CustomUtils.Runtime.UI.Windows.Windows.Base;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Parameterized
{
    [PublicAPI]
    public abstract class ParameterizedWindowBase<TParameters> : WindowBase, IParameterizedWindow<TParameters>
    {
        public abstract void SetParameters(TParameters parameters);
    }
}
