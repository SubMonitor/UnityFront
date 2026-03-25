#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SubMonitor.Auth.Editor
{
    public static class AuthSceneCreator
    {
        private const string ScenePath = "Assets/Scenes/Auth.unity";

        [MenuItem("Strelka/Auth/Create Or Rebuild Auth Scene")]
        public static void CreateOrRebuildAuthScene()
        {
            SceneAsset existingScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (existingScene != null)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                Selection.activeGameObject = GameObject.Find("AuthScreenRoot");
                Debug.Log("Opened Auth scene for manual scene editing.");
                return;
            }

            string sceneDirectory = Path.GetDirectoryName(ScenePath);
            if (!string.IsNullOrWhiteSpace(sceneDirectory) && !Directory.Exists(sceneDirectory))
            {
                Directory.CreateDirectory(sceneDirectory);
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject root = new GameObject("AuthScreenRoot");
            root.AddComponent<AuthScreenController>();

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError("Failed to save Auth scene.");
                return;
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Selection.activeGameObject = GameObject.Find("AuthScreenRoot");
            Debug.Log("Created an empty Auth scene. UI should now be built directly in the editor.");
        }

        [MenuItem("Strelka/Auth/Open Auth Scene", true)]
        private static bool ValidateOpenAuthScene()
        {
            return AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null;
        }

        [MenuItem("Strelka/Auth/Open Auth Scene")]
        private static void OpenAuthScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
    }
}
#endif
