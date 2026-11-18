using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.AnimationSkipping
{
    [PublicAPI]
    public interface ISkippableAnimation
    {
        void SpeedUp(float timeScale);
    }
}
