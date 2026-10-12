using UnityEngine;

namespace NeonReflexVR
{
    [RequireComponent(typeof(TestTargetFeedback), typeof(SphereCollider))]
    public sealed class Target : MonoBehaviour
    {
        const float HitDisplaySeconds = 0.15f;
        public bool IsHit { get; private set; }
        public float ExpiresAt { get; private set; }

        public void Initialize(float now)
        {
            IsHit = false;
            ExpiresAt = now + 5f;
        }

        public void DelayExpiry(float seconds) => ExpiresAt += Mathf.Max(0f, seconds);

        public bool TryHit() => TryHit(Time.time);

        public bool TryHit(float now)
        {
            if (IsHit) return false;
            IsHit = true;
            ExpiresAt = now + HitDisplaySeconds;
            GetComponent<TestTargetFeedback>().Flash();
            return true;
        }
    }
}