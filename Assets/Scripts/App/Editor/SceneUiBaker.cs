#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SubMonitor.Auth;
using SubMonitor.SubscriptionsUI;

namespace SubMonitor.App.Editor
{
    public static class SceneUiBaker
    {
        private const string AuthScenePath = "Assets/Scenes/Auth.unity";
        private const string MainScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("Strelka/UI/Bake All Scenes")]
        public static void BakeAllScenes()
        {
            BakeScene(AuthScenePath, BakeAuthScene);
            BakeScene(MainScenePath, BakeMainScene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("UI baked into Auth and Main scenes.");
        }

        public static void BakeAllScenesFromBatchMode()
        {
            try
            {
                BakeAllScenes();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
                return;
            }

            EditorApplication.Exit(0);
        }

        private static void BakeScene(string scenePath, Action sceneBaker)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            sceneBaker.Invoke();
            EditorSceneManager.MarkSceneDirty(scene);

            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException("Failed to save scene: " + scenePath);
            }
        }

        private static void BakeAuthScene()
        {
            AuthScreenController controller = UnityEngine.Object.FindObjectOfType<AuthScreenController>();
            if (controller == null)
            {
                throw new InvalidOperationException("AuthScreenController not found in Auth scene.");
            }

            controller.RebuildInEditMode();
        }

        private static void BakeMainScene()
        {
            SubscriptionsScreenController controller = UnityEngine.Object.FindObjectOfType<SubscriptionsScreenController>();
            if (controller == null)
            {
                throw new InvalidOperationException("SubscriptionsScreenController not found in Main scene.");
            }

            controller.RebuildInEditMode();
        }
    }
}
#endif
