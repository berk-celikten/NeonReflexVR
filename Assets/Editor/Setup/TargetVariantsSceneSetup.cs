using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace NeonReflexVR.Editor
{
    /// <summary>Imports three Blender targets while retaining the original fallback prefab/hitbox.</summary>
    public static class TargetVariantsSceneSetup
    {
        const string Folder = "Assets/Art/Targets";
        static readonly string[] Names = { "Target_HexCore", "Target_Drone", "Target_Capsule" };

        public static void Configure()
        {
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || BuildPipeline.isBuildingPlayer)
                throw new InvalidOperationException("Stop Play Mode and wait for compilation/build.");
            for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
                if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                    throw new InvalidOperationException("Save scenes before target setup.");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EnsureFolder(Folder+"/Materials");EnsureFolder(Folder+"/Prefabs");
            var shader=Shader.Find("NeonReflexVR/Target Variant Surface");
            if (shader==null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Target shader missing/invalid.");
            var armor=Material("Target_Armor",shader,new Color(.035f,.075f,.13f),0);
            var amber=Material("Target_Amber",shader,new Color(2.1f,.45f,.035f),1);
            var materials=new[]{armor,amber};
            var prefabs=new Target[Names.Length];
            for (int i=0;i<Names.Length;i++)
            {
                var name=Names[i];var path=Folder+"/"+name+".fbx";
                var importer=AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer==null) throw new InvalidOperationException("Missing Blender model: "+path);
                importer.globalScale=1;importer.useFileScale=true;importer.bakeAxisConversion=true;
                importer.importAnimation=false;importer.importCameras=false;importer.importLights=false;
                importer.addCollider=false;importer.isReadable=false;
                importer.meshCompression=ModelImporterMeshCompression.Off;importer.importNormals=ModelImporterNormals.Import;
                importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
                importer.materialLocation=ModelImporterMaterialLocation.InPrefab;importer.SaveAndReimport();
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var filter=model.GetComponentsInChildren<MeshFilter>(true).Single();
                var mesh=filter.sharedMesh;
                // Every visible vertex must remain inside the unchanged simple hitbox.
                if (mesh.subMeshCount!=2 || mesh.triangles.Length/3>=4000 || mesh.vertices.Any(v=>v.magnitude>.15001f))
                    throw new InvalidOperationException("Target mesh exceeds budget or hitbox: "+name);
                var targetObject=new GameObject(name);
                try {
                    targetObject.layer=0;
                    targetObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=targetObject.AddComponent<MeshRenderer>();
                    renderer.sharedMaterials=filter.GetComponent<Renderer>().sharedMaterials
                        .Select(slot=>materials.Single(m=>m.name==slot.name)).ToArray();
                    renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                    renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
                    var collider=targetObject.AddComponent<SphereCollider>();collider.radius=.15f;collider.center=Vector3.zero;collider.isTrigger=false;
                    targetObject.AddComponent<TestTargetFeedback>();targetObject.AddComponent<Target>();
                    var prefab=PrefabUtility.SaveAsPrefabAsset(targetObject,Folder+"/Prefabs/"+name+".prefab");
                    prefabs[i]=prefab.GetComponent<Target>();
                    Debug.Log($"[TARGET_VARIANT] {name}: triangles={mesh.triangles.Length/3}, materials={renderer.sharedMaterials.Length}, radius=0.15m");
                }
                finally { UnityEngine.Object.DestroyImmediate(targetObject); }
            }
            var scene=EditorSceneManager.OpenScene(GameSceneSetup.ScenePath,OpenSceneMode.Single);
            // OpenScene may unload prefab assets; resolve persistent components again afterwards.
            prefabs=Names.Select(name=>AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Prefabs/"+name+".prefab").GetComponent<Target>()).ToArray();
            if(prefabs.Any(x=>x==null)) throw new InvalidOperationException("A target prefab lost its persistent component reference.");
            var spawner=scene.GetRootGameObjects().SelectMany(x=>x.GetComponentsInChildren<TargetSpawner>(true)).Single();
            if (spawner.Prefab!=AssetDatabase.LoadAssetAtPath<GameObject>(TargetSceneSetup.PrefabPath).GetComponent<Target>())
                throw new InvalidOperationException("Original fallback must be retained.");
            var data=new SerializedObject(spawner);var array=data.FindProperty("targetVariants");array.arraySize=prefabs.Length;
            for (int i=0;i<prefabs.Length;i++) array.GetArrayElementAtIndex(i).objectReferenceValue=prefabs[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new IOException("GameScene save failed.");
            AssetDatabase.SaveAssets();
            Debug.Log("[TARGET_VARIANT] Three optional variants configured; sphere fallback and gameplay deadlines preserved.");
        }

        // Run as a separate CLI command after model import/GPU upload, never during Configure.
        public static void RenderPreview()
        {
            var scene=EditorSceneManager.NewPreviewScene();
            var cameraObject=new GameObject("Targets preview camera");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);
            var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;
            camera.transform.position=new Vector3(.24f,.22f,-1.2f);camera.transform.LookAt(Vector3.zero);
            camera.orthographic=true;camera.orthographicSize=.22f;camera.aspect=2f;
            camera.nearClipPlane=.01f;camera.farClipPlane=3;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.016f,.022f,.037f);
            for (int i=0;i<Names.Length;i++) {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Prefabs/"+Names[i]+".prefab");
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
                instance.transform.position=new Vector3((i-1)*.32f,0,0);
            }
            var texture=new RenderTexture(1800,900,24);
            var pixels=new Texture2D(1800,900,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try {
                camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;
                pixels.ReadPixels(new Rect(0,0,1800,900),0,0);pixels.Apply();
                File.WriteAllBytes("Tools/Blender/previews/NeonTargets-Unity.png",pixels.EncodeToPNG());
            }
            finally {
                camera.targetTexture=null;RenderTexture.active=previous;
                UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(pixels);EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        static Material Material(string name,Shader shader,Color color,float energy)
        {
            var path=Folder+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat==null) {mat=new Material(shader){name=name};AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",color);mat.SetFloat("_Energy",energy);mat.enableInstancing=true;
            EditorUtility.SetDirty(mat);return mat;
        }
        static void EnsureFolder(string path)
        {
            var parts=path.Split('/');var current=parts[0];
            foreach(var part in parts.Skip(1)){var next=current+"/"+part;if(!AssetDatabase.IsValidFolder(next))AssetDatabase.CreateFolder(current,part);current=next;}
        }
    }
}
