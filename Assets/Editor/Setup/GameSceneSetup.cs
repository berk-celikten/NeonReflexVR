using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace NeonReflexVR.Editor
{
    /// <summary>Creates the initial tracking-only scene without editing existing scenes.</summary>
    public static class GameSceneSetup
    {
        public const string ScenePath = "Assets/Scenes/GameScene.unity";
        public const string InputPath = "Assets/Input/XRI Default Input Actions.inputactions";
        const string MaterialFolder = "Assets/Materials/VRTest";

        [MenuItem("NeonReflexVR/Create Initial GameScene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling)
                throw new InvalidOperationException("Stop Play Mode and wait for compilation before creating the scene.");
            if (File.Exists(ScenePath) || (Directory.Exists(MaterialFolder) && Directory.EnumerateFiles(MaterialFolder, "*", SearchOption.AllDirectories).Any()))
                throw new InvalidOperationException("GameScene output already exists; creation will not overwrite it.");
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save or discard current scene changes before creating GameScene.");
            if (Shader.Find("Universal Render Pipeline/Unlit") == null)
                throw new InvalidOperationException("URP Unlit shader is required.");

            ImportOfficialInputActions();
            EnsureFolder(MaterialFolder);
            var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var references = AssetDatabase.LoadAllAssetsAtPath(InputPath).OfType<InputActionReference>().ToArray();
            foreach (var map in new[] { "XRI Head", "XRI Left", "XRI Right" })
                foreach (var action in new[] { "Position", "Rotation", "Tracking State" })
                    RequireReference(actions, references, map + "/" + action);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // NewScene can unload unused assets. Resolve imported references again for the new scene.
            actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            references = AssetDatabase.LoadAllAssetsAtPath(InputPath).OfType<InputActionReference>().ToArray();
            var rig = new GameObject("XR Origin (VR)");
            var origin = rig.AddComponent<XROrigin>();
            var offset = new GameObject("Camera Offset");
            offset.transform.SetParent(rig.transform, false);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(offset.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.015f, 0.025f, 0.06f, 1f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 30f;
            camera.allowHDR = false;
            var cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.allowXRRendering = true;
            cameraData.renderPostProcessing = false;
            cameraObject.AddComponent<AudioListener>();
            AddTracking(cameraObject, actions, references, "XRI Head");
            origin.Origin = rig;
            origin.CameraFloorOffsetObject = offset;
            origin.Camera = camera;
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            origin.CameraYOffset = 1.6f;

            var inputManager = rig.AddComponent<InputActionManager>();
            inputManager.actionAssets = new List<InputActionAsset> { actions };
            new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();

            var leftMaterial = CreateMaterial("LeftController", new Color(0.05f, 0.8f, 1f));
            var rightMaterial = CreateMaterial("RightController", new Color(1f, 0.55f, 0.08f));
            CreateController(offset.transform, "Left Controller", "XRI Left", new Vector3(-0.3f, 1.2f, 0.3f), leftMaterial, actions, references);
            CreateController(offset.transform, "Right Controller", "XRI Right", new Vector3(0.3f, 1.2f, 0.3f), rightMaterial, actions, references);

            var environment = new GameObject("Test Environment");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(environment.transform, false);
            floor.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            floor.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Floor", new Color(0.12f, 0.16f, 0.22f));
            CreateCube(environment.transform, "Red Cube", new Vector3(-1.2f, 1.25f, 2.5f), CreateMaterial("RedCube", new Color(0.95f, 0.08f, 0.12f)));
            CreateCube(environment.transform, "Green Cube", new Vector3(0f, 1.25f, 2.5f), CreateMaterial("GreenCube", new Color(0.08f, 0.9f, 0.25f)));
            CreateCube(environment.transform, "Blue Cube", new Vector3(1.2f, 1.25f, 2.5f), CreateMaterial("BlueCube", new Color(0.1f, 0.4f, 1f)));
            var lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(environment.transform, false);
            lightObject.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.shadows = LightShadows.None;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.25f, 0.3f, 0.4f);
            RenderSettings.fog = false;
            RenderSettings.sun = light;

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("GameScene could not be saved.");
            // Launch GameScene first; preserve every existing scene entry, including SampleScene.
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) }
                .Concat(EditorBuildSettings.scenes.Where(entry => entry.path != ScenePath)).ToArray();
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = rig;
        }

        static void ImportOfficialInputActions()
        {
            var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(XRInteractionManager).Assembly);
            if (package == null || package.version != "3.3.2")
                throw new InvalidOperationException("Expected installed XR Interaction Toolkit 3.3.2.");
            var source = Path.Combine(package.resolvedPath, "Samples~/Starter Assets/XRI Default Input Actions.inputactions");
            if (!File.Exists(source) || !File.Exists(source + ".meta"))
                throw new FileNotFoundException("Official Starter Assets input actions were not found.");
            // A failed preflight may have imported the asset already. Reuse only an exact copy.
            if (File.Exists(InputPath))
            {
                if (!File.ReadAllBytes(source).SequenceEqual(File.ReadAllBytes(InputPath)) ||
                    !File.Exists(InputPath + ".meta") ||
                    !File.ReadAllBytes(source + ".meta").SequenceEqual(File.ReadAllBytes(InputPath + ".meta")))
                    throw new InvalidOperationException("An existing input asset differs from the official sample; it will not be overwritten.");
                return;
            }
            // Preserve the original sample's GUID; refuse a collision with an existing imported asset.
            var guidLine = File.ReadLines(source + ".meta").Single(line => line.StartsWith("guid: "));
            if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guidLine.Substring(6))))
                throw new InvalidOperationException("The official input action asset is already imported elsewhere.");
            EnsureFolder("Assets/Input");
            File.Copy(source, InputPath, false);
            File.Copy(source + ".meta", InputPath + ".meta", false);
            AssetDatabase.ImportAsset(InputPath, ImportAssetOptions.ForceSynchronousImport);
        }

        static InputActionReference RequireReference(InputActionAsset asset, InputActionReference[] references, string actionPath)
        {
            var action = asset.FindAction(actionPath, true);
            // Input System also emits hidden name-based compatibility references. Use the current ID-based reference.
            return references.Single(reference => reference.action != null && reference.action.id == action.id &&
                (reference.hideFlags & HideFlags.HideInHierarchy) == 0);
        }

        static void AddTracking(GameObject target, InputActionAsset actions, InputActionReference[] references, string map)
        {
            var driver = target.AddComponent<TrackedPoseDriver>();
            driver.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            driver.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            driver.ignoreTrackingState = false;
            driver.positionInput = new InputActionProperty(RequireReference(actions, references, map + "/Position"));
            driver.rotationInput = new InputActionProperty(RequireReference(actions, references, map + "/Rotation"));
            driver.trackingStateInput = new InputActionProperty(RequireReference(actions, references, map + "/Tracking State"));
        }

        static void CreateController(Transform parent, string name, string map, Vector3 position, Material material, InputActionAsset actions, InputActionReference[] references)
        {
            var controller = new GameObject(name);
            controller.transform.SetParent(parent, false);
            controller.transform.localPosition = position;
            AddTracking(controller, actions, references, map);
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Controller Visual";
            visual.transform.SetParent(controller.transform, false);
            visual.transform.localScale = new Vector3(0.05f, 0.14f, 0.07f);
            visual.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(visual.GetComponent<Collider>());
        }

        static void CreateCube(Transform parent, string name, Vector3 position, Material material)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = position;
            cube.transform.localScale = Vector3.one * 0.45f;
            cube.GetComponent<Renderer>().sharedMaterial = material;
        }

        static Material CreateMaterial(string name, Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { name = name };
            material.SetColor("_BaseColor", color);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, MaterialFolder + "/" + name + ".mat");
            return material;
        }

        static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            foreach (var part in parts.Skip(1))
            {
                var next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
                current = next;
            }
        }
    }
}
