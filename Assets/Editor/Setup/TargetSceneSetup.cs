using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NeonReflexVR.Editor
{
    public static class TargetSceneSetup
    {
        public const string PrefabPath = "Assets/Prefabs/Targets/Target.prefab";
        [MenuItem("NeonReflexVR/Add Random Targets")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            if (scene.GetRootGameObjects().Any(root => root.GetComponent<TargetSpawner>() != null) || AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
                throw new InvalidOperationException("Target setup already exists; no overwrite.");
            var camera = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Camera>()).Single();
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Targets")) AssetDatabase.CreateFolder("Assets/Prefabs", "Targets");
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = "Target" };
            material.SetColor("_BaseColor", new Color(1f, 0.35f, 0.08f));
            AssetDatabase.CreateAsset(material, "Assets/Materials/VRTest/Target.mat");
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Target";
            sphere.transform.localScale = Vector3.one * 0.3f;
            sphere.GetComponent<Renderer>().sharedMaterial = material;
            sphere.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            sphere.GetComponent<Renderer>().receiveShadows = false;
            sphere.AddComponent<TestTargetFeedback>();
            sphere.AddComponent<Target>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(sphere, PrefabPath);
            UnityEngine.Object.DestroyImmediate(sphere);
            var spawner = new GameObject("Target Spawner").AddComponent<TargetSpawner>();
            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("targetPrefab").objectReferenceValue = prefab.GetComponent<Target>();
            serialized.FindProperty("view").objectReferenceValue = camera.transform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
    }
}