using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    public enum WindowMode
    {
        /// <summary>Shows over the windows below and leaves them visible.</summary>
        Overlay = 0,

        /// <summary>
        /// Covers the windows below. Opening, it shows on top while they play their hide; closing, they move on top
        /// and play their hide backwards while it plays its hide. A hiding window leaves the screen once its hide
        /// and the show in front of it have finished.
        /// </summary>
        Blend = 1,

        /// <summary>
        /// Covers the windows below: they stay still while it shows and leave the screen instantly afterwards, and
        /// reappear instantly behind it when it closes.
        /// </summary>
        SlideOver = 2
    }
}