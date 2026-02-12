using System;
using System.Collections.Generic;
using UnityEngine;

namespace SilentHill2Prototype.Camera
{
    [RequireComponent(typeof(Collider))]
    public sealed class SH2CameraZoneTracker : MonoBehaviour
    {
        public event Action ZonesChanged;

        private readonly HashSet<SH2CameraZone> _zones = new();

        public IReadOnlyCollection<SH2CameraZone> ActiveZones => _zones;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out SH2CameraZone zone) && _zones.Add(zone))
            {
                ZonesChanged?.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out SH2CameraZone zone) && _zones.Remove(zone))
            {
                ZonesChanged?.Invoke();
            }
        }
    }
}
