using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Build;

namespace Assets.Scripts.Editor
{
    public class ZolianBuildMenu : EditorWindow
    {
        private static readonly string ClientBuildPath = "Builds/Client/";
        private static readonly string ServerBuildPath = "Builds/Server/";
        
        private BuildTarget selectedPlatform = BuildTarget.StandaloneWindows64;
        private bool developmentBuild = false;
        private bool autoConnectProfiler = false;
        private bool scriptDebugging = false;
        
        [MenuItem("Zolian/Build Options")]
        public static void ShowWindow()
        {
            var window = GetWindow<ZolianBuildMenu>("Zolian Build Options");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        [MenuItem("Zolian/Quick Build/Client Build")]
        public static void QuickBuildClient()
        {
            BuildClient(BuildTarget.StandaloneWindows64, false);
        }

        [MenuItem("Zolian/Quick Build/Server Build")]
        public static void QuickBuildServer()
        {
            BuildServer(BuildTarget.StandaloneWindows64, false);
        }

        [MenuItem("Zolian/Quick Build/Development Client")]
        public static void QuickBuildDevClient()
        {
            BuildClient(BuildTarget.StandaloneWindows64, true);
        }

        [MenuItem("Zolian/Quick Build/Development Server")]
        public static void QuickBuildDevServer()
        {
            BuildServer(BuildTarget.StandaloneWindows64, true);
        }

        private void OnGUI()
        {
            GUILayout.Label("Zolian Build Configuration", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // Platform selection
            GUILayout.Label("Target Platform:", EditorStyles.label);
            selectedPlatform = (BuildTarget)EditorGUILayout.EnumPopup(selectedPlatform);
            
            EditorGUILayout.Space();
            
            // Build options
            GUILayout.Label("Build Options:", EditorStyles.label);
            developmentBuild = EditorGUILayout.Toggle("Development Build", developmentBuild);
            
            if (developmentBuild)
            {
                EditorGUI.indentLevel++;
                autoConnectProfiler = EditorGUILayout.Toggle("Autoconnect Profiler", autoConnectProfiler);
                scriptDebugging = EditorGUILayout.Toggle("Script Debugging", scriptDebugging);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Build buttons
            GUILayout.Label("Build Targets:", EditorStyles.label);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build Client", GUILayout.Height(40)))
            {
                BuildClient(selectedPlatform, developmentBuild);
            }
            
            if (GUILayout.Button("Build Server", GUILayout.Height(40)))
            {
                BuildServer(selectedPlatform, developmentBuild);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Build Both", GUILayout.Height(50)))
            {
                BuildClient(selectedPlatform, developmentBuild);
                BuildServer(selectedPlatform, developmentBuild);
            }
            
            EditorGUILayout.Space();
            
            // Information
            EditorGUILayout.HelpBox(
                "Client Build: Standard Zolian client that connects to Lobby/Login servers, then to FishNet World Server.\n\n" +
                "Server Build: Dedicated Zolian World Server using FishNet and Unity Physics. Bypasses login and starts World scene directly.",
                MessageType.Info);
        }

        private static void BuildClient(BuildTarget target, bool development)
        {
            Debug.Log("Starting Zolian Client build...");
            
            var scenes = GetScenesInBuild();
            var buildPath = GetBuildPath(ClientBuildPath, target, "ZolianClient");
            
            var buildOptions = BuildOptions.None;
            if (development)
            {
                buildOptions |= BuildOptions.Development;
                if (EditorApplication.isPlaying)
                    buildOptions |= BuildOptions.ConnectWithProfiler;
            }
            
            // Set client-specific defines
            SetScriptingDefines(target, new[] { "ZOLIAN_CLIENT" });
            
            var buildReport = BuildPipeline.BuildPlayer(scenes, buildPath, target, buildOptions);
            
            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Client build completed successfully: {buildPath}");
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"Client build failed: {buildReport.summary.result}");
            }
        }

        private static void BuildServer(BuildTarget target, bool development)
        {
            Debug.Log("Starting Zolian Server build...");
            
            var scenes = GetServerScenesInBuild();
            var buildPath = GetBuildPath(ServerBuildPath, target, "ZolianServer");
            
            var buildOptions = BuildOptions.EnableHeadlessMode;
            if (development)
            {
                buildOptions |= BuildOptions.Development;
            }
            
            // Set server-specific defines
            SetScriptingDefines(target, new[] { "ZOLIAN_SERVER", "UNITY_SERVER" });
            
            var buildReport = BuildPipeline.BuildPlayer(scenes, buildPath, target, buildOptions);
            
            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Server build completed successfully: {buildPath}");
                
                // Create server startup script
                CreateServerStartupScript(Path.GetDirectoryName(buildPath));
                
                EditorUtility.RevealInFinder(buildPath);
            }
            else
            {
                Debug.LogError($"Server build failed: {buildReport.summary.result}");
            }
            
            // Reset defines
            SetScriptingDefines(target, new string[0]);
        }

        private static string[] GetScenesInBuild()
        {
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }
            return scenes.ToArray();
        }

        private static string[] GetServerScenesInBuild()
        {
            // For server builds, we want to skip Lobby/Login and go straight to World
            var scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    // Skip lobby scene for server builds, or modify this logic as needed
                    if (!scene.path.Contains("Lobby") || scene.path.Contains("World"))
                        scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }

        private static string GetBuildPath(string basePath, BuildTarget target, string executableName)
        {
            var platformFolder = target.ToString();
            var extension = GetExecutableExtension(target);
            
            var fullPath = Path.Combine(basePath, platformFolder, $"{executableName}{extension}");
            
            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            
            return fullPath;
        }

        private static string GetExecutableExtension(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.StandaloneLinux64 => "",
                BuildTarget.StandaloneOSX => ".app",
                _ => ""
            };
        }

        private static void SetScriptingDefines(BuildTarget target, string[] defines)
        {
            var group = BuildPipeline.GetBuildTargetGroup(target);
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(group);
            
            var existingDefines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            var definesList = new List<string>(existingDefines.Split(';'));
            
            // Remove old Zolian defines
            definesList.RemoveAll(d => d.StartsWith("ZOLIAN_") || d == "UNITY_SERVER");
            
            // Add new defines
            definesList.AddRange(defines);
            
            var newDefines = string.Join(";", definesList);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newDefines);
            
            Debug.Log($"Set scripting defines for {target}: {newDefines}");
        }

        private static void CreateServerStartupScript(string buildDirectory)
        {
            var batchContent = @"@echo off
echo Starting Zolian World Server...
ZolianServer.exe -batchmode -nographics -server
pause";

            var shContent = @"#!/bin/bash
echo ""Starting Zolian World Server...""
./ZolianServer -batchmode -nographics -server";

            File.WriteAllText(Path.Combine(buildDirectory, "StartServer.bat"), batchContent);
            File.WriteAllText(Path.Combine(buildDirectory, "StartServer.sh"), shContent);
            
            Debug.Log("Created server startup scripts: StartServer.bat and StartServer.sh");
        }
    }
}