using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace NeonReflexVR.Editor
{
    /// <summary>Static, shared geometry outside the combat volume. Does not rebuild the verified neon lines.</summary>
    public static class SciFiArenaSceneSetup
    {
        const string Folder = "Assets/Materials/ArcadeChamber";
        [MenuItem("NeonReflexVR/Apply Sci-Fi Visual Overhaul")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var arena = scene.GetRootGameObjects().Single(x => x.name == "Neon Arena");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Materials", "ArcadeChamber");
            var chamber = Child(arena.transform, "Training Chamber");
            var shell = new Geometry();
            var armor = new Geometry();
            var insets = new Geometry();
            var cyan = new Geometry();
            var violet = new Geometry();

            // Side panels form an enclosed room without colliders or geometry in the target volume.
            foreach (var sign in new[] { -1f, 1f })
            {
                for (var z = -5f; z <= 9f; z += 2.8f)
                {
                    for (var y = 1f; y < 6f; y += 2f)
                        shell.Box(new Vector3(sign * 6.4f, y, z), new Vector3(0.18f, 1.87f, 2.62f));
                    armor.Box(new Vector3(sign * 6.1f, 3f, z - 1.33f), new Vector3(0.42f, 6f, 0.2f));
                    insets.Box(new Vector3(sign * 6.25f, 2.9f, z), new Vector3(0.08f, 0.68f, 1.65f));
                    var normal = Vector3.right;
                    cyan.Line(new Vector3(sign * 6.02f, 1.8f, z - 0.85f), new Vector3(sign * 6.02f, 1.8f, z + 0.85f), normal);
                    cyan.Line(new Vector3(sign * 6.02f, 4.4f, z - 0.65f), new Vector3(sign * 6.02f, 4.85f, z + 0.65f), normal);
                    violet.Line(new Vector3(sign * 6.01f, 0.5f, z - 0.7f), new Vector3(sign * 6.01f, 1.1f, z - 0.7f), normal);
                }
            }
            shell.Box(new Vector3(0, 3.2f, 11.4f), new Vector3(13, 6.4f, 0.2f));
            shell.Box(new Vector3(0, 3.2f, -7f), new Vector3(13, 6.4f, 0.2f));
            shell.Box(new Vector3(0, 6.3f, 2.2f), new Vector3(13, 0.2f, 18.5f));
            insets.Box(new Vector3(0, -0.08f, 8.65f), new Vector3(13, 0.15f, 5.5f));

            // Bevelled structural portals: side, shoulder and ceiling members merged into one mesh.
            foreach (var z in new[] { -4.5f, -1.5f, 2f, 5.5f, 9f })
            {
                foreach (var sign in new[] { -1f, 1f })
                {
                    var a = new Vector3(sign * 5.6f, 0.05f, z);
                    var b = new Vector3(sign * 5.6f, 4.8f, z);
                    var c = new Vector3(sign * 4.25f, 6f, z);
                    armor.Beam(a, b, 0.26f, 0.36f);
                    armor.Beam(b, c, 0.26f, 0.36f);
                    cyan.Line(a + Vector3.back * 0.2f, b + Vector3.back * 0.2f, Vector3.back);
                    cyan.Line(b + Vector3.back * 0.2f, c + Vector3.back * 0.2f, Vector3.back);
                }
                armor.Beam(new Vector3(-4.25f, 6f, z), new Vector3(4.25f, 6f, z), 0.26f, 0.36f);
                cyan.Line(new Vector3(-4.25f, 5.86f, z - 0.2f), new Vector3(4.25f, 5.86f, z - 0.2f), Vector3.back);
            }
            // Far training-station emblem and circuit branches, behind all spawn positions.
            insets.Hexagon(new Vector3(0, 3f, 11.23f), 2.15f);
            foreach (var radius in new[] { 1.35f, 1.75f })
                cyan.HexOutline(new Vector3(0, 3f, 11.19f), radius);
            violet.HexOutline(new Vector3(0, 3f, 11.17f), 2f);
            for (var i = 0; i < 6; i++)
            {
                float angle = (i * 60f + 30f) * Mathf.Deg2Rad;
                var radial = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                cyan.Line(new Vector3(0, 3f, 11.16f) + radial * 0.4f,
                    new Vector3(0, 3f, 11.16f) + radial * 1.05f, Vector3.back);
            }
            foreach (var sign in new[] { -1f, 1f })
            {
                cyan.Line(new Vector3(sign * 2.45f, 1.1f, 11.15f), new Vector3(sign * 4.7f, 1.1f, 11.15f), Vector3.back);
                violet.Line(new Vector3(sign * 2.45f, 4.9f, 11.15f), new Vector3(sign * 4.7f, 4.9f, 11.15f), Vector3.back);
                // Wide, filtered floor lane markers; no screen-facing overlays in the combat volume.
                cyan.Line(new Vector3(sign * 1.8f, 0.04f, 0.8f), new Vector3(sign * 2.4f, 0.04f, 4.8f), Vector3.up);
                cyan.Line(new Vector3(sign * 2.4f, 0.04f, 4.8f), new Vector3(sign * 1.9f, 0.04f, 5.3f), Vector3.up);
            }
            SaveMesh(chamber, "Chamber Panels", shell, Material("ChamberPanels", "NeonReflexVR/Chamber Surface", new Color(0.065f, 0.10f, 0.17f)));
            SaveMesh(chamber, "Structural Portals", armor, Material("Structure", "NeonReflexVR/Chamber Surface", new Color(0.11f, 0.17f, 0.25f)));
            SaveMesh(chamber, "Station Insets", insets, Material("Insets", "NeonReflexVR/Chamber Surface", new Color(0.028f, 0.05f, 0.085f)));
            SaveMesh(chamber, "Chamber Cyan", cyan, Filtered("ChamberCyan", new Color(0.055f, 0.78f, 1f)));
            SaveMesh(chamber, "Chamber Violet", violet, Filtered("ChamberViolet", new Color(0.72f, 0.12f, 0.93f)));

            var target = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VRTest/Target.mat");
            if (target == null) throw new InvalidOperationException("Target material required.");
            target.shader = RequiredShader("NeonReflexVR/Arcade Target");
            target.SetColor("_BaseColor", new Color(2.2f, 0.24f, 0.025f));
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
            var shooters = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
            if (shooters.Length != 2) throw new InvalidOperationException("Two existing shooters required.");
            foreach (var shooter in shooters)
            {
                bool left = shooter.name == "Left Controller";
                var line = shooter.Laser;
                line.sharedMaterial = Material(left ? "BeamCyan" : "BeamBlue", "NeonReflexVR/Arcade Beam",
                    left ? new Color(0.12f, 1.25f, 1.65f) : new Color(0.28f, 0.65f, 1.75f));
                // The wider support is mostly transparent: the bright core remains only ~3 mm.
                line.startWidth = 0.014f;
                line.endWidth = 0.008f;
                line.startColor = line.endColor = Color.white;
                line.textureMode = LineTextureMode.Stretch;
                line.alignment = LineAlignment.View;
                line.numCapVertices = 2;
                line.enabled = false;
            }
            StyleUi(scene.GetRootGameObjects().Single(x => x.name == "Game Manager").GetComponent<GameManager>());
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
        static void StyleUi(GameManager manager)
        {
            var label = manager.StatusText;
            label.canvas.overrideSorting = true;
            label.canvas.sortingOrder = 10; // Draw legible UI after the distant transparent decoration.
            var canvas = (RectTransform)label.canvas.transform;
            var panel = (RectTransform)canvas.Find("Arcade Backdrop");
            if (panel == null) throw new InvalidOperationException("Existing arcade panel required.");
            label.fontStyle = FontStyle.Bold;
            var panelMaterial = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/ArcadePanel.mat");
            if (panelMaterial == null) { panelMaterial = new Material(RequiredShader("NeonReflexVR/Arcade Panel")) { name = "ArcadePanel" }; AssetDatabase.CreateAsset(panelMaterial, Folder + "/ArcadePanel.mat"); }
            panel.GetComponent<Image>().material = panelMaterial;
            label.lineSpacing = 1f; // Keep exact text, font and fitting of all existing states.
            label.raycastTarget = false;
            var cyan = new Color(0.27f, 0.82f, 1f, 0.85f);
            var amber = new Color(1f, 0.57f, 0.24f, 0.85f);
            Bar(panel, "Arcade Accent", new Vector2(0.5f, 1f), new Vector2(0, -9), new Vector2(540, 3), cyan);
            Bar(panel, "Lower Accent", new Vector2(0.5f, 0), new Vector2(0, 9), new Vector2(260, 3), amber);
            foreach (var x in new[] { 0f, 1f })
                foreach (var y in new[] { 0f, 1f })
                {
                    var anchor = new Vector2(x, y);
                    var offset = new Vector2(x == 0 ? 14 : -14, y == 0 ? 14 : -14);
                    var name = "Corner " + x + y;
                    Bar(panel, name + " Vertical", anchor, offset + new Vector2(0, y == 0 ? 12 : -12), new Vector2(4, 28), cyan);
                    Bar(panel, name + " Horizontal", anchor, offset + new Vector2(x == 0 ? 18 : -18, 0), new Vector2(40, 4), cyan);
                }
            ArcadeUiSceneSetup.ApplyStableDepth(label);
        }
        static void Bar(RectTransform panel, string name, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var obj = Child(panel, name, true);
            var rect = (RectTransform)obj.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = Vector2.one * 0.5f;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = obj.GetComponent<Image>();
            if (image == null) image = obj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }
        static GameObject Child(Transform parent, string name, bool ui = false)
        {
            var found = parent.Find(name);
            var obj = found != null ? found.gameObject : ui ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.layer = ui ? parent.gameObject.layer : 2;
            return obj;
        }
        static Shader RequiredShader(string name)
        {
            var shader = Shader.Find(name);
            if (shader == null) throw new InvalidOperationException("Missing shader: " + name);
            return shader;
        }
        static Material Material(string name, string shader, Color color)
        {
            string path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(RequiredShader(shader)) { name = name }; AssetDatabase.CreateAsset(material, path); }
            material.shader = RequiredShader(shader);
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }
        static Material Filtered(string name, Color color)
        {
            var material = Material(name, "NeonReflexVR/Stable Neon Lines", color);
            material.SetFloat("_LineWidth", 0.05f);
            material.SetFloat("_FadeStart", 8f);
            material.SetFloat("_FadeEnd", 19f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }
        static void SaveMesh(GameObject parent, string name, Geometry geometry, Material material)
        {
            string path = Folder + "/" + name.Replace(" ", "") + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear();
            mesh.SetVertices(geometry.vertices);
            mesh.SetTriangles(geometry.triangles, 0);
            if (geometry.uv.Count > 0) mesh.SetUVs(0, geometry.uv);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssetIfDirty(mesh);
            var obj = Child(parent.transform, name);
            var filter = obj.GetComponent<MeshFilter>();
            if (filter == null) filter = obj.AddComponent<MeshFilter>();
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = obj.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
        sealed class Geometry
        {
            public readonly List<Vector3> vertices = new List<Vector3>();
            public readonly List<int> triangles = new List<int>();
            public readonly List<Vector3> uv = new List<Vector3>();
            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                int start = vertices.Count;
                vertices.AddRange(new[] { a, b, c, d });
                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }
            public void Box(Vector3 center, Vector3 size) => Box(center, size, Quaternion.identity);
            void Box(Vector3 center, Vector3 size, Quaternion rotation)
            {
                var h = size * 0.5f;
                var corners = new Vector3[8];
                for (int i = 0; i < 8; i++)
                    corners[i] = center + rotation * new Vector3((i & 1) == 0 ? -h.x : h.x, (i & 2) == 0 ? -h.y : h.y, (i & 4) == 0 ? -h.z : h.z);
                Quad(corners[0], corners[2], corners[3], corners[1]);
                Quad(corners[4], corners[5], corners[7], corners[6]);
                Quad(corners[0], corners[4], corners[6], corners[2]);
                Quad(corners[1], corners[3], corners[7], corners[5]);
                Quad(corners[0], corners[1], corners[5], corners[4]);
                Quad(corners[2], corners[6], corners[7], corners[3]);
            }
            public void Beam(Vector3 a, Vector3 b, float width, float depth) =>
                Box((a + b) * 0.5f, new Vector3(width, depth, Vector3.Distance(a, b)), Quaternion.LookRotation(b - a));
            public void Line(Vector3 a, Vector3 b, Vector3 normal)
            {
                const float padding = 0.08f;
                var direction = (b - a).normalized;
                var across = Vector3.Cross(direction, normal);
                if (across.sqrMagnitude < 0.0001f) throw new ArgumentException("Invalid line support plane.");
                var side = across.normalized * padding;
                float half = Vector3.Distance(a, b) * 0.5f;
                Quad(a - direction * padding - side, a - direction * padding + side, b + direction * padding + side, b + direction * padding - side);
                uv.AddRange(new[] { new Vector3(-half - padding, -padding, half), new Vector3(-half - padding, padding, half),
                    new Vector3(half + padding, padding, half), new Vector3(half + padding, -padding, half) });
            }
            public void HexOutline(Vector3 center, float radius)
            {
                for (int i = 0; i < 6; i++) Line(HexPoint(center, radius, i), HexPoint(center, radius, i + 1), Vector3.back);
            }
            public void Hexagon(Vector3 center, float radius)
            {
                for (int i = 0; i < 6; i++)
                {
                    int start = vertices.Count;
                    vertices.AddRange(new[] { center, HexPoint(center, radius, i + 1), HexPoint(center, radius, i) });
                    triangles.AddRange(new[] { start, start + 1, start + 2 });
                }
            }
            static Vector3 HexPoint(Vector3 center, float radius, int i) =>
                center + new Vector3(Mathf.Cos(i * Mathf.PI / 3), Mathf.Sin(i * Mathf.PI / 3), 0) * radius;
        }
    }
}