using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NeonReflexVR.Tests
{
    public class ShootingTests : InputTestFixture
    {
        GameObject controller;
        GameObject cube;
        GameObject leftController;
        VRRaycastShooter leftShooter;
        LineRenderer leftLaser;
        InputActionReference leftReference;
        VRRaycastShooter shooter;
        LineRenderer laser;
        Material material;
        Keyboard keyboard;
        InputActionAsset inputAsset;
        InputAction action;
        InputActionReference reference;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            keyboard = InputSystem.AddDevice<Keyboard>();
            inputAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Test");
            inputAsset.AddActionMap(map);
            action = map.AddAction("Fire", InputActionType.Button, "<Keyboard>/space");
            reference = InputActionReference.Create(action);

            controller = new GameObject("Shooting test controller");
            controller.transform.position = new Vector3(100f, 100f, 100f);
            shooter = controller.AddComponent<VRRaycastShooter>();
            var beam = new GameObject("Test laser");
            beam.transform.SetParent(controller.transform, false);
            laser = beam.AddComponent<LineRenderer>();
            laser.useWorldSpace = true;
            laser.positionCount = 2;
            laser.enabled = false;
            var serialized = new SerializedObject(shooter);
            serialized.FindProperty("triggerAction").objectReferenceValue = reference;
            serialized.FindProperty("laser").objectReferenceValue = laser;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var leftAction = map.AddAction("LeftFire", InputActionType.Button, "<Keyboard>/leftCtrl");
            leftReference = InputActionReference.Create(leftAction);
            action.Enable();
            leftAction.Enable();
            leftController = Object.Instantiate(controller);
            leftController.transform.position += Vector3.right;
            leftShooter = leftController.GetComponent<VRRaycastShooter>();
            leftLaser = leftController.GetComponentInChildren<LineRenderer>();
            var leftSerialized = new SerializedObject(leftShooter);
            leftSerialized.FindProperty("triggerAction").objectReferenceValue = leftReference;
            leftSerialized.FindProperty("laser").objectReferenceValue = leftLaser;
            leftSerialized.ApplyModifiedPropertiesWithoutUndo();
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = controller.transform.position + Vector3.forward * 3f;
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", Color.red);
            cube.GetComponent<Renderer>().sharedMaterial = material;
            cube.AddComponent<TestTargetFeedback>();
            Physics.SyncTransforms();
        }

        [TearDown]
        public override void TearDown()
        {
            Object.DestroyImmediate(leftController);
            Object.DestroyImmediate(leftReference);
            Object.DestroyImmediate(controller);
            Object.DestroyImmediate(cube);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(reference);
            inputAsset.Disable();
            Object.DestroyImmediate(inputAsset);
            InputSystem.RemoveDevice(keyboard);
            base.TearDown();
        }

        // EditMode does not dispatch ordinary MonoBehaviour messages; invoke their logic directly.
        static void InvokeLifecycle(MonoBehaviour component, string method) =>
            component.GetType().GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(component, null);

        void Tick(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            InvokeLifecycle(shooter, "LateUpdate");
            InvokeLifecycle(leftShooter, "LateUpdate");
        }

        [TestCase(Key.Space)]
        [TestCase(Key.LeftCtrl)]
        public void TriggerHoldFiresOnceAndReleaseAllowsNextShot(Key key)
        {
            var activeController = key == Key.Space ? controller : leftController;
            var activeLaser = key == Key.Space ? laser : leftLaser;
            Tick(key);
            Assert.That(activeLaser.enabled, Is.True);
            var firstEnd = activeLaser.GetPosition(1);
            activeController.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Tick(key);
            Assert.That(activeLaser.GetPosition(1), Is.EqualTo(firstEnd), "Holding must not fire again.");
            Tick();
            Tick(key);
            Assert.That(activeLaser.GetPosition(1).x, Is.GreaterThan(119f), "New press uses its own controller direction.");
        }
        [Test]
        public void RayHitsTargetAndLaserStopsAtSurfaceWithoutChangingMaterial()
        {
            Assert.That(shooter.Fire(), Is.True);
            Assert.That(laser.GetPosition(1).z, Is.EqualTo(102.5f).Within(0.001f));
            var block = new MaterialPropertyBlock();
            cube.GetComponent<Renderer>().GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.white));
            Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(Color.red));
        }

        [Test]
        public void MissUsesConfiguredRangeAndControllerDirection()
        {
            controller.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Assert.That(shooter.Fire(), Is.False);
            Assert.That(laser.GetPosition(0).x, Is.EqualTo(100.06f).Within(0.001f));
            Assert.That(laser.GetPosition(1).x, Is.EqualTo(120.06f).Within(0.001f));
        }

        [Test]
        public void BlockingColliderPreventsTargetFeedback()
        {
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                blocker.transform.position = controller.transform.position + Vector3.forward;
                Physics.SyncTransforms();
                Assert.That(shooter.Fire(), Is.False);
                Assert.That(laser.GetPosition(1).z, Is.EqualTo(100.5f).Within(0.001f));
                var block = new MaterialPropertyBlock();
                cube.GetComponent<Renderer>().GetPropertyBlock(block);
                Assert.That(block.isEmpty, Is.True);
            }
            finally { Object.DestroyImmediate(blocker); }
        }

        [Test]
        public void RepeatedFlashRestoresExistingPropertyBlockWhenDisabled()
        {
            var renderer = cube.GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.green);
            renderer.SetPropertyBlock(block);
            var feedback = cube.GetComponent<TestTargetFeedback>();
            feedback.Flash();
            feedback.Flash();
            InvokeLifecycle(feedback, "OnDisable");
            renderer.GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.green));
        }

        [Test]
        public void DisablingShooterHidesLaserAndKeepsSharedInputEnabled()
        {
            shooter.Fire();
            InvokeLifecycle(shooter, "OnDisable");
            Assert.That(laser.enabled, Is.False);
            Assert.That(action.enabled, Is.True);
        }

        [Test]
        public void LeftTriggerDoesNotFireRightAndBothCanFireInSameFrame()
        {
            Tick(Key.LeftCtrl);
            Assert.That(leftLaser.enabled, Is.True);
            Assert.That(laser.enabled, Is.False);
            var leftEnd = leftLaser.GetPosition(1);
            Tick(Key.LeftCtrl, Key.Space);
            Assert.That(laser.enabled, Is.True);
            Assert.That(leftLaser.GetPosition(1), Is.EqualTo(leftEnd), "Held left trigger must not fire when right presses.");
            Tick();
            controller.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            leftController.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            Tick(Key.LeftCtrl, Key.Space);
            Assert.That(laser.GetPosition(1).x, Is.LessThan(81f));
            Assert.That(leftLaser.GetPosition(1).x, Is.GreaterThan(120f));
            Assert.That(laser.GetPosition(0), Is.Not.EqualTo(leftLaser.GetPosition(0)));
        }

        [Test]
        public void SimultaneousAndRapidHitsKeepOriginalTargetColor()
        {
            var renderer = cube.GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();
            block.SetColor("_BaseColor", Color.green);
            renderer.SetPropertyBlock(block);
            leftController.transform.LookAt(cube.transform);
            Tick(Key.Space, Key.LeftCtrl);
            Assert.That(laser.GetPosition(1).z, Is.EqualTo(102.5f).Within(0.001f));
            Assert.That(leftLaser.GetPosition(1).z, Is.EqualTo(102.5f).Within(0.001f));
            Tick();
            Tick(Key.LeftCtrl);
            Tick(Key.Space);
            renderer.GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.white));
            Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(Color.red));
            InvokeLifecycle(cube.GetComponent<TestTargetFeedback>(), "OnDisable");
            renderer.GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.green));
        }
        [TestCase("Left")]
        [TestCase("Right")]
        public void SavedSceneUsesOfficialHandTriggerWithoutOldTestCubes(string hand)
        {
            const string path = "Assets/Scenes/GameScene.unity";
            var scene = SceneManager.GetSceneByPath(path);
            var opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var saved = roots.SelectMany(root => root.GetComponentsInChildren<VRRaycastShooter>(true)).Single(item => item.name == hand + " Controller");
                Assert.That(saved.name, Is.EqualTo(hand + " Controller"));
                Assert.That(EditorUtility.IsPersistent(saved.TriggerAction), Is.True);
                Assert.That(saved.TriggerAction.action.actionMap.name, Is.EqualTo("XRI " + hand + " Interaction"));
                Assert.That(saved.TriggerAction.action.name, Is.EqualTo("Activate"));
                Assert.That(saved.TriggerAction.action.type, Is.EqualTo(InputActionType.Button));
                Assert.That(saved.TriggerAction.action.bindings.Select(binding => binding.path),
                    Does.Contain("<XRController>{" + hand + "Hand}/{TriggerButton}"));
                Assert.That(saved.Laser, Is.Not.Null);
                Assert.That(saved.Laser.enabled, Is.False);
                Assert.That(saved.Laser.positionCount, Is.EqualTo(2));
                Assert.That(saved.Laser.useWorldSpace, Is.True);
                Assert.That(saved.Laser.transform.parent, Is.EqualTo(saved.transform));
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<TestTargetFeedback>(true))
                    .Select(target => target.name), Is.Empty);
                Assert.That(scene.isDirty, Is.False);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}