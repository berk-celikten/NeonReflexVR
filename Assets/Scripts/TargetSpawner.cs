using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace NeonReflexVR
{
    /// <summary>Three fixed slots; one clock, no per-target coroutine or event subscriptions.</summary>
    public sealed class TargetSpawner : MonoBehaviour
    {
        [SerializeField] bool autoStart = true;
        [SerializeField] Target targetPrefab;
        [SerializeField] Transform view;
        readonly Target[] targets = new Target[3];
        readonly float[] nextAttempt = new float[3];
        Vector3 anchor;
        Vector3 forward;
        Vector3 right;
        bool initialized;
        bool warned;

        public bool AutoStart => autoStart;
        public Target Prefab => targetPrefab;
        public Transform View => view;
        public int ActiveCount
        {
            get
            {
                var count = 0;
                foreach (var target in targets) if (target != null) count++;
                return count;
            }
        }

        void LateUpdate()
        {
            if (!autoStart) return;
            if (!initialized && targetPrefab != null && TryGetStartPose(out var position, out var direction))
                Initialize(position, direction, Time.time);
            Tick(Time.time);
        }

        // Read the fresh HMD controls directly instead of a camera transform from an earlier frame.
        public bool TryGetStartPose(out Vector3 position, out Vector3 direction)
        {
            position = default;
            direction = default;
            if (view == null) return false;
            if (view.GetComponent<TrackedPoseDriver>() != null)
            {
                var hmd = InputSystem.GetDevice<XRHMD>();
                const int positionAndRotation = 3;
                if (hmd == null || !hmd.enabled || !hmd.isTracked.isPressed ||
                    (hmd.trackingState.ReadValue() & positionAndRotation) != positionAndRotation) return false;
                var rotation = hmd.centerEyeRotation.ReadValue();
                if (Quaternion.Dot(rotation, rotation) < 0.5f) return false;
                var space = view.parent;
                position = space != null ? space.TransformPoint(hmd.centerEyePosition.ReadValue()) : hmd.centerEyePosition.ReadValue();
                direction = (space != null ? space.rotation * rotation : rotation) * Vector3.forward;
            }
            else
            {
                // Non-XR views are useful for Editor tests; GameScene always has a pose driver.
                position = view.position;
                direction = view.forward;
            }
            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
            return direction.sqrMagnitude > 0.5f &&
                !float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z) &&
                !float.IsInfinity(position.x) && !float.IsInfinity(position.y) && !float.IsInfinity(position.z);
        }
        public void Initialize(Vector3 eyePosition, Vector3 lookDirection, float now)
        {
            if (initialized || targetPrefab == null) return;
            anchor = eyePosition;
            forward = Vector3.ProjectOnPlane(lookDirection, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.5f) return;
            right = Vector3.Cross(Vector3.up, forward);
            initialized = true;
            for (var i = 0; i < targets.Length; i++) nextAttempt[i] = now;
            Tick(now);
        }

        public void Tick(float now)
        {
            if (!initialized) return;
            for (var i = 0; i < targets.Length; i++)
            {
                var previous = Vector3.zero;
                var replacing = targets[i] != null && now >= targets[i].ExpiresAt;
                if (replacing)
                {
                    previous = targets[i].transform.position;
                    Remove(targets[i]);
                    targets[i] = null;
                    nextAttempt[i] = now;
                }
                if (targets[i] != null || now < nextAttempt[i]) continue;
                if (TryPosition(i, previous, replacing, out var position))
                {
                    var target = Instantiate(targetPrefab, position, Quaternion.identity, transform);
                    target.Initialize(now);
                    target.gameObject.SetActive(true);
                    targets[i] = target;
                }
                else
                {
                    nextAttempt[i] = now + 0.5f;
                    if (!warned)
                    {
                        Debug.LogWarning("TargetSpawner: safe region blocked; retrying without adding unsafe targets.", this);
                        warned = true;
                    }
                }
            }
        }

        bool TryPosition(int region, Vector3 previous, bool replacing, out Vector3 position)
        {
            for (var attempt = 0; attempt < 91; attempt++)
            {
                var sample = attempt - 64;
                // Random first; 27 bounded grid candidates cover rare crowded-region cases.
                var distance = attempt < 64 ? Random.Range(2.5f, 3.5f) : 2.5f + (sample % 3) * 0.5f;
                var height = attempt < 64 ? Random.Range(-0.5f, 0.1f) : -0.5f + (sample / 3 % 3) * 0.3f;
                var minAngle = -25f + region * (50f / 3f);
                var degrees = attempt < 64 ? Random.Range(minAngle, minAngle + 50f / 3f)
                    : minAngle + 0.1f + (sample / 9) * ((50f / 3f - 0.2f) / 2f);
                var angle = degrees * Mathf.Deg2Rad;
                var horizontal = Mathf.Sqrt(distance * distance - height * height);
                position = anchor + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * horizontal + Vector3.up * height;
                if (replacing && Vector3.Distance(position, previous) < 0.65f) continue;
                var overlaps = false;
                foreach (var target in targets)
                    if (target != null && Vector3.Distance(position, target.transform.position) < 0.65f) overlaps = true;
                if (overlaps || Physics.CheckSphere(position, 0.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                // Keep the target visible from the fixed round anchor.
                var direction = position - anchor;
                if (Physics.Raycast(anchor, direction.normalized, direction.magnitude, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                return true;
            }
            position = default;
            return false;
        }

        static void Remove(Target target)
        {
            target.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(target.gameObject);
            else DestroyImmediate(target.gameObject);
        }

        void OnDisable() => Stop();

        public void Stop()
        {
            for (var i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null) Remove(targets[i]);
                targets[i] = null;
            }
            initialized = false;
            warned = false;
        }
    }
}