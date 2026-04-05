#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TowerDefense.EditorTools
{
    /// <summary>
    /// Workaround for editor callbacks that expect Library/LastBuild.buildreport to exist.
    /// Some package post-build hooks throw FileNotFoundException when the file is missing.
    /// </summary>
    [InitializeOnLoad]
    public sealed class BuildReportWorkaround : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private static readonly string ReportPath = Path.Combine(GetProjectRoot(), "Library", "LastBuild.buildreport");

        static BuildReportWorkaround()
        {
            EnsureReportFileExists();
        }

        public int callbackOrder => int.MinValue;

        public void OnPreprocessBuild(BuildReport report)
        {
            EnsureReportFileExists();
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            EnsureReportFileExists();
            EditorApplication.delayCall += EnsureReportFileExists;
        }

        [MenuItem("Tools/Tower Defense/Fixes/Create Missing LastBuild.buildreport")]
        private static void CreateMissingBuildReportFile()
        {
            EnsureReportFileExists();
            Debug.Log($"[BuildFix] Ensured build report file exists at: {ReportPath}");
        }

        private static void EnsureReportFileExists()
        {
            try
            {
                var directory = Path.GetDirectoryName(ReportPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (!File.Exists(ReportPath))
                {
                    using var stream = File.Create(ReportPath);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BuildFix] Could not ensure LastBuild.buildreport exists. {ex.Message}");
            }
        }

        private static string GetProjectRoot()
        {
            var dataPath = Application.dataPath;
            return Directory.GetParent(dataPath)?.FullName ?? dataPath;
        }
    }
}
#endif

