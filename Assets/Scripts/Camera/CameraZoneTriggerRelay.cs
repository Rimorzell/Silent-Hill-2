namespace SilentHill2.Camera
{
    using SilentHill2.Player;
    using UnityEngine;

    /// <summary>
    /// Listens for trigger enter/exit events and registers or unregisters the
    /// sibling <see cref="CameraZoneVolume"/> with the scene's
    /// <see cref="FixedCameraDirector"/>. Attach to the same GameObject as
    /// <see cref="CameraZoneVolume"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CameraZoneVolume))]
    public class CameraZoneTriggerRelay : MonoBehaviour
    {
        [Tooltip("Reference to the camera director. Auto-discovered from the " +
                 "Main Camera or the scene if not assigned.")]
        [SerializeField] private FixedCameraDirector director;

        private CameraZoneVolume _zone;

        private void Awake()
        {
            _zone = GetComponent<CameraZoneVolume>();

            if (director == null)
            {
                // First choice: the Main Camera, which is the recommended host.
                if (Camera.main != null)
                {
                    director = Camera.main.GetComponent<FixedCameraDirector>();
                }

                // Fallback: search the entire scene.
                if (director == null)
                {
                    director = FindFirstObjectByType<FixedCameraDirector>();
                }

                if (director == null)
                {
                    Debug.LogError(
                        $"[CameraZoneTriggerRelay] '{name}': No FixedCameraDirector " +
                        "found in the scene. This zone will not function.", this);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (director != null && IsPlayer(other))
            {
                director.RegisterZone(_zone);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (director != null && IsPlayer(other))
            {
                director.UnregisterZone(_zone);
            }
        }

        private void OnDisable()
        {
            // If the zone is disabled at runtime (e.g. a one-shot cutscene zone),
            // make sure the director drops it from the active list.
            if (director != null)
            {
                director.UnregisterZone(_zone);
            }
        }

        /// <summary>
        /// Identifies the player by checking for a <see cref="PlayerMotor"/>
        /// anywhere in the collider's parent hierarchy. This supports player
        /// setups where the CharacterController's capsule is on a child object.
        /// </summary>
        private static bool IsPlayer(Collider other)
        {
            return other.GetComponentInParent<PlayerMotor>() != null;
        }
    }
}
