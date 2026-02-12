using SilentHill2Like.Player;
using UnityEngine;

namespace SilentHill2Like.CameraSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CameraZoneVolume))]
    public class CameraZoneTriggerRelay : MonoBehaviour
    {
        [SerializeField] private FixedCameraDirector director;

        private CameraZoneVolume _zone;

        private void Awake()
        {
            _zone = GetComponent<CameraZoneVolume>();
            if (director == null && Camera.main != null)
            {
                director = Camera.main.GetComponent<FixedCameraDirector>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (director == null || !IsPlayer(other))
            {
                return;
            }

            director.RegisterZone(_zone);
        }

        private void OnTriggerExit(Collider other)
        {
            if (director == null || !IsPlayer(other))
            {
                return;
            }

            director.UnregisterZone(_zone);
        }

        private void OnDisable()
        {
            if (director != null)
            {
                director.UnregisterZone(_zone);
            }
        }

        private static bool IsPlayer(Collider other)
        {
            return other.GetComponentInParent<PlayerMotor>() != null;
        }
    }
}
