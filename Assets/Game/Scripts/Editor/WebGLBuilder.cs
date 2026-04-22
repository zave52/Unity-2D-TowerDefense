#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TowerDefense.EditorTools
{
    public static class WebGLBuilder
    {
        [MenuItem("Tools/Tower Defense/Build/WebGL Optimized Build")]
        public static void BuildWebGL()
        {
            Debug.Log("[WebGLBuilder] Configuring WebGL Optimization Settings...");
            
            // Optimizations for WebGL
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Master);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.High);

            // Get scenes from Build Settings
            var scenes = EditorBuildSettings.scenes;
            string[] scenePaths = new string[scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenePaths[i] = scenes[i].path;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = "Builds/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };
            
            Debug.Log("[WebGLBuilder] Starting WebGL Build...");
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WebGLBuilder] Build succeeded: {summary.totalSize / 1024 / 1024} MB. Time: {summary.totalTime.TotalSeconds} seconds");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("[WebGLBuilder] Build failed");
            }
        }
    }
}
#endif
