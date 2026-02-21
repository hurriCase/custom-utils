using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace CustomUtils.Editor.Scripts.Extensions
{
    /// <summary>
    /// Provides Editor time extension methods for reflection-based operations.
    /// </summary>
    [PublicAPI]
    public static class ReflectionExtensionsEditor
    {
        /// <summary>
        /// Gets the name of the first field of type <typeparamref name="TField"/> in the class <typeparamref name="TClass"/>.
        /// </summary>
        /// <typeparam name="TClass">The class type to search for fields.</typeparam>
        /// <typeparam name="TField">The field type to find.</typeparam>
        /// <param name="_">Placeholder parameter that enables extension method syntax. Not used.</param>
        /// <param name="bindingFlags">Binding flags that determine which fields to search. Defaults to non-public instance fields.</param>
        /// <returns>The name of the first field of type <typeparamref name="TField"/> found in the class.</returns>
        /// <remarks>
        /// This method is useful when you need to reference a field by name in serialized properties
        /// but want to avoid hardcoding strings. Throws an exception if no matching field is found.
        /// </remarks>
        public static string GetFieldName<TClass, TField>(this TClass _,
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
            where TClass : class =>
            typeof(TClass)
                .GetFields(bindingFlags)
                .First(static fieldInfo => fieldInfo.FieldType == typeof(TField))
                .Name;

        /// <summary>
        /// Gets the name of the first field of type List&lt;<typeparamref name="TElement"/>&gt; in the class <typeparamref name="TClass"/>.
        /// </summary>
        /// <typeparam name="TClass">The class type to search for fields.</typeparam>
        /// <typeparam name="TElement">The element type of the List field to find.</typeparam>
        /// <param name="_">Placeholder parameter that enables extension method syntax. Not used.</param>
        /// <param name="bindingFlags">Binding flags that determine which fields to search. Defaults to non-public instance fields.</param>
        /// <returns>The name of the first List&lt;<typeparamref name="TElement"/>&gt; field found in the class.</returns>
        /// <remarks>
        /// This method is useful when you need to reference a List field by name in serialized properties
        /// but want to avoid hardcoding strings. Throws an exception if no matching field is found.
        /// </remarks>
        public static string GetListFieldName<TClass, TElement>(this TClass _,
            BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance)
            where TClass : class =>
            typeof(TClass)
                .GetFields(bindingFlags)
                .First(static fieldInfo => fieldInfo.FieldType == typeof(List<TElement>))
                .Name;

        /// <summary>
        /// Searches for a field with the specified name in the type hierarchy, including private and protected fields from base classes.
        /// </summary>
        /// <param name="type">The type to search for the field.</param>
        /// <param name="fieldName">The name of the field to find.</param>
        /// <returns>The <see cref="FieldInfo"/> object representing the field if found; otherwise, null.</returns>
        public static FieldInfo GetFieldInfo(this Type type, string fieldName)
        {
            while (type != null)
            {
                var fieldInfo = type.GetField(fieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (fieldInfo != null)
                    return fieldInfo;

                type = type.BaseType;
            }

            return null;
        }
    }
}