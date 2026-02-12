using UnityEngine;

namespace SilentHill2Prototype.Interaction
{
    [DisallowMultipleComponent]
    public sealed class SH2PickupItem : MonoBehaviour, ISH2Interactable
    {
        [SerializeField] private string itemName = "Collectible";
        [SerializeField] private Renderer[] highlightRenderers;
        [SerializeField] private Color highlightColor = new(1f, 0.9f, 0.4f, 1f);

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock _propertyBlock;
        private bool _picked;

        public bool CanInteract(Transform interactor)
        {
            return !_picked;
        }

        public string GetPrompt()
        {
            return _picked ? string.Empty : $"Pick up {itemName}";
        }

        public void Interact(Transform interactor)
        {
            if (_picked)
            {
                return;
            }

            _picked = true;
            OnFocusChanged(false);
            gameObject.SetActive(false);
        }

        public void OnFocusChanged(bool focused)
        {
            if (highlightRenderers == null || highlightRenderers.Length == 0)
            {
                return;
            }

            _propertyBlock ??= new MaterialPropertyBlock();

            foreach (Renderer renderer in highlightRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(EmissionColor, focused ? highlightColor : Color.black);
                renderer.SetPropertyBlock(_propertyBlock);
            }
        }

        private void Reset()
        {
            if (highlightRenderers == null || highlightRenderers.Length == 0)
            {
                highlightRenderers = GetComponentsInChildren<Renderer>();
            }
        }
    }
}
