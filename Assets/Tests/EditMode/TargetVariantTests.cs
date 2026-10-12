using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonReflexVR.Tests
{
    public class TargetVariantTests
    {
        const string Folder="Assets/Art/Targets/Prefabs/";
        static readonly string[] Names={"Target_HexCore","Target_Drone","Target_Capsule"};
        static GameObject Prefab(string name)=>AssetDatabase.LoadAssetAtPath<GameObject>(Folder+name+".prefab");

        [TestCase("Target_HexCore")]
        [TestCase("Target_Drone")]
        [TestCase("Target_Capsule")]
        public void ImportedVariantHasBoundedGeometryTwoSharedMaterialsAndOriginalSimpleHitbox(string name)
        {
            var prefab=Prefab(name);
            Assert.That(prefab,Is.Not.Null);Assert.That(prefab.transform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1));
            var collider=prefab.GetComponent<SphereCollider>();
            Assert.That(collider.center,Is.EqualTo(Vector3.zero));Assert.That(collider.radius,Is.EqualTo(.15f));
            Assert.That(collider.isTrigger,Is.False);Assert.That(prefab.layer,Is.EqualTo(0));
            Assert.That((Physics.DefaultRaycastLayers & (1<<prefab.layer))!=0,Is.True);
            Assert.That(prefab.GetComponentsInChildren<Renderer>().Length,Is.EqualTo(1));
            Assert.That(prefab.GetComponent<Target>(),Is.Not.Null);Assert.That(prefab.GetComponent<TestTargetFeedback>(),Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<Light>(),Is.Empty);Assert.That(prefab.GetComponentsInChildren<Animator>(),Is.Empty);
            var mesh=prefab.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(mesh.triangles.Length/3,Is.InRange(1500,3999));Assert.That(mesh.subMeshCount,Is.EqualTo(2));
            Assert.That(mesh.vertices.All(v=>v.magnitude<=.15001f),Is.True,"All visible parts must fit the retained hitbox.");
            Assert.That(mesh.bounds.size.x,Is.InRange(.18f,.30f));
            var slots=prefab.GetComponent<Renderer>().sharedMaterials;
            Assert.That(slots.Length,Is.EqualTo(2));Assert.That(slots.Select(x=>x.name),Is.EquivalentTo(new[]{"Target_Amber","Target_Armor"}));
            foreach(var material in slots){Assert.That(EditorUtility.IsPersistent(material),Is.True);Assert.That(material.shader.name,Is.EqualTo("NeonReflexVR/Target Variant Surface"));}
            Assert.That(slots.Single(x=>x.name=="Target_Amber").GetColor("_BaseColor").r,Is.GreaterThan(1));
        }

        [TestCase("Target_HexCore")]
        [TestCase("Target_Drone")]
        [TestCase("Target_Capsule")]
        public void BothRaycastersHitRootOnceAndWhiteFlashKeepsSharedMaterialsAndDeadlines(string name)
        {
            var instance=Object.Instantiate(Prefab(name));var shooterObject=new GameObject("Variant ray test");
            var second=new GameObject("Second variant ray test");
            try {
                instance.transform.position=new Vector3(150,150,153);
                var target=instance.GetComponent<Target>();target.Initialize(1f);
                Assert.That(target.ExpiresAt,Is.EqualTo(6f));
                var renderer=instance.GetComponent<Renderer>();var materials=renderer.sharedMaterials;
                var colors=materials.Select(m=>m.GetColor("_BaseColor")).ToArray();
                shooterObject.transform.position=second.transform.position=new Vector3(150,150,150);
                Physics.SyncTransforms();
                Assert.That(shooterObject.AddComponent<VRRaycastShooter>().Fire(),Is.True);
                var expiry=target.ExpiresAt;
                Assert.That(second.AddComponent<VRRaycastShooter>().Fire(),Is.False);
                Assert.That(target.ExpiresAt,Is.EqualTo(expiry));Assert.That(target.IsHit,Is.True);
                var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);
                Assert.That(block.GetColor("_BaseColor"),Is.EqualTo(Color.white));
                Assert.That(renderer.sharedMaterials,Is.EqualTo(materials));
                Assert.That(materials.Select(m=>m.GetColor("_BaseColor")),Is.EqualTo(colors));
                instance.SetActive(false); // EditMode lifecycle must be invoked explicitly.
                typeof(TestTargetFeedback).GetMethod("OnDisable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
                    .Invoke(instance.GetComponent<TestTargetFeedback>(),null);
                renderer.GetPropertyBlock(block);Assert.That(block.isEmpty,Is.True);
            }
            finally{Object.DestroyImmediate(instance);Object.DestroyImmediate(shooterObject);Object.DestroyImmediate(second);}
        }

        [Test]
        public void SavedSceneKeepsFallbackAndAllThreeVariantReferencesWithOneXrCameraAndListener()
        {
            const string path="Assets/Scenes/GameScene.unity";var scene=SceneManager.GetSceneByPath(path);var opened=!scene.isLoaded;
            if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            try {
                var roots=scene.GetRootGameObjects();var spawner=roots.SelectMany(x=>x.GetComponentsInChildren<TargetSpawner>()).Single();
                Assert.That(spawner.Prefab,Is.SameAs(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Targets/Target.prefab").GetComponent<Target>()));
                Assert.That(spawner.Variants.Count,Is.EqualTo(3));
                Assert.That(spawner.Variants.Select(x=>x.name),Is.EquivalentTo(Names));
                Assert.That(spawner.Variants.All(x=>EditorUtility.IsPersistent(x)),Is.True);
                Assert.That(roots.SelectMany(x=>x.GetComponentsInChildren<Camera>()).Count(),Is.EqualTo(1));
                Assert.That(roots.SelectMany(x=>x.GetComponentsInChildren<AudioListener>()).Count(),Is.EqualTo(1));
                Assert.That(roots.SelectMany(x=>x.GetComponentsInChildren<Target>()).Count(),Is.Zero);
                Assert.That(scene.isDirty,Is.False);
            }
            finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
        }

        static TargetSpawner Spawner(GameObject host)
        {
            var spawner=host.AddComponent<TargetSpawner>();var data=new SerializedObject(spawner);
            data.FindProperty("autoStart").boolValue=false;
            data.FindProperty("targetPrefab").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Targets/Target.prefab").GetComponent<Target>();
            var array=data.FindProperty("targetVariants");array.arraySize=3;
            for(int i=0;i<3;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=Prefab(Names[i]).GetComponent<Target>();
            data.ApplyModifiedPropertiesWithoutUndo();return spawner;
        }

        [Test]
        public void ReplacementsSelectAllThreeVariantsWithoutExceedingThreeTargetsOrChangingFiveSecondExpiry()
        {
            var host=new GameObject("Variant selection test");var random=Random.state;Random.InitState(1247);
            try {
                var spawner=Spawner(host);spawner.Initialize(new Vector3(100,100,100),Vector3.forward,0);
                var seen=new System.Collections.Generic.HashSet<Mesh>();
                for(int cycle=0;cycle<30;cycle++){
                    float now=cycle*5.1f;spawner.Tick(now);Assert.That(spawner.ActiveCount,Is.EqualTo(3));
                    var targets=host.GetComponentsInChildren<Target>();Assert.That(targets.Length,Is.EqualTo(3));
                    foreach(var target in targets){seen.Add(target.GetComponent<MeshFilter>().sharedMesh);Assert.That(target.ExpiresAt,Is.EqualTo(now+5).Within(.001));}
                }
                Assert.That(seen,Is.EquivalentTo(Names.Select(n=>Prefab(n).GetComponent<MeshFilter>().sharedMesh)));
                spawner.Stop();Assert.That(spawner.ActiveCount,Is.Zero);
            }
            finally{Object.DestroyImmediate(host);Random.state=random;}
        }

        [Test]
        public void HitReplacementAndPausedExpiryPreserveOneReplacementAndThreeSlotLimit()
        {
            var host=new GameObject("Variant expiry test");var random=Random.state;Random.InitState(42);
            try {
                var spawner=Spawner(host);spawner.Initialize(new Vector3(100,100,100),Vector3.forward,0);
                var targets=host.GetComponentsInChildren<Target>();var hit=targets[0];var untouched=targets.Skip(1).ToArray();
                Assert.That(hit.TryHit(1f),Is.True);Assert.That(hit.ExpiresAt,Is.EqualTo(1.15f).Within(.001));
                spawner.DelayClock(2f);Assert.That(hit.ExpiresAt,Is.EqualTo(3.15f).Within(.001));
                spawner.Tick(3.14f);Assert.That(host.GetComponentsInChildren<Target>(),Is.EquivalentTo(targets));
                spawner.Tick(3.16f);Assert.That(hit==null,Is.True);
                Assert.That(host.GetComponentsInChildren<Target>().Intersect(untouched).Count(),Is.EqualTo(2));
                Assert.That(spawner.ActiveCount,Is.EqualTo(3));
            }
            finally{Object.DestroyImmediate(host);Random.state=random;}
        }

        [Test]
        public void VariantStrikeFacePointsAtFixedRoundAnchorWithoutRuntimeBillboard()
        {
            var host=new GameObject("Variant orientation test");var random=Random.state;Random.InitState(17);
            try {
                var eye=new Vector3(100,100,100);var spawner=Spawner(host);spawner.Initialize(eye,Quaternion.Euler(0,130,0)*Vector3.forward,0);
                foreach(var target in host.GetComponentsInChildren<Target>())
                    Assert.That(Vector3.Dot(-target.transform.forward,(eye-target.transform.position).normalized),Is.GreaterThan(.9999f));
            }
            finally{Object.DestroyImmediate(host);Random.state=random;}
        }

        [Test]
        public void NullVariantEntryFallsBackToPreservedSphere()
        {
            var host=new GameObject("Variant fallback test");var random=Random.state;Random.InitState(23);
            try {
                var spawner=Spawner(host);var data=new SerializedObject(spawner);var array=data.FindProperty("targetVariants");array.arraySize=1;
                array.GetArrayElementAtIndex(0).objectReferenceValue=null;data.ApplyModifiedPropertiesWithoutUndo();
                spawner.Initialize(new Vector3(100,100,100),Vector3.forward,0);
                Assert.That(spawner.ActiveCount,Is.EqualTo(3));
                foreach(var target in host.GetComponentsInChildren<Target>())Assert.That(target.GetComponent<MeshFilter>().sharedMesh,Is.SameAs(spawner.Prefab.GetComponent<MeshFilter>().sharedMesh));
            }
            finally{Object.DestroyImmediate(host);Random.state=random;}
        }

        [TestCase("Target_HexCore")]
        [TestCase("Target_Drone")]
        [TestCase("Target_Capsule")]
        public void ActualGpuFlashWhitesAllSubmeshesWithSupportedUrpShader(string name)
        {
            var shader=Shader.Find("NeonReflexVR/Target Variant Surface");Assert.That(shader.isSupported,Is.True);Assert.That(ShaderUtil.ShaderHasError(shader),Is.False);
            var scene=EditorSceneManager.NewPreviewScene();var instance=(GameObject)PrefabUtility.InstantiatePrefab(Prefab(name),scene);
            var cameraObject=new GameObject("Variant flash camera");SceneManager.MoveGameObjectToScene(cameraObject,scene);
            var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;
            camera.transform.position=Vector3.back;camera.transform.rotation=Quaternion.identity;
            camera.orthographic=true;camera.orthographicSize=.16f;camera.nearClipPlane=.01f;camera.farClipPlane=2;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            var texture=new RenderTexture(256,256,24);var pixels=new Texture2D(256,256,TextureFormat.RGBA32,false);var previous=RenderTexture.active;
            try{
                instance.GetComponent<TestTargetFeedback>().Flash();camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;
                pixels.ReadPixels(new Rect(0,0,256,256),0,0);pixels.Apply();var colors=pixels.GetPixels32();
                int white=colors.Count(c=>c.r>245 && c.g>245 && c.b>245);
                int visible=colors.Count(c=>c.r>20 || c.g>20 || c.b>20);
                Assert.That(white,Is.GreaterThan(10000));Assert.That((float)white/visible,Is.GreaterThan(.97f),"White flash must affect armor and amber submeshes.");
            }
            finally{
                camera.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(texture);Object.DestroyImmediate(pixels);EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
