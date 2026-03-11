using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Parameterized
{
    [PublicAPI]
    public interface IParameterizedWindow<in TParameters>
    {
        void SetParameters(TParameters parameters);
    }
}