using System;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Attributes
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InspectorButtonAttribute : Attribute { }
}