using UnityEngine;
using UnityEngine.Events;

namespace SilentHillStyle.Interaction
{
    /// <summary>
    /// Minimal pickup interaction for prototype boxes.
    /// </summary>
    public sealed class PickupInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Pickup";
        [SerializeField] private bool destroyOnPickup = true;
        [SerializeField] private UnityEvent onPickedUp;

        private bool picked;

        public string DisplayName => displayName;

        public bool CanInteract(InteractorContext context)
        {
            return !picked;
        }

        public void Interact(InteractorContext context)
        {
            if (picked)
            {
                return;
            }

            picked = true;
            onPickedUp?.Invoke();

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
