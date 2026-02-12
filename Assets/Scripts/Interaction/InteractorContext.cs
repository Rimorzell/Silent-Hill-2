using UnityEngine;

namespace SilentHillStyle.Interaction
{
    public readonly struct InteractorContext
    {
        public Transform Actor { get; }

        public InteractorContext(Transform actor)
        {
            Actor = actor;
        }
    }
}
