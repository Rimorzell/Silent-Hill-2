using System.Collections.Generic;
using SilentHill2Like.Player;
using UnityEngine;

namespace SilentHill2Like.CameraSystem
{
    [DisallowMultipleComponent]
    public class FixedCameraDirector : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 fallbackOffset = new(0f, 4f, -6f);
        [SerializeField] private float fallbackPositionDamping = 8f;
        [SerializeField] private float fallbackRotationDamping = 10f;

        private readonly List<CameraZoneVolume> _activeZones = new();

        private CameraZoneVolume _currentZone;
        private Vector3 _velocity;

        public Transform CurrentCameraReference => _currentZone != null ? _currentZone.CameraPose : transform;

        private void Awake()
        {
            if (player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null)
                {
                    player = motor.transform;
                }
            }
        }

        private void LateUpdate()
        {
            _currentZone = ResolveBestZone();

            if (_currentZone != null)
            {
                UpdateZoneCamera(_currentZone);
            }
            else
            {
                UpdateFallbackCamera();
            }
        }

        public void RegisterZone(CameraZoneVolume zone)
        {
            if (zone == null || _activeZones.Contains(zone))
            {
                return;
            }

            _activeZones.Add(zone);
        }

        public void UnregisterZone(CameraZoneVolume zone)
        {
            if (zone == null)
            {
                return;
            }

            _activeZones.Remove(zone);
        }

        private CameraZoneVolume ResolveBestZone()
        {
            CameraZoneVolume best = null;
            var bestPriority = int.MinValue;

            for (var i = _activeZones.Count - 1; i >= 0; i--)
            {
                var zone = _activeZones[i];
                if (zone == null || !zone.isActiveAndEnabled)
                {
                    _activeZones.RemoveAt(i);
                    continue;
                }

                if (zone.Priority >= bestPriority)
                {
                    bestPriority = zone.Priority;
                    best = zone;
                }
            }

            return best;
        }

        private void UpdateZoneCamera(CameraZoneVolume zone)
        {
            var pose = zone.CameraPose;
            var damp = Mathf.Max(0.01f, zone.BlendTime);
            transform.position = Vector3.SmoothDamp(transform.position, pose.position, ref _velocity, damp);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                pose.rotation,
                1f - Mathf.Exp(-Time.deltaTime / damp));
        }

        private void UpdateFallbackCamera()
        {
            if (player == null)
            {
                return;
            }

            var targetPosition = player.position + fallbackOffset;
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                1f - Mathf.Exp(-fallbackPositionDamping * Time.deltaTime));

            var targetRotation = Quaternion.LookRotation(player.position - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-fallbackRotationDamping * Time.deltaTime));
        }
    }
}
