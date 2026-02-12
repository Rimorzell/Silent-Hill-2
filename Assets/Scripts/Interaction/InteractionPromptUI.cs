namespace SilentHill2.Interaction
{
    using UnityEngine;

    /// <summary>
    /// Minimal debug-grade UI that displays an interaction prompt when the
    /// <see cref="InteractionSensor"/> has a valid target.
    /// <para>Attach to the player GameObject alongside <see cref="InteractionSensor"/>.</para>
    /// <para>
    /// This uses <c>OnGUI</c> for zero-dependency simplicity. Replace with a
    /// proper UI Toolkit or uGUI implementation for production use.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class InteractionPromptUI : MonoBehaviour
    {
        [Tooltip("Interaction sensor to observe. Auto-discovered on the same " +
                 "GameObject if not assigned.")]
        [SerializeField] private InteractionSensor sensor;

        [Tooltip("Font size for the prompt text.")]
        [SerializeField] private int fontSize = 22;

        [Tooltip("Main text colour.")]
        [SerializeField] private Color textColor = Color.white;

        [Tooltip("Drop-shadow colour for readability against any background.")]
        [SerializeField] private Color shadowColor = new(0f, 0f, 0f, 0.8f);

        private GUIStyle _style;
        private GUIStyle _shadowStyle;

        private void Awake()
        {
            if (sensor == null)
            {
                sensor = GetComponent<InteractionSensor>();
            }
        }

        private void OnGUI()
        {
            if (sensor == null || sensor.CurrentTarget == null) return;

            EnsureStyles();

            string text = $"[E] {sensor.CurrentTarget.InteractionLabel}";

            float width = 300f;
            float height = 40f;
            float x = (Screen.width - width) * 0.5f;
            float y = Screen.height * 0.75f;

            // Drop shadow (offset 2 pixels right and down).
            var shadowRect = new Rect(x + 2f, y + 2f, width, height);
            GUI.Label(shadowRect, text, _shadowStyle);

            // Main text.
            var rect = new Rect(x, y, width, height);
            GUI.Label(rect, text, _style);
        }

        private void EnsureStyles()
        {
            if (_style != null) return;

            _style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };
            _style.normal.textColor = textColor;

            _shadowStyle = new GUIStyle(_style);
            _shadowStyle.normal.textColor = shadowColor;
        }
    }
}
