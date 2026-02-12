namespace SilentHill2.Interaction
{
    using UnityEngine;

    /// <summary>
    /// A concrete <see cref="IInteractable"/> for items the player can pick up.
    /// The item is deactivated (or optionally destroyed) on collection and
    /// registered with the instigator's <see cref="PickupInventory"/> if one exists.
    /// </summary>
    [DisallowMultipleComponent]
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("Label shown in the interaction prompt (e.g. \"Pick up\", \"Take\").")]
        [SerializeField] private string interactionLabel = "Pick up";

        [Tooltip("Optional transform used as the detection target point. " +
                 "If unset, the object's own transform is used.")]
        [SerializeField] private Transform interactionPoint;

        [Tooltip("Destroy the GameObject on pickup instead of deactivating it.")]
        [SerializeField] private bool destroyOnPickup;

        private bool _pickedUp;

        // --- IInteractable ---

        public Transform InteractionPoint =>
            interactionPoint != null ? interactionPoint : transform;

        public string InteractionLabel => interactionLabel;

        public bool IsAvailable => !_pickedUp && isActiveAndEnabled;

        public bool CanInteract(InteractionContext context) => IsAvailable;

        public void Interact(InteractionContext context)
        {
            if (_pickedUp) return;
            _pickedUp = true;

            // Register with the instigator's inventory if one is present.
            if (context.Instigator != null
                && context.Instigator.TryGetComponent(out PickupInventory inventory))
            {
                inventory.RegisterPickup(this);
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void Reset()
        {
            // When first attached in the editor, ensure the object has a collider
            // so the InteractionSensor's spherecast can detect it.
            if (GetComponent<Collider>() == null)
            {
                var box = gameObject.AddComponent<BoxCollider>();
                box.isTrigger = false;
            }
        }
    }
}
