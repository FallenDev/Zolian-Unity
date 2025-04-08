using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.CharacterSelection
{
    public static class CharacterPrefabLoader
    {
        public static GameObject GetPrefabForCreation(Sex gender)
        {
            var prefabPath = $"Characters/CC_{gender}";
            var prefab = Resources.Load<GameObject>(prefabPath);
            return prefab != null ? prefab : Resources.Load<GameObject>("Characters/CC_Male"); // Default fallback
        }

        public static GameObject GetPrefabForSelection(Sex gender)
        {
            var prefabPath = $"Characters/CS_{gender}";
            var prefab = Resources.Load<GameObject>(prefabPath);
            return prefab != null ? prefab : Resources.Load<GameObject>("Characters/CS_Male"); // Default fallback
        }

        public static GameObject GetPrefabForLogin(Sex gender)
        {
            var prefabPath = $"Characters/{gender}";
            var prefab = Resources.Load<GameObject>(prefabPath);
            return prefab != null ? prefab : Resources.Load<GameObject>("Characters/Male"); // Default fallback
        }
    }
}