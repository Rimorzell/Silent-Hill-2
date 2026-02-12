using UnityEngine;

namespace SilentHill2Like.Interaction
{
    public class InteractionPromptDebug : MonoBehaviour
    {
        [SerializeField] private InteractionSensor sensor;

        private void Awake()
        {
            if (sensor == null)
            {
                sensor = GetComponent<InteractionSensor>();
            }
        }

        private void OnGUI()
        {
            if (sensor == null || sensor.CurrentTarget == null)
            {
                return;
            }

            var label = sensor.CurrentTarget.InteractionLabel;
            GUI.Label(new Rect(20, 20, 500, 30), $"[E] {label}");
        }
    }
}
