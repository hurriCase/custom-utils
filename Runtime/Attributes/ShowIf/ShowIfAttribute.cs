using System;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Attributes.ShowIf
{
    /// <inheritdoc />
    /// <summary>
    /// Attribute that conditionally shows a field or property in the Unity Inspector based on the value of another field.
    /// Can be applied to any serialized field or property to create dynamic Inspector behavior.
    /// </summary>
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class ShowIfAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the name of the field that controls the visibility of the decorated field.
        /// </summary>
        public string ConditionalSourceField { get; private set; }

        /// <summary>
        /// Gets the show type that determines when the field should be shown.
        /// </summary>
        public ShowType ShowType { get; private set; }

        public object[] ExpectedValues { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ShowIfAttribute class.
        /// </summary>
        /// <param name="conditionalSourceField">
        /// The name of the boolean field that controls the visibility of the decorated field.
        /// This field must exist within the same class.
        /// </param>
        /// <param name="showType">
        /// Specifies when to show the field. Default is ShowType.True.
        /// </param>
        public ShowIfAttribute(string conditionalSourceField, ShowType showType = ShowType.True)
        {
            ConditionalSourceField = conditionalSourceField;
            ShowType = showType;
        }

        public ShowIfAttribute(string conditionalSourceField, params object[] expectedValues)
        {
            ConditionalSourceField = conditionalSourceField;
            ExpectedValues = expectedValues;
        }
    }
}