namespace SilentHill2.Interaction
{
    using System;
    using System.Collections.Generic;
    using SilentHill2.Core;
    using UnityEngine;

    /// <summary>
    /// Detects <see cref="IInteractable"/> targets in front of the player using a
    /// spherecast, then filters by angle, line-of-sight, and a weighted score to
    /// select the single best candidate each frame.
    /// <para>Attach to the player GameObject.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class InteractionSensor : MonoBehaviour
    {
        [Header("Detection")]
        [Tooltip("Maximum distance of the spherecast in units.")]
        [SerializeField, Min(0.1f)] private float maxDistance = 2.2f;

        [Tooltip("Radius of the spherecast. Larger values are more forgiving.")]
        [SerializeField, Min(0f)] private float sphereRadius = 0.25f;

        [Tooltip("Maximum angle (degrees) from the player's forward direction " +
                 "within which targets are considered.")]
        [SerializeField, Range(1f, 180f)] private float maxViewAngle = 55f;

        [Header("Line of Sight")]
        [Tooltip("When enabled, a raycast verifies nothing blocks the path " +
                 "between the player and the target.")]
        [SerializeField] private bool requireLineOfSight = true;

        [Tooltip("Physics layers that contain interactable objects.")]
        [SerializeField] private LayerMask interactionMask = ~0;

        [Tooltip("Physics layers that can block line of sight.")]
        [SerializeField] private LayerMask obstructionMask = ~0;

        [Header("References")]
        [Tooltip("Camera used for the secondary 'behind-the-camera' rejection check. " +
                 "Auto-discovered from Camera.main if not set.")]
        [SerializeField] private Camera viewCamera;

        [Header("Scoring")]
        [Tooltip("How much distance influences the score (higher = prefer closer).")]
        [SerializeField, Min(0f)] private float distanceWeight = 1.0f;

        [Tooltip("How much angle-from-center influences the score (higher = prefer centered).")]
        [SerializeField, Min(0f)] private float angleWeight = 0.5f;

        // --- Pre-allocated buffers ---
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[16];
        private readonly HashSet<int> _processedInstances = new();

        /// <summary>The current best interaction target, or <c>null</c>.</summary>
        public IInteractable CurrentTarget { get; private set; }

        /// <summary>
        /// Fired whenever <see cref="CurrentTarget"/> changes, including to
        /// <c>null</c> when no target is available.
        /// </summary>
        public event Action<IInteractable> TargetChanged;

        private void Awake()
        {
            if (viewCamera == null)
            {
                viewCamera = Camera.main;
            }
        }

        private void Update()
        {
            ValidateCurrentTarget();
            ScanForBestTarget();
        }

        /// <summary>
        /// Allows external code (e.g. a camera system) to swap the view camera
        /// reference at runtime.
        /// </summary>
        public void SetViewCamera(Camera cam) => viewCamera = cam;

        /// <summary>
        /// Attempts to interact with <see cref="CurrentTarget"/>.
        /// Returns <c>true</c> if the interaction was executed.
        /// </summary>
        public bool TryInteract()
        {
            if (CurrentTarget == null) return false;

            var context = new InteractionContext(gameObject, viewCamera);
            if (!CurrentTarget.CanInteract(context)) return false;

            CurrentTarget.Interact(context);

            // The target may have become unavailable (picked up, destroyed, etc.).
            if (!CurrentTarget.IsAvailable)
            {
                SetTarget(null);
            }

            return true;
        }

        // ------------------------------------------------------------------
        //  Target validation
        // ------------------------------------------------------------------

        /// <summary>
        /// Ensures the current target is still a valid, living object. Handles
        /// the case where the backing MonoBehaviour was destroyed between frames.
        /// </summary>
        private void ValidateCurrentTarget()
        {
            if (CurrentTarget == null) return;

            // Unity's "fake null": a destroyed MonoBehaviour still satisfies
            // a C# null check on the interface reference but fails Unity's
            // equality operator.
            if (CurrentTarget is MonoBehaviour mb && mb == null)
            {
                SetTarget(null);
                return;
            }

            if (!CurrentTarget.IsAvailable)
            {
                SetTarget(null);
            }
        }

        // ------------------------------------------------------------------
        //  Scanning
        // ------------------------------------------------------------------

        private void ScanForBestTarget()
        {
            // Origin slightly above the player's feet so the cast clears
            // ground-level geometry.
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 castDir = transform.forward;

            int hitCount = Physics.SphereCastNonAlloc(
                origin, sphereRadius, castDir,
                _hitBuffer, maxDistance,
                interactionMask, QueryTriggerInteraction.Collide);

            _processedInstances.Clear();

            IInteractable best = null;
            float bestScore = float.MaxValue; // lower is better

            for (int i = 0; i < hitCount; i++)
            {
                ref RaycastHit hit = ref _hitBuffer[i];
                Collider hitCollider = hit.collider;
                if (hitCollider == null) continue;

                // Find an IInteractable on the hit object or its parents.
                if (!hitCollider.TryGetComponentInParent(out IInteractable interactable))
                    continue;

                // Deduplicate: multiple colliders can belong to the same
                // interactable (e.g. compound colliders).
                int instanceId = interactable is MonoBehaviour interactableMb
                    ? interactableMb.gameObject.GetInstanceID()
                    : hitCollider.gameObject.GetInstanceID();

                if (!_processedInstances.Add(instanceId)) continue;
                if (!interactable.IsAvailable) continue;

                // Resolve the interaction point.
                Transform targetPoint = interactable.InteractionPoint;
                if (targetPoint == null && interactable is MonoBehaviour mb2)
                    targetPoint = mb2.transform;
                if (targetPoint == null) continue;

                Vector3 toTarget = targetPoint.position - origin;
                float distance = toTarget.magnitude;
                if (distance < Mathf.Epsilon || distance > maxDistance) continue;

                // Primary filter: angle from the player's forward direction.
                Vector3 dirToTarget = toTarget / distance;
                float angle = Vector3.Angle(transform.forward, dirToTarget);
                if (angle > maxViewAngle) continue;

                // Secondary filter: reject targets that are behind the camera.
                // This prevents interacting with objects the player can't see.
                if (viewCamera != null)
                {
                    Vector3 camToTarget = targetPoint.position - viewCamera.transform.position;
                    float camAngle = Vector3.Angle(viewCamera.transform.forward, camToTarget.normalized);
                    if (camAngle > 90f) continue;
                }

                // Line-of-sight check.
                if (requireLineOfSight && IsObstructed(origin, targetPoint.position, distance, hitCollider))
                    continue;

                // Score: lower is better (closer + more centred).
                float normDist = distance / maxDistance;
                float normAngle = angle / maxViewAngle;
                float score = normDist * distanceWeight + normAngle * angleWeight;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = interactable;
                }
            }

            SetTarget(best);
        }

