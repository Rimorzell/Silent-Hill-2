using UnityEngine;

namespace SilentHill2Like.Interaction
{
    public interface IInteractable
    {
        Transform InteractionPoint { get; }
        string InteractionLabel { get; }
        bool IsAvailable { get; }
        bool CanInteract(InteractionContext context);
        void Interact(InteractionContext context);
    }
}
