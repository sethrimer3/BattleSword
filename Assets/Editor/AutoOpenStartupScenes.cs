#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class AutoOpenStartupScenes
{
    private const string OpenedThisEditorSessionKey = "AutoOpenStartupScenes_OpenedThisSession";

    // Change these paths to match your real scene paths.
    private static readonly string[] StartupScenePaths =
    {
        "Assets/Scenes/Dev/LAN_Multiplayer_Test.unity"
        "Assets/Infima Games/Low Poly Shooter Pack - Free Sample/Scenes/S_Content_Overview.unity"
    };

    [InitializeOnLoadMethod]
    private static void OnEditorLoaded()
    {
        EditorApplication.delayCall += OpenStartupScenesOnce;
    }

    private static void OpenStartupScenesOnce()
    {
        if (SessionState.GetBool(OpenedThisEditorSessionKey, false))
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        SessionState.SetBool(OpenedThisEditorSessionKey, true);

        bool openedAnyScene = false;

        foreach (string scenePath in StartupScenePaths)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (sceneAsset == null)
            {
                UnityEngine.Debug.LogWarning($"Startup scene not found: {scenePath}");
                continue;
            }

            OpenSceneMode mode = openedAnyScene
                ? OpenSceneMode.Additive
                : OpenSceneMode.Single;

            EditorSceneManager.OpenScene(scenePath, mode);
            openedAnyScene = true;
        }
    }

    [MenuItem("Tools/Scenes/Open Startup Scenes Now")]
    private static void OpenStartupScenesManually()
    {
        bool openedAnyScene = false;

        foreach (string scenePath in StartupScenePaths)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            if (sceneAsset == null)
            {
                UnityEngine.Debug.LogWarning($"Startup scene not found: {scenePath}");
                continue;
            }

            OpenSceneMode mode = openedAnyScene
                ? OpenSceneMode.Additive
                : OpenSceneMode.Single;

            EditorSceneManager.OpenScene(scenePath, mode);
            openedAnyScene = true;
        }
    }
}
#endif