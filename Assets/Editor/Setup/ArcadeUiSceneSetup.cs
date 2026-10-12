using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace NeonReflexVR.Editor
{
    public static class ArcadeUiSceneSetup
    {
        /// <summary>Keep depth-writing backdrop, frame and glyphs on distinct planes at the existing scale.</summary>
        public static void ApplyStableDepth(Text label)
        {
            var panel = label.canvas.transform.Find("Arcade Backdrop");
            if (panel == null) throw new InvalidOperationException("Existing arcade backdrop required.");
            var textPosition = label.rectTransform.anchoredPosition3D;
            textPosition.z = -8f; // 12 mm towards the viewer at the existing 0.0015 scale.
            label.rectTransform.anchoredPosition3D = textPosition;
            var panelRect = (RectTransform)panel;
            var panelPosition = panelRect.anchoredPosition3D;
            panelPosition.z = 8f; // 24 mm total text/backdrop separation; no font or resolution change.
            panelRect.anchoredPosition3D = panelPosition;
            foreach (var image in panel.GetComponentsInChildren<Image>())
            {
                if (image.transform == panel) continue;
                var rect = image.rectTransform;
                var position = rect.anchoredPosition3D;
                position.z = -2f; // Keep decorative borders just in front of the depth-writing backdrop.
                rect.anchoredPosition3D = position;
            }
        }

        [MenuItem("NeonReflexVR/Fix Scoreboard Text Depth")]
        public static void FixScoreboardRendering()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var manager = scene.GetRootGameObjects().Single(root => root.name == "Game Manager").GetComponent<GameManager>();
            ApplyStableDepth(manager.StatusText);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
        [MenuItem("NeonReflexVR/Configure Arcade UI")]
        public static void Configure()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || EditorApplication.isCompiling || scene.isDirty || scene.path != GameSceneSetup.ScenePath)
                throw new InvalidOperationException("Open saved GameScene outside Play Mode.");
            var manager = scene.GetRootGameObjects().Single(root => root.name == "Game Manager").GetComponent<GameManager>();
            var label = manager.StatusText;
            var canvas = (RectTransform)label.canvas.transform;
            if (canvas.Find("Arcade Backdrop") != null) throw new InvalidOperationException("Arcade UI already configured.");
            var panelObject = new GameObject("Arcade Backdrop", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvas, false);
            panelObject.transform.SetAsFirstSibling();
            var panel = panelObject.GetComponent<Image>();
            panel.raycastTarget = false;
            panel.color = new Color(0.02f, 0.035f, 0.055f, 0.88f);
            var panelRect = (RectTransform)panelObject.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            var stripeObject = new GameObject("Arcade Accent", typeof(RectTransform), typeof(Image));
            stripeObject.transform.SetParent(panelRect, false);
            var stripe = stripeObject.GetComponent<Image>();
            stripe.raycastTarget = false;
            stripe.color = new Color(0.4f, 0.87f, 0.99f, 0.8f);
            var stripeRect = (RectTransform)stripeObject.transform;
            stripeRect.anchorMin = new Vector2(0f, 1f);
            stripeRect.anchorMax = Vector2.one;
            stripeRect.pivot = new Vector2(0.5f, 1f);
            stripeRect.anchoredPosition = Vector2.zero;
            stripeRect.sizeDelta = new Vector2(0f, 4f);
            label.supportRichText = true;
            label.fontSize = 40;
            label.fontStyle = FontStyle.Normal;
            label.color = Color.white;
            var labelRect = (RectTransform)label.transform;
            labelRect.offsetMin = new Vector2(24f, 12f);
            labelRect.offsetMax = new Vector2(-24f, -12f);
            ApplyStableDepth(label);
            canvas.sizeDelta = new Vector2(900f, 440f);
            canvas.position = manager.Spawner.View.position + manager.Spawner.View.forward * 2f + Vector3.up * 0.05f;
            var data = new SerializedObject(manager);
            data.FindProperty("backdrop").objectReferenceValue = panel;
            data.ApplyModifiedPropertiesWithoutUndo();
            typeof(GameManager).GetMethod("RefreshDisplay", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(manager, null);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        }
    }
}