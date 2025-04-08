using System;

using Assets.Scripts.CharacterSelection;
using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Managers
{
    /// <summary>
    /// Manages static information for the Game
    /// </summary>
    public class CharacterGameManager : MonoBehaviour
    {
        public long SteamId;
        // Character Login to World Server
        public ushort WorldPort = 4202; // ToDo: Change to a UI box to pick a world server
        public Guid Serial;
        public string UserName;
        private GameObject _spawnedPlayer;

        private static CharacterGameManager _instance;

        public static CharacterGameManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<CharacterGameManager>();
                if (_instance != null) return _instance;

                Debug.LogError("CharacterSceneManager instance not found in the scene!");
                return null;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
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

        public void SpawnPlayerPrefab(CharacterDataArgs args)
        {
            var prefab = CharacterPrefabLoader.GetPrefabForLogin(args.Sex);
            if (prefab == null)
            {
                Debug.LogError($"No prefab found for gender: {args.Sex}");
                return;
            }

            if (_spawnedPlayer != null)
                Destroy(_spawnedPlayer);

            _spawnedPlayer = Instantiate(prefab, args.Position, Quaternion.identity);

            var playerScript = _spawnedPlayer.GetComponent<Player>();
            if (playerScript == null)
            {
                Debug.LogError("Spawned prefab is missing the Player component!");
                return;
            }

            playerScript.InitializeFromData(args);           // Set stats and metadata
            Debug.Log($"Spawned player '{args.UserName}' at position {args.Position}");
        }
    }
}
