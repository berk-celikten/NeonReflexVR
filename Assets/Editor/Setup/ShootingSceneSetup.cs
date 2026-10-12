using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonReflexVR.Editor
{
    public static class ShootingSceneSetup
    {
        [MenuItem("NeonReflexVR/Add Initial Shooting")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open the saved GameScene outside Play Mode before configuring shooting.");
            var transforms = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true)).ToArray();
            var right = transforms.Single(item => item.name == "Right Controller");
            var cubes = new[] { "Red Cube", "Green Cube", "Blue Cube" }.Select(name => transforms.Single(item => item.name == name)).ToArray();
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(GameSceneSetup.InputPath);
            var action = actions.FindAction("XRI Right Interaction/Activate", true);
            var reference = AssetDatabase.LoadAllAssetsAtPath(GameSceneSetup.InputPath).OfType<InputActionReference>()
                .Single(item => item.action != null && item.action.id == action.id && (item.hideFlags & HideFlags.HideInHierarchy) == 0);
            if (right.GetComponent<VRRaycastShooter>() != null)
                throw new InvalidOperationException("Shooting already exists; this setup does not overwrite it.");
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) throw new InvalidOperationException("URP Unlit shader is required.");
            const string materialPath = "Assets/Materials/VRTest/ShotLaser.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null)
                throw new InvalidOperationException("Laser material already exists; inspect before setup.");
            var material = new Material(shader) { name = "ShotLaser" };
            material.SetColor("_BaseColor", new Color(0.1f, 1f, 1f));
            AssetDatabase.CreateAsset(material, materialPath);
            var laserObject = new GameObject("Shot Laser");
            laserObject.transform.SetParent(right, false);
            var line = laserObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = 0.008f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            line.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            line.enabled = false;
            var shooter = right.gameObject.AddComponent<VRRaycastShooter>();
            var serialized = new SerializedObject(shooter);
            serialized.FindProperty("triggerAction").objectReferenceValue = reference;
            serialized.FindProperty("laser").objectReferenceValue = line;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            foreach (var cube in cubes) cube.gameObject.AddComponent<TestTargetFeedback>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save GameScene.");
            AssetDatabase.SaveAssets();
        }
    }
}