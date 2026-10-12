using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NeonReflexVR.Editor
{
    /// <summary>Static shared meshes and materials; no runtime animation or physics decoration.</summary>
    public static class NeonArenaSceneSetup
    {
        const string Folder = "Assets/Materials/NeonArena";
        [MenuItem("NeonReflexVR/Configure Neon Arena")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var floor = GameObject.Find("Floor");
            var camera = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Camera>()).Single();
            var shooters = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
            if (floor == null || shooters.Length != 2) throw new InvalidOperationException("Expected floor and two shooters.");
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/Materials", "NeonArena");
            var dark = MaterialAsset("ArenaDark", new Color(0.012f, 0.018f, 0.036f));
            var cyan = MaterialAsset("ArenaCyan", new Color(0.08f, 1.45f, 1.9f));
            var blue = MaterialAsset("ArenaBlue", new Color(0.08f, 0.55f, 2f));
            var target = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/VRTest/Target.mat");
            if (target == null) throw new InvalidOperationException("Missing target material.");
            // URP Unlit HDR base color emits without lights. Flash still overrides _BaseColor to white.
            target.SetColor("_BaseColor", new Color(2.2f, 0.24f, 0.025f));
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
            floor.GetComponent<Renderer>().sharedMaterial = dark;
            floor.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.006f, 0.009f, 0.022f);
            camera.allowHDR = true;
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            var root = scene.GetRootGameObjects().SingleOrDefault(x => x.name == "Neon Arena") ?? new GameObject("Neon Arena");
            root.layer = 2; // Ignore Raycast; children also have no colliders.
            ConfigureGrid(root);
            ConfigurePurpleLines(root);
            ConfigureSideRails(root);
            foreach (var shooter in shooters)
            {
                var line = shooter.Laser;
                line.sharedMaterial = shooter.name == "Left Controller" ? cyan : blue;
                line.startWidth = 0.006f;
                line.endWidth = 0.003f;
                line.startColor = line.endColor = Color.white;
                line.numCapVertices = 2;
                line.enabled = false;
            }
            var volumeObject = root.transform.Find("Neon Bloom")?.gameObject ?? new GameObject("Neon Bloom");
            volumeObject.transform.SetParent(root.transform, false);
            volumeObject.layer = 2;
            var volume = volumeObject.GetComponent<Volume>();
            if (volume == null) volume = volumeObject.AddComponent<Volume>();
            const string profilePath = Folder + "/NeonBloom.asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null) { profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, profilePath); }
            if (!profile.TryGet<Bloom>(out var bloom)) { bloom = profile.Add<Bloom>(); AssetDatabase.AddObjectToAsset(bloom, profile); }
            bloom.active = true;
            bloom.threshold.Override(1.3f); // SDR UI stays below this threshold.
            bloom.intensity.Override(0.15f);
            bloom.scatter.Override(0.35f);
            bloom.highQualityFiltering.Override(false);
            bloom.downscale.Override(BloomDownscaleMode.Quarter);
            bloom.maxIterations.Override(4);
            EditorUtility.SetDirty(bloom);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssetIfDirty(profile);
            volume.isGlobal = true;
            volume.priority = 10;
            volume.sharedProfile = profile;
            volume.weight = 1f;
            data.volumeLayerMask |= 1 << 2;
            RenderSettings.skybox = null;
            RenderSettings.fog = false;
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
        [MenuItem("NeonReflexVR/Fix Grid Aliasing")]
        public static void FixGridAliasing()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var root = scene.GetRootGameObjects().Single(x => x.name == "Neon Arena");
            ConfigureGrid(root);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
        static void ConfigureGrid(GameObject root)
        {
            const string path = Folder + "/StableGrid.mat";
            var shader = Shader.Find("NeonReflexVR/Stable Neon Grid");
            if (shader == null) throw new InvalidOperationException("Stable grid shader required.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader) { name = "StableGrid" }; AssetDatabase.CreateAsset(material, path); }
            material.SetColor("_BaseColor", new Color(0.04f, 0.7f, 0.95f));
            material.SetColor("_FloorColor", new Color(0.012f, 0.018f, 0.036f));
            material.SetFloat("_Spacing", 1.5f);
            material.SetFloat("_LineWidth", 0.03f);
            material.SetFloat("_FadeStart", 3f);
            material.SetFloat("_FadeEnd", 7f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            var grid = new Strips();
            grid.vertices.AddRange(new[] { new Vector3(-6, 0.012f, -6), new Vector3(-6, 0.012f, 6),
                new Vector3(6, 0.012f, 6), new Vector3(6, 0.012f, -6) });
            grid.indices.AddRange(new[] { 0, 1, 2, 0, 2, 3 });
            MeshObject(root, "Neon Grid", grid, material);
        }
        [MenuItem("NeonReflexVR/Fix Purple Neon Lines")]
        public static void FixPurpleLines()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            ConfigurePurpleLines(scene.GetRootGameObjects().Single(x => x.name == "Neon Arena"));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
        static void ConfigurePurpleLines(GameObject root)
        {
            var shader = Shader.Find("NeonReflexVR/Stable Neon Lines");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/ArenaViolet.mat");
            if (shader == null || material == null) throw new InvalidOperationException("Existing violet material and stable lines shader required.");
            material.shader = shader;
            material.SetColor("_BaseColor", new Color(0.7f, 0.12f, 0.95f));
            material.SetFloat("_LineWidth", 0.04f);
            material.SetFloat("_FadeStart", 8f);
            material.SetFloat("_FadeEnd", 18f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            var frames = new Strips();
            foreach (var z in new[] { 6f, 8f, 10f })
            {
                frames.AddFiltered(new Vector3(-5, 0.02f, z), new Vector3(-5, 5.8f, z));
                frames.AddFiltered(new Vector3(5, 0.02f, z), new Vector3(5, 5.8f, z));
                frames.AddFiltered(new Vector3(-5, 5.8f, z), new Vector3(5, 5.8f, z));
                frames.AddFiltered(new Vector3(-5, 0.025f, z), new Vector3(5, 0.025f, z));
            }
            MeshObject(root, "Distant Frames", frames, material);
        }
        [MenuItem("NeonReflexVR/Fix Outer Side Rails")]
        public static void FixSideRails()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            ConfigureSideRails(scene.GetRootGameObjects().Single(x => x.name == "Neon Arena"));
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
        static void ConfigureSideRails(GameObject root)
        {
            var shader = Shader.Find("NeonReflexVR/Stable Neon Lines");
            var blue = AssetDatabase.LoadAssetAtPath<Material>(Folder + "/ArenaBlue.mat");
            if (shader == null || blue == null) throw new InvalidOperationException("Stable lines shader and existing blue material required.");
            const string path = Folder + "/StableRails.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader) { name = "StableRails" }; AssetDatabase.CreateAsset(material, path); }
            // Preserve blue RGB ratios while keeping these decorative lines below Bloom threshold.
            var color = blue.GetColor("_BaseColor");
            color /= Mathf.Max(1f, Mathf.Max(color.r, Mathf.Max(color.g, color.b)));
            color.a = 1f;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_LineWidth", 0.04f);
            material.SetFloat("_FadeStart", 8f);
            material.SetFloat("_FadeEnd", 18f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            var rails = new Strips();
            foreach (var x in new[] { -5f, 5f })
            {
                // YZ support planes face the playable interior; unlike direction x back, this cross is nonzero.
                rails.AddFiltered(new Vector3(x, 0.035f, -6), new Vector3(x, 0.035f, 10), Vector3.right);
                rails.AddFiltered(new Vector3(x, 5.8f, -6), new Vector3(x, 5.8f, 10), Vector3.right);
            }
            MeshObject(root, "Side Rails", rails, material);
        }
        static Material MaterialAsset(string name, Color color)
        {
            var path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) throw new InvalidOperationException("URP Unlit required.");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            return material;
        }
        static void MeshObject(GameObject root, string name, Strips strips, Material material)
        {
            var path = Folder + "/" + name.Replace(" ", "") + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) { mesh = new Mesh { name = name }; AssetDatabase.CreateAsset(mesh, path); }
            mesh.Clear();
            mesh.SetVertices(strips.vertices);
            mesh.SetTriangles(strips.indices, 0);
            if (strips.lineData.Count > 0) mesh.SetUVs(0, strips.lineData);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            EditorUtility.SetDirty(mesh);
            AssetDatabase.SaveAssetIfDirty(mesh);
            var obj = root.transform.Find(name)?.gameObject ?? new GameObject(name);
            obj.transform.SetParent(root.transform, false);
            obj.layer = 2;
            var filter = obj.GetComponent<MeshFilter>();
            if (filter == null) filter = obj.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
        sealed class Strips
        {
            public readonly List<Vector3> vertices = new List<Vector3>();
            public readonly List<int> indices = new List<int>();
            public readonly List<Vector3> lineData = new List<Vector3>();
            public void AddFiltered(Vector3 a, Vector3 b) => AddFiltered(a, b, Vector3.back);
            public void AddFiltered(Vector3 a, Vector3 b, Vector3 normal)
            {
                // Invisible support margin allows the fragment filter to cover subpixel edges/end caps.
                const float padding = 0.08f;
                var direction = (b - a).normalized;
                var across = Vector3.Cross(direction, normal);
                if (across.sqrMagnitude < 0.0001f) throw new ArgumentException("Line direction and support normal must not be parallel.");
                var side = across.normalized * padding;
                var halfLength = Vector3.Distance(a, b) * 0.5f;
                var start = vertices.Count;
                vertices.Add(a - direction * padding - side); vertices.Add(a - direction * padding + side);
                vertices.Add(b + direction * padding + side); vertices.Add(b + direction * padding - side);
                lineData.Add(new Vector3(-halfLength - padding, -padding, halfLength));
                lineData.Add(new Vector3(-halfLength - padding, padding, halfLength));
                lineData.Add(new Vector3(halfLength + padding, padding, halfLength));
                lineData.Add(new Vector3(halfLength + padding, -padding, halfLength));
                indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
                indices.Add(start); indices.Add(start + 2); indices.Add(start + 3);
            }
            public void Add(Vector3 a, Vector3 b, Vector3 normal, float width)
            {
                var side = Vector3.Cross((b - a).normalized, normal) * width * 0.5f;
                var start = vertices.Count;
                vertices.Add(a - side); vertices.Add(a + side); vertices.Add(b + side); vertices.Add(b - side);
                indices.Add(start); indices.Add(start + 2); indices.Add(start + 1);
                indices.Add(start); indices.Add(start + 3); indices.Add(start + 2);
                // Double sided strips still use opaque Unlit; no overlapping transparent overdraw.
                indices.Add(start); indices.Add(start + 1); indices.Add(start + 2);
                indices.Add(start); indices.Add(start + 2); indices.Add(start + 3);
            }
        }
    }
}