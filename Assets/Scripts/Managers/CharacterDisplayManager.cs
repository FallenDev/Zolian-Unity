using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class CharacterDisplayManager : MonoBehaviour
    {
        public Transform characterSpawnPoint; // Empty GameObject in the scene
        [Header("This remains empty and is dynamically filled")]
        private GameObject currentCharacter; // Active character prefab

        public static CharacterDisplayManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        // Call this method when Race/Sex selection changes
        public GameObject LoadCharacter(GameObject characterPrefab)
        {
            // Destroy previous character
            if (currentCharacter != null)
                Destroy(currentCharacter);

            if (characterPrefab == null)
            {
                Debug.LogError("Character prefab is NULL! Check if the correct race/sex prefab exists.");
                return null;
            }

            // Instantiate new character at the spawn point
            currentCharacter = Instantiate(characterPrefab, characterSpawnPoint.position, Quaternion.identity);
            currentCharacter.transform.SetParent(characterSpawnPoint);

            // Adjust scale
            currentCharacter.transform.localScale = Vector3.one * 4f;

            Debug.Log($"Spawned character prefab: {characterPrefab.name}");

            var animator = currentCharacter.GetComponent<Animator>();
            if (animator != null)
                animator.Play("Idle");
            else
                Debug.LogWarning("Animator not found on character prefab.");

            return currentCharacter;
        }
    }
}