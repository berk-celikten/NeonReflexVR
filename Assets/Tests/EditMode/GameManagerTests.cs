using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeonReflexVR.Tests
{
    public class GameManagerTests : InputTestFixture
    {
        GameObject host;
        GameObject template;
        GameManager manager;
        TargetSpawner spawner;
        Material material;
        Text label;
        Keyboard keyboard;
        InputActionAsset input;
        InputActionReference restart;
        Random.State randomState;
        readonly Vector3 eye = new Vector3(100f, 100f, 100f);

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            randomState = Random.state;
            Random.InitState(42);
            keyboard = InputSystem.AddDevice<Keyboard>();
            input = ScriptableObject.CreateInstance<InputActionAsset>();
            var map = new InputActionMap("Test");
            input.AddActionMap(map);
            restart = InputActionReference.Create(map.AddAction("Restart", InputActionType.Button, "<Keyboard>/r"));
            input.Enable();
            template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            template.transform.localScale = Vector3.one * 0.3f;
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", Color.yellow);
            template.GetComponent<Renderer>().sharedMaterial = material;
            template.AddComponent<Target>();
            template.SetActive(false);
            host = new GameObject("Game loop test");
            manager = host.AddComponent<GameManager>();
            spawner = host.AddComponent<TargetSpawner>();
            var view = new GameObject("Test view");
            view.transform.SetParent(host.transform);
            view.transform.position = eye;
            var data = new SerializedObject(spawner);
            data.FindProperty("targetPrefab").objectReferenceValue = template.GetComponent<Target>();
            data.FindProperty("view").objectReferenceValue = view.transform;
            data.FindProperty("autoStart").boolValue = false;
            data.ApplyModifiedPropertiesWithoutUndo();
            var canvas = new GameObject("Test canvas", typeof(RectTransform), typeof(Canvas));
            canvas.transform.SetParent(host.transform);
            canvas.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            canvas.transform.localScale = Vector3.one * 0.0015f;
            var text = new GameObject("Test label", typeof(RectTransform), typeof(Text));
            text.transform.SetParent(canvas.transform);
            label = text.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            data = new SerializedObject(manager);
            data.FindProperty("spawner").objectReferenceValue = spawner;
            data.FindProperty("restartAction").objectReferenceValue = restart;
            data.FindProperty("statusText").objectReferenceValue = label;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public override void TearDown()
        {
            spawner.Stop();
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(template);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(restart);
            input.Disable();
            Object.DestroyImmediate(input);
            InputSystem.RemoveDevice(keyboard);
            Random.state = randomState;
            base.TearDown();
        }

        Target[] Targets() => host.GetComponentsInChildren<Target>();
        static void LateUpdate(MonoBehaviour component) => component.GetType().GetMethod("LateUpdate",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(component, null);
        void InputTick(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            LateUpdate(manager);
        }

        [Test]
        public void ReadyWaitsWithoutTargetsOrCountdownUntilStartButtonPress()
        {
            Assert.That(manager.State, Is.EqualTo(GameState.Ready));
            LateUpdate(manager);
            manager.Tick(100f);
            Assert.That(manager.State, Is.EqualTo(GameState.Ready));
            Assert.That(spawner.ActiveCount, Is.Zero);
            Assert.That(manager.RemainingSeconds, Is.EqualTo(60f));
            Assert.That(label.text, Does.Contain("NEON REFLEX VR"));
            Assert.That(label.text, Does.Contain("Shoot the targets."));
            Assert.That(label.text, Does.Contain("Score as many points as possible in 60 seconds."));
            Assert.That(label.text, Does.Contain("Press A to Start"));
            Assert.That(manager.TryHit(template.GetComponent<Target>(), 100f), Is.False);
            InputTick(Key.R);
            Assert.That(manager.State, Is.EqualTo(GameState.Playing));
            Assert.That(manager.Score, Is.Zero);
            Assert.That(manager.RemainingSeconds, Is.EqualTo(60f));
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
            Assert.That(manager.Restart(Time.time), Is.False);
        }
        [Test]
        public void CountdownClampsToZeroAndStopsTargetsAtExactlySixtySeconds()
        {
            manager.StartGame(0f);
            manager.Tick(30f);
            Assert.That(manager.RemainingSeconds, Is.EqualTo(30f));
            manager.Tick(59.99f);
            Assert.That(manager.State, Is.EqualTo(GameState.Playing));
            manager.Tick(60f);
            Assert.That(manager.State, Is.EqualTo(GameState.GameOver));
            Assert.That(manager.RemainingSeconds, Is.Zero);
            Assert.That(spawner.ActiveCount, Is.Zero);
            manager.Tick(100f);
            spawner.Tick(100f);
            LateUpdate(spawner);
            Assert.That(manager.RemainingSeconds, Is.Zero);
            Assert.That(Targets(), Is.Empty);
        }

        [Test]
        public void EachFirstHitScoresOneAndDuplicateHitScoresNothing()
        {
            manager.StartGame(0f);
            var targets = Targets();
            Assert.That(manager.TryHit(targets[0], 1f), Is.True);
            Assert.That(manager.TryHit(targets[0], 1f), Is.False);
            Assert.That(manager.Score, Is.EqualTo(1));
            Assert.That(manager.TryHit(targets[1], 1f), Is.True);
            Assert.That(manager.Score, Is.EqualTo(2));
        }

        [Test]
        public void DeadlineHitIsRejectedEvenBeforeManagerUpdate()
        {
            manager.StartGame(0f);
            var target = Targets()[0];
            Assert.That(manager.TryHit(target, 60f), Is.False);
            Assert.That(manager.State, Is.EqualTo(GameState.GameOver));
            Assert.That(manager.Score, Is.Zero);
            Assert.That(spawner.ActiveCount, Is.Zero);
        }

        [Test]
        public void GameOverPreservesFinalScoreAndRejectsLaterHits()
        {
            manager.StartGame(0f);
            manager.TryHit(Targets()[0], 1f);
            manager.Tick(60f);
            Assert.That(manager.TryHit(template.GetComponent<Target>(), 61f), Is.False);
            Assert.That(manager.Score, Is.EqualTo(1));
            Assert.That(label.text, Does.Contain("GAME OVER"));
            Assert.That(label.text, Does.Contain("FINAL SCORE: "));
            Assert.That(label.text, Does.Contain("Press A to Play Again"));
        }

        VRRaycastShooter ShooterAt(Vector3 position, Vector3 aim)
        {
            var controller = new GameObject("Test shooter");
            controller.transform.SetParent(host.transform);
            controller.transform.position = position;
            controller.transform.LookAt(aim);
            var shooter = controller.AddComponent<VRRaycastShooter>();
            var data = new SerializedObject(shooter);
            data.FindProperty("gameManager").objectReferenceValue = manager;
            data.ApplyModifiedPropertiesWithoutUndo();
            return shooter;
        }

        [Test]
        public void TwoControllerRaycastsScoreTheSameTargetOnlyOnce()
        {
            manager.StartGame(Time.time);
            var target = Targets()[0];
            var left = ShooterAt(eye, target.transform.position);
            var right = ShooterAt(eye, target.transform.position);
            Physics.SyncTransforms();
            Assert.That(left.Fire(), Is.True);
            Assert.That(right.Fire(), Is.False);
            Assert.That(manager.Score, Is.EqualTo(1));
        }

        [Test]
        public void TestCubeStillFlashesButDoesNotScore()
        {
            manager.StartGame(Time.time);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(host.transform);
            cube.transform.position = new Vector3(150f, 100f, 103f);
            cube.GetComponent<Renderer>().sharedMaterial = material;
            cube.AddComponent<TestTargetFeedback>();
            var shooter = ShooterAt(new Vector3(150f, 100f, 100f), cube.transform.position);
            Physics.SyncTransforms();
            Assert.That(shooter.Fire(), Is.True);
            Assert.That(manager.Score, Is.Zero);
            var block = new MaterialPropertyBlock();
            cube.GetComponent<Renderer>().GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.white));
        }

        [Test]
        public void RestartResetsValuesAndRepeatedRoundsDoNotAccumulateTargets()
        {
            manager.StartGame(0f);
            manager.TryHit(Targets()[0], 1f);
            Assert.That(manager.Restart(2f), Is.False);
            for (var round = 0; round < 15; round++)
            {
                var start = round * 61f;
                manager.Tick(start + 60f);
                Assert.That(spawner.ActiveCount, Is.Zero);
                Assert.That(manager.Restart(start + 61f), Is.True);
                Assert.That(manager.Score, Is.Zero);
                Assert.That(manager.RemainingSeconds, Is.EqualTo(60f));
                Assert.That(Targets().Length, Is.EqualTo(3));
            }
        }

        [Test]
        public void RestartRequiresNewButtonPressAfterGameOver()
        {
            InputTick(Key.R); // One press starts Ready; holding must not reset an active round.
            manager.TryHit(Targets()[0], Time.time);
            InputTick(Key.R);
            Assert.That(manager.Score, Is.EqualTo(1));
            manager.Tick(Time.time + 60f);
            InputTick(Key.R); // Still held: no new press, so remain GameOver.
            Assert.That(manager.State, Is.EqualTo(GameState.GameOver));
            InputTick();
            InputTick(Key.R);
            Assert.That(manager.State, Is.EqualTo(GameState.Playing));
            Assert.That(manager.Score, Is.Zero);
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
            InputTick(Key.R);
            Assert.That(manager.Score, Is.Zero);
        }

        [Test]
        public void HudShowsScoreAndRemainingWholeSeconds()
        {
            manager.StartGame(0f);
            manager.TryHit(Targets()[0], 1f);
            manager.Tick(30.2f);
            Assert.That(label.text, Does.Contain("SCORE:</color> 1"));
            Assert.That(label.text, Does.Contain("TIME:</color> 30"));
            Assert.That(label.canvas.renderMode, Is.EqualTo(RenderMode.WorldSpace));
        }

        [Test]
        public void RoundWaitsForValidHmdPoseAndUsesFreshControlsInsteadOfStaleView()
        {
            spawner.View.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            var hmd = InputSystem.AddDevice<UnityEngine.InputSystem.XR.XRHMD>();
            Assert.That(manager.StartGame(0f), Is.False);
            Assert.That(manager.State, Is.EqualTo(GameState.Ready));
            Assert.That(spawner.ActiveCount, Is.Zero);
            Assert.That(manager.RemainingSeconds, Is.EqualTo(60f));
            Set(hmd.isTracked, 1f);
            Set(hmd.trackingState, 2);
            Set(hmd.centerEyePosition, eye);
            Set(hmd.centerEyeRotation, Quaternion.Euler(0f, 70f, 0f));
            Assert.That(manager.StartGame(5f), Is.False, "Both position and rotation must be tracked.");
            Set(hmd.trackingState, 3);
            Assert.That(manager.StartGame(10f), Is.True);
            var facing = Quaternion.Euler(0f, 70f, 0f) * Vector3.forward;
            var regions = Targets().Select(target => Vector3.SignedAngle(facing,
                Vector3.ProjectOnPlane(target.transform.position - eye, Vector3.up), Vector3.up)).OrderBy(angle => angle).ToArray();
            foreach (var angle in regions) Assert.That(angle, Is.InRange(-25.01f, 25.01f));
            Assert.That(Vector3.Angle(label.canvas.transform.forward, facing), Is.LessThan(0.01f));
            var positions = Targets().Select(target => target.transform.position).ToArray();
            Set(hmd.centerEyeRotation, Quaternion.Euler(0f, -60f, 0f));
            manager.Tick(11f);
            Assert.That(Targets().Select(target => target.transform.position), Is.EqualTo(positions));
            Assert.That(manager.RemainingSeconds, Is.EqualTo(59f));
        }

        [Test]
        public void RestartWaitsForTrackingWithoutDiscardingFinalScore()
        {
            manager.StartGame(0f);
            manager.TryHit(Targets()[0], 1f);
            manager.Tick(60f);
            spawner.View.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            var hmd = InputSystem.AddDevice<UnityEngine.InputSystem.XR.XRHMD>();
            Assert.That(manager.Restart(61f), Is.False);
            Assert.That(manager.State, Is.EqualTo(GameState.GameOver));
            Assert.That(manager.Score, Is.EqualTo(1));
            Assert.That(spawner.ActiveCount, Is.Zero);
            Set(hmd.centerEyePosition, eye);
            Set(hmd.centerEyeRotation, Quaternion.Euler(0f, 90f, 0f));
            Set(hmd.trackingState, 3);
            Set(hmd.isTracked, 1f);
            Assert.That(manager.Restart(62f), Is.True);
            Assert.That(manager.Score, Is.Zero);
            Assert.That(manager.RemainingSeconds, Is.EqualTo(60f));
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
        }
        [Test]
        public void StartButtonDoesNotBypassHmdReadinessOrAutoStartWhenHeld()
        {
            spawner.View.gameObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            var hmd = InputSystem.AddDevice<UnityEngine.InputSystem.XR.XRHMD>();
            InputTick(Key.R);
            Assert.That(manager.State, Is.EqualTo(GameState.Ready));
            Assert.That(spawner.ActiveCount, Is.Zero);
            Assert.That(manager.RemainingSeconds, Is.EqualTo(60f));
            Set(hmd.centerEyePosition, eye);
            Set(hmd.centerEyeRotation, Quaternion.identity);
            Set(hmd.trackingState, 3);
            Set(hmd.isTracked, 1f);
            InputTick(Key.R);
            Assert.That(manager.State, Is.EqualTo(GameState.Ready), "A held across tracking recovery is not a new press.");
            InputTick();
            InputTick(Key.R);
            Assert.That(manager.State, Is.EqualTo(GameState.Playing));
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
        }

        [Test]
        public void HudStaysAboveTargetsAndDoesNotFollowViewDuringRound()
        {
            var now = Time.time;
            manager.StartGame(now);
            var canvas = (RectTransform)label.canvas.transform;
            var position = canvas.position;
            var rotation = canvas.rotation;
            Assert.That(position.y - canvas.rect.height * canvas.lossyScale.y * 0.5f,
                Is.GreaterThan(eye.y + 0.25f));
            spawner.View.position += Vector3.right;
            spawner.View.rotation = Quaternion.Euler(0f, 80f, 0f);
            manager.Tick(now + 1.1f);
            LateUpdate(manager);
            Assert.That(canvas.position, Is.EqualTo(position));
            Assert.That(canvas.rotation, Is.EqualTo(rotation));
            Assert.That(label.text, Does.Not.Contain("Press A"));
            Assert.That(label.text, Does.Not.Contain("NEON REFLEX VR"));
        }
        [Test]
        public void SavedSceneBindsManagerBothShootersWorldHudAndOfficialRightAAction()
        {
            const string path = "Assets/Scenes/GameScene.unity";
            var scene = SceneManager.GetSceneByPath(path);
            var opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var saved = roots.Single(root => root.name == "Game Manager").GetComponent<GameManager>();
                Assert.That(saved.Spawner.AutoStart, Is.False);
                Assert.That(saved.RestartAction.action.actionMap.name, Is.EqualTo("XRI Right Locomotion"));
                Assert.That(saved.RestartAction.action.name, Is.EqualTo("Jump"));
                Assert.That(saved.RestartAction.action.bindings.Select(binding => binding.path), Does.Contain("<XRController>{RightHand}/{PrimaryButton}"));
                Assert.That(EditorUtility.IsPersistent(saved.RestartAction), Is.True);
                Assert.That(saved.StatusText.canvas.renderMode, Is.EqualTo(RenderMode.WorldSpace));
                Assert.That(saved.StatusText.raycastTarget, Is.False);
                Assert.That(saved.StatusText.font, Is.Not.Null);
                Assert.That(saved.StatusText.supportRichText, Is.True);
                var panel = new SerializedObject(saved).FindProperty("backdrop").objectReferenceValue as Image;
                Assert.That(panel, Is.Not.Null);
                Assert.That(panel.raycastTarget, Is.False);
                Assert.That(panel.transform.parent, Is.EqualTo(saved.StatusText.canvas.transform));
                Assert.That(panel.transform.GetSiblingIndex(), Is.LessThan(saved.StatusText.transform.GetSiblingIndex()));
                var shooters = roots.SelectMany(root => root.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
                Assert.That(shooters.Length, Is.EqualTo(2));
                foreach (var shooter in shooters)
                    Assert.That(new SerializedObject(shooter).FindProperty("gameManager").objectReferenceValue, Is.EqualTo(saved));
                Assert.That(scene.isDirty, Is.False);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}