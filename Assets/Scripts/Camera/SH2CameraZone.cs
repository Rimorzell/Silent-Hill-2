using System.Collections.Generic;
using UnityEngine;

namespace SilentHillStyle.CameraSystem
{
    /// <summary>
    /// Trigger volume that activates one or more anchors.
    /// Highest priority anchor wins.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class SH2CameraZone : MonoBehaviour
    {
        [SerializeField] private List<SH2CameraAnchor> anchors = new();
        [SerializeField] private bool includeAnchorsInChildren = true;

        public IReadOnlyList<SH2CameraAnchor> Anchors => anchors;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Awake()
        {
            if (includeAnchorsInChildren)
            {
                var found = GetComponentsInChildren<SH2CameraAnchor>(true);
                foreach (var anchor in found)
                {
                    if (anchor != null && !anchors.Contains(anchor))
                    {
                        anchors.Add(anchor);
                    }
                }
            }
        }

        public SH2CameraAnchor GetBestAnchor()
        {
            SH2CameraAnchor best = null;
            var bestPriority = int.MinValue;

            foreach (var anchor in anchors)
            {
                if (anchor == null || !anchor.isActiveAndEnabled)
                {
                    continue;
                }

                if (anchor.Priority > bestPriority)
                {
                    bestPriority = anchor.Priority;
                    best = anchor;
                }
            }

            return best;
        }
    }
}
