using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SilentHill2Prototype.Core
{
    /// <summary>
    /// Creates and owns the default gameplay actions in code so scene setup stays minimal.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public sealed class SH2InputReader : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private bool invertY;
        [SerializeField] private float lookSensitivity = 1f;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool IsRunning => _runAction.IsPressed();

        public event Action InteractPressed;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _interactAction;
        private InputAction _runAction;

        private void Awake()
        {
            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddBinding("<Gamepad>/leftStick");

            _lookAction = new InputAction("Look", InputActionType.Value);
            _lookAction.AddBinding("<Mouse>/delta");
            _lookAction.AddBinding("<Gamepad>/rightStick");

            _interactAction = new InputAction("Interact", InputActionType.Button);
            _interactAction.AddBinding("<Keyboard>/e");
            _interactAction.AddBinding("<Keyboard>/f");
            _interactAction.AddBinding("<Gamepad>/buttonSouth");

            _runAction = new InputAction("Run", InputActionType.Button);
            _runAction.AddBinding("<Keyboard>/leftShift");
            _runAction.AddBinding("<Gamepad>/leftStickPress");

            _interactAction.performed += OnInteractPerformed;
        }

        private void OnEnable()
        {
            _moveAction.Enable();
            _lookAction.Enable();
            _interactAction.Enable();
            _runAction.Enable();
        }

        private void OnDisable()
        {
            _moveAction.Disable();
            _lookAction.Disable();
            _interactAction.Disable();
            _runAction.Disable();
        }

        private void Update()
        {
            Move = _moveAction.ReadValue<Vector2>();
            Look = _lookAction.ReadValue<Vector2>() * lookSensitivity;

            if (invertY)
            {
                Look = new Vector2(Look.x, -Look.y);
            }
        }

        private void OnDestroy()
        {
            _interactAction.performed -= OnInteractPerformed;

            _moveAction.Dispose();
            _lookAction.Dispose();
            _interactAction.Dispose();
            _runAction.Dispose();
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (context.ReadValueAsButton())
            {
                InteractPressed?.Invoke();
            }
        }
    }
}
