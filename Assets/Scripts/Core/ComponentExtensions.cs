namespace SilentHill2.Core
{
    using UnityEngine;

    /// <summary>
    /// Utility extensions for Unity component lookups.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Attempts to find a component of type <typeparamref name="T"/> on this
        /// component's GameObject or any of its parents.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Component self, out T result)
        {
            result = self.GetComponentInParent<T>();
            return result != null;
        }

        /// <summary>
        /// Collider-specific overload so callers can invoke directly on a Collider
        /// reference without an explicit cast.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Collider self, out T result)
        {
            result = self.GetComponentInParent<T>();
            return result != null;
        }
    }
}
