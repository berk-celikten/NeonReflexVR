using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NeonReflexVR.Editor
{
    public static class GameLoopSceneSetup
    {
        [MenuItem("NeonReflexVR/Add Game Loop")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var roots = scene.GetRootGameObjects();
            if (roots.Any(root => root.GetComponent<GameManager>() != null)) throw new InvalidOperationException("GameManager already exists; no overwrite.");
            var spawner = roots.SelectMany(root => root.GetComponentsInChildren<TargetSpawner>()).Single();
            var shooters = roots.SelectMany(root => root.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
            if (shooters.Length != 2) throw new InvalidOperationException("Expected two shooters.");
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(GameSceneSetup.InputPath);
            var action = actions.FindAction("XRI Right Locomotion/Jump", true);
            var reference = AssetDatabase.LoadAllAssetsAtPath(GameSceneSetup.InputPath).OfType<InputActionReference>()
                .Single(item => item.action != null && item.action.id == action.id && (item.hideFlags & HideFlags.HideInHierarchy) == 0);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Game Status Canvas", typeof(RectTransform), typeof(Canvas));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = spawner.View.GetComponent<Camera>();
            var rect = canvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900f, 300f);
            rect.localScale = Vector3.one * 0.0015f;
            rect.position = spawner.View.position + Vector3.forward * 2f + Vector3.up * 0.55f;
            var labelObject = new GameObject("Game Status", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(rect, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontSize = 52;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;
            label.text = "Score: 0\nTime: 60";
            var manager = new GameObject("Game Manager").AddComponent<GameManager>();
            var managerData = new SerializedObject(manager);
            managerData.FindProperty("spawner").objectReferenceValue = spawner;
            managerData.FindProperty("restartAction").objectReferenceValue = reference;
            managerData.FindProperty("statusText").objectReferenceValue = label;
            managerData.ApplyModifiedPropertiesWithoutUndo();
            var spawnerData = new SerializedObject(spawner);
            spawnerData.FindProperty("autoStart").boolValue = false;
            spawnerData.ApplyModifiedPropertiesWithoutUndo();
            foreach (var shooter in shooters)
            {
                var data = new SerializedObject(shooter);
                data.FindProperty("gameManager").objectReferenceValue = manager;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
    }
}