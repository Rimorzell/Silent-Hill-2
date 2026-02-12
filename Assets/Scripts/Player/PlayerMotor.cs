using UnityEngine;

namespace SilentHill2Like.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float moveSpeed = 2.3f;
        [SerializeField, Min(0f)] private float rotationSpeed = 12f;
        [SerializeField, Min(0f)] private float gravity = 20f;
        [SerializeField] private Transform moveReference;
        [SerializeField] private bool useCameraRelativeMovement = true;

        private CharacterController _controller;
        private Vector2 _moveInput;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (moveReference == null && Camera.main != null)
            {
                moveReference = Camera.main.transform;
            }
        }

        public void SetMoveInput(Vector2 input)
        {
            _moveInput = Vector2.ClampMagnitude(input, 1f);
        }

        public void SetMoveReference(Transform reference)
        {
            moveReference = reference;
        }

        private void Update()
        {
            if (_controller == null)
            {
                return;
            }

            var moveDirection = ResolveMoveDirection();
            var horizontalVelocity = moveDirection * moveSpeed;

            if (_controller.isGrounded)
            {
                _verticalVelocity = -1f;
            }
            else
            {
                _verticalVelocity -= gravity * Time.deltaTime;
            }

            var velocity = horizontalVelocity;
            velocity.y = _verticalVelocity;

            _controller.Move(velocity * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                var desiredRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    desiredRotation,
                    1f - Mathf.Exp(-rotationSpeed * Time.deltaTime));
            }
        }

        private Vector3 ResolveMoveDirection()
        {
            var raw = new Vector3(_moveInput.x, 0f, _moveInput.y);
            if (raw.sqrMagnitude <= Mathf.Epsilon)
            {
                return Vector3.zero;
            }

            if (!useCameraRelativeMovement || moveReference == null)
            {
                return raw.normalized;
            }

            var forward = Vector3.ProjectOnPlane(moveReference.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(moveReference.right, Vector3.up).normalized;
            var move = (forward * raw.z + right * raw.x);
            return move.sqrMagnitude > Mathf.Epsilon ? move.normalized : Vector3.zero;
        }
    }
}
