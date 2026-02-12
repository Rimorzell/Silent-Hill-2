namespace SilentHill2.Camera
{
    using UnityEngine;

    /// <summary>
    /// Data component that defines a fixed-camera zone. Attach to a GameObject
    /// with a trigger <see cref="Collider"/>. When the player enters the trigger,
    /// <see cref="CameraZoneTriggerRelay"/> registers this zone with the
    /// <see cref="FixedCameraDirector"/>, which blends the camera to
    /// <see cref="CameraPose"/>.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class CameraZoneVolume : MonoBehaviour
    {
        [Tooltip("Transform defining the camera position and rotation for this zone. " +
                 "Create an empty child GameObject, position/rotate it to the desired " +
                 "camera angle, and drag it here.")]
        [SerializeField] private Transform cameraPose;

        [Tooltip("Time in seconds to blend to this zone's camera pose.")]
        [SerializeField, Min(0.01f)] private float blendTime = 0.5f;

        [Tooltip("Higher-priority zones override lower ones when the player is " +
                 "inside multiple zones simultaneously.")]
        [SerializeField] private int priority;

        /// <summary>
        /// The target camera transform. Falls back to this component's own
        /// transform (with a warning) if no pose is assigned.
        /// </summary>
        public Transform CameraPose
        {
            get
            {
                if (cameraPose != null) return cameraPose;

                Debug.LogWarning(
                    $"[CameraZoneVolume] '{name}' has no camera pose assigned. " +
                    "Falling back to the zone's own transform.", this);
                return transform;
            }
        }

        /// <summary>
        /// <c>true</c> when <see cref="cameraPose"/> references a live, non-destroyed
        /// transform. The director checks this before blending to avoid logging
        /// a warning every frame.
        /// </summary>
        public bool IsValid => cameraPose != null;

        public float BlendTime => blendTime;
        public int Priority => priority;

        private void Reset()
        {
            // When the component is first added in the editor, ensure the
            // collider is configured as a trigger.
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnValidate()
        {
            var col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                Debug.LogWarning(
                    $"[CameraZoneVolume] '{name}': The collider must be set to " +
                    "Is Trigger for camera zone detection to work.", this);
            }
        }
    }
}
