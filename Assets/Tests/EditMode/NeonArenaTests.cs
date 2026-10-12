using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NeonReflexVR.Tests
{
    public class NeonArenaTests
    {
        Scene scene;
        bool opened;
        GameObject[] roots;
        [SetUp]
        public void Setup()
        {
            scene = SceneManager.GetSceneByPath("Assets/Scenes/GameScene.unity");
            opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Additive);
            roots = scene.GetRootGameObjects();
        }
        [TearDown]
        public void Cleanup() { if (opened) EditorSceneManager.CloseScene(scene, true); }
        [Test]
        public void DecorationsHaveNoCollidersAndCannotInterceptTargetRays()
        {
            var arena = roots.Single(x => x.name == "Neon Arena");
            Assert.That(arena.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(arena.GetComponentsInChildren<Transform>().All(x => x.gameObject.layer == 2), Is.True);
            var targetObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            try
            {
                targetObject.transform.position = new Vector3(0, 1.6f, 7);
                targetObject.transform.localScale = Vector3.one * 0.3f;
                Physics.SyncTransforms();
                Assert.That(Physics.Raycast(new Vector3(0, 1.6f, 0), Vector3.forward, out var hit, 8,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore), Is.True);
                Assert.That(hit.collider.gameObject, Is.EqualTo(targetObject));
            }
            finally { Object.DestroyImmediate(targetObject); }
        }
        [Test]
        public void DecorationBudgetAndPersistentSharedReferencesStaySmall()
        {
            var arena = roots.Single(x => x.name == "Neon Arena");
            var renderers = arena.GetComponentsInChildren<MeshRenderer>();
            Assert.That(renderers.Length, Is.EqualTo(3));
            Assert.That(arena.GetComponentsInChildren<Transform>().Length, Is.EqualTo(5));
            Assert.That(arena.GetComponentsInChildren<Light>(), Is.Empty);
            foreach (var renderer in renderers)
            {
                var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(EditorUtility.IsPersistent(mesh), Is.True);
                Assert.That(mesh.vertexCount, Is.LessThan(150));
                Assert.That(EditorUtility.IsPersistent(renderer.sharedMaterial), Is.True);
                Assert.That(renderer.sharedMaterial.shader.name, Is.EqualTo(renderer.name == "Neon Grid"
                    ? "NeonReflexVR/Stable Neon Grid" : renderer.name == "Distant Frames"
                    ? "NeonReflexVR/Stable Neon Lines" : "Universal Render Pipeline/Unlit"));
                Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
            }
            var grid = arena.transform.Find("Neon Grid").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(grid.bounds.min.y, Is.GreaterThan(0f));
            Assert.That(grid.bounds.max.y, Is.LessThan(0.02f));
        }
        [Test]
        public void GridUsesFilteredSingleQuadWithFadeWithoutChangingLaserBloomColors()
        {
            var arena = roots.Single(x => x.name == "Neon Arena");
            var grid = arena.transform.Find("Neon Grid");
            var mesh = grid.GetComponent<MeshFilter>().sharedMesh;
            var material = grid.GetComponent<Renderer>().sharedMaterial;
            Assert.That(mesh.vertexCount, Is.EqualTo(4));
            Assert.That(mesh.triangles.Length, Is.EqualTo(6));
            Assert.That(material.shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
            Assert.That(material.passCount, Is.EqualTo(1));
            Assert.That(material.GetTexturePropertyNames(), Is.Empty);
            Assert.That(material.GetFloat("_Spacing"), Is.EqualTo(1.5f));
            Assert.That(material.GetFloat("_LineWidth"), Is.InRange(0.025f, 0.04f));
            Assert.That(material.GetFloat("_FadeStart"), Is.EqualTo(3f));
            Assert.That(material.GetFloat("_FadeEnd"), Is.EqualTo(7f));
            Assert.That(material.GetColor("_BaseColor").maxColorComponent, Is.LessThanOrEqualTo(1f));
            foreach (var shooter in roots.SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>()))
                Assert.That(shooter.Laser.sharedMaterial, Is.Not.SameAs(material));
            var floor = roots.SelectMany(x => x.GetComponentsInChildren<Transform>()).Single(x => x.name == "Floor");
            Assert.That(material.GetColor("_FloorColor"), Is.EqualTo(floor.GetComponent<Renderer>().sharedMaterial.GetColor("_BaseColor")));
        }
        [Test]
        public void PurpleLinesUseIndependentFilteredLowBloomMaterialWithSmoothDistanceRange()
        {
            var material = roots.Single(x => x.name == "Neon Arena").transform.Find("Distant Frames").GetComponent<Renderer>().sharedMaterial;
            Assert.That(material, Is.EqualTo(AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonArena/ArenaViolet.mat")));
            Assert.That(material.shader.name, Is.EqualTo("NeonReflexVR/Stable Neon Lines"));
            Assert.That(material.shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
            Assert.That(material.passCount, Is.EqualTo(1));
            Assert.That(material.GetFloat("_LineWidth"), Is.EqualTo(0.04f));
            Assert.That(material.GetFloat("_FadeStart"), Is.EqualTo(8f));
            Assert.That(material.GetFloat("_FadeEnd"), Is.EqualTo(18f));
            var color = material.GetColor("_BaseColor");
            Assert.That(color.b, Is.GreaterThan(color.r));
            Assert.That(color.r, Is.GreaterThan(color.g));
            Assert.That(color.maxColorComponent, Is.LessThanOrEqualTo(1f));
            Assert.That(material.renderQueue, Is.EqualTo(3000));
        }
        [Test]
        public void HorizontalVerticalPurpleSegmentsHavePaddedFilterCoordinatesAndFiniteEnds()
        {
            var mesh = roots.Single(x => x.name == "Neon Arena").transform.Find("Distant Frames").GetComponent<MeshFilter>().sharedMesh;
            var vertices = mesh.vertices;
            var coordinates = new System.Collections.Generic.List<Vector3>();
            mesh.GetUVs(0, coordinates);
            Assert.That(vertices.Length, Is.EqualTo(48));
            Assert.That(mesh.triangles.Length, Is.EqualTo(72));
            Assert.That(coordinates.Count, Is.EqualTo(vertices.Length));
            var horizontal = 0;
            var vertical = 0;
            for (var i = 0; i < vertices.Length; i += 4)
            {
                var direction = (vertices[i + 3] - vertices[i]).normalized;
                if (Mathf.Abs(direction.x) > 0.99f) horizontal++;
                else if (Mathf.Abs(direction.y) > 0.99f) vertical++;
                else Assert.Fail("Expected horizontal or vertical frame segment.");
                Assert.That(vertices[i].z, Is.EqualTo(vertices[i + 2].z));
                Assert.That(coordinates[i].x, Is.LessThan(-coordinates[i].z));
                Assert.That(coordinates[i + 2].x, Is.GreaterThan(coordinates[i].z));
                Assert.That(coordinates[i].y, Is.LessThan(-0.02f));
                Assert.That(coordinates[i + 1].y, Is.GreaterThan(0.02f));
                Assert.That(coordinates[i].z, Is.GreaterThan(2f));
                for (var corner = 0; corner < 4; corner++)
                    Assert.That(coordinates[i + corner].z, Is.EqualTo(coordinates[i].z));
            }
            Assert.That(horizontal, Is.EqualTo(6));
            Assert.That(vertical, Is.EqualTo(6));
        }
        [Test]
        public void BloomIsLowCostAndBelowUiWhiteThresholdWithOneXrCamera()
        {
            var cameras = roots.SelectMany(x => x.GetComponentsInChildren<Camera>()).ToArray();
            Assert.That(cameras.Length, Is.EqualTo(1));
            Assert.That(cameras[0].CompareTag("MainCamera"), Is.True);
            Assert.That(cameras[0].allowHDR, Is.True);
            Assert.That(cameras[0].GetUniversalAdditionalCameraData().renderPostProcessing, Is.True);
            Assert.That(roots.SelectMany(x => x.GetComponentsInChildren<AudioListener>()).Count(), Is.EqualTo(1));
            var volume = roots.Single(x => x.name == "Neon Arena").GetComponentInChildren<Volume>();
            Assert.That(volume.isGlobal, Is.True);
            Assert.That(volume.sharedProfile.TryGet<Bloom>(out var bloom), Is.True);
            Assert.That(volume.sharedProfile.components.Count, Is.EqualTo(1));
            Assert.That(bloom.threshold.value, Is.GreaterThan(1f));
            Assert.That(bloom.intensity.value, Is.InRange(0.05f, 0.2f));
            Assert.That(bloom.highQualityFiltering.value, Is.False);
            Assert.That(bloom.downscale.value, Is.EqualTo(BloomDownscaleMode.Quarter));
            Assert.That(bloom.maxIterations.value, Is.EqualTo(4));
        }
        [Test]
        public void OrangeHdrTargetPreservesColliderAndWhiteFlashWithoutMaterialCloning()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Targets/Target.prefab");
            var instance = Object.Instantiate(prefab);
            try
            {
                Assert.That(instance.transform.localScale, Is.EqualTo(Vector3.one * 0.3f));
                Assert.That(instance.GetComponent<SphereCollider>().radius, Is.EqualTo(0.5f));
                var renderer = instance.GetComponent<Renderer>();
                var material = renderer.sharedMaterial;
                var color = material.GetColor("_BaseColor");
                Assert.That(color.r, Is.GreaterThan(1f));
                Assert.That(color.g, Is.LessThan(color.r));
                instance.GetComponent<TestTargetFeedback>().Flash();
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                Assert.That(block.GetColor("_BaseColor"), Is.EqualTo(Color.white));
                Assert.That(renderer.sharedMaterial, Is.SameAs(material));
                Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(color));
            }
            finally { Object.DestroyImmediate(instance); }
        }
        [Test]
        public void BothLasersRemainSeparateThinBriefAndBoundToManagerAndHaptics()
        {
            var manager = roots.Single(x => x.name == "Game Manager").GetComponent<GameManager>();
            var shooters = roots.SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
            Assert.That(shooters.Length, Is.EqualTo(2));
            Assert.That(shooters[0].Laser, Is.Not.SameAs(shooters[1].Laser));
            foreach (var shooter in shooters)
            {
                var serialized = new SerializedObject(shooter);
                Assert.That(serialized.FindProperty("gameManager").objectReferenceValue, Is.EqualTo(manager));
                Assert.That(serialized.FindProperty("laserDuration").floatValue, Is.EqualTo(0.08f));
                Assert.That(shooter.TriggerAction, Is.Not.Null);
                Assert.That(shooter.HitHaptics, Is.Not.Null);
                Assert.That(shooter.Laser.startWidth, Is.EqualTo(0.006f));
                Assert.That(shooter.Laser.endWidth, Is.EqualTo(0.003f));
                Assert.That(shooter.Laser.sharedMaterial, Is.Not.Null);
                Assert.That(shooter.Laser.enabled, Is.False);
            }
        }
        [Test]
        public void ActualArenaAllowsThreeSafeTargetsAcrossSeededStarts()
        {
            var saved = roots.Single(x => x.name == "Target Spawner").GetComponent<TargetSpawner>();
            var randomState = Random.state;
            var host = new GameObject("Arena spawn test");
            var spawner = host.AddComponent<TargetSpawner>();
            var data = new SerializedObject(spawner);
            data.FindProperty("targetPrefab").objectReferenceValue = saved.Prefab;
            data.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                for (var seed = 0; seed < 12; seed++)
                {
                    Random.InitState(seed);
                    var eye = new Vector3(0, 1.6f, 0);
                    var direction = Quaternion.Euler(0, seed * 30, 0) * Vector3.forward;
                    spawner.Initialize(eye, direction, 0);
                    Assert.That(spawner.ActiveCount, Is.EqualTo(3));
                    foreach (var target in host.GetComponentsInChildren<Target>())
                    {
                        Assert.That(Vector3.Distance(eye, target.transform.position), Is.InRange(2.499f, 3.501f));
                        Assert.That(Vector3.Angle(direction, Vector3.ProjectOnPlane(target.transform.position - eye, Vector3.up)), Is.LessThanOrEqualTo(25.01f));
                    }
                    spawner.Stop();
                    Assert.That(spawner.ActiveCount, Is.Zero);
                }
            }
            finally { spawner.Stop(); Object.DestroyImmediate(host); Random.state = randomState; }
        }
    }
}