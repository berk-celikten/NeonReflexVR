using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonReflexVR.Tests
{
    public class TargetSpawnerTests
    {
        GameObject host;
        GameObject template;
        TargetSpawner spawner;
        Material material;
        Random.State randomState;
        readonly Vector3 eye = new Vector3(100f, 100f, 100f);

        [SetUp]
        public void Setup()
        {
            randomState = Random.state;
            Random.InitState(42);
            template = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            template.name = "Test target template";
            template.transform.localScale = Vector3.one * 0.3f;
            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", Color.yellow);
            template.GetComponent<Renderer>().sharedMaterial = material;
            template.AddComponent<TestTargetFeedback>();
            template.AddComponent<Target>();
            template.SetActive(false);
            host = new GameObject("Test spawner");
            spawner = host.AddComponent<TargetSpawner>();
            var serialized = new SerializedObject(spawner);
            serialized.FindProperty("targetPrefab").objectReferenceValue = template.GetComponent<Target>();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            spawner.Initialize(eye, Vector3.forward, 0f);
        }

        [TearDown]
        public void TearDown()
        {
            InvokeDisable();
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(template);
            Object.DestroyImmediate(material);
            Random.state = randomState;
        }

        Target[] Targets() => host.GetComponentsInChildren<Target>();
        void InvokeDisable() => typeof(TargetSpawner).GetMethod("OnDisable", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(spawner, null);

        void AssertSafe()
        {
            var targets = Targets();
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
            Assert.That(targets.Length, Is.EqualTo(3));
            for (var i = 0; i < targets.Length; i++)
            {
                var offset = targets[i].transform.position - eye;
                Assert.That(offset.magnitude, Is.InRange(2.499f, 3.501f));
                Assert.That(offset.y, Is.InRange(-0.501f, 0.101f));
                Assert.That(Vector3.Angle(Vector3.ProjectOnPlane(offset, Vector3.up), Vector3.forward), Is.LessThanOrEqualTo(25.01f));
                Assert.That(offset.z, Is.GreaterThan(2f));
                for (var j = i + 1; j < targets.Length; j++)
                    Assert.That(Vector3.Distance(targets[i].transform.position, targets[j].transform.position), Is.GreaterThanOrEqualTo(0.649f));
            }
        }

        [Test]
        public void StartsWithThreeSafeNonOverlappingTargets() => AssertSafe();

        [Test]
        public void RegionStaysFixedWhenViewTurns()
        {
            var positions = Targets().Select(target => target.transform.position).ToArray();
            spawner.Initialize(eye + Vector3.right * 10f, Vector3.back, 1f);
            spawner.Tick(1f);
            Assert.That(Targets().Select(target => target.transform.position), Is.EqualTo(positions));
            spawner.Tick(5.01f);
            AssertSafe();
        }

        [Test]
        public void UnhitTargetsExpireAtFiveSecondsAndMoveWithoutAccumulating()
        {
            var original = Targets();
            var positions = original.Select(target => target.transform.position).ToArray();
            spawner.Tick(4.99f);
            Assert.That(Targets(), Is.EquivalentTo(original));
            spawner.Tick(5.01f);
            Assert.That(original.All(target => target == null), Is.True);
            foreach (var position in Targets().Select(target => target.transform.position))
                Assert.That(positions.Contains(position), Is.False);
            for (var i = 2; i <= 40; i++) { spawner.Tick(i * 5.1f); AssertSafe(); }
        }

        [Test]
        public void HitFlashesThenReplacesExactlyOneTarget()
        {
            var target = Targets()[0];
            var previous = target.transform.position;
            var untouched = Targets().Skip(1).ToArray();
            Assert.That(target.TryHit(1f), Is.True);
            Assert.That(target.TryHit(1.1f), Is.False);
            Assert.That(target.ExpiresAt, Is.EqualTo(1.15f).Within(0.001f));
            var block = new MaterialPropertyBlock();
            target.GetComponent<Renderer>().GetPropertyBlock(block);
            Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.white));
            spawner.Tick(1.14f);
            Assert.That(target != null, Is.True);
            spawner.Tick(1.16f);
            Assert.That(target == null, Is.True);
            Assert.That(Targets().Intersect(untouched).Count(), Is.EqualTo(2));
            var replacement = Targets().Except(untouched).Single();
            Assert.That(Vector3.Distance(replacement.transform.position, previous), Is.GreaterThanOrEqualTo(0.649f));
            Assert.That(replacement.IsHit, Is.False);
            AssertSafe();
        }

        [Test]
        public void BothRaycastersHitSameTargetOnlyOnce()
        {
            var target = Targets()[0];
            var left = new GameObject("Left raycaster");
            var right = new GameObject("Right raycaster");
            try
            {
                left.transform.position = right.transform.position = eye;
                left.transform.LookAt(target.transform);
                right.transform.LookAt(target.transform);
                Physics.SyncTransforms();
                Assert.That(left.AddComponent<VRRaycastShooter>().Fire(), Is.True);
                var deadline = target.ExpiresAt;
                Assert.That(right.AddComponent<VRRaycastShooter>().Fire(), Is.False);
                Assert.That(target.ExpiresAt, Is.EqualTo(deadline));
                spawner.Tick(deadline + 0.01f);
                Assert.That(target == null, Is.True);
                AssertSafe();
            }
            finally { Object.DestroyImmediate(left); Object.DestroyImmediate(right); }
        }

        [Test]
        public void DisablingSpawnerCleansTargetsAndCanRestart()
        {
            InvokeDisable();
            Assert.That(spawner.ActiveCount, Is.Zero);
            Assert.That(host.GetComponentsInChildren<Target>(true), Is.Empty);
            spawner.Initialize(eye, Vector3.forward, 10f);
            AssertSafe();
        }

        [Test]
        public void SavedPrefabAndScenePreserveCubesAndCameraReference()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Targets/Target.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent<Target>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<TestTargetFeedback>(), Is.Not.Null);
            Assert.That(prefab.GetComponent<SphereCollider>().isTrigger, Is.False);
            Assert.That(prefab.GetComponent<Renderer>().sharedMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Unlit"));
            const string path = "Assets/Scenes/GameScene.unity";
            var scene = SceneManager.GetSceneByPath(path);
            var opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var saved = roots.SelectMany(root => root.GetComponentsInChildren<TargetSpawner>()).Single();
                Assert.That(saved.Prefab, Is.EqualTo(prefab.GetComponent<Target>()));
                Assert.That(saved.View.GetComponent<Camera>(), Is.EqualTo(roots.SelectMany(root => root.GetComponentsInChildren<Camera>()).Single()));
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<Target>()), Is.Empty, "Spawn only in Play Mode.");
                Assert.That(roots.SelectMany(root => root.GetComponentsInChildren<TestTargetFeedback>()).Select(item => item.name),
                    Is.EquivalentTo(new[] { "Red Cube", "Green Cube", "Blue Cube" }));
                // Also exercise the actual scene region against the preserved cubes and floor.
                InvokeDisable();
                spawner.Initialize(saved.View.position, saved.View.forward, 0f);
                Physics.SyncTransforms();
                Assert.That(spawner.ActiveCount, Is.EqualTo(3));
                foreach (var target in Targets())
                    Assert.That(Physics.OverlapSphere(target.transform.position, 0.2f)
                        .All(collider => collider.gameObject == target.gameObject), Is.True, "Target must not overlap scene obstacles.");
                Assert.That(scene.isDirty, Is.False);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}