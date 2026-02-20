using CustomUtils.Runtime.UI;
using JetBrains.Annotations;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="RectTransform"/>.
    /// </summary>
    [PublicAPI]
    public static class RectTransformExtensions
    {
        /// <summary>
        /// Creates an observable that emits when the RectTransform height changes.
        /// </summary>
        /// <param name="target">The RectTransform to observe.</param>
        /// <returns>Observable that emits the new height value.</returns>
        public static Observable<float> OnReactTransformHeightChangeAsObservable(this RectTransform target) =>
            target.OnRectTransformDimensionsChangeAsObservable()
                .Select(target, static (_, target) => GetDimensionValue(target, DimensionType.Height))
                .DistinctUntilChanged();

        /// <summary>
        /// Creates an observable that emits when the RectTransform width changes.
        /// </summary>
        /// <param name="target">The RectTransform to observe.</param>
        /// <returns>Observable that emits the new width value.</returns>
        public static Observable<float> OnReactTransformWidthChangeAsObservable(this RectTransform target) =>
            target.OnRectTransformDimensionsChangeAsObservable()
                .Select(target, static (_, target) => GetDimensionValue(target, DimensionType.Width))
                .DistinctUntilChanged();

        /// <summary>
        /// Creates an observable that emits when the specified RectTransform dimension changes.
        /// </summary>
        /// <param name="target">The RectTransform to observe.</param>
        /// <param name="dimension">The dimension type to observe.</param>
        /// <returns>Observable that emits the new dimension value.</returns>
        public static Observable<float> OnReactTransformDimensionChangeAsObservable(
            this RectTransform target,
            DimensionType dimension) =>
            target.OnRectTransformDimensionsChangeAsObservable()
                .Select((target, dimensionToCopy: dimension),
                    static (_, tuple) => GetDimensionValue(tuple.target, tuple.dimensionToCopy))
                .DistinctUntilChanged();

        /// <summary>
        /// Marks the specified RectTransform for rebuild by the LayoutRebuilder.
        /// </summary>
        /// <param name="rectTransform">The RectTransform to mark for rebuild.</param>
        public static void MarkLayoutForRebuild(this RectTransform rectTransform) =>
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);

        private static float GetDimensionValue(RectTransform rectTransform, DimensionType dimension) =>
            dimension switch
            {
                DimensionType.Width => rectTransform.rect.width,
                DimensionType.Height => rectTransform.rect.height,
                _ => 0f
            };
    }
}