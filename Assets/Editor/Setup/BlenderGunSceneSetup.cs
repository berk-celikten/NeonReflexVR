using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace NeonReflexVR.Editor
{
    /// <summary>CLI-only, repeatable weapon import/setup. Leaves verified gameplay and arena intact.</summary>
    public static class BlenderGunSceneSetup
    {
        const string ModelPath = "Assets/Art/Weapons/NeonBlaster.fbx";
        const string Folder = "Assets/Art/Weapons";
        const string PrefabPath = Folder + "/Prefabs/NeonBlaster.prefab";

        public static void Configure()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Stop Play Mode and wait for compilation/build before configuring weapons.");
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save current scenes before configuring weapons.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            if (importer == null) throw new InvalidOperationException("Blender FBX missing.");
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = true;
            importer.importAnimation = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.addCollider = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
            importer.SaveAndReimport();
            EnsureFolder(Folder + "/Materials");
            EnsureFolder(Folder + "/Prefabs");
            var materials = new[] {
                Material("NR_Body", new Color(.018f,.035f,.063f), .65f,.38f,Color.black),
                Material("NR_Alloy", new Color(.17f,.23f,.31f), .75f,.42f,Color.black),
                Material("NR_Cyan", new Color(.015f,.42f,.65f), .25f,.4f,new Color(.025f,.62f,1f)*1.6f),
                Material("NR_Violet", new Color(.19f,.025f,.36f), .35f,.4f,new Color(.32f,.06f,.7f)*.8f)
            };
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            var filters = model.GetComponentsInChildren<MeshFilter>(true);
            if (filters.Length != 1) throw new InvalidOperationException("Expected one merged Blender mesh.");
            var mesh = filters[0].sharedMesh;
            var bounds = mesh.bounds;
            Debug.Log($"[BLASTER] imported bounds={bounds} triangles={mesh.triangles.Length / 3} submeshes={mesh.subMeshCount}");
            if (mesh.triangles.Length / 3 >= 5000 || mesh.subMeshCount != 4 || bounds.size.z < .20f || bounds.size.z > .25f || bounds.min.y > -.10f || bounds.max.z < .16f)
                throw new InvalidOperationException("FBX axis/scale/material budget mismatch; inspect before scene mutation.");

            var weapon = new GameObject("Neon Blaster");
            try
            {
                weapon.layer = 2;
                var meshObject = new GameObject("NeonBlasterMesh");
                meshObject.layer = 2;
                meshObject.transform.SetParent(weapon.transform, false);
                meshObject.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = meshObject.AddComponent<MeshRenderer>();
                // FBX slots are deliberately remapped by name, never by assumed slot order.
                var importedSlots = filters[0].GetComponent<MeshRenderer>().sharedMaterials;
                renderer.sharedMaterials = importedSlots.Select(slot => materials.Single(m => m.name == slot.name)).ToArray();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                var muzzle = new GameObject("Muzzle");
                muzzle.layer = 2;
                muzzle.transform.SetParent(weapon.transform, false);
                muzzle.transform.localPosition = new Vector3(0,0,.163f);
                PrefabUtility.SaveAsPrefabAsset(weapon, PrefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(weapon); }

            var scene = EditorSceneManager.OpenScene(GameSceneSetup.ScenePath, OpenSceneMode.Single);
            var shooters = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>(true)).ToArray();
            if (shooters.Length != 2) throw new InvalidOperationException("Expected verified dual shooters.");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            foreach (var shooter in shooters)
            {
                var oldVisual = shooter.transform.Find("Controller Visual");
                if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
                var previous = shooter.transform.Find("Neon Blaster");
                if (previous != null && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(previous.gameObject) != PrefabPath)
                    throw new InvalidOperationException("An unrelated Neon Blaster object exists; inspect it before setup.");
                // Reuse existing prefab instances, including any later device-approved visual offsets.
                var instance = previous != null ? previous.gameObject : (GameObject)PrefabUtility.InstantiatePrefab(prefab, shooter.transform);
                if (previous == null) {
                    instance.name = "Neon Blaster";
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                }
                var serialized = new SerializedObject(shooter);
                serialized.FindProperty("visualMuzzle").objectReferenceValue = instance.transform.Find("Muzzle");
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("GameScene save failed.");
            AssetDatabase.SaveAssets();
            Debug.Log("[BLASTER] integration complete; two prefab instances; gameplay ray origin remains 0.06m.");
        }

        // Call in a separate CLI command after import/upload has completed.
        public static void RenderPreview() => Preview(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));

        public static void BuildWindows()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { GameSceneSetup.ScenePath },
                locationPathName = "Builds/Windows-BlenderGun/NeonReflexVR.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            Debug.Log($"[BLASTER_BUILD] result={report.summary.result} errors={report.summary.totalErrors} warnings={report.summary.totalWarnings} bytes={report.summary.totalSize}");
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Windows build failed.");
        }

        static Material Material(string name, Color color, float metal, float smooth, Color emission)
        {
            var path = Folder + "/Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metal);
            material.SetFloat("_Smoothness", smooth);
            material.SetColor("_EmissionColor", emission);
            // URP validation derives _EMISSION from AnyEmissive, not color alone.
            // BakedEmissive keeps the keyword stable; moving non-static guns are not lightmap baked.
            material.globalIlluminationFlags = emission.maxColorComponent > 0
                ? MaterialGlobalIlluminationFlags.BakedEmissive : MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            if (emission.maxColorComponent > 0) material.EnableKeyword("_EMISSION");
            else material.DisableKeyword("_EMISSION");
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Preview(GameObject prefab)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            var previewRoot = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var cameraObject = new GameObject("Weapon preview camera");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(.32f,.22f,.40f);
            camera.transform.LookAt(new Vector3(0,-.035f,.035f));
            camera.orthographic = true; camera.orthographicSize = .18f;
            camera.nearClipPlane = .01f; camera.farClipPlane = 3f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.028f,.035f,.055f);
            camera.scene = scene;
            var lightObject = new GameObject("Weapon preview light");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject, scene);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional; light.intensity = 2;
            light.transform.rotation = Quaternion.Euler(35,-35,0);
            var texture = new RenderTexture(1200,900,24);
            var pixels = new Texture2D(1200,900,TextureFormat.RGB24,false);
            var previous = RenderTexture.active;
            try {
                camera.targetTexture = texture;
                camera.Render();
                RenderTexture.active = texture;
                pixels.ReadPixels(new Rect(0,0,1200,900),0,0);pixels.Apply();
                Directory.CreateDirectory("Tools/Blender/previews");
                File.WriteAllBytes("Tools/Blender/previews/NeonBlaster-Unity.png", pixels.EncodeToPNG());
            }
            finally {
                camera.targetTexture = null; RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(pixels);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        static void EnsureFolder(string path)
        {
            var parts = path.Split('/');var current = parts[0];
            foreach (var part in parts.Skip(1)) {
                var next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current,part);
                current = next;
            }
        }
    }
}
