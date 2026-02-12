using SilentHill2Prototype.Core;
using UnityEngine;

namespace SilentHill2Prototype.Character
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class SH2CharacterMotor : MonoBehaviour
    {
        [SerializeField] private SH2InputReader inputReader;
        [SerializeField] private Transform cameraReference;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 3.6f;
        [SerializeField] private float acceleration = 12f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float rotationSharpness = 14f;

        private CharacterController _controller;
        private Vector3 _planarVelocity;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();

            if (inputReader == null)
            {
                inputReader = FindFirstObjectByType<SH2InputReader>();
            }

            if (cameraReference == null && Camera.main != null)
            {
                cameraReference = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (inputReader == null)
            {
                return;
            }

            Vector2 input = Vector2.ClampMagnitude(inputReader.Move, 1f);
            Vector3 desiredDirection = GetCameraRelativeDirection(input);
            float targetSpeed = inputReader.IsRunning ? runSpeed : walkSpeed;
            Vector3 targetVelocity = desiredDirection * targetSpeed;

            _planarVelocity = Vector3.MoveTowards(_planarVelocity, targetVelocity, acceleration * Time.deltaTime);

            if (_controller.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -1f;
            }

            _verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = _planarVelocity + Vector3.up * _verticalVelocity;

            _controller.Move(movement * Time.deltaTime);

            RotateTowardMovement();
        }

        private Vector3 GetCameraRelativeDirection(Vector2 input)
        {
            if (cameraReference == null)
            {
                return new Vector3(input.x, 0f, input.y);
            }

            Vector3 forward = cameraReference.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cameraReference.right;
            right.y = 0f;
            right.Normalize();

            Vector3 worldDirection = right * input.x + forward * input.y;
            if (worldDirection.sqrMagnitude > 1f)
            {
                worldDirection.Normalize();
            }

            return worldDirection;
        }

        private void RotateTowardMovement()
        {
            Vector3 planar = _planarVelocity;
            planar.y = 0f;

            if (planar.sqrMagnitude < 0.01f)
            {
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(planar.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }
    }
}
