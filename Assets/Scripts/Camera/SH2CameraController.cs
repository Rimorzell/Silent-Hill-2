using System.Linq;
using UnityEngine;

namespace SilentHill2Prototype.Camera
{
    public sealed class SH2CameraController : MonoBehaviour
    {
        [SerializeField] private Transform playerTarget;
        [SerializeField] private Vector3 fallbackOffset = new(0f, 1.7f, -3.6f);
        [SerializeField] private float fallbackPositionSharpness = 8f;
        [SerializeField] private float fallbackLookAtHeight = 1.2f;
        [SerializeField] private LayerMask zoneMask = ~0;
        [SerializeField] private float zoneProbeRadius = 0.2f;

        private Camera _camera;
        private SH2CameraZone _activeZone;
        private float _zoneBlendProgress;
        private Vector3 _blendFromPosition;
        private Quaternion _blendFromRotation;
        private float _blendFromFov;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (playerTarget == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTarget = player.transform;
                }
            }

            _blendFromPosition = transform.position;
            _blendFromRotation = transform.rotation;
            _blendFromFov = _camera != null ? _camera.fieldOfView : 60f;
        }

        private void LateUpdate()
        {
            if (playerTarget == null)
            {
                return;
            }

            SH2CameraZone bestZone = FindBestZone();
            if (bestZone != _activeZone)
            {
                _activeZone = bestZone;
                _zoneBlendProgress = 0f;
                _blendFromPosition = transform.position;
                _blendFromRotation = transform.rotation;
                _blendFromFov = _camera.fieldOfView;
            }

            if (_activeZone != null)
            {
                UpdateZoneCamera(_activeZone);
            }
            else
            {
                UpdateFallbackCamera();
            }
        }

        private SH2CameraZone FindBestZone()
        {
            Collider[] colliders = Physics.OverlapSphere(playerTarget.position, zoneProbeRadius, zoneMask, QueryTriggerInteraction.Collide);
            return colliders
                .Select(c => c.GetComponent<SH2CameraZone>())
                .Where(z => z != null)
                .OrderByDescending(z => z.Priority)
                .FirstOrDefault();
        }

        private void UpdateZoneCamera(SH2CameraZone zone)
        {
            _zoneBlendProgress += Time.deltaTime / zone.BlendTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(_zoneBlendProgress));

            Transform point = zone.VirtualCameraPoint;
            transform.position = Vector3.Lerp(_blendFromPosition, point.position, t);
            transform.rotation = Quaternion.Slerp(_blendFromRotation, point.rotation, t);
            _camera.fieldOfView = Mathf.Lerp(_blendFromFov, zone.FieldOfView, t);
        }

        private void UpdateFallbackCamera()
        {
            Vector3 desiredPosition = playerTarget.TransformPoint(fallbackOffset);
            float sharp = 1f - Mathf.Exp(-fallbackPositionSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, sharp);

            Vector3 lookPoint = playerTarget.position + Vector3.up * fallbackLookAtHeight;
            Quaternion targetRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, sharp);
        }
    }
}
