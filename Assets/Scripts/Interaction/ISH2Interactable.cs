using UnityEngine;

namespace SilentHill2Prototype.Interaction
{
    public interface ISH2Interactable
    {
        bool CanInteract(Transform interactor);
        string GetPrompt();
        void Interact(Transform interactor);
        void OnFocusChanged(bool focused);
    }
}
