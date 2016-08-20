using UnityEngine;
using UnityEditor;
using System.Collections;

public class Build : Editor
{
    [MenuItem("Builds/Win 64bits/DRM Free Worldwide")]
    public static void BuildWin64NoDRMWorldwide()
    {
        string[] levels = new string[] {
            "Assets/Test_Levels/LoadingScreen.unity",
            "Assets/Test_Levels/Main_Menu.unity",
            "Assets/Test_Levels/Credits.unity",
            "Assets/Test_Levels/Tutorial.unity",
            "Assets/Test_Levels/Test_Level.unity",
            "Assets/Test_Levels/Death_Menu.unity"
        };

        // Overwrite LanguageLoader
        System.IO.File.Copy("Langs/LanguageLoader_NoDRM_WorldWide.prefab", "Resources/Langs/LanguageLoader.prefab", true);
        // Reimport the prefab, because otherwise Unity will use the "Library" imported version.
        AssetDatabase.Refresh();

        UnityEditor.PlayerSettings.runInBackground = false;
        string message = BuildPipeline.BuildPlayer(
            levels,
            "../Builds/DRMFree/Win64/GameTitle.exe",
            BuildTarget.StandaloneWindows64,
            BuildOptions.ShowBuiltPlayer
        );

        if (string.IsNullOrEmpty(message))
        {
            UnityEngine.Debug.Log("Win64 build complete");
        }
        else
        {
            UnityEngine.Debug.LogError("Error building Win64:\n" + message);
        }
    }
}
