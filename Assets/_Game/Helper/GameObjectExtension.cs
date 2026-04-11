using UnityEngine;

namespace Helper
{
    public static class GameObjectExtension
    {
        public static void SetLayerRecursively(this GameObject gameObject, int layer)
        {
            if (null==gameObject)
            {
                return;
            }
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                if (null==child)
                {
                    continue;
                }
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        public static T GetOrAddComponent<T>(this GameObject owner) where T : Component
        {
            var result = owner.GetComponent<T>();
            if (result == null)
                result = owner.AddComponent<T>();

            return result;
        }
        public static T GetOrAddOn<T>(this T field, Component owner) where T : Component
        {
            if (field != null)
                return field;

            var result = owner.GetComponent<T>();
            if (result == null)
                result = owner.gameObject.AddComponent<T>();

            return result;
        }

        public static T GetOrAddOn<T>(this T field, GameObject owner) where T : Component
        {
            if (field != null)
                return field;

            var result = owner.GetComponent<T>();
            if (result == null)
                result = owner.AddComponent<T>();

            return result;
        }
    }
}