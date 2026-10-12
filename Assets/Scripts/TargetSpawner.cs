using UnityEngine;

namespace NeonReflexVR
{
    /// <summary>Three fixed slots; one clock, no per-target coroutine or event subscriptions.</summary>
    public sealed class TargetSpawner : MonoBehaviour
    {
        [SerializeField] Target targetPrefab;
        [SerializeField] Transform view;
        readonly Target[] targets = new Target[3];
        readonly float[] nextAttempt = new float[3];
        Vector3 anchor;
        Vector3 forward;
        Vector3 right;
        bool initialized;
        bool warned;

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
            if (!initialized && view != null && targetPrefab != null)
                Initialize(view.position, view.forward, Time.time);
            Tick(Time.time);
        }

        public void Initialize(Vector3 eyePosition, Vector3 lookDirection, float now)
        {
            if (initialized || targetPrefab == null) return;
            anchor = eyePosition;
            forward = Vector3.ProjectOnPlane(lookDirection, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.5f) forward = Vector3.forward;
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
                if (TryPosition(previous, replacing, out var position))
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

        bool TryPosition(Vector3 previous, bool replacing, out Vector3 position)
        {
            for (var attempt = 0; attempt < 64; attempt++)
            {
                var distance = Random.Range(2.5f, 3.5f);
                var height = Random.Range(-0.5f, 0.1f);
                var angle = Random.Range(-25f, 25f) * Mathf.Deg2Rad;
                var horizontal = Mathf.Sqrt(distance * distance - height * height);
                position = anchor + (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)) * horizontal + Vector3.up * height;
                if (replacing && Vector3.Distance(position, previous) < 0.65f) continue;
                var overlaps = false;
                foreach (var target in targets)
                    if (target != null && Vector3.Distance(position, target.transform.position) < 0.65f) overlaps = true;
                if (overlaps || Physics.CheckSphere(position, 0.2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)) continue;
                // Avoid spawning behind the preserved test cubes or other solid obstacles.
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

        void OnDisable()
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