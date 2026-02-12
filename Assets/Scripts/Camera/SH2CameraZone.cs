using UnityEngine;

namespace SilentHill2Prototype.Camera
{
    [DisallowMultipleComponent]
    public sealed class SH2CameraZone : MonoBehaviour
    {
        [SerializeField] private Transform virtualCameraPoint;
        [SerializeField] private float fieldOfView = 52f;
        [SerializeField] private int priority;
        [SerializeField] private float blendTime = 0.55f;

        public Transform VirtualCameraPoint => virtualCameraPoint != null ? virtualCameraPoint : transform;
        public float FieldOfView => fieldOfView;
        public int Priority => priority;
        public float BlendTime => Mathf.Max(0.01f, blendTime);

        private void Reset()
        {
            if (TryGetComponent<Collider>(out Collider col))
            {
                col.isTrigger = true;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            Gizmos.DrawSphere(VirtualCameraPoint.position, 0.2f);
            Gizmos.DrawLine(VirtualCameraPoint.position, VirtualCameraPoint.position + VirtualCameraPoint.forward * 1.3f);
        }
#endif
    }
}
