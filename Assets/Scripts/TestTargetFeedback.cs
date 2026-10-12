using UnityEngine;

namespace NeonReflexVR
{
    /// <summary>Temporarily overrides one renderer without cloning or changing its material asset.</summary>
    [RequireComponent(typeof(Renderer))]
    public sealed class TestTargetFeedback : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] float flashDuration = 0.15f;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        Renderer targetRenderer;
        MaterialPropertyBlock original;
        MaterialPropertyBlock flash;
        float flashUntil;
        bool flashing;

        void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            original = new MaterialPropertyBlock();
            flash = new MaterialPropertyBlock();
        }

        public void Flash()
        {
            if (targetRenderer == null) Awake();
            if (!flashing) targetRenderer.GetPropertyBlock(original);
            targetRenderer.GetPropertyBlock(flash);
            flash.SetColor(BaseColor, Color.white);
            targetRenderer.SetPropertyBlock(flash);
            flashing = true;
            flashUntil = Time.unscaledTime + flashDuration;
        }

        void Update()
        {
            if (flashing && Time.unscaledTime >= flashUntil) Restore();
        }

        void OnDisable() => Restore();

        void Restore()
        {
            if (!flashing || targetRenderer == null) return;
            targetRenderer.SetPropertyBlock(original);
            flashing = false;
        }
    }
}