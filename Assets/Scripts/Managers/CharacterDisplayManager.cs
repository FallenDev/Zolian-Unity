using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class CharacterDisplayManager : MonoBehaviour
    {
        public Transform CharacterSpawnPoint; // Empty GameObject in the scene
        [Header("This remains empty and is dynamically filled")]
        private GameObject _currentCharacter; // Active character prefab

        public static CharacterDisplayManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Call this method when Race/Sex selection changes
        /// </summary>
        public GameObject LoadCharacter(GameObject characterPrefab)
        {
            // Destroy previous character
            if (_currentCharacter != null)
                Destroy(_currentCharacter);

            if (characterPrefab == null)
            {
                Debug.LogError("Character prefab is NULL! Check if the correct race/sex prefab exists.");
                return null;
            }

            // Instantiate new character at the spawn point
            _currentCharacter = Instantiate(characterPrefab, CharacterSpawnPoint.position, Quaternion.identity);
            _currentCharacter.transform.SetParent(CharacterSpawnPoint);

            // Adjust scale
            _currentCharacter.transform.localScale = Vector3.one * 4f;

            Debug.Log($"Spawned character prefab: {characterPrefab.name}");

            var animator = _currentCharacter.GetComponent<Animator>();
            if (animator != null)
                animator.Play("Standard Movement");
            else
                Debug.LogWarning("Animator not found on character prefab.");

            return _currentCharacter;
        }
    }
}