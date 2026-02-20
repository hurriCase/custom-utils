using System;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.UI.CustomComponents.ProceduralUIImage.Attributes
{
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Class)]
    public class ModifierIDAttribute : Attribute
    {
        public string Name { get; }

        public ModifierIDAttribute(string name)
        {
            Name = name;
        }
    }
}