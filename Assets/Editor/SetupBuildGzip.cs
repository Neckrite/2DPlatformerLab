using UnityEngine;
using UnityEditor;

public static class SetupBuildGzip
{
    public static void SetupAndBuildGzip()
    {
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
            };
        }

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = false;
        Debug.Log("[LR3] WebGL compression set to Gzip, DecompressionFallback OFF");

        BuildManager.BuildWebGL();
    }
}
