namespace SilentHillStyle.Interaction
{
    public interface IInteractable
    {
        string DisplayName { get; }
        bool CanInteract(InteractorContext context);
        void Interact(InteractorContext context);
    }
}
