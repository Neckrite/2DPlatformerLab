using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildManager
{
    private static readonly string WebGLBuildPath = "Builds/WebGL";

    public static void BuildWebGL()
    {
        Debug.Log("[CI/CD] Starting automatic WebGL build process...");

        string[] levels = GetScenes();
        if (levels.Length == 0)
        {
            Debug.LogError("[CI/CD] Error: No active scenes found in Build Settings!");
            ExitWithCode(1);
            return;
        }

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = levels,
            locationPathName = WebGLBuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[CI/CD] SUCCESS! WebGL build created successfully.");
            Debug.Log("[CI/CD] Build time: " + summary.totalTime.TotalSeconds.ToString("F2") + " sec. Size: " + summary.totalSize + " bytes.");
            ExitWithCode(0);
        }
        else
        {
            Debug.LogError("[CI/CD] BUILD ERROR! Error count: " + summary.totalErrors);
            ExitWithCode(1);
        }
    }

    private static string[] GetScenes()
    {
        var editorScenes = EditorBuildSettings.scenes;

        int activeCount = 0;
        foreach (var scene in editorScenes)
        {
            if (scene.enabled) activeCount++;
        }
        string[] scenePaths = new string[activeCount];
        int index = 0;

        foreach (var scene in editorScenes)
        {
            if (scene.enabled)
            {
                scenePaths[index] = scene.path;
                index++;
            }
        }
        return scenePaths;
    }

    private static void ExitWithCode(int code)
    {
        if (System.Environment.CommandLine.Contains("-batchmode"))
        {
            EditorApplication.Exit(code);
        }
    }
}
