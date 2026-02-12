using UnityEngine;

namespace SilentHillStyle.Player
{
    /// <summary>
    /// CharacterController-based movement with camera-relative input and tank-turn option.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class SH2PlayerController : MonoBehaviour
    {
        public enum InputStyle
        {
            CameraRelative,
            TankControls
        }

        [Header("Movement")]
        [SerializeField] private InputStyle inputStyle = InputStyle.CameraRelative;
        [SerializeField] private float moveSpeed = 2.4f;
        [SerializeField] private float runSpeed = 4.3f;
        [SerializeField] private float turnSpeedDeg = 420f;
        [SerializeField] private float acceleration = 14f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float groundedStickForce = -2f;

        [Header("References")]
        [SerializeField] private Transform movementCamera;
        [SerializeField] private bool autoFindMainCamera = true;

        private CharacterController controller;
        private Vector3 planarVelocity;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (movementCamera == null && autoFindMainCamera && Camera.main != null)
            {
                movementCamera = Camera.main.transform;
            }
        }

        private void Update()
        {
            var input = ReadInput();
            var isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var speed = isRunning ? runSpeed : moveSpeed;

            var desiredPlanar = ResolveDesiredPlanarVelocity(input, speed);
            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredPlanar, acceleration * Time.deltaTime);

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickForce;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            var frameVelocity = planarVelocity;
            frameVelocity.y = verticalVelocity;
            controller.Move(frameVelocity * Time.deltaTime);
        }

        private Vector2 ReadInput()
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            var v = new Vector2(x, y);
            return Vector2.ClampMagnitude(v, 1f);
        }

        private Vector3 ResolveDesiredPlanarVelocity(Vector2 input, float speed)
        {
            if (inputStyle == InputStyle.TankControls)
            {
                var turn = input.x * turnSpeedDeg * Time.deltaTime;
                transform.Rotate(0f, turn, 0f, Space.Self);

                var forward = Mathf.Max(0f, input.y);
                return transform.forward * (forward * speed);
            }

            if (movementCamera == null)
            {
                return new Vector3(input.x, 0f, input.y) * speed;
            }

            var camForward = movementCamera.forward;
            camForward.y = 0f;
            camForward.Normalize();

            var camRight = movementCamera.right;
            camRight.y = 0f;
            camRight.Normalize();

            var move = camForward * input.y + camRight * input.x;
            if (move.sqrMagnitude > 0.0001f)
            {
                var targetYaw = Quaternion.LookRotation(move, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetYaw,
                    turnSpeedDeg * Time.deltaTime);
            }

            return Vector3.ClampMagnitude(move, 1f) * speed;
        }
    }
}
