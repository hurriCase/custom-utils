using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CustomUtils.Editor.Scripts.CustomEditorUtilities;
using UnityEditor;
using UnityEngine;
using ZLinq;

namespace CustomUtils.Editor.Scripts.UnusedAssemblyReference
{
    internal sealed class UnusedAssemblyReferenceAnalyzer : EditorWindow
    {
        [MenuItem(MenuItemNames.AnalyzeUnusedAssemblyReferencesMenuName)]
        public static void ShowWindow()
        {
            GetWindow<UnusedAssemblyReferenceAnalyzer>("Assembly Reference Analyzer");
        }

        private Vector2 _scrollPosition;
        private readonly Dictionary<string, List<string>> _potentiallyUnusedReferences = new();
        private bool _isAnalyzing;
        private float _progress;
        private string _currentAsmdef = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _hasError;

        private int _selectedAssemblyIndex;
        private string[] _assemblyNames = Array.Empty<string>();
        private string[] _assemblyPaths = Array.Empty<string>();

        private float _analysisStartTime;
        private const float AnalysisTimeout = 30f;

        private void OnEnable()
        {
            LoadAssemblyDefinitions();
        }

        private void LoadAssemblyDefinitions()
        {
            try
            {
                _statusMessage = "Loading assembly definitions...";
                var asmdefFiles = Directory.GetFiles(Application.dataPath, "*.asmdef", SearchOption.AllDirectories);

                var names = new List<string>();
                var paths = new List<string>();

                foreach (var asmdefPath in asmdefFiles)
                {
                    var json = File.ReadAllText(asmdefPath);
                    try
                    {
                        var asmdefData = JsonUtility.FromJson<AssemblyDefinitionData>(json);
                        if (asmdefData != null && !string.IsNullOrEmpty(asmdefData.Name))
                        {
                            names.Add(asmdefData.Name);
                            paths.Add(asmdefPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error parsing asmdef {asmdefPath}: {ex.Message}");
                    }
                }

                _assemblyNames = names.ToArray();
                _assemblyPaths = paths.ToArray();
                _statusMessage = $"Found {_assemblyNames.Length} assembly definitions";
            }
            catch (Exception ex)
            {
                _statusMessage = "Error loading assembly definitions: " + ex.Message;
                _hasError = true;
                Debug.LogException(ex);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Unused Assembly References Analyzer", EditorStyles.boldLabel);

            if (_hasError)
            {
                EditorVisualControls.ErrorBox(_statusMessage);

                if (!GUILayout.Button("Retry"))
                    return;

                _hasError = false;
                LoadAssemblyDefinitions();
                return;
            }

            DrawAssemblySelectionSection();

            DrawAnalysisButtons();

            if (_isAnalyzing)
                DrawAnalysisProgress();
            else if (_potentiallyUnusedReferences.Count > 0)
                DrawAnalysisResults();
        }

        private void DrawAssemblySelectionSection()
        {
            EditorVisualControls.DrawBoxedSection("Assembly Selection", () =>
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Select Assembly");
                if (_assemblyNames.Length > 0)
                    _selectedAssemblyIndex = EditorGUILayout.Popup(_selectedAssemblyIndex, _assemblyNames);
                else
                    EditorGUILayout.LabelField("No assembly definitions found");
                EditorGUILayout.EndHorizontal();
            });
        }

        private void DrawAnalysisButtons()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(_isAnalyzing || _assemblyNames.Length == 0);
            if (GUILayout.Button("Analyze Selected Assembly"))
            {
                _isAnalyzing = true;
                _progress = 0f;
                _potentiallyUnusedReferences.Clear();
                _analysisStartTime = Time.realtimeSinceStartup;

                var selectedAssemblyPath = _assemblyPaths[_selectedAssemblyIndex];
                var selectedAssemblyName = _assemblyNames[_selectedAssemblyIndex];

                EditorApplication.update += () => AnalyzeSingleAssembly(selectedAssemblyPath, selectedAssemblyName);
            }

            if (GUILayout.Button("Analyze All Assemblies"))
            {
                _isAnalyzing = true;
                _progress = 0f;
                _potentiallyUnusedReferences.Clear();
                _analysisStartTime = Time.realtimeSinceStartup;
                EditorApplication.update += AnalyzeProjectAsync;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawAnalysisProgress()
        {
            EditorVisualControls.DrawBoxedSection("Analysis Progress", () =>
            {
                if (Time.realtimeSinceStartup - _analysisStartTime > AnalysisTimeout)
                    EditorVisualControls.WarningBox(
                        "Analysis is taking longer than expected. It may be stuck on a large assembly. Click 'Cancel' to stop.");

                if (GUILayout.Button("Cancel Analysis"))
                    CancelAnalysis();

                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(false, 20), _progress,
                    "Analyzing: " + _currentAsmdef);
                GUILayout.Space(5);
                EditorGUILayout.LabelField(_statusMessage);
            });
        }

        private void DrawAnalysisResults()
        {
            EditorVisualControls.DrawBoxedSection("Analysis Results", () =>
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

                foreach (var asmdef in _potentiallyUnusedReferences)
                {
                    EditorVisualControls.DrawBoxedSection($"Assembly: {asmdef.Key}", () =>
                    {
                        if (asmdef.Value.Count == 0)
                            GUILayout.Label("No potentially unused references found.", EditorStyles.miniLabel);
                        else
                        {
                            GUILayout.Label("Potentially unused references:", EditorStyles.boldLabel);
                            foreach (var reference in asmdef.Value)
                            {
                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(20);

                                var displayName = reference;
                                if (reference.StartsWith("GUID:"))
                                {
                                    var guid = reference.Substring(5);
                                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                                    if (!string.IsNullOrEmpty(assetPath))
                                        try
                                        {
                                            var asmdefJson = File.ReadAllText(assetPath);
                                            var referencedAsmdef =
                                                JsonUtility.FromJson<AssemblyDefinitionData>(asmdefJson);

                                            if (referencedAsmdef != null &&
                                                !string.IsNullOrEmpty(referencedAsmdef.Name))
                                                displayName = $"{referencedAsmdef.Name} ({reference})";
                                        }
                                        catch (Exception)
                                        {
                                            displayName =
                                                $"{Path.GetFileNameWithoutExtension(assetPath)} ({reference})";
                                        }
                                }

                                GUILayout.Label($"• {displayName}");
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    });
                    GUILayout.Space(10);
                }

                GUILayout.EndScrollView();

                GUILayout.Space(10);
                EditorVisualControls.WarningBox(
                    "This analysis is based on static code scanning and may not catch dynamic references. Always test after removing references.");
            });
        }

        private void CancelAnalysis()
        {
            _isAnalyzing = false;
            EditorApplication.update -= AnalyzeProjectAsync;
            _statusMessage = "Analysis cancelled";
            Repaint();
        }

        private void AnalyzeSingleAssembly(string asmdefPath, string assemblyName)
        {
            try
            {
                _currentAsmdef = assemblyName;
                _statusMessage = $"Reading assembly definition for {assemblyName}";

                var json = File.ReadAllText(asmdefPath);
                var asmdefData = JsonUtility.FromJson<AssemblyDefinitionData>(json);

                if (asmdefData.References is { Length: > 0 })
                {
                    _statusMessage =
                        $"Analyzing references in {assemblyName} ({asmdefData.References.Length} references)";
                    var directory = Path.GetDirectoryName(asmdefPath);

                    var unused = FindUnusedReferences(directory, asmdefData.References);
                    _potentiallyUnusedReferences[assemblyName] = unused;
                }
                else
                    _potentiallyUnusedReferences[assemblyName] = new List<string>();

                _progress = 1f;
                _statusMessage = $"Completed analysis of {assemblyName}";
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _statusMessage = $"Error analyzing {assemblyName}: {ex.Message}";
                _hasError = true;
            }

            _isAnalyzing = false;
            EditorApplication.update -= () => AnalyzeSingleAssembly(asmdefPath, assemblyName);
            Repaint();
        }

        private void AnalyzeProjectAsync()
        {
            if (_assemblyPaths.Length == 0)
            {
                _statusMessage = "No assembly definitions found to analyze";
                _isAnalyzing = false;
                EditorApplication.update -= AnalyzeProjectAsync;
                return;
            }

            try
            {
                var totalFiles = _assemblyPaths.Length;
                var processedFiles = (int)(_progress * totalFiles);

                if (processedFiles >= totalFiles)
                {
                    _statusMessage = "Analysis complete";
                    _isAnalyzing = false;
                    EditorApplication.update -= AnalyzeProjectAsync;
                    return;
                }

                var asmdefPath = _assemblyPaths[processedFiles];
                var assemblyName = _assemblyNames[processedFiles];
                _currentAsmdef = assemblyName;

                _statusMessage = $"Analyzing {processedFiles + 1}/{totalFiles}: {assemblyName}";
                var json = File.ReadAllText(asmdefPath);

                try
                {
                    var asmdefData = JsonUtility.FromJson<AssemblyDefinitionData>(json);

                    if (asmdefData.References is { Length: > 0 })
                    {
                        var directory = Path.GetDirectoryName(asmdefPath);
                        var unused = FindUnusedReferences(directory, asmdefData.References);
                        _potentiallyUnusedReferences[assemblyName] = unused;
                    }
                    else
                        _potentiallyUnusedReferences[assemblyName] = new List<string>();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing asmdef {asmdefPath}: {ex.Message}");
                    _potentiallyUnusedReferences[assemblyName] = new List<string>();
                }

                processedFiles++;
                _progress = (float)processedFiles / totalFiles;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _statusMessage = "Error during analysis: " + ex.Message;
                _hasError = true;
                _isAnalyzing = false;
                EditorApplication.update -= AnalyzeProjectAsync;
            }

            Repaint();
        }

        private List<string> FindUnusedReferences(string directory, string[] references)
        {
            var potentiallyUnused = new List<string>(references);

            try
            {
                var scriptFiles = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);

                // Build a reference mapping table
                var referenceMappings = BuildReferenceMappings(references);

                var usedReferences = new HashSet<string>();

                // First pass: check for direct references
                foreach (var file in scriptFiles)
                {
                    var fileContent = File.ReadAllText(file);

                    foreach (var (originalRef, value) in referenceMappings)
                    {
                        if (!potentiallyUnused.Contains(originalRef) || usedReferences.Contains(originalRef))
                            continue;

                        if (IsReferenceUsed(fileContent, value))
                            usedReferences.Add(originalRef);
                    }
                }

                return potentiallyUnused.Where(r => !usedReferences.Contains(r)).ToList();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return new List<string>();
            }
        }

        private Dictionary<string, ReferenceInfo> BuildReferenceMappings(string[] references)
        {
            var mappings = new Dictionary<string, ReferenceInfo>();

            foreach (var reference in references)
            {
                var referenceInfo = new ReferenceInfo
                {
                    Name = ExtractAssemblyName(reference)
                };

                referenceInfo.Patterns = GetNamespacePatterns(referenceInfo.Name);

                mappings[reference] = referenceInfo;
            }

            return mappings;
        }

        private string ExtractAssemblyName(string reference)
        {
            var simpleName = reference;

            try
            {
                if (reference.StartsWith("GUID:"))
                {
                    var guid = reference[5..];
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        var asmdefJson = File.ReadAllText(assetPath);
                        var referencedAsmdef = JsonUtility.FromJson<AssemblyDefinitionData>(asmdefJson);

                        if (referencedAsmdef != null && !string.IsNullOrEmpty(referencedAsmdef.Name))
                        {
                            simpleName = referencedAsmdef.Name;

                            if (!string.IsNullOrEmpty(referencedAsmdef.RootNamespace))
                                return referencedAsmdef.RootNamespace;
                        }
                    }
                }

                if (reference.Contains('.'))
                    simpleName = reference.Split('.').Last();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error extracting assembly name from {reference}: {ex.Message}");
            }

            return simpleName;
        }

        private List<Regex> GetNamespacePatterns(string assemblyName)
        {
            var patterns = new List<Regex>();

            // Common pattern 1: Direct namespace usage
            patterns.Add(new Regex($@"using\s+{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_]+)*\s*;",
                RegexOptions.Compiled));

            // Common pattern 2: Fully qualified type usage
            patterns.Add(new Regex($@"{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_]+)+", RegexOptions.Compiled));

            // Common pattern 3: Interface or base class implementation
            patterns.Add(new Regex($@":\s*(([A-Za-z0-9_.<>]+,\s*)*)?{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_.<>]+)+",
                RegexOptions.Compiled));

            // Common pattern 4: Attribute usage
            patterns.Add(new Regex($@"\[\s*{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_]+)+", RegexOptions.Compiled));

            // Common pattern 5: Namespace declaration
            patterns.Add(new Regex($@"namespace\s+{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_]+)*",
                RegexOptions.Compiled));

            // Common pattern 6: Type parameter constraints (where T : SomeType)
            patterns.Add(new Regex($@"where\s+[A-Za-z0-9_]+\s*:\s*{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_.<>]+)+",
                RegexOptions.Compiled));

            // Common pattern 7: Type casts
            patterns.Add(new Regex($@"\(\s*{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_.<>]+)+\s*\)",
                RegexOptions.Compiled));

            // Common pattern 8: Generic type arguments
            patterns.Add(new Regex($@"<\s*([^<>]*,\s*)*{Regex.Escape(assemblyName)}(\.[A-Za-z0-9_.<>]+)+",
                RegexOptions.Compiled));

            return patterns;
        }

        private bool IsReferenceUsed(string fileContent, ReferenceInfo reference)
        {
            try
            {
                return fileContent.Contains(reference.Name) && reference.Patterns
                    .Any(pattern => pattern.IsMatch(fileContent));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error checking for reference to {reference.Name}: {ex.Message}");
                return false;
            }
        }
    }
}