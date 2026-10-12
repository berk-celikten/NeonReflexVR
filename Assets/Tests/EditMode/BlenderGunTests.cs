using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.SceneManagement;

namespace NeonReflexVR.Tests
{
    public class BlenderGunTests
    {
        Scene scene;
        bool opened;
        VRRaycastShooter[] shooters;
        [SetUp]
        public void Setup()
        {
            scene = SceneManager.GetSceneByPath("Assets/Scenes/GameScene.unity");
            opened = !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity", OpenSceneMode.Additive);
            shooters = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<VRRaycastShooter>(true)).ToArray();
        }
        [TearDown]
        public void Cleanup() { if (opened) EditorSceneManager.CloseScene(scene,true); }

        [TestCase("Left")]
        [TestCase("Right")]
        public void EachWeaponIsPersistentPrefabUnderTrackedControllerWithForwardMuzzle(string hand)
        {
            var shooter = shooters.Single(x => x.name == hand + " Controller");
            var gun = shooter.transform.Find("Neon Blaster");
            Assert.That(gun, Is.Not.Null);
            Assert.That(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gun.gameObject), Is.EqualTo("Assets/Art/Weapons/Prefabs/NeonBlaster.prefab"));
            Assert.That(shooter.GetComponent<TrackedPoseDriver>().updateType, Is.EqualTo(TrackedPoseDriver.UpdateType.UpdateAndBeforeRender));
            Assert.That(gun.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(gun.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(gun.localScale, Is.EqualTo(Vector3.one));
            Assert.That(shooter.VisualMuzzle, Is.EqualTo(gun.Find("Muzzle")));
            Assert.That(shooter.VisualMuzzle.localPosition, Is.EqualTo(new Vector3(0,0,.163f)));
            Assert.That(Vector3.Dot(shooter.VisualMuzzle.forward,shooter.transform.forward), Is.GreaterThan(.99999f));
            Assert.That(shooter.transform.InverseTransformPoint(shooter.VisualMuzzle.position).x, Is.EqualTo(0).Within(.00001));
            Assert.That(shooter.transform.InverseTransformPoint(shooter.VisualMuzzle.position).y, Is.EqualTo(0).Within(.00001));
            var data = new SerializedObject(shooter);
            Assert.That(data.FindProperty("muzzleOffset").floatValue, Is.EqualTo(.06f));
            Assert.That(data.FindProperty("range").floatValue, Is.EqualTo(20f));
            Assert.That(shooter.transform.Find("Controller Visual"), Is.Null);
        }

        [TestCase("Left")]
        [TestCase("Right")]
        public void EachGunHasNoCollidersLightsOrRuntimeComponentsAndUsesIgnoreRaycast(string hand)
        {
            var gun = shooters.Single(x => x.name == hand + " Controller").transform.Find("Neon Blaster");
            Assert.That(gun.GetComponentsInChildren<Transform>(true).All(x => x.gameObject.layer == 2), Is.True);
            Assert.That(gun.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(gun.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(gun.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(gun.GetComponentsInChildren<Renderer>(true).Length, Is.EqualTo(1));
        }

        [Test]
        public void ImportedGeometryHasRealWorldForwardScaleSharedMeshAndFourUrpMaterials()
        {
            var guns = shooters.Select(x => x.transform.Find("Neon Blaster")).ToArray();
            var filters = guns.Select(x => x.GetComponentInChildren<MeshFilter>()).ToArray();
            Assert.That(filters[0].sharedMesh, Is.SameAs(filters[1].sharedMesh));
            var mesh = filters[0].sharedMesh;
            Assert.That(mesh.triangles.Length/3, Is.InRange(1,4999));
            Assert.That(mesh.bounds.size.z, Is.InRange(.20f,.25f));
            Assert.That(mesh.bounds.size.x, Is.LessThan(.06f));
            Assert.That(mesh.bounds.min.y, Is.LessThan(-.12f));
            var slots = filters[0].GetComponent<Renderer>().sharedMaterials;
            Assert.That(slots.Length, Is.EqualTo(4));
            Assert.That(slots.Select(x => x.name), Is.EquivalentTo(new[] {"NR_Body","NR_Alloy","NR_Cyan","NR_Violet"}));
            foreach (var material in slots) {
                Assert.That(EditorUtility.IsPersistent(material), Is.True);
                Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
                Assert.That(ShaderUtil.ShaderHasError(material.shader), Is.False);
                Assert.That(material.GetColor("_EmissionColor").maxColorComponent, Is.LessThanOrEqualTo(1.6f));
                if (material.GetColor("_EmissionColor").maxColorComponent > 0) {
                    Assert.That(material.IsKeywordEnabled("_EMISSION"), Is.True, "Must survive URP material/build validation.");
                    Assert.That(material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.AnyEmissive, Is.Not.EqualTo(MaterialGlobalIlluminationFlags.None));
                }
            }
            Assert.That(filters[1].GetComponent<Renderer>().sharedMaterials, Is.EqualTo(slots));
        }

        [Test]
        public void RotatedWeaponMovesVisualStartOnlyAndStillHitsTargetAtOriginalRayEndpoint()
        {
            var controller = new GameObject("Isolated blaster test");
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            try {
                controller.transform.SetPositionAndRotation(new Vector3(150,150,150),Quaternion.Euler(15,60,5));
                var shooter = controller.AddComponent<VRRaycastShooter>();
                var line = controller.AddComponent<LineRenderer>();line.positionCount=2;line.useWorldSpace=true;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Weapons/Prefabs/NeonBlaster.prefab");
                var gun = Object.Instantiate(prefab,controller.transform,false);
                var muzzle = gun.transform.Find("Muzzle");
                var data = new SerializedObject(shooter);
                data.FindProperty("laser").objectReferenceValue=line;
                data.FindProperty("visualMuzzle").objectReferenceValue=muzzle;
                data.ApplyModifiedPropertiesWithoutUndo();
                target.transform.SetPositionAndRotation(controller.transform.position + controller.transform.forward*3,controller.transform.rotation);
                target.GetComponent<Renderer>().sharedMaterial=mat;target.AddComponent<TestTargetFeedback>();
                Physics.SyncTransforms();
                Assert.That(shooter.Fire(), Is.True);
                Assert.That(Vector3.Distance(line.GetPosition(0),muzzle.position), Is.LessThan(.00001));
                Assert.That(Vector3.Distance(line.GetPosition(1),controller.transform.position+controller.transform.forward*2.5f), Is.LessThan(.0001));
                Assert.That(shooter.GetComponentsInChildren<Collider>(), Is.Empty);
            }
            finally { Object.DestroyImmediate(controller);Object.DestroyImmediate(target);Object.DestroyImmediate(mat); }
        }

        [Test]
        public void CloseBlockerBeforeMuzzleStillBlocksRayAndDoesNotDrawBackwardsBeam()
        {
            var controller = new GameObject("Close blocker test");
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try {
                controller.transform.position = new Vector3(150,150,150);
                var shooter=controller.AddComponent<VRRaycastShooter>();
                var laser=controller.AddComponent<LineRenderer>();laser.useWorldSpace=true;laser.positionCount=2;
                var muzzle=new GameObject("Muzzle").transform;muzzle.SetParent(controller.transform,false);muzzle.localPosition=Vector3.forward*.163f;
                var data=new SerializedObject(shooter);
                data.FindProperty("laser").objectReferenceValue=laser;
                data.FindProperty("visualMuzzle").objectReferenceValue=muzzle;
                data.ApplyModifiedPropertiesWithoutUndo();
                blocker.transform.position=controller.transform.position+Vector3.forward*.10f;
                blocker.transform.localScale=Vector3.one*.02f;
                Physics.SyncTransforms();
                Assert.That(shooter.Fire(), Is.False);
                Assert.That(laser.GetPosition(1).z, Is.EqualTo(150.09f).Within(.0001));
                Assert.That(laser.GetPosition(0), Is.EqualTo(laser.GetPosition(1)));
            }
            finally { Object.DestroyImmediate(controller);Object.DestroyImmediate(blocker); }
        }
    }
}
