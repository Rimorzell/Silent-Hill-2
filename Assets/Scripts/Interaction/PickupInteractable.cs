using UnityEngine;

namespace SilentHill2Like.Interaction
{
    [DisallowMultipleComponent]
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionLabel = "Pick up";
        [SerializeField] private Transform interactionPoint;
        [SerializeField] private bool destroyOnPickup = false;

        private bool _picked;

        public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;
        public string InteractionLabel => interactionLabel;
        public bool IsAvailable => !_picked && isActiveAndEnabled;

        public bool CanInteract(InteractionContext context)
        {
            return IsAvailable;
        }

        public void Interact(InteractionContext context)
        {
            if (_picked)
            {
                return;
            }

            _picked = true;

            if (context.Instigator != null &&
                context.Instigator.TryGetComponent(out PickupInventory inventory))
            {
                inventory.RegisterPickup(this);
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
