using SilentHill2Prototype.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SilentHill2Prototype.Interaction
{
    public sealed class SH2InteractionController : MonoBehaviour
    {
        [SerializeField] private SH2InputReader inputReader;
        [SerializeField] private Transform interactor;
        [SerializeField] private Camera interactionCamera;
        [SerializeField] private float interactionDistance = 2.2f;
        [SerializeField] private float sphereCastRadius = 0.15f;
        [SerializeField] private LayerMask interactableMask = ~0;
        [SerializeField] private Text promptText;

        private ISH2Interactable _focused;
        private readonly RaycastHit[] _hits = new RaycastHit[8];

        private void Awake()
        {
            if (interactor == null)
            {
                interactor = transform;
            }

            if (inputReader == null)
            {
                inputReader = FindFirstObjectByType<SH2InputReader>();
            }

            if (interactionCamera == null)
            {
                interactionCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.InteractPressed += HandleInteractPressed;
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.InteractPressed -= HandleInteractPressed;
            }

            SetFocused(null);
        }

        private void Update()
        {
            RefreshFocus();
            UpdatePrompt();
        }

        private void RefreshFocus()
        {
            if (interactionCamera == null)
            {
                SetFocused(null);
                return;
            }

            Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
            int hitCount = Physics.SphereCastNonAlloc(ray, sphereCastRadius, _hits, interactionDistance, interactableMask, QueryTriggerInteraction.Collide);

            ISH2Interactable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hits[i];
                if (!hit.collider.TryGetComponent(out MonoBehaviour mb))
                {
                    continue;
                }

                if (mb is not ISH2Interactable interactable)
                {
                    continue;
                }

                if (!interactable.CanInteract(interactor))
                {
                    continue;
                }

                if (hit.distance < bestDistance)
                {
                    best = interactable;
                    bestDistance = hit.distance;
                }
            }

            SetFocused(best);
        }

        private void SetFocused(ISH2Interactable next)
        {
            if (ReferenceEquals(_focused, next))
            {
                return;
            }

            _focused?.OnFocusChanged(false);
            _focused = next;
            _focused?.OnFocusChanged(true);
        }

        private void HandleInteractPressed()
        {
            if (_focused == null || !_focused.CanInteract(interactor))
            {
                return;
            }

            _focused.Interact(interactor);
            RefreshFocus();
            UpdatePrompt();
        }

        private void UpdatePrompt()
        {
            if (promptText == null)
            {
                return;
            }

            promptText.enabled = _focused != null;
            promptText.text = _focused?.GetPrompt() ?? string.Empty;
        }
    }
}
