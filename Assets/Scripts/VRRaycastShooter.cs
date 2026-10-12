using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonReflexVR
{
    /// <summary>One raycast per trigger press; shared input ownership stays with InputActionManager.</summary>
    public sealed class VRRaycastShooter : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        [SerializeField] InputActionReference triggerAction;
        [SerializeField] LineRenderer laser;
        [SerializeField, Min(0.1f)] float range = 20f;
        [SerializeField, Min(0f)] float muzzleOffset = 0.06f;
        [SerializeField, Min(0.01f)] float laserDuration = 0.08f;
        float laserUntil;

        public InputActionReference TriggerAction => triggerAction;
        public LineRenderer Laser => laser;

        void LateUpdate()
        {
            if (triggerAction != null && triggerAction.action != null && triggerAction.action.WasPressedThisFrame())
                Fire();
            if (laser != null && laser.enabled && Time.unscaledTime >= laserUntil)
                laser.enabled = false;
        }

        public bool Fire()
        {
            var start = transform.position + transform.forward * muzzleOffset;
            var end = start + transform.forward * range;
            var targetHit = false;
            if (Physics.Raycast(start, transform.forward, out var hit, range,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                end = hit.point;
                if (hit.collider.TryGetComponent<Target>(out var spawnedTarget))
                    targetHit = gameManager != null ? gameManager.TryHit(spawnedTarget, Time.time) : spawnedTarget.TryHit();
                else if (hit.collider.TryGetComponent<TestTargetFeedback>(out var target))
                {
                    target.Flash();
                    targetHit = true;
                }
            }
            if (laser != null)
            {
                laser.SetPosition(0, start);
                laser.SetPosition(1, end);
                laser.enabled = true;
                laserUntil = Time.unscaledTime + laserDuration;
            }
            return targetHit;
        }

        void OnDisable()
        {
            if (laser != null) laser.enabled = false;
            // Never disable the shared action: tracking and other consumers own its lifetime.
        }
    }
}