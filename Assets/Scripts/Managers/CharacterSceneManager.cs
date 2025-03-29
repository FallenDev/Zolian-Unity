using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
    /// <summary>
    /// Manages the loading and unloading of character-related scenes
    /// </summary>
    public class CharacterSceneManager : MonoBehaviour
    {
        public static CharacterSceneManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Loads Scene for visually seeing character (Character Creation, Selection, Inventory, etc)
        /// </summary>
        protected virtual bool LoadCharacterScene(string characterSceneName, bool isSceneLoaded)
        {
            if (isSceneLoaded) return false;
            SceneManager.LoadScene(characterSceneName, LoadSceneMode.Additive);
            return true;
        }

        /// <summary>
        /// Unloads CharacterCreationDisplay Scene during character selection
        /// </summary>
        protected virtual bool UnloadCharacterScene(string characterSceneName, bool isSceneLoaded)
        {
            if (!isSceneLoaded) return true;
            SceneManager.UnloadSceneAsync(characterSceneName);
            return false;
        }
    }
}
