// Assets/Editor/ZipProjectScripts.cs
// Unity Editor script to collect all C# scripts in Assets and zip them.

#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ZipProjectScripts
{
    [MenuItem("Tools/Export/Zip Project Scripts")]
    public static void ZipAllScripts()
    {
        try
        {
            // Find all MonoScripts (C#) in the project (Assets only)
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");
            if (guids == null || guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Zip Project Scripts", "No C# scripts found under Assets.", "OK");
                return;
            }

            // Resolve paths and filter to .cs within Assets/
            var scriptPaths = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                .Where(p => p.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            if (scriptPaths.Count == 0)
            {
                EditorUtility.DisplayDialog("Zip Project Scripts", "No .cs files found under Assets.", "OK");
                return;
            }

            // Ask user where to save the zip
            string defaultName = $"{SanitizeFileName(Application.productName)}_Scripts_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            string savePath = EditorUtility.SaveFilePanel("Save Scripts Zip", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), defaultName, "zip");
            if (string.IsNullOrEmpty(savePath))
                return; // user canceled

            // Create/overwrite zip
            if (File.Exists(savePath))
            {
                // Try delete if exists to avoid append behavior
                File.Delete(savePath);
            }

            // Build the zip and add each file with a path relative to project root
            using (FileStream zipToOpen = new FileStream(savePath, FileMode.CreateNew))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                for (int i = 0; i < scriptPaths.Count; i++)
                {
                    string assetPath = scriptPaths[i];

                    // Update progress
                    if (i % 10 == 0 || i == scriptPaths.Count - 1)
                    {
                        bool cancel = EditorUtility.DisplayCancelableProgressBar(
                            "Zipping Scripts",
                            $"Adding {Path.GetFileName(assetPath)} ({i + 1}/{scriptPaths.Count})",
                            (float)(i + 1) / scriptPaths.Count
                        );
                        if (cancel)
                        {
                            EditorUtility.ClearProgressBar();
                            EditorUtility.DisplayDialog("Zip Project Scripts", "Canceled by user.", "OK");
                            return;
                        }
                    }

                    string fullPath = Path.GetFullPath(assetPath);
                    if (!File.Exists(fullPath))
                        continue; // skip if somehow missing

                    // Keep folder structure starting at "Assets/"
                    string entryName = assetPath.Replace('\\', '/');

                    // Add file to zip
var entry = archive.CreateEntry(entryName, System.IO.Compression.CompressionLevel.Optimal);

                    using (var entryStream = entry.Open())
                    using (var fileStream = File.OpenRead(fullPath))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }

            EditorUtility.ClearProgressBar();
            EditorUtility.RevealInFinder(savePath);
            EditorUtility.DisplayDialog("Zip Project Scripts", $"Created:\n{savePath}", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Zip Project Scripts", $"Failed: {ex.Message}", "OK");
        }
    }

    private static string SanitizeFileName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "UnityProject";
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
    }
}
#endif
