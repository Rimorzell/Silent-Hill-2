using UnityEngine;

namespace SilentHillStyle.CameraSystem
{
    /// <summary>
    /// Registers/deregisters zone occupancy for the player.
    /// </summary>
    [RequireComponent(typeof(SH2CameraZone), typeof(Collider))]
    public sealed class SH2CameraZoneTrigger : MonoBehaviour
    {
        [SerializeField] private SH2CameraBrain cameraBrain;
        [SerializeField] private string playerTag = "Player";

        private SH2CameraZone zone;

        private void Awake()
        {
            zone = GetComponent<SH2CameraZone>();

            if (cameraBrain == null)
            {
                cameraBrain = FindFirstObjectByType<SH2CameraBrain>();
            }

            var col = GetComponent<Collider>();
            if (!col.isTrigger)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (cameraBrain == null || !other.CompareTag(playerTag))
            {
                return;
            }

            cameraBrain.RegisterZone(zone);
        }

        private void OnTriggerExit(Collider other)
        {
            if (cameraBrain == null || !other.CompareTag(playerTag))
            {
                return;
            }

            cameraBrain.UnregisterZone(zone);
        }
    }
}
