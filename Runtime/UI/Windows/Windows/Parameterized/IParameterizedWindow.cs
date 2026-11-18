using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Parameterized
{
    /// <summary>
    /// A window that receives parameters before it opens.
    /// </summary>
    [PublicAPI]
    public interface IParameterizedWindow<in TParameters>
    {
        /// <summary>
        /// Receives the parameters. The controller calls it before the window shows.
        /// </summary>
        void SetParameters(TParameters parameters);
    }
}