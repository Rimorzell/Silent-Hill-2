namespace SilentHill2.Player
{
    using UnityEngine;

    /// <summary>
    /// Drives the player's <see cref="CharacterController"/> using camera-relative
    /// input. Attach to the player GameObject (the cylinder).
    /// <para>
    /// Movement direction is computed by projecting the camera's forward and right
    /// axes onto the ground plane and remapping the stick/WASD input accordingly.
    /// This means "push up" always moves the character away from the camera,
    /// regardless of the current camera angle.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Horizontal walk speed in units per second.")]
        [SerializeField, Min(0f)] private float walkSpeed = 2.5f;

        [Tooltip("How quickly the character rotates to face the movement direction. " +
                 "Higher values give snappier turning.")]
        [SerializeField, Min(0f)] private float rotationSpeed = 8f;

        [Tooltip("Downward acceleration applied every frame (negative value).")]
        [SerializeField] private float gravity = -20f;

        [Header("Camera Reference")]
        [Tooltip("Transform whose forward/right axes define 'camera-relative' movement. " +
                 "Auto-discovered from Camera.main if left empty.")]
        [SerializeField] private Transform cameraReference;

        [Tooltip("Uncheck to use raw world-space input instead of camera-relative.")]
        [SerializeField] private bool useCameraRelativeMovement = true;

        private CharacterController _controller;
        private Vector2 _moveInput;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (cameraReference == null && Camera.main != null)
            {
                cameraReference = Camera.main.transform;
            }
        }

        /// <summary>
        /// Called every frame by <see cref="Core.PlayerInputBridge"/> with the
        /// raw analogue stick / WASD input.
        /// </summary>
        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        /// <summary>
        /// Allows the camera system to swap the reference at runtime
        /// (e.g. when switching between cameras).
        /// </summary>
        public void SetCameraReference(Transform reference)
        {
            cameraReference = reference;
        }

        private void Update()
        {
            if (_controller == null) return;

            // --- Horizontal movement ---
            Vector3 moveDir = ComputeWorldMoveDirection();
            Vector3 horizontalVelocity = moveDir * walkSpeed;

            // --- Gravity ---
            if (_controller.isGrounded)
            {
                // A small constant downward force keeps isGrounded stable on
                // slopes and prevents the controller from "hovering."
                _verticalVelocity = -2f;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            // --- Apply ---
            Vector3 finalVelocity = horizontalVelocity;
            finalVelocity.y = _verticalVelocity;
            _controller.Move(finalVelocity * Time.deltaTime);

            // --- Rotate toward movement direction ---
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                float t = 1f - Mathf.Exp(-rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            }
        }

        private Vector3 ComputeWorldMoveDirection()
        {
            if (_moveInput.sqrMagnitude < Mathf.Epsilon)
                return Vector3.zero;

            // Fallback: treat input as world-space if no camera reference or
            // camera-relative mode is disabled.
            if (!useCameraRelativeMovement || cameraReference == null)
            {
                return new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
            }

            // Project camera axes onto the horizontal plane.
            Vector3 camForward = Vector3.ProjectOnPlane(
                cameraReference.forward, Vector3.up).normalized;
            Vector3 camRight = Vector3.ProjectOnPlane(
                cameraReference.right, Vector3.up).normalized;

            // If the camera is pointing straight down the projection collapses;
            // fall back to the camera's up axis projected onto the ground.
            if (camForward.sqrMagnitude < 0.001f)
            {
                camForward = Vector3.ProjectOnPlane(
                    cameraReference.up, Vector3.up).normalized;
            }

            Vector3 worldDir = camForward * _moveInput.y + camRight * _moveInput.x;
            return worldDir.sqrMagnitude > Mathf.Epsilon
                ? worldDir.normalized
                : Vector3.zero;
        }
    }
}
