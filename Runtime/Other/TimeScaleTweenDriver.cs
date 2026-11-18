#if UNITY_EDITOR && PRIME_TWEEN_EXPERIMENTAL
using PrimeTween;
using UnityEngine;

namespace CustomUtils.Runtime.Other
{
    [DefaultExecutionOrder(PrimeTweenManagerOrder)]
    internal sealed class TimeScaleTweenDriver : MonoBehaviour
    {
        private const int PrimeTweenManagerOrder = -2000;

        private void FixedUpdate()
        {
            UpdateTweens(UpdateType.FixedUpdate, Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
        }

        private void Update()
        {
            UpdateTweens(UpdateType.Update, Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            UpdateTweens(UpdateType.LateUpdate, Time.deltaTime, Time.unscaledDeltaTime);
            PrimeTweenConfig.ManualUpdateApplyStartValues(UpdateType.Update);
            PrimeTweenConfig.ManualUpdateApplyStartValues(UpdateType.LateUpdate);
        }

        private void UpdateTweens(UpdateType updateType, float deltaTime, float unscaledDeltaTime)
        {
            var scaledUnscaledDeltaTime = unscaledDeltaTime * Time.timeScale;
            PrimeTweenConfig.ManualUpdate(updateType, deltaTime, scaledUnscaledDeltaTime);
        }
    }
}
#endif
