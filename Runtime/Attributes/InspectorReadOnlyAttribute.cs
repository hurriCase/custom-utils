using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute that makes a field read-only in the Unity Inspector.
    /// Can be applied to any serialized field to prevent editing while still showing the value.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class InspectorReadOnlyAttribute : PropertyAttribute { }
}