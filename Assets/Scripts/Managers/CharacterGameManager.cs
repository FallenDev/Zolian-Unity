using System;
using System.Collections.Concurrent;

using Assets.Scripts.CharacterSelection;
using Assets.Scripts.Entity;
using Assets.Scripts.Entity.Entities;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

using JohnStairs.RCC.Character.Cam;
using JohnStairs.RCC.Character.Motor;

using UnityEngine;
using UnityEngine.InputSystem;
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
        public Player LocalPlayer { get; private set; }

        // Cache for entities nearby
        public ConcurrentDictionary<Guid, Player> CachedPlayers { get; set; } = new();
        public ConcurrentDictionary<Guid, Movable> CachedItems { get; set; } = new();
        public ConcurrentDictionary<Guid, Movable> CachedNpcs { get; set; } = new();
        public ConcurrentDictionary<Guid, Damageable> CachedMobs { get; set; } = new();


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

        public Player SpawnPlayerPrefab(CharacterDataArgs args)
        {
            if (LocalPlayer != null)
            {
                Debug.LogWarning("Local player already exists.");
                return LocalPlayer;
            }

            var prefab = CharacterPrefabLoader.GetPrefabForLogin(args.Sex);
            var playerGO = Instantiate(prefab, args.Position, Quaternion.identity);

            var player = playerGO.GetComponent<Player>();
            player.InitializeLocalPlayerFromData(args);

            LocalPlayer = player;
            Serial = args.Serial;
            CachedPlayers[args.Serial] = player;

            return player;
        }

        public Player SpawnOtherPlayerPrefab(EntitySpawnArgs args)
        {
            var prefab = CharacterPrefabLoader.GetPrefabForLogin(args.Sex);
            if (prefab == null)
            {
                Debug.LogError($"No prefab found for gender: {args.Sex}");
                return null;
            }

            // Spawn the remote player at the correct position and rotation
            var spawnedRemotePlayer = Instantiate(
                prefab,
                args.Position,
                Quaternion.Euler(0, args.CameraYaw, 0)
            );

            // Ensure the tag is removed or changed to prevent camera from attaching
            spawnedRemotePlayer.tag = "Untagged";

            var playerScript = spawnedRemotePlayer.GetComponent<Player>();
            if (playerScript == null)
            {
                Debug.LogError("Spawned remote player prefab is missing the Player component!");
                Destroy(spawnedRemotePlayer); // Cleanup broken instance
                return null;
            }

            playerScript.InitializeFromSpawnData(args);
            Debug.Log($"Spawned remote player '{args.UserName}' at position {args.Position}");
            return playerScript;
        }
    }
}
