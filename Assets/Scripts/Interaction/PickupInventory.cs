using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilentHill2Like.Interaction
{
    [DisallowMultipleComponent]
    public class PickupInventory : MonoBehaviour
    {
        [SerializeField] private bool logPickupEvents = true;

        private readonly List<string> _pickupIds = new();

        public IReadOnlyList<string> PickupIds => _pickupIds;

        public event Action<string> PickupRegistered;

        public void RegisterPickup(PickupInteractable pickup)
        {
            if (pickup == null)
            {
                return;
            }

            var pickupId = pickup.name;
            _pickupIds.Add(pickupId);
            PickupRegistered?.Invoke(pickupId);

            if (logPickupEvents)
            {
                Debug.Log($"Pickup collected: {pickupId}", this);
            }
        }
    }
}
