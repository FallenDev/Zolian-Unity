using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.CharacterSelection
{
    public static class CharacterPrefabLoader
    {
        public static GameObject GetPrefab(Race race, Sex gender)
        {
            string prefabPath = $"Characters/{race}_{gender}";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab == null)
            {
                Debug.LogWarning($"Character prefab not found: {prefabPath}. Using default Human_Male.");
                return Resources.Load<GameObject>("Characters/Human_Male"); // Default fallback
            }

            return prefab;
        }
    }
}