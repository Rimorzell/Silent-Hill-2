using UnityEngine;

namespace SilentHillStyle.CameraSystem
{
    /// <summary>
    /// Defines a fixed (or lightly dynamic) cinematic camera shot.
    /// </summary>
    public sealed class SH2CameraAnchor : MonoBehaviour
    {
        [Header("Shot")]
        [SerializeField] private float fieldOfView = 55f;
        [SerializeField] private float blendInSeconds = 0.35f;
        [SerializeField] private int priority = 0;

        [Header("Follow Offset (optional)")]
        [SerializeField] private bool followPlayerYaw;
        [SerializeField] private Vector3 localFollowOffset = new(0f, 0f, 0f);

        [Header("Collision")]
        [SerializeField] private bool enforceLineOfSight = true;
        [SerializeField] private LayerMask collisionMask = ~0;

        public float FieldOfView => fieldOfView;
        public float BlendInSeconds => Mathf.Max(0f, blendInSeconds);
        public int Priority => priority;
        public bool FollowPlayerYaw => followPlayerYaw;
        public Vector3 LocalFollowOffset => localFollowOffset;
        public bool EnforceLineOfSight => enforceLineOfSight;
        public LayerMask CollisionMask => collisionMask;

        private void OnValidate()
        {
            fieldOfView = Mathf.Clamp(fieldOfView, 20f, 90f);
            blendInSeconds = Mathf.Max(0f, blendInSeconds);
        }

        public Pose EvaluatePose(Transform player)
        {
            if (player == null || !followPlayerYaw)
            {
                return new Pose(transform.position, transform.rotation);
            }

            var yaw = Quaternion.Euler(0f, player.eulerAngles.y, 0f);
            var worldOffset = yaw * localFollowOffset;
            var pos = transform.position + worldOffset;
            return new Pose(pos, transform.rotation);
        }
    }
}
