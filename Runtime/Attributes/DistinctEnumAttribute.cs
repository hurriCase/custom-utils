using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute that makes an enum field contains only distinct values.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DistinctEnumAttribute : PropertyAttribute { }
}