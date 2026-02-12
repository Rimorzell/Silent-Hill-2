namespace SilentHill2.Core
{
    using SilentHill2.Interaction;
    using SilentHill2.Player;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Reads actions from a <see cref="PlayerInput"/> component and forwards them
    /// to <see cref="PlayerMotor"/> (movement) and <see cref="InteractionSensor"/>
    /// (interaction). Attach to the player GameObject.
    /// <para>
    /// Requires the Unity Input System package (<c>com.unity.inputsystem</c>) to
    /// be installed and the active input handling set to "Input System Package
    /// (New)" or "Both" in Project Settings &gt; Player.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBridge : MonoBehaviour
    {
        [Header("Action Names")]
        [Tooltip("Name of the Vector2 action for movement (WASD / left stick).")]
        [SerializeField] private string moveActionName = "Move";

        [Tooltip("Name of the Button action for interaction (E / gamepad south).")]
        [SerializeField] private string interactActionName = "Interact";

        [Header("References (auto-discovered if not assigned)")]
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private InteractionSensor interactionSensor;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _interactAction;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();

            // Auto-discover on same object, then parent hierarchy.
            if (playerMotor == null) playerMotor = GetComponent<PlayerMotor>();
            if (playerMotor == null) playerMotor = GetComponentInParent<PlayerMotor>();

            if (interactionSensor == null) interactionSensor = GetComponent<InteractionSensor>();
            if (interactionSensor == null) interactionSensor = GetComponentInParent<InteractionSensor>();

            if (playerMotor == null)
            {
                Debug.LogWarning(
                    "[PlayerInputBridge] No PlayerMotor found. " +
                    "Movement input will be ignored.", this);
            }

            if (interactionSensor == null)
            {
                Debug.LogWarning(
                    "[PlayerInputBridge] No InteractionSensor found. " +
                    "Interact input will be ignored.", this);
            }
        }

        private void OnEnable()
        {
            if (_playerInput == null || _playerInput.actions == null) return;

            _moveAction = _playerInput.actions.FindAction(moveActionName);
            _interactAction = _playerInput.actions.FindAction(interactActionName);

            if (_moveAction == null)
            {
                Debug.LogWarning(
                    $"[PlayerInputBridge] Action '{moveActionName}' not found " +
                    "in the assigned Input Actions asset.", this);
            }

            if (_interactAction != null)
            {
                _interactAction.performed += OnInteractPerformed;
            }
            else
            {
                Debug.LogWarning(
                    $"[PlayerInputBridge] Action '{interactActionName}' not found " +
                    "in the assigned Input Actions asset.", this);
            }
        }

        private void OnDisable()
        {
            if (_interactAction != null)
            {
                _interactAction.performed -= OnInteractPerformed;
            }
        }

        private void Update()
        {
            if (playerMotor == null || _moveAction == null) return;
            playerMotor.SetMoveInput(_moveAction.ReadValue<Vector2>());
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (interactionSensor != null)
            {
                interactionSensor.TryInteract();
            }
        }
    }
}
