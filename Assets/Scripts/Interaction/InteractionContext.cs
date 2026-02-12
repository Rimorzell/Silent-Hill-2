using UnityEngine;

namespace SilentHill2Like.Interaction
{
    public readonly struct InteractionContext
    {
        public GameObject Instigator { get; }
        public Camera ViewCamera { get; }

        public InteractionContext(GameObject instigator, Camera viewCamera)
        {
            Instigator = instigator;
            ViewCamera = viewCamera;
        }
    }
}
