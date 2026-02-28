#if UNITY_ANDROID
using CustomUtils.Editor.Scripts.InputDialog;
using CustomUtils.Runtime.Extensions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CustomUtils.Editor.Scripts
{
    internal sealed class EnvironmentVariableInjector : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!PlayerSettings.Android.useCustomKeystore)
                return;

            if (!Application.identifier.TryGetValueFromEnvironment(out var password))
            {
                Debug.LogWarning("No password found in environment variables! Using default or empty value.");
                password = EditorUtility.DisplayDialog("Password Required",
                    "No password found in environment variables. Would you like to enter it now?",
                    "Yes", "No")
                    ? EditorInputDialog.Show("Enter Password", "Password:", string.Empty)
                    : string.Empty;
            }

            PlayerSettings.Android.keystorePass = password;
            PlayerSettings.Android.keyaliasPass = password;

            Debug.Log("Password injected from environment variables.");
        }
    }
}

#endif