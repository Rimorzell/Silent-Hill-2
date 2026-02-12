namespace SilentHill2.Interaction
{
    using UnityEngine;

    /// <summary>
    /// Immutable payload passed to <see cref="IInteractable"/> methods so the
    /// interactable knows who triggered the interaction and from which viewpoint.
    /// </summary>
    public readonly struct InteractionContext
    {
        public readonly GameObject Instigator;
        public readonly Camera ViewCamera;

        public InteractionContext(GameObject instigator, Camera viewCamera)
        {
            Instigator = instigator;
            ViewCamera = viewCamera;
        }
    }
}
