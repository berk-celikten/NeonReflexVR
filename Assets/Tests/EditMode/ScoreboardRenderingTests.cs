using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NeonReflexVR.Tests
{
    public class ScoreboardRenderingTests
    {
        Scene scene;
        bool opened;
        GameObject canvasObject;
        Text text;
        Image panel;
        RectTransform canvas;
        [SetUp]
        public void Setup()
        {
            scene = SceneManager.GetSceneByPath("Assets/Scenes/GameScene.unity");
            opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Additive);
            var saved = scene.GetRootGameObjects().Single(x => x.name == "Game Manager").GetComponent<GameManager>();
            canvasObject = Object.Instantiate(saved.StatusText.canvas.gameObject);
            canvasObject.name = "HUD render regression copy";
            canvas = (RectTransform)canvasObject.transform;
            canvas.SetPositionAndRotation(new Vector3(0, 0, 2), Quaternion.identity);
            canvas.sizeDelta = new Vector2(900, 180);
            text = canvasObject.GetComponentInChildren<Text>();
            panel = canvas.Find("Arcade Backdrop").GetComponent<Image>();
            Canvas.ForceUpdateCanvases();
        }
        [TearDown]
        public void Cleanup()
        {
            Object.DestroyImmediate(canvasObject);
            if (opened) EditorSceneManager.CloseScene(scene, true);
        }
        [Test]
        public void DepthWritingBackdropFrameAndTextHaveRealSeparationWithoutResizingHud()
        {
            Assert.That(canvas.localScale.x, Is.EqualTo(0.0015f));
            Assert.That(canvas.sizeDelta, Is.EqualTo(new Vector2(900, 180)));
            Assert.That(Vector3.Dot(panel.transform.position - text.transform.position, canvas.forward),
                Is.EqualTo(0.024f).Within(0.0001f));
            foreach (var image in panel.GetComponentsInChildren<Image>().Where(x => x != panel))
            {
                Assert.That(Vector3.Dot(image.transform.position - panel.transform.position, canvas.forward), Is.LessThan(-0.002f));
                Assert.That(Vector3.Dot(text.transform.position - image.transform.position, canvas.forward), Is.LessThan(-0.01f));
            }
            Assert.That(panel.material.shader.name, Is.EqualTo("NeonReflexVR/Arcade Panel"));
            Assert.That(panel.material.renderQueue, Is.LessThan(2500));
            Assert.That(text.material.shader.name, Is.EqualTo("UI/Default"));
            Assert.That(text.resizeTextForBestFit, Is.False);
            Assert.That(text.fontStyle, Is.EqualTo(FontStyle.Bold));
            Assert.That(text.supportRichText, Is.True);
        }
        [Test]
        public void AllDigitsAndTwoDigitValuesFitWithoutLostQuadsOrClipping()
        {
            foreach (int value in Enumerable.Range(0, 10).Concat(new[] { 10, 59, 60, 9999 }))
            {
                text.text = Hud(value.ToString(), value.ToString());
                Canvas.ForceUpdateCanvases();
                Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(text.rectTransform.rect.height), "Height: " + value);
                Assert.That(text.preferredWidth, Is.LessThanOrEqualTo(text.rectTransform.rect.width), "Width: " + value);
                var settings = text.GetGenerationSettings(text.rectTransform.rect.size);
                var clipped = new TextGenerator();
                var overflow = new TextGenerator();
                Assert.That(clipped.Populate(text.text, settings), Is.True);
                settings.verticalOverflow = VerticalWrapMode.Overflow;
                Assert.That(overflow.Populate(text.text, settings), Is.True);
                Assert.That(clipped.vertexCount, Is.EqualTo(overflow.vertexCount), "Missing glyph quad: " + value);
                Assert.That(clipped.lineCount, Is.EqualTo(2));
                foreach (var vertex in clipped.verts)
                    Assert.That(text.rectTransform.rect.Contains(new Vector2(vertex.position.x, vertex.position.y)), Is.True, "Glyph bounds: " + value);
            }
        }
        [Test]
        public void BuiltInBoldAtlasHasAllTenDigitsAtTheActualRichTextSize()
        {
            Assert.That(text.font.name, Is.EqualTo("LegacyRuntime"));
            Assert.That(text.font.dynamic, Is.True);
            text.font.RequestCharactersInTexture("0123456789", 64, FontStyle.Bold);
            foreach (char digit in "0123456789")
            {
                Assert.That(text.font.GetCharacterInfo(digit, out var glyph, 64, FontStyle.Bold), Is.True, "Missing atlas digit: " + digit);
                Assert.That(glyph.advance, Is.GreaterThan(0));
                Assert.That(glyph.maxX, Is.GreaterThan(glyph.minX));
                Assert.That(glyph.maxY, Is.GreaterThan(glyph.minY));
                Assert.That(Vector2.Distance(glyph.uvBottomLeft, glyph.uvTopRight), Is.GreaterThan(0));
            }
        }
        [Test]
        public void RealGameStateScoreTimeAndRestartKeepTextInFrontOfPanel()
        {
            var host = new GameObject("HUD game-state regression");
            var randomState = Random.state;
            try
            {
                var manager = host.AddComponent<GameManager>();
                var spawner = host.AddComponent<TargetSpawner>();
                var view = new GameObject("Non-XR test view");
                view.transform.SetParent(host.transform);
                view.transform.position = new Vector3(100, 100, 100);
                var data = new SerializedObject(spawner);
                data.FindProperty("targetPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Targets/Target.prefab").GetComponent<Target>();
                data.FindProperty("view").objectReferenceValue = view.transform;
                data.FindProperty("autoStart").boolValue = false;
                data.ApplyModifiedPropertiesWithoutUndo();
                data = new SerializedObject(manager);
                data.FindProperty("spawner").objectReferenceValue = spawner;
                data.FindProperty("statusText").objectReferenceValue = text;
                data.FindProperty("backdrop").objectReferenceValue = panel;
                data.ApplyModifiedPropertiesWithoutUndo();
                typeof(GameManager).GetMethod("RefreshDisplay", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
                Assert.That(text.text, Does.Contain("Press A to Start"));
                Assert.That(manager.StartGame(0), Is.True);
                Assert.That(manager.TryHit(spawner.GetComponentsInChildren<Target>().First(), 1), Is.True);
                manager.Tick(5);
                Assert.That(manager.Score, Is.EqualTo(1));
                Assert.That(text.text, Does.Contain("SCORE:").And.Contain(" 1").And.Contain("55"));
                AssertDepth();
                Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(text.rectTransform.rect.height));
                manager.Tick(60);
                Assert.That(text.text, Does.Contain("GAME OVER").And.Contain("FINAL SCORE:"));
                Assert.That(spawner.ActiveCount, Is.Zero);
                AssertDepth();
                Assert.That(manager.Restart(61), Is.True);
                Assert.That(manager.Score, Is.Zero);
                Assert.That(text.text, Does.Contain("60"));
                Assert.That(spawner.ActiveCount, Is.EqualTo(3));
                AssertDepth();
            }
            finally { Object.DestroyImmediate(host); Random.state = randomState; }
        }
        void AssertDepth() =>
            Assert.That(Vector3.Dot(panel.transform.position - text.transform.position, canvas.forward), Is.EqualTo(0.024f).Within(0.0001f));

        [Test]
        public void PanelDoesNotRemoveGlyphPixelsAcrossEyeOffsetsAndSmallViewChanges()
        {
            foreach (var item in canvasObject.GetComponentsInChildren<Transform>()) item.gameObject.layer = 31;
            text.text = Hud("0123456789", "10 59 60");
            var cameraObject = new GameObject("HUD offscreen regression camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 0.15f;
            camera.aspect = 5;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 10;
            camera.cullingMask = 1 << 31;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            var data = camera.GetUniversalAdditionalCameraData();
            data.allowXRRendering = false; // Offscreen mono regression, not a physical stereo-device test.
            data.renderPostProcessing = false;
            var target = new RenderTexture(1280, 256, 24);
            var readback = new Texture2D(1280, 256, TextureFormat.RGBA32, false);
            var previousTarget = RenderTexture.active;
            var depth = text.rectTransform.anchoredPosition3D;
            try
            {
                target.Create();
                for (int i = 0; i < 3; i++)
                {
                    camera.transform.SetPositionAndRotation(new Vector3(i == 0 ? -0.032f : i == 1 ? 0.032f : 0.08f, 0, 0),
                        Quaternion.Euler(0, i == 2 ? 2 : 0, 0));
                    panel.gameObject.SetActive(false);
                    int reference = WhitePixels(camera, target, readback);
                    Assert.That(reference, Is.GreaterThan(1000), "No glyphs rendered; invalid comparison.");
                    panel.gameObject.SetActive(true);
                    int foreground = WhitePixels(camera, target, readback);
                    Assert.That(foreground, Is.GreaterThanOrEqualTo(reference * 0.99f), "Panel occludes foreground glyph pixels.");
                    Directory.CreateDirectory("Assets/_Recovery");
                    File.WriteAllBytes("Assets/_Recovery/HUD-digits-" + i + ".png", readback.EncodeToPNG());
                    // Compare the old coplanar geometry at the same text position, without assuming every GPU reproduces it.
                    var panelDepth = panel.rectTransform.anchoredPosition3D;
                    panel.rectTransform.anchoredPosition3D = new Vector3(panelDepth.x, panelDepth.y, 0);
                    text.rectTransform.anchoredPosition3D = new Vector3(depth.x, depth.y, 0);
                    panel.gameObject.SetActive(false);
                    int oldReference = WhitePixels(camera, target, readback);
                    panel.gameObject.SetActive(true);
                    int oldVisible = WhitePixels(camera, target, readback);
                    Debug.Log("HUD render eye " + i + ": fixed glyph pixels=" + foreground + "/" + reference
                        + "; coplanar glyph pixels=" + oldVisible + "/" + oldReference + ". Editor offscreen comparison only.");
                    File.WriteAllBytes("Assets/_Recovery/HUD-coplanar-" + i + ".png", readback.EncodeToPNG());
                    panel.rectTransform.anchoredPosition3D = panelDepth;
                    text.rectTransform.anchoredPosition3D = depth;
                    text.rectTransform.anchoredPosition3D = new Vector3(depth.x, depth.y, panel.rectTransform.anchoredPosition3D.z + 2);
                    int behind = WhitePixels(camera, target, readback);
                    Assert.That(behind, Is.LessThan(reference * 0.05f), "Negative control failed: panel must still occlude geometry behind it.");
                    text.rectTransform.anchoredPosition3D = depth;
                }
            }
            finally
            {
                text.rectTransform.anchoredPosition3D = depth;
                RenderTexture.active = previousTarget;
                Object.DestroyImmediate(cameraObject);
                target.Release();
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(readback);
            }
        }
        static int WhitePixels(Camera camera, RenderTexture target, Texture2D readback)
        {
            Canvas.ForceUpdateCanvases();
            RenderPipeline.SubmitRenderRequest(camera, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            readback.Apply();
            return readback.GetPixels32().Count(x => x.r > 210 && x.g > 210 && x.b > 210);
        }
        static string Hud(string score, string time) =>
            "<size=64><color=#67DFFC>SCORE:</color> " + score + "\n<color=#FFAD66>TIME:</color> " + time + "</size>";
    }
}