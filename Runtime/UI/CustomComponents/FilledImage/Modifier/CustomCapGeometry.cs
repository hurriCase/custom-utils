using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using UnityEngine;

namespace CustomUtils.Runtime.UI.CustomComponents.FilledImage.Modifier
{
    [DisallowMultipleComponent]
    internal sealed class CustomCapGeometry : CapGeometryBase
    {
        [SerializeField] private float _radius;

#if UNITY_EDITOR // to prevent OnValidate from updating the color
        [SerializeField, HideInInspector] private float _previousRadius;
#endif

        private const float QuarterCircle = Mathf.PI * 0.5f;

        internal override Vector2[] CreateStartCap(CapParameters parameters, float startRadians) =>
            CreateCap(parameters, startRadians, -1);

        internal override Vector2[] CreateEndCap(CapParameters parameters, float endRadians) =>
            CreateCap(parameters, endRadians, 1);

        private Vector2[] CreateCap(CapParameters parameters, float radians, int directionSign)
        {
            var points = new List<Vector2>();
            FillCircle(parameters, radians, parameters.OuterRadius, radians, -1, directionSign, points);
            var startRadians = radians + QuarterCircle * directionSign;
            FillCircle(parameters, radians, parameters.InnerRadius, startRadians, 1, directionSign, points);

            return points.ToArray();
        }

        private void FillCircle(
            CapParameters parameters,
            float radians,
            float initialRadius,
            float startRadians,
            int radiusSign,
            int directionSign,
            List<Vector2> points)
        {
            var capRadius = (parameters.OuterRadius - parameters.InnerRadius) * 0.5f;
            var adjustedRadius = Mathf.Min(_radius, capRadius);
            var direction = radians.GetDirectionFromAngle();
            var center = initialRadius + adjustedRadius * radiusSign;
            var centerPoint = parameters.Center + direction * center;

            for (var i = 0; i <= parameters.Resolution; i++)
            {
                var t = (float)i / parameters.Resolution;
                var angle = Mathf.Lerp(startRadians, startRadians + QuarterCircle * directionSign, t);
                var point = centerPoint + angle.GetDirectionFromAngle() * adjustedRadius;
                points.Add(point);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!TryGetComponent<RoundedFilledImage>(out var roundedFilledImage)
                && Mathf.Approximately(_radius, _previousRadius))
                return;

            _previousRadius = _radius;
            roundedFilledImage.SetAllDirty();
        }
#endif
    }
}