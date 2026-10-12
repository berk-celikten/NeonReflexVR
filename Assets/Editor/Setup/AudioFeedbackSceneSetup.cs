using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR;

namespace NeonReflexVR.Editor
{
    public static class AudioFeedbackSceneSetup
    {
        [MenuItem("NeonReflexVR/Configure Audio and Haptics")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var manager = scene.GetRootGameObjects().Single(root => root.name == "Game Manager").GetComponent<GameManager>();
            var shooters = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<VRRaycastShooter>()).ToArray();
            if (shooters.Length != 2 || shooters.Any(shooter => shooter.name != "Left Controller" && shooter.name != "Right Controller"))
                throw new InvalidOperationException("Expected left and right shooters.");
            if (manager.GetComponents<AudioSource>().Length > 1) throw new InvalidOperationException("Unexpected duplicate audio sources.");
            var audio = manager.GetComponent<ArcadeAudioFeedback>() ?? manager.gameObject.AddComponent<ArcadeAudioFeedback>();
            var source = manager.GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.panStereo = 0f;
            source.dopplerLevel = 0f;
            var data = new SerializedObject(manager);
            data.FindProperty("audioFeedback").objectReferenceValue = audio;
            data.ApplyModifiedPropertiesWithoutUndo();
            foreach (var shooter in shooters)
            {
                var haptics = shooter.GetComponent<ControllerHitHaptics>() ?? shooter.gameObject.AddComponent<ControllerHitHaptics>();
                data = new SerializedObject(haptics);
                data.FindProperty("hand").intValue = (int)(shooter.name == "Left Controller" ? XRNode.LeftHand : XRNode.RightHand);
                data.ApplyModifiedPropertiesWithoutUndo();
                data = new SerializedObject(shooter);
                data.FindProperty("hitHaptics").objectReferenceValue = haptics;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Save failed.");
        }
    }
}