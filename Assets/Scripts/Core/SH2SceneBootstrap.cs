using SilentHillStyle.CameraSystem;
using SilentHillStyle.Interaction;
using SilentHillStyle.Player;
using UnityEngine;

namespace SilentHillStyle.Core
{
    /// <summary>
    /// One-click setup helper that wires references at runtime.
    /// Safe to keep in scene during prototyping.
    /// </summary>
    public sealed class SH2SceneBootstrap : MonoBehaviour
    {
        [SerializeField] private GameObject playerObject;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private bool assignPlayerTag = true;

        private void Awake()
        {
            if (playerObject == null)
            {
                playerObject = GameObject.FindGameObjectWithTag("Player");
            }

            if (playerObject == null)
            {
                var foundController = FindFirstObjectByType<SH2PlayerController>();
                if (foundController != null)
                {
                    playerObject = foundController.gameObject;
                }
            }

            if (mainCamera == null)
            {
                mainCamera = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
            }

            if (playerObject == null || mainCamera == null)
            {
                Debug.LogWarning("SH2SceneBootstrap: Missing player or camera reference.");
                return;
            }

            if (assignPlayerTag)
            {
                playerObject.tag = "Player";
            }

            if (!playerObject.TryGetComponent<SH2PlayerController>(out var controller))
            {
                controller = playerObject.AddComponent<SH2PlayerController>();
            }

            if (!playerObject.TryGetComponent<CharacterController>(out _))
            {
                playerObject.AddComponent<CharacterController>();
            }

            if (!playerObject.TryGetComponent<SH2InteractionSensor>(out _))
            {
                playerObject.AddComponent<SH2InteractionSensor>();
            }

            if (!mainCamera.TryGetComponent<SH2CameraBrain>(out _))
            {
                mainCamera.gameObject.AddComponent<SH2CameraBrain>();
            }

            controller.enabled = true;
        }
    }
}
