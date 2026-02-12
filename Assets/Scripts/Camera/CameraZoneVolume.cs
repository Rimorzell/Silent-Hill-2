using UnityEngine;

namespace SilentHill2Like.CameraSystem
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class CameraZoneVolume : MonoBehaviour
    {
        [SerializeField] private Transform cameraPose;
        [SerializeField, Min(0f)] private float blendTime = 0.5f;
        [SerializeField] private int priority;

        public Transform CameraPose => cameraPose != null ? cameraPose : transform;
        public float BlendTime => blendTime;
        public int Priority => priority;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }
    }
}
