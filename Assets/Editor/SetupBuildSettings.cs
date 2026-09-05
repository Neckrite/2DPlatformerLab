using UnityEditor;
using UnityEngine;

public static class SetupBuildSettings
{
    public static void SetupAndBuild()
    {
        // Ensure the scene is in build settings
        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || scenes.Length == 0)
        {
            Debug.Log("[Setup] Adding scene to Build Settings...");
            var newScenes = new EditorBuildSettingsScene[] 
            { 
                new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true) 
            };
            EditorBuildSettings.scenes = newScenes;
        }

        // Disable WebGL compression
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        Debug.Log("[Setup] WebGL compression set to Disabled");
        
        BuildManager.BuildWebGL();
    }
}
