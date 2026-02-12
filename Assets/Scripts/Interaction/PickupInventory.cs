namespace SilentHill2.Interaction
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Keeps a running list of pickup IDs the player has collected.
    /// Attach to the player GameObject alongside <see cref="InteractionSensor"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public class PickupInventory : MonoBehaviour
    {
        [Tooltip("Log each pickup to the console for debugging.")]
        [SerializeField] private bool logPickups = true;

        private readonly List<string> _collectedIds = new();

        /// <summary>Read-only view of every collected pickup ID.</summary>
        public IReadOnlyList<string> CollectedIds => _collectedIds;

        /// <summary>
        /// Fired immediately after a pickup is added to the inventory.
        /// The argument is the pickup's ID (its GameObject name).
        /// </summary>
        public event Action<string> OnPickupCollected;

        /// <summary>
        /// Registers a pickup. Called by <see cref="PickupInteractable.Interact"/>
        /// when the player picks up an item.
        /// </summary>
        public void RegisterPickup(PickupInteractable pickup)
        {
            if (pickup == null) return;

            string id = pickup.gameObject.name;
            _collectedIds.Add(id);
            OnPickupCollected?.Invoke(id);

            if (logPickups)
            {
                Debug.Log(
                    $"[PickupInventory] Collected: {id} (total: {_collectedIds.Count})",
                    this);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if an item with the given ID has been collected.
        /// Useful for conditional interactions (e.g. "you need the key").
        /// </summary>
        public bool HasPickup(string id) => _collectedIds.Contains(id);
    }
}
