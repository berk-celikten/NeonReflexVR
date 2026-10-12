using System;
using System.Linq;
using NUnit.Framework;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace NeonReflexVR.Tests
{
    public class GameSceneTests
    {
        const string ScenePath = "Assets/Scenes/GameScene.unity";
        const string InputPath = "Assets/Input/XRI Default Input Actions.inputactions";
        Scene scene;
        bool openedForTest;

        [SetUp]
        public void OpenScene()
        {
            scene = SceneManager.GetSceneByPath(ScenePath);
            openedForTest = !scene.isLoaded;
            if (openedForTest) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void CloseScene()
        {
            if (openedForTest) EditorSceneManager.CloseScene(scene, true);
        }

        T[] Components<T>() where T : Component => scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();

        [Test]
        public void OriginHasOneTrackedXrCameraAndAudioListener()
        {
            var origin = Components<XROrigin>().Single();
            var camera = Components<Camera>().Single();
            Assert.That(origin.Camera, Is.EqualTo(camera));
            Assert.That(origin.Origin, Is.EqualTo(origin.gameObject));
            Assert.That(origin.CameraFloorOffsetObject.transform.parent, Is.EqualTo(origin.transform));
            Assert.That(camera.transform.parent, Is.EqualTo(origin.CameraFloorOffsetObject.transform));
            Assert.That(origin.RequestedTrackingOriginMode, Is.EqualTo(XROrigin.TrackingOriginMode.Floor));
            Assert.That(camera.CompareTag("MainCamera"), Is.True);
            Assert.That(camera.enabled && camera.gameObject.activeInHierarchy, Is.True);
            var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            Assert.That(cameraData, Is.Not.Null);
            Assert.That(cameraData.allowXRRendering, Is.True);
            Assert.That(cameraData.renderPostProcessing, Is.False);
            var listener = Components<AudioListener>().Single();
            Assert.That(listener.gameObject, Is.EqualTo(camera.gameObject));
            Assert.That(listener.enabled, Is.True);
            Assert.That(origin.transform.position, Is.EqualTo(Vector3.zero));
        }

        [TestCase("Main Camera", "XRI Head", "<XRHMD>/centerEyePosition", "<XRHMD>/centerEyeRotation")]
        [TestCase("Left Controller", "XRI Left", "<XRController>{LeftHand}/devicePosition", "<XRController>{LeftHand}/deviceRotation")]
        [TestCase("Right Controller", "XRI Right", "<XRController>{RightHand}/devicePosition", "<XRController>{RightHand}/deviceRotation")]
        public void TrackingUsesPersistentOfficialActions(string objectName, string map, string positionBinding, string rotationBinding)
        {
            var driver = Components<TrackedPoseDriver>().Single(component => component.name == objectName);
            Assert.That(driver.enabled, Is.True);
            Assert.That(driver.trackingType, Is.EqualTo(TrackedPoseDriver.TrackingType.RotationAndPosition));
            Assert.That(driver.updateType, Is.EqualTo(TrackedPoseDriver.UpdateType.UpdateAndBeforeRender));
            Assert.That(driver.ignoreTrackingState, Is.False);
            CheckAction(driver.positionInput, map + "/Position", positionBinding);
            CheckAction(driver.rotationInput, map + "/Rotation", rotationBinding);
            CheckAction(driver.trackingStateInput, map + "/Tracking State", null);
            if (objectName != "Main Camera")
            {
                var origin = Components<XROrigin>().Single();
                Assert.That(driver.transform.parent, Is.EqualTo(origin.CameraFloorOffsetObject.transform));
                Assert.That(driver.GetComponentsInChildren<Renderer>().Length, Is.EqualTo(1));
                Assert.That(driver.GetComponentsInChildren<Collider>(), Is.Empty);
            }
        }

        static void CheckAction(InputActionProperty property, string expectedPath, string binding)
        {
            Assert.That(property.reference, Is.Not.Null);
            Assert.That(EditorUtility.IsPersistent(property.reference), Is.True, "Reference must survive scene reload.");
            Assert.That(property.reference.hideFlags & HideFlags.HideInHierarchy, Is.EqualTo(HideFlags.None), "Use current ID-based action references.");
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            Assert.That(property.reference.action, Is.Not.Null);
            Assert.That(property.reference.action.id, Is.EqualTo(asset.FindAction(expectedPath, true).id));
            Assert.That(property.reference.action.actionMap.asset, Is.EqualTo(asset));
            if (binding != null) Assert.That(property.reference.action.bindings.Select(item => item.path), Does.Contain(binding));
        }

        [Test]
        public void InputAssetIsEnabledByManagerAndNoMovementMechanicExists()
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath);
            var manager = Components<InputActionManager>().Single();
            Assert.That(manager.enabled && manager.gameObject.activeInHierarchy, Is.True);
            Assert.That(manager.actionAssets, Is.EquivalentTo(new[] { asset }));
            Assert.That(Components<XRInteractionManager>().Length, Is.EqualTo(1));
            Assert.That(Components<TrackedPoseDriver>().Length, Is.EqualTo(3));
            Assert.That(Components<CharacterController>(), Is.Empty);
            Assert.That(Components<Rigidbody>(), Is.Empty);
            Assert.That(Components<LineRenderer>(), Is.Empty);
            foreach (var component in Components<MonoBehaviour>())
            {
                Assert.That(component.GetType().FullName, Does.Not.Contain("Locomotion"));
                Assert.That(component.GetType().FullName, Does.Not.Contain("Teleport"));
                Assert.That(component.GetType().FullName, Does.Not.Contain("ActionBasedController"));
            }
        }

        [Test]
        public void TestEnvironmentHasDistinctCubesAndValidLowCostMaterials()
        {
            var renderers = Components<Renderer>();
            var cubes = renderers.Where(renderer => renderer.name.EndsWith(" Cube", StringComparison.Ordinal)).ToArray();
            Assert.That(cubes.Length, Is.EqualTo(3));
            Assert.That(cubes.Select(cube => cube.sharedMaterial.GetColor("_BaseColor")).Distinct().Count(), Is.EqualTo(3));
            Assert.That(renderers.Count(renderer => renderer.name == "Floor"), Is.EqualTo(1));
            foreach (var renderer in renderers)
            {
                Assert.That(renderer.sharedMaterial, Is.Not.Null);
                Assert.That(renderer.sharedMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Unlit"));
                Assert.That(EditorUtility.IsPersistent(renderer.sharedMaterial), Is.True);
            }
            Assert.That(Components<Light>().Single().shadows, Is.EqualTo(LightShadows.None));
        }

        [Test]
        public void SceneHasNoMissingScriptsAndBuildListPreservesSampleScene()
        {
            foreach (var transform in Components<Transform>())
                Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject), Is.Zero, transform.name);
            Assert.That(EditorBuildSettings.scenes[0].path, Is.EqualTo(ScenePath));
            Assert.That(EditorBuildSettings.scenes[0].enabled, Is.True);
            Assert.That(EditorBuildSettings.scenes.Any(entry => entry.path == "Assets/Scenes/SampleScene.unity"), Is.True);
            Assert.That(scene.isDirty, Is.False);
        }
    }
}
