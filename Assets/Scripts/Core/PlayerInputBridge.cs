using SilentHill2Like.Interaction;
using SilentHill2Like.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SilentHill2Like.Core
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputBridge : MonoBehaviour
    {
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string interactActionName = "Interact";
        [SerializeField] private PlayerMotor playerMotor;
        [SerializeField] private InteractionSensor interactionSensor;

        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _interactAction;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (playerMotor == null)
            {
                playerMotor = GetComponent<PlayerMotor>();
            }

            if (interactionSensor == null)
            {
                interactionSensor = GetComponent<InteractionSensor>();
            }
        }

        private void OnEnable()
        {
            _moveAction = _playerInput.actions[moveActionName];
            _interactAction = _playerInput.actions[interactActionName];

            if (_interactAction != null)
            {
                _interactAction.performed += OnInteractPerformed;
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
            if (playerMotor == null || _moveAction == null)
            {
                return;
            }

            playerMotor.SetMoveInput(_moveAction.ReadValue<Vector2>());
        }

        private void OnInteractPerformed(InputAction.CallbackContext _)
        {
            interactionSensor?.TryInteract();
        }
    }
}
