using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// Automatically assigns the component from the same <see cref="T:UnityEngine.GameObject">GameObject</see> in the Editor.
    /// The field is displayed as read-only in the Inspector.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SelfAttribute : PropertyAttribute { }
}