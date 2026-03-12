#if UNITY_EDITOR
using System;

namespace CustomUtils.Runtime.Animations.Base
{
    internal interface IAnimationPreview
    {
        void PreviewAnimation(Enum state);
    }
}
#endif