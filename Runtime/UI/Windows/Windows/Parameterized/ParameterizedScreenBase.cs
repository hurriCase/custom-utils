using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Parameterized
{
    [PublicAPI]
    public abstract class ParameterizedScreenBase<TParameters> : ScreenBase, IParameterizedWindow<TParameters>
    {
        public abstract void SetParameters(TParameters parameters);
    }
}