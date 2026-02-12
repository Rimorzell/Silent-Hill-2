using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilentHill2Like.Interaction
{
    [DisallowMultipleComponent]
    public class InteractionSensor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField, Min(0.1f)] private float maxDistance = 2.2f;
        [SerializeField, Min(0f)] private float sphereRadius = 0.25f;
        [SerializeField] private LayerMask interactionMask = ~0;
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private bool requireLineOfSight = true;
        [SerializeField] private float maxViewAngle = 55f;

        [Header("References")]
        [SerializeField] private Camera viewCamera;

        private readonly RaycastHit[] _hits = new RaycastHit[16];
        private readonly HashSet<IInteractable> _uniqueBuffer = new();

        public IInteractable CurrentTarget { get; private set; }

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
            ScanForTarget();
        }

        public void SetViewCamera(Camera cameraRef)
        {
            viewCamera = cameraRef;
        }

        public bool TryInteract()
        {
            if (CurrentTarget == null)
            {
                return false;
            }

            var context = new InteractionContext(gameObject, viewCamera);
            if (!CurrentTarget.CanInteract(context))
            {
                return false;
            }

            CurrentTarget.Interact(context);
            if (!CurrentTarget.IsAvailable)
            {
                SetTarget(null);
            }

            return true;
        }

        private void ScanForTarget()
        {
            if (viewCamera == null)
            {
                SetTarget(null);
                return;
            }

            var ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var hitCount = Physics.SphereCastNonAlloc(
                ray,
                sphereRadius,
                _hits,
                maxDistance,
                interactionMask,
                QueryTriggerInteraction.Collide);

            _uniqueBuffer.Clear();

            IInteractable bestTarget = null;
            var bestScore = float.MinValue;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];
                if (!hit.collider.TryGetComponentInParent(out MonoBehaviour mono) || mono is not IInteractable interactable)
                {
                    continue;
                }

                if (!_uniqueBuffer.Add(interactable) || !interactable.IsAvailable)
                {
                    continue;
                }

                var targetTransform = interactable.InteractionPoint != null ? interactable.InteractionPoint : mono.transform;
                var toTarget = targetTransform.position - viewCamera.transform.position;
                var distance = toTarget.magnitude;
                if (distance > maxDistance || distance <= Mathf.Epsilon)
                {
                    continue;
                }

                var direction = toTarget / distance;
                var viewAngle = Vector3.Angle(viewCamera.transform.forward, direction);
                if (viewAngle > maxViewAngle)
                {
                    continue;
                }

                if (requireLineOfSight && IsObstructed(viewCamera.transform.position, targetTransform.position, distance, hit.collider))
                {
                    continue;
                }

                var score = 1000f - (distance * 10f) - viewAngle;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = interactable;
                }
            }

            SetTarget(bestTarget);
        }

        private bool IsObstructed(Vector3 origin, Vector3 target, float distance, Collider targetCollider)
        {
            if (Physics.Linecast(origin, target, out var obstruction, obstructionMask, QueryTriggerInteraction.Ignore))
            {
                return obstruction.collider != targetCollider && !obstruction.collider.transform.IsChildOf(targetCollider.transform);
            }

            return false;
        }

        private void SetTarget(IInteractable newTarget)
        {
            if (ReferenceEquals(CurrentTarget, newTarget))
            {
                return;
            }

            CurrentTarget = newTarget;
            TargetChanged?.Invoke(CurrentTarget);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (viewCamera == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            var origin = viewCamera.transform.position;
            var direction = viewCamera.transform.forward;
            Gizmos.DrawLine(origin, origin + direction * maxDistance);
            Gizmos.DrawWireSphere(origin + direction * maxDistance, sphereRadius);
        }
#endif
    }
}
