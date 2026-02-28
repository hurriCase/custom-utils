using System;
using System.Collections.Generic;
using System.IO;
using CustomUtils.Editor.Scripts.CustomMenu.MenuItems.Helpers;
using CustomUtils.Editor.Scripts.CustomMenu.MenuItems.MenuItems;
using CustomUtils.Editor.Scripts.CustomMenu.MenuItems.MenuItems.MethodExecution;
using UnityEditor;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.CustomMenu
{
    internal static class MenuController
    {
        private const string ScriptPath =
            "Assets/Editor Default Resources/CustomMenu/Scripts/Editor/GeneratedMenuItems.cs";

        internal static void GenerateMenuItemsScriptFromSettings(CustomMenuSettings settings)
        {
            var scriptContent = GenerateMenuItemsScriptContentFromSettings(settings);

            if (string.IsNullOrWhiteSpace(scriptContent))
                return;

            WriteScriptFile(scriptContent);
        }

        private static void WriteScriptFile(string scriptContent)
        {
            var directory = Path.GetDirectoryName(ScriptPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(ScriptPath, scriptContent);
            AssetDatabase.Refresh();

            Debug.Log("Generated menu items script successfully at: " + ScriptPath);
        }

        private static string GenerateMenuItemsScriptContentFromSettings(CustomMenuSettings settings)
        {
            var content = @"using CustomUtils.Editor.Scripts.CustomMenu.MenuItems.Helpers;
using CustomUtils.Editor.Scripts.CustomMenu.MenuItems.MenuItems.MethodExecution;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor_Default_Resources.CustomMenu.Scripts.Editor
{
    internal static class GeneratedMenuItems
    {";

            var isFirstMenuItem = true;
            var usedMethodNames = new HashSet<string>();
            var usedMenuPaths = new HashSet<string>();
            var usedShortcuts = new HashSet<string>();

            if (!GenerateSceneMenuItems(settings,
                    ref content, ref isFirstMenuItem, usedMethodNames, usedMenuPaths, usedShortcuts)
                || !GenerateAssetMenuItems(settings,
                    ref content, ref isFirstMenuItem, usedMethodNames, usedMenuPaths, usedShortcuts)
                || !GeneratePrefabMenuItems(settings,
                    ref content, ref isFirstMenuItem, usedMethodNames, usedMenuPaths, usedShortcuts)
                || !GenerateMethodExecutionMenuItems(settings,
                    ref content, ref isFirstMenuItem, usedMethodNames, usedMenuPaths, usedShortcuts)
                || !GenerateScriptingSymbolMenuItems(settings,
                    ref content, ref isFirstMenuItem, usedMethodNames, usedMenuPaths, usedShortcuts))
                return string.Empty;

            content += @"
    }
}";

            return content;
        }

        private static bool GenerateSceneMenuItems(
            CustomMenuSettings settings,
            ref string content,
            ref bool isFirstMenuItem,
            HashSet<string> usedMethodNames,
            HashSet<string> usedMenuPaths,
            HashSet<string> usedShortcuts)
        {
            if (settings.SceneMenuItems == null)
                return true;

            foreach (var item in settings.SceneMenuItems)
            {
                if (!MenuValidationHelper.Validate(item.MenuTarget, item.MenuPath, item.Shortcut, item.SceneName,
                        "scene", usedMenuPaths, usedShortcuts))
                    return false;

                var baseMethodName = $"OpenScene{MenuValidationHelper.SanitizeMethodName(item.SceneName)}";
                var methodName = MenuValidationHelper.GetUniqueMethodName(baseMethodName, usedMethodNames);

                AddMethodSeparator(ref content, ref isFirstMenuItem);

                content += $@"
        [MenuItem(""{item.GetMenuPathWithShortcut()}"", priority = {item.Priority})]
        private static void {methodName}()
        {{
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo() is false)
                return;

            var scenePath = ""{item.ScenePath}"";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        }}";
            }

            return true;
        }

        private static bool GenerateAssetMenuItems(
            CustomMenuSettings settings,
            ref string content,
            ref bool isFirstMenuItem,
            HashSet<string> usedMethodNames,
            HashSet<string> usedMenuPaths,
            HashSet<string> usedShortcuts)
        {
            if (settings.AssetMenuItems == null)
                return true;

            foreach (var item in settings.AssetMenuItems)
            {
                if (!MenuValidationHelper.Validate(item.MenuTarget, item.MenuPath, item.Shortcut,
                        item.MenuTarget.name, "asset", usedMenuPaths, usedShortcuts))
                    return false;

                var baseMethodName = $"SelectAsset{MenuValidationHelper.SanitizeMethodName(item.MenuTarget.name)}";
                var methodName = MenuValidationHelper.GetUniqueMethodName(baseMethodName, usedMethodNames);
                var assetPath = AssetDatabase.GetAssetPath(item.MenuTarget);

                AddMethodSeparator(ref content, ref isFirstMenuItem);

                content += $@"
        [MenuItem(""{item.GetMenuPathWithShortcut()}"", priority = {item.Priority})]
        private static void {methodName}()
        {{
            var asset = AssetDatabase.LoadAssetAtPath<Object>(""{assetPath}"");
            Selection.activeObject = asset;
        }}";
            }

            return true;
        }

        private static bool GeneratePrefabMenuItems(
            CustomMenuSettings settings,
            ref string content,
            ref bool isFirstMenuItem,
            HashSet<string> usedMethodNames,
            HashSet<string> usedMenuPaths,
            HashSet<string> usedShortcuts)
        {
            if (settings.PrefabMenuItems == null)
                return true;

            foreach (var item in settings.PrefabMenuItems)
            {
                if (!MenuValidationHelper.Validate(item.MenuTarget, item.MenuPath, item.Shortcut,
                        item.MenuTarget.name, "prefab", usedMenuPaths, usedShortcuts))
                    return false;

                var baseMethodName = $"Create{MenuValidationHelper.SanitizeMethodName(item.MenuTarget.name)}";
                var methodName = MenuValidationHelper.GetUniqueMethodName(baseMethodName, usedMethodNames);
                var prefabPath = AssetDatabase.GetAssetPath(item.MenuTarget);

                AddMethodSeparator(ref content, ref isFirstMenuItem);

                content += GeneratePrefabMenuItemContent(item, methodName, prefabPath);
            }

            return true;
        }

        private static bool GenerateMethodExecutionMenuItems(
            CustomMenuSettings settings,
            ref string content,
            ref bool isFirstMenuItem,
            HashSet<string> usedMethodNames,
            HashSet<string> usedMenuPaths,
            HashSet<string> usedShortcuts)
        {
            if (settings.MethodExecutionItems == null)
                return true;

            foreach (var item in settings.MethodExecutionItems)
            {
                if (!MenuValidationHelper.Validate(item.MenuTarget, item.MenuPath, item.Shortcut,
                        item.MenuTarget.ToString(), "method", usedMenuPaths, usedShortcuts))
                    return false;

                AddMethodSeparator(ref content, ref isFirstMenuItem);

                content += GenerateMethodExecutionContent(item, usedMethodNames);
            }

            return true;
        }

        private static bool GenerateScriptingSymbolMenuItems(
            CustomMenuSettings settings,
            ref string content,
            ref bool isFirstMenuItem,
            HashSet<string> usedMethodNames,
            HashSet<string> usedMenuPaths,
            HashSet<string> usedShortcuts)
        {
            if (settings.ScriptingSymbols == null)
                return true;

            foreach (var item in settings.ScriptingSymbols)
            {
                if (!MenuValidationHelper.Validate(item.MenuTarget, item.MenuPath, item.Shortcut,
                        item.MenuTarget, "symbol", usedMenuPaths, usedShortcuts))
                    return false;

                var baseMethodName = $"ToggleSymbol_{MenuValidationHelper.SanitizeMethodName(item.MenuTarget)}";
                var methodName = MenuValidationHelper.GetUniqueMethodName(baseMethodName, usedMethodNames);
                var validateMethodName = $"Validate{methodName}";
                var prefsKey = item.GetPrefsKey();

                AddMethodSeparator(ref content, ref isFirstMenuItem);

                content += $@"
        [MenuItem(""{item.GetMenuPathWithShortcut()}"", priority = {item.Priority})]
        private static void {methodName}()
        {{
            ScriptingSymbolHandler.ToggleSymbol(""{item.MenuTarget}"", ""{prefsKey}"");
        }}

        [MenuItem(""{item.MenuPath}"", true)]
        private static bool {validateMethodName}()
        {{
            Menu.SetChecked(""{item.MenuPath}"", ScriptingSymbolHandler.IsSymbolEnabled(""{prefsKey}""));
            return true;
        }}";
            }

            return true;
        }

        private static void AddMethodSeparator(ref string content, ref bool isFirstMenuItem)
        {
            if (isFirstMenuItem)
                isFirstMenuItem = false;
            else
                content += "\n";
        }

        private static string GeneratePrefabMenuItemContent(
            PrefabMenuItem item,
            string methodName,
            string prefabPath)
        {
            var content = $@"
        [MenuItem(""{item.GetMenuPathWithShortcut()}"", priority = {item.Priority})]
        private static void {methodName}(MenuCommand menuCommand)
        {{
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(""{prefabPath}"");

            if (!prefab)
            {{
                Debug.LogError(""[GeneratedMenuItems::{methodName}] Prefab not found at path: {prefabPath}"");
                return;
            }}

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            if (!instance)
            {{
                Debug.LogError(""[GeneratedMenuItems::{methodName}] Failed to instantiate prefab"");
                return;
            }}

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage)
            {{
                var selectedInPrefab = Selection.activeGameObject;
                if (selectedInPrefab && prefabStage.IsPartOfPrefabContents(selectedInPrefab))
                    instance.transform.SetParent(selectedInPrefab.transform);
                else
                    instance.transform.SetParent(prefabStage.prefabContentsRoot.transform);

                instance.transform.localPosition = Vector3.zero;
            }}
            else
                GameObjectUtility.SetParentAndAlign(instance, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(instance, ""Create "" + instance.name);

            Selection.activeObject = instance;
        }}";

            return content;
        }

        private static string GenerateMethodExecutionContent(MethodExecutionMenuItem menuItem,
            HashSet<string> usedMethodNames)
        {
            var baseMethodName = menuItem.MenuTarget.ToString();
            var methodName = MenuValidationHelper.GetUniqueMethodName(baseMethodName, usedMethodNames);

            return menuItem.MenuTarget switch
            {
                MethodExecutionType.DeleteAllStoredData => $@"
        [MenuItem(""{menuItem.GetMenuPathWithShortcut()}"", priority = {menuItem.Priority})]
        private static void {methodName}()
        {{
            StorageHelper.TryDeleteAllAsync().Forget();
        }}",

                MethodExecutionType.ToggleDefaultSceneAutoLoad => $@"
        [MenuItem(""{menuItem.GetMenuPathWithShortcut()}"", priority = {menuItem.Priority})]
        private static void {methodName}()
        {{
            DefaultSceneLoader.ToggleAutoLoad();
        }}

        [MenuItem(""{menuItem.MenuPath}"", true)]
        private static bool Validate{methodName}()
        {{
            Menu.SetChecked(""{menuItem.MenuPath}"", DefaultSceneLoader.IsDefaultSceneSet());
            return true;
        }}",

                _ => throw new ArgumentOutOfRangeException(nameof(menuItem.MenuTarget), menuItem.MenuTarget, null)
            };
        }
    }
}