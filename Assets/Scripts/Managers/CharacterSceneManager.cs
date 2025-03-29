using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Entity;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
    public class CharacterSceneManager : MonoBehaviour
    {
        protected const string CharacterSceneName = "CharacterCreationDisplay";
        protected const string SelectionSceneName = "CharacterSelectionDisplay";
        public bool IsCreationSceneLoaded;
        public bool IsSelectionSceneLoaded;

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
            if (!isSceneLoaded)
            {
                SceneManager.LoadScene(characterSceneName, LoadSceneMode.Additive);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Unloads CharacterCreationDisplay Scene during character selection
        /// </summary>
        protected virtual bool UnloadCharacterScene(string characterSceneName, bool isSceneLoaded)
        {
            if (isSceneLoaded)
            {
                SceneManager.UnloadSceneAsync(characterSceneName);
                return false;
            }

            return true;
        }
    }
}
