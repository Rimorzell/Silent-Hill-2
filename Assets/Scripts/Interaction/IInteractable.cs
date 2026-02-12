namespace SilentHill2.Interaction
{
    using UnityEngine;

    /// <summary>
    /// Contract for any object the player can interact with.
    /// Implement on a MonoBehaviour and attach to the interactable GameObject
    /// (or a parent). The <see cref="InteractionSensor"/> discovers implementations
    /// via <c>GetComponentInParent</c>.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// World-space point used for distance and angle calculations.
        /// Return the MonoBehaviour's own transform if no dedicated point exists.
        /// </summary>
        Transform InteractionPoint { get; }

        /// <summary>
        /// Short label displayed in the UI prompt (e.g. "Pick up", "Examine").
        /// </summary>
        string InteractionLabel { get; }

        /// <summary>
        /// <c>false</c> once the interactable has been consumed, destroyed, or
        /// otherwise made permanently unavailable. The sensor skips unavailable targets.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Per-frame eligibility check. Allows conditional gating such as
        /// requiring a key item. Called only when <see cref="IsAvailable"/> is true.
        /// </summary>
        bool CanInteract(InteractionContext context);

        /// <summary>
        /// Execute the interaction. Called only after <see cref="CanInteract"/>
        /// returns <c>true</c>.
        /// </summary>
        void Interact(InteractionContext context);
    }
}
