namespace SilentHill2.Camera
{
    using System.Collections.Generic;
    using SilentHill2.Player;
    using UnityEngine;

    /// <summary>
    /// Core camera controller that implements the Silent Hill 2 hybrid camera
    /// system. When the player is inside one or more <see cref="CameraZoneVolume"/>
    /// triggers the camera blends to the highest-priority zone's pose. When no
    /// zones are active a smooth follow-camera tracks behind the player.
    /// <para>Attach to the <b>Main Camera</b> GameObject.</para>
    /// </summary>
    [DisallowMultipleComponent]
    public class FixedCameraDirector : MonoBehaviour
    {
        [Header("Fallback Follow Camera")]
        [Tooltip("Player transform. Auto-discovered via PlayerMotor if not assigned.")]
        [SerializeField] private Transform player;

        [Tooltip("Offset from the player in the player's local space. " +
                 "Default places the camera above and behind.")]
        [SerializeField] private Vector3 followOffset = new(0f, 4f, -6f);

        [Tooltip("SmoothDamp time for the follow camera's position (seconds). " +
                 "Lower = faster response.")]
        [SerializeField, Min(0.01f)] private float followPositionSmoothing = 0.15f;

        [Tooltip("Exponential decay rate for the follow camera's rotation. " +
                 "Higher = snappier look-at.")]
        [SerializeField, Min(0.1f)] private float followRotationSpeed = 8f;

        [Tooltip("Vertical offset on the player to look at (e.g. chest height).")]
        [SerializeField] private float lookAtHeightOffset = 1.0f;

        // --- Runtime state ---
        private readonly List<CameraZoneVolume> _activeZones = new();
        private CameraZoneVolume _currentZone;
        private Vector3 _positionVelocity;

        private void Awake()
        {
            if (player == null)
            {
                var motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null) player = motor.transform;
            }

            if (player == null)
            {
                Debug.LogWarning(
                    "[FixedCameraDirector] No player found. The follow camera " +
                    "will not function until a player is present.", this);
            }
        }

        private void LateUpdate()
        {
            PruneInvalidZones();
            _currentZone = ResolveBestZone();

            if (_currentZone != null && _currentZone.IsValid)
            {
                BlendToZone(_currentZone);
            }
            else
            {
                UpdateFollowCamera();
            }
        }

        // ------------------------------------------------------------------
        //  Public API -- called by CameraZoneTriggerRelay
        // ------------------------------------------------------------------

        public void RegisterZone(CameraZoneVolume zone)
        {
            if (zone == null || _activeZones.Contains(zone)) return;
            _activeZones.Add(zone);
        }

        public void UnregisterZone(CameraZoneVolume zone)
        {
            if (zone == null) return;
            _activeZones.Remove(zone);

            // Reset SmoothDamp velocity when the zone we were blending toward
            // is removed, preventing a "slingshot" from leftover velocity.
            if (_currentZone == zone)
            {
                _positionVelocity = Vector3.zero;
            }
        }

        // ------------------------------------------------------------------
        //  Zone resolution
        // ------------------------------------------------------------------

        /// <summary>
        /// Removes null, destroyed, or disabled zones from the active list.
        /// Called every frame before resolution so stale entries never persist.
        /// </summary>
        private void PruneInvalidZones()
        {
            for (int i = _activeZones.Count - 1; i >= 0; i--)
            {
                if (_activeZones[i] == null || !_activeZones[i].isActiveAndEnabled)
                {
                    _activeZones.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Selects the highest-priority valid zone from the active list.
        /// Returns <c>null</c> when no valid zones are active.
        /// </summary>
        private CameraZoneVolume ResolveBestZone()
        {
            CameraZoneVolume best = null;
            int bestPriority = int.MinValue;

            for (int i = 0; i < _activeZones.Count; i++)
            {
                var zone = _activeZones[i];
                if (!zone.IsValid) continue;

                if (zone.Priority > bestPriority)
                {
                    bestPriority = zone.Priority;
                    best = zone;
                }
            }

            return best;
        }

        // ------------------------------------------------------------------
        //  Zone camera blend
        // ------------------------------------------------------------------

        private void BlendToZone(CameraZoneVolume zone)
        {
            Transform pose = zone.CameraPose;
            float dampTime = zone.BlendTime;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                pose.position,
                ref _positionVelocity,
                dampTime);

            // Frame-rate-independent exponential decay for rotation.
            float t = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.01f, dampTime));
            transform.rotation = Quaternion.Slerp(transform.rotation, pose.rotation, t);
        }

        // ------------------------------------------------------------------
        //  Fallback follow camera
        // ------------------------------------------------------------------

        private void UpdateFollowCamera()
        {
            if (player == null) return;

            // Target position: offset rotated into the player's local space so
            // the camera always sits "behind" the player regardless of facing.
            Vector3 targetPosition = player.position + player.rotation * followOffset;

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref _positionVelocity,
                followPositionSmoothing);

            // Look at the player's approximate chest height.
            Vector3 lookTarget = player.position + Vector3.up * lookAtHeightOffset;
            Quaternion targetRotation = Quaternion.LookRotation(
                lookTarget - transform.position, Vector3.up);

            float rotT = 1f - Mathf.Exp(-followRotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotT);
        }
    }
}
