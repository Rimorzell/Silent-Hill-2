using UnityEngine;
using UnityEngine.Events;

namespace SilentHillStyle.Interaction
{
    /// <summary>
    /// Finds nearest interactable in view and invokes interaction when key is pressed.
    /// </summary>
    public sealed class SH2InteractionSensor : MonoBehaviour
    {
        [Header("Search")]
        [SerializeField] private float interactionDistance = 2f;
        [SerializeField] private float interactionRadius = 0.45f;
        [SerializeField] private LayerMask interactionMask = ~0;

        [Header("Input")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Header("References")]
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private bool autoUseMainCamera = true;

        [Header("UI Hooks")]
        [SerializeField] private UnityEvent<string> onFocusChanged;

        private readonly Collider[] overlapCache = new Collider[16];
        private IInteractable focused;

        public IInteractable Focused => focused;

        private void Awake()
        {
            if (rayOrigin == null && autoUseMainCamera && Camera.main != null)
            {
                rayOrigin = Camera.main.transform;
            }

            if (rayOrigin == null)
            {
                rayOrigin = transform;
            }
        }

        private void Update()
        {
            focused = ResolveFocusedInteractable();
            onFocusChanged?.Invoke(focused?.DisplayName ?? string.Empty);

            if (focused != null && Input.GetKeyDown(interactKey))
            {
                var context = new InteractorContext(transform);
                if (focused.CanInteract(context))
                {
                    focused.Interact(context);
                }
            }
        }

        private IInteractable ResolveFocusedInteractable()
        {
            var origin = rayOrigin.position;
            var dir = rayOrigin.forward;

            if (Physics.SphereCast(origin, interactionRadius, dir, out var hit, interactionDistance, interactionMask, QueryTriggerInteraction.Collide))
            {
                return FindInteractable(hit.collider);
            }

            var count = Physics.OverlapSphereNonAlloc(origin + dir * (interactionDistance * 0.65f), interactionRadius, overlapCache, interactionMask, QueryTriggerInteraction.Collide);
            IInteractable best = null;
            var bestDist = float.MaxValue;

            for (var i = 0; i < count; i++)
            {
                var col = overlapCache[i];
                if (col == null)
                {
                    continue;
                }

                var candidate = FindInteractable(col);
                if (candidate == null)
                {
                    continue;
                }

                var d = Vector3.Distance(origin, col.bounds.center);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }
            }

            return best;
        }

        private static IInteractable FindInteractable(Collider col)
        {
            if (col == null)
            {
                return null;
            }

            if (col.TryGetComponent<IInteractable>(out var own))
            {
                return own;
            }

            return col.GetComponentInParent<IInteractable>();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var originTransform = rayOrigin != null ? rayOrigin : transform;
            var origin = originTransform.position;
            var end = origin + originTransform.forward * interactionDistance;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, interactionRadius);
        }
#endif
    }
}
