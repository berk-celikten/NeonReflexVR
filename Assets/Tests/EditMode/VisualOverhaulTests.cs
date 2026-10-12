using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeonReflexVR.Tests
{
    public class VisualOverhaulTests
    {
        Scene scene;
        bool opened;
        GameObject[] roots;
        Transform chamber;
        [SetUp]
        public void Setup()
        {
            scene = SceneManager.GetSceneByPath("Assets/Scenes/GameScene.unity");
            opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Additive);
            roots = scene.GetRootGameObjects();
            chamber = roots.Single(x => x.name == "Neon Arena").transform.Find("Training Chamber");
        }
        [TearDown]
        public void Cleanup() { if (opened) EditorSceneManager.CloseScene(scene, true); }

        [Test]
        public void ChamberHasBoundedSharedGeometryAndNoRuntimeOrPhysicsDecoration()
        {
            Assert.That(chamber, Is.Not.Null);
            var renderers = chamber.GetComponentsInChildren<MeshRenderer>();
            Assert.That(renderers.Length, Is.EqualTo(5));
            Assert.That(chamber.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(chamber.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(chamber.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(chamber.GetComponentsInChildren<Transform>().All(x => x.gameObject.layer == 2), Is.True);
            Assert.That(renderers.Sum(x => x.GetComponent<MeshFilter>().sharedMesh.vertexCount), Is.LessThan(3000));
            foreach (var renderer in renderers)
            {
                Assert.That(EditorUtility.IsPersistent(renderer.sharedMaterial), Is.True);
                Assert.That(EditorUtility.IsPersistent(renderer.GetComponent<MeshFilter>().sharedMesh), Is.True);
                Assert.That(renderer.sharedMaterials.Length, Is.EqualTo(1));
                Assert.That(renderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
            }
            Assert.That(renderers.Count(x => x.sharedMaterial.renderQueue >= 3000), Is.EqualTo(2));
        }
        [Test]
        public void SolidArchitectureKeepsCombatVolumeClearInAllStartDirections()
        {
            foreach (var filter in chamber.GetComponentsInChildren<MeshFilter>())
                foreach (var point in filter.sharedMesh.vertices.Select(filter.transform.TransformPoint))
                    if (point.y > 0.3f && point.y < 4.8f)
                        Assert.That(new Vector2(point.x, point.z).magnitude, Is.GreaterThan(5f), filter.name);
            var shell = chamber.Find("Chamber Panels").GetComponent<MeshFilter>().sharedMesh.bounds;
            Assert.That(shell.min.x, Is.LessThan(-6f));
            Assert.That(shell.max.x, Is.GreaterThan(6f));
            Assert.That(shell.min.z, Is.LessThan(-6f));
            Assert.That(shell.max.z, Is.GreaterThan(11f));
            Assert.That(shell.max.y, Is.GreaterThan(6f));
        }
        [Test]
        public void NewAccentGeometryHasFiniteAntialiasingSupportAndComfortableFade()
        {
            foreach (var name in new[] { "Chamber Cyan", "Chamber Violet" })
            {
                var item = chamber.Find(name);
                var mesh = item.GetComponent<MeshFilter>().sharedMesh;
                var data = new System.Collections.Generic.List<Vector3>();
                mesh.GetUVs(0, data);
                Assert.That(data.Count, Is.EqualTo(mesh.vertexCount));
                for (int i = 0; i < data.Count; i += 4)
                {
                    Assert.That(data[i].x, Is.LessThan(-data[i].z));
                    Assert.That(data[i + 2].x, Is.GreaterThan(data[i].z));
                    Assert.That(Vector3.Cross(mesh.vertices[i + 1] - mesh.vertices[i], mesh.vertices[i + 3] - mesh.vertices[i]).magnitude, Is.GreaterThan(0.01f));
                }
                var mat = item.GetComponent<Renderer>().sharedMaterial;
                Assert.That(mat.shader.name, Is.EqualTo("NeonReflexVR/Stable Neon Lines"));
                Assert.That(mat.GetColor("_BaseColor").maxColorComponent, Is.LessThanOrEqualTo(1f));
                Assert.That(mat.GetFloat("_LineWidth"), Is.EqualTo(0.05f));
                Assert.That(mat.GetFloat("_FadeEnd"), Is.GreaterThan(mat.GetFloat("_FadeStart")));
            }
        }
        [TestCase("NeonReflexVR/Chamber Surface")]
        [TestCase("NeonReflexVR/Arcade Target")]
        [TestCase("NeonReflexVR/Arcade Beam")]
        [TestCase("NeonReflexVR/Arcade Panel")]
        public void NewShadersAreSupportedSinglePassWithOnlyTheNativeUiTextureSlot(string name)
        {
            var shader = Shader.Find(name);
            Assert.That(shader, Is.Not.Null);
            Assert.That(shader.isSupported, Is.True);
            Assert.That(ShaderUtil.ShaderHasError(shader), Is.False);
            var material = new Material(shader);
            try
            {
                Assert.That(material.passCount, Is.EqualTo(1));
                if (name == "NeonReflexVR/Arcade Panel")
                    Assert.That(material.GetTexturePropertyNames(), Is.EqualTo(new[] { "_MainTex" }));
                else Assert.That(material.GetTexturePropertyNames(), Is.Empty);
            }
            finally { Object.DestroyImmediate(material); }
        }
        [Test]
        public void BothBeamsUseSharedProfileAndDoNotAddExtraRenderersOrControllers()
        {
            var shooters = roots.SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
            Assert.That(shooters.Length, Is.EqualTo(2));
            foreach (var shooter in shooters)
            {
                Assert.That(shooter.Laser.sharedMaterial.shader.name, Is.EqualTo("NeonReflexVR/Arcade Beam"));
                Assert.That(shooter.GetComponentsInChildren<LineRenderer>().Length, Is.EqualTo(1));
                Assert.That(shooter.Laser.alignment, Is.EqualTo(LineAlignment.View));
                Assert.That(shooter.Laser.textureMode, Is.EqualTo(LineTextureMode.Stretch));
                Assert.That(shooter.Laser.startWidth, Is.LessThanOrEqualTo(0.014f));
                Assert.That(shooter.Laser.positionCount, Is.EqualTo(2));
            }
        }
        [Test]
        public void HudDecorationRemainsBoundedBelowBloomAndDoesNotConsumeInput()
        {
            var manager = roots.Single(x => x.name == "Game Manager").GetComponent<GameManager>();
            var label = manager.StatusText;
            Assert.That(label.fontStyle, Is.EqualTo(FontStyle.Bold));
            Assert.That(label.supportRichText, Is.True);
            Assert.That(label.canvas.GetComponentsInChildren<Text>().Length, Is.EqualTo(1));
            var images = label.canvas.GetComponentsInChildren<Image>();
            Assert.That(images.Length, Is.InRange(10, 12));
            foreach (var image in images)
            {
                Assert.That(image.raycastTarget, Is.False);
                Assert.That(image.color.maxColorComponent, Is.LessThanOrEqualTo(1f));
            }
            var panel = label.canvas.transform.Find("Arcade Backdrop").GetComponent<Image>();
            Assert.That(panel.material.shader.name, Is.EqualTo("NeonReflexVR/Arcade Panel"));
            Assert.That(panel.material.renderQueue, Is.LessThan(2500), "Opaque panel keeps distant lines behind the text.");
            Assert.That(label.raycastTarget, Is.False);
            Assert.That(label.canvas.renderMode, Is.EqualTo(RenderMode.WorldSpace));
        }
    }
}