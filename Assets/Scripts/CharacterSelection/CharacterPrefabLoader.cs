using Assets.Scripts.Models;
using UnityEngine;

namespace Assets.Scripts.CharacterSelection
{
    public static class CharacterPrefabLoader
    {
        public static GameObject GetPrefab(Race race, Sex gender)
        {
            var prefabPath = $"Characters/{race}_{gender}";
            var prefab = Resources.Load<GameObject>(prefabPath);

            if (prefab != null) return prefab;
            Debug.LogWarning($"Character prefab not found: {prefabPath}. Using default Human_Male.");
            return Resources.Load<GameObject>("Characters/Human_Male"); // Default fallback
        }
    }
}