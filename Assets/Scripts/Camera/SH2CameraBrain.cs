using System.Collections.Generic;
using UnityEngine;

namespace SilentHillStyle.CameraSystem
{
    /// <summary>
    /// Handles camera shot switching and blending.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class SH2CameraBrain : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private SH2CameraAnchor fallbackAnchor;

        [Header("Blend")]
        [SerializeField] private float defaultBlendSeconds = 0.25f;
        [SerializeField] private float hardCutDistance = 40f;

        [Header("Safety")]
        [SerializeField] private bool autoFindPlayerByTag = true;
        [SerializeField] private string playerTag = "Player";

        private readonly HashSet<SH2CameraZone> activeZones = new();
        private Camera cachedCamera;

        private SH2CameraAnchor currentAnchor;
        private Pose fromPose;
        private float fromFov;
        private float blendDuration;
        private float blendT;

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();

            if (player == null && autoFindPlayerByTag)
            {
                var p = GameObject.FindGameObjectWithTag(playerTag);
                if (p != null)
                {
                    player = p.transform;
                }
            }

            var initial = ResolveAnchor();
            SnapToAnchor(initial);
        }

        private void LateUpdate()
        {
            var targetAnchor = ResolveAnchor();
            if (targetAnchor != currentAnchor)
            {
                StartBlend(targetAnchor);
            }

            ApplyBlend(targetAnchor);
        }

        public void RegisterZone(SH2CameraZone zone)
        {
            if (zone != null)
            {
                activeZones.Add(zone);
            }
        }

        public void UnregisterZone(SH2CameraZone zone)
        {
            if (zone != null)
            {
                activeZones.Remove(zone);
            }
        }

        private SH2CameraAnchor ResolveAnchor()
        {
            SH2CameraAnchor best = null;
            var bestPriority = int.MinValue;

            foreach (var zone in activeZones)
            {
                if (zone == null || !zone.isActiveAndEnabled)
                {
                    continue;
                }

                var candidate = zone.GetBestAnchor();
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.Priority > bestPriority)
                {
                    bestPriority = candidate.Priority;
                    best = candidate;
                }
            }

            return best != null ? best : fallbackAnchor;
        }

        private void SnapToAnchor(SH2CameraAnchor anchor)
        {
            currentAnchor = anchor;
            if (currentAnchor == null)
            {
                return;
            }

            var pose = currentAnchor.EvaluatePose(player);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
            cachedCamera.fieldOfView = currentAnchor.FieldOfView;
            blendT = 1f;
        }

        private void StartBlend(SH2CameraAnchor target)
        {
            fromPose = new Pose(transform.position, transform.rotation);
            fromFov = cachedCamera.fieldOfView;
            currentAnchor = target;
            blendT = 0f;

            var requested = target != null ? target.BlendInSeconds : defaultBlendSeconds;
            blendDuration = Mathf.Max(0.001f, requested > 0f ? requested : defaultBlendSeconds);

            if (target == null)
            {
                return;
            }

            var toPose = target.EvaluatePose(player);
            if (Vector3.Distance(fromPose.position, toPose.position) > hardCutDistance)
            {
                blendDuration = 0.001f;
            }
        }

        private void ApplyBlend(SH2CameraAnchor target)
        {
            if (target == null)
            {
                return;
            }

            var toPose = target.EvaluatePose(player);
            var targetFov = target.FieldOfView;

            blendT = Mathf.Clamp01(blendT + Time.deltaTime / blendDuration);
            var easedT = 1f - Mathf.Pow(1f - blendT, 3f);

            var blendedPos = Vector3.Lerp(fromPose.position, toPose.position, easedT);
            var blendedRot = Quaternion.Slerp(fromPose.rotation, toPose.rotation, easedT);

            if (target.EnforceLineOfSight && player != null)
            {
                blendedPos = ResolveCollisionSafePosition(target, blendedPos);
            }

            transform.SetPositionAndRotation(blendedPos, blendedRot);
            cachedCamera.fieldOfView = Mathf.Lerp(fromFov, targetFov, easedT);
        }

        private Vector3 ResolveCollisionSafePosition(SH2CameraAnchor anchor, Vector3 desired)
        {
            var playerFocus = player.position + Vector3.up * 1.5f;
            var direction = desired - playerFocus;
            var distance = direction.magnitude;

            if (distance < 0.01f)
            {
                return desired;
            }

            direction /= distance;
            if (Physics.SphereCast(playerFocus, 0.15f, direction, out var hit, distance, anchor.CollisionMask, QueryTriggerInteraction.Ignore))
            {
                return playerFocus + direction * Mathf.Max(0.5f, hit.distance - 0.05f);
            }

            return desired;
        }
    }
}