        // ------------------------------------------------------------------
        //  Line of sight
        // ------------------------------------------------------------------

        private bool IsObstructed(Vector3 origin, Vector3 target, float distance, Collider targetCollider)
        {
            Vector3 direction = (target - origin).normalized;

            if (Physics.Raycast(origin, direction, out RaycastHit obstruction, distance,
                    obstructionMask, QueryTriggerInteraction.Ignore))
            {
                // The hit is not an obstruction if it belongs to the target itself
                // or any of its children (compound collider setups).
                return obstruction.collider != targetCollider
                    && !obstruction.collider.transform.IsChildOf(targetCollider.transform);
            }

            return false;
        }

        // ------------------------------------------------------------------
        //  Target bookkeeping
        // ------------------------------------------------------------------

        private void SetTarget(IInteractable newTarget)
        {
            if (ReferenceEquals(CurrentTarget, newTarget)) return;

            CurrentTarget = newTarget;
            TargetChanged?.Invoke(CurrentTarget);
        }

        // ------------------------------------------------------------------
        //  Editor gizmos
        // ------------------------------------------------------------------

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 end = origin + transform.forward * maxDistance;

            // Detection ray
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, sphereRadius);

            // Origin sphere
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(origin, sphereRadius);
        }
#endif
    }
}
