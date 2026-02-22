using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    public interface IParameterizedWindow<in TParameters>
    {
        void SetParameters(TParameters parameters);
    }
}