using CustomUtils.Runtime.UI.Windows.Windows.Base;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Parameterized
{
    /// <summary>
    /// Base for a window opened with parameters through
    /// <see cref="WindowsController.Open{TParameterizedWindow, TParameters}"/>.
    /// </summary>
    [PublicAPI]
    public abstract class ParameterizedWindowBase<TParameters> : WindowBase, IParameterizedWindow<TParameters>
    {
        public abstract void SetParameters(TParameters parameters);
    }
}
