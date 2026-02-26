#if INSPECTOR_BUTTONS
using System;
using System.Collections.Generic;
using System.Reflection;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Formatter;
using UnityEditor;
using UnityEngine;
using ZLinq;
using Object = UnityEngine.Object;

namespace CustomUtils.Editor.Scripts.AttributeDrawers
{
    [InitializeOnLoad]
    internal static class InspectorButtonDrawer
    {
        private const string FoldoutKeyPrefix = nameof(InspectorButtonDrawer) + "_Foldout_";
        private const string ParameterKeyPrefix = nameof(InspectorButtonDrawer) + "_Parameter_";
        private const string ScriptReferenceLabel = "Script";

        private static readonly Dictionary<string, object[]> _parameterValues = new();

        static InspectorButtonDrawer()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += DrawMethodCallButtons;
        }

        private static void DrawMethodCallButtons(UnityEditor.Editor editor)
        {
            if (editor.target is not GameObject gameObject)
                return;

            foreach (var monoBehaviour in gameObject.GetComponents<MonoBehaviour>())
                DrawMethodButtons(monoBehaviour);
        }

        private static void DrawMethodButtons(Object monoBehaviour)
        {
            var methods = monoBehaviour.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(static method => method.GetCustomAttribute<InspectorButtonAttribute>() != null)
                .ToArray();

            if (methods.Length == 0)
                return;

            if (!DrawFoldout(monoBehaviour))
                return;

            DrawScriptReference((MonoBehaviour)monoBehaviour);

            foreach (var method in methods)
                DrawMethod(monoBehaviour, method);
        }

        private static bool DrawFoldout(Object monoBehaviour)
        {
            var key = $"{FoldoutKeyPrefix}{monoBehaviour.GetInstanceID()}";
            var expanded = SessionState.GetBool(key, true);
            EditorGUI.indentLevel++;
            expanded = EditorGUILayout.Foldout(expanded, monoBehaviour.GetType().Name, true);
            EditorGUI.indentLevel--;
            SessionState.SetBool(key, expanded);
            return expanded;
        }

        private static void DrawScriptReference(MonoBehaviour monoBehaviour)
        {
            GUI.enabled = false;

            EditorGUILayout.ObjectField(
                ScriptReferenceLabel,
                MonoScript.FromMonoBehaviour(monoBehaviour),
                typeof(MonoScript),
                false);

            GUI.enabled = true;
        }

        private static void DrawMethod(Object monoBehaviour, MethodBase method)
        {
            var parameters = method.GetParameters();
            var methodNames = parameters.Select(static parameter => parameter.ParameterType.Name).ToArray();
            var signature = StringFormatter.Join("_", methodNames);
            var key = $"{ParameterKeyPrefix}{monoBehaviour.GetInstanceID()}_{method.Name}_{signature}";

            if (!_parameterValues.ContainsKey(key))
                _parameterValues[key] = new object[parameters.Length];

            var values = _parameterValues[key];

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(method.Name, EditorStyles.boldLabel);

            for (var i = 0; i < parameters.Length; i++)
                values[i] = DrawParameter(parameters[i], values[i]);

            if (GUILayout.Button($"Call {method.Name}"))
                method.Invoke(monoBehaviour, values);

            EditorGUILayout.EndVertical();
        }

        private static object DrawParameter(ParameterInfo parameter, object currentValue)
        {
            var label = ObjectNames.NicifyVariableName(parameter.Name);

            return parameter.ParameterType switch
            {
                var type when type == typeof(int) => EditorGUILayout.IntField(label,
                    currentValue is int intVal ? intVal : 0),
                var type when type == typeof(float) => EditorGUILayout.FloatField(label,
                    currentValue is float floatVal ? floatVal : 0f),
                var type when type == typeof(string) => EditorGUILayout.TextField(label,
                    currentValue as string ?? string.Empty),
                var type when type == typeof(bool) => EditorGUILayout.Toggle(label,
                    currentValue is true),
                var type when type.IsEnum => EditorGUILayout.EnumPopup(label,
                    currentValue as Enum ?? (Enum)Enum.GetValues(type).GetValue(0)),
                _ => currentValue
            };
        }
    }
}
#endif