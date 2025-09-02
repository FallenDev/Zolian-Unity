using System;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Connection;
using FishNet.Object;
using Assets.Scripts.GameEntities.Entities;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Managers;
using Assets.Scripts.Network.FishNetPro.Auth;

namespace Assets.Scripts.Network.FishNetPro
{
    public class ZolianWorldClient : MonoBehaviour
    {
        [Header("Client Configuration")]
        public string serverAddress = "localhost";
        public ushort serverPort = 7777;
        public bool autoConnect = false;

        [Header("Debug Settings")]
        public bool enableDebugLogs = true;

        private NetworkManager _networkManager;
        private WorldClient _legacyWorldClient;
        private readonly Dictionary<Guid, Player> _networkedPlayers = new();

        public static ZolianWorldClient Instance { get; private set; }

        public bool IsClientConnected => _networkManager != null && _networkManager.ClientManager.Started;
        public Dictionary<Guid, Player> NetworkedPlayers => _networkedPlayers;

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
                return;
            }

            InitializeClient();
        }

        private void InitializeClient()
        {
            _networkManager = FindFirstObjectByType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("No NetworkManager found in scene. Please add one to use FishNet World Client.");
                return;
            }

            // Subscribe to FishNet events
            _networkManager.ClientManager.OnClientConnectionState += OnClientConnectionState;

            DebugLog("ZolianWorldClient initialized");
        }

        private void Start()
        {
            // Get reference to legacy WorldClient for bridging
            _legacyWorldClient = WorldClient.Instance;

            if (autoConnect)
            {
                ConnectToServer();
            }
        }

        #region Connection Management

        public void ConnectToServer()
        {
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found. Cannot connect to server.");
                return;
            }

            if (IsClientConnected)
            {
                DebugLog("Already connected to server");
                return;
            }

            try
            {
                _networkManager.ClientManager.StartConnection(serverAddress, serverPort);
                DebugLog($"Attempting to connect to Zolian World Server at {serverAddress}:{serverPort}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to connect to server: {e.Message}");
            }
        }

        private void SendAuthTicket()
        {
            // ToDo: Implement actual authentication ticket sending logic
            var ticket = PlayerPrefs.GetString("Zolian.WorldTicket", "");
            var steamId = PlayerPrefs.HasKey("Zolian.SteamId") ? PlayerPrefs.GetString("ZolianSteamId") : "123456";
            long.TryParse(steamId, out var steamIdLong);
            var characterManager = CharacterGameManager.Instance;
            var characterGuid = characterManager != null ? characterManager.Serial : System.Guid.Empty;

            var msg = new Assets.Scripts.Network.FishNetPro.Auth.AuthTicketBroadcast
            {
                Ticket = ticket,
                SteamId = steamIdLong,
                AccountId = System.Guid.Empty,
                CharacterId = characterGuid
            };

            DebugLog("Sending AuthTicketBroadcast.");
            InstanceFinder.NetworkManager.ClientManager.Broadcast(msg);
        }

        public void DisconnectFromServer()
        {
            if (_networkManager != null && _networkManager.ClientManager.Started)
            {
                _networkManager.ClientManager.StopConnection();
                DebugLog("Disconnecting from Zolian World Server");
            }
        }

        #endregion

        #region FishNet Event Handlers

        private void OnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                DebugLog("Successfully connected to Zolian World Server");
                OnConnectedToServer();
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                DebugLog("Disconnected from Zolian World Server");
                OnDisconnectedFromServer();
            }
            else if (args.ConnectionState == LocalConnectionState.Starting)
            {
                DebugLog("Connecting to Zolian World Server...");
            }
            else if (args.ConnectionState == LocalConnectionState.Stopping)
            {
                DebugLog("Disconnecting from Zolian World Server...");
            }
        }

        #endregion

        #region Connection Event Handlers

        private void OnConnectedToServer()
        {
            // Send AuthTicket immediately after starting (pre-spawn)
            SendAuthTicket();

            // After server PassAuthentication, send EnterGameRequest
            SendEnterGameRequest();

            // Notify ZolianNetworkManager
            var networkManager = ZolianNetworkManager.Instance;
            if (networkManager != null)
            {
                // Trigger connected event
                DebugLog("Notifying ZolianNetworkManager of world connection");
            }
        }

        private void OnDisconnectedFromServer()
        {
            // Clear all networked players
            _networkedPlayers.Clear();

            // Clean up CharacterGameManager references
            var characterManager = CharacterGameManager.Instance;
            if (characterManager != null)
            {
                // Clear networked player references
                characterManager.LocalPlayer = null;
                // Note: We don't clear CachedPlayers as they might include non-networked players
            }

            DebugLog("Cleaned up after server disconnection");
        }

        #endregion

        private void SendEnterGameRequest()
        {
            var characterManager = CharacterGameManager.Instance;
            if (characterManager != null)
            {
                var req = new Assets.Scripts.Network.FishNetPro.Auth.EnterGameRequest
                {
                    Serial = characterManager.Serial,
                    UserName = characterManager.UserName
                };

                DebugLog($"Sending EnterGame for {req.UserName} ({req.Serial})");
                InstanceFinder.NetworkManager.ClientManager.Broadcast(req);
            }
            else
            {
                Debug.LogWarning("CharacterGameManager not found, cannot send enter game request");
            }
        }

        public void HandleServerPlayerSpawn(EntitySpawnArgs spawnArgs)
        {
            // Handle legacy packet system player spawn
            DebugLog($"Legacy player spawn received: {spawnArgs.UserName} at {spawnArgs.Position}");

            // In the FishNet system, this is handled automatically by OnNetworkObjectSpawned
            // This method exists for compatibility with the legacy packet bridge
        }

        public void HandleServerMovementUpdate(EntityMovementArgs movementArgs)
        {
            // Handle legacy packet system movement updates
            if (_networkedPlayers.TryGetValue(movementArgs.Serial, out var player))
            {
                if (player != null && !player.IsLocalPlayer)
                {
                    // Update remote player position
                    // In FishNet, this is handled by SyncVars, but we can use this for fallback
                    player.Position = movementArgs.Position;
                    player.CameraYaw = movementArgs.CameraYaw;
                    player.Speed = movementArgs.Speed;
                    player.InputDirection = movementArgs.InputDirection;

                    DebugLog($"Updated remote player position via legacy packet: {player.UserName}");
                }
            }
        }

        public void SendMovementToServer(MovementInputArgs movementArgs)
        {
            // Bridge method for legacy movement system
            // In the new FishNet system, movement is handled by FishNetMovementController
            DebugLog($"Legacy movement packet received: {movementArgs.Position}");

            // This can be used as a fallback or for additional validation
        }

        #region Utility Methods

        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[ZolianWorldClient] {message}");
            }
        }

        public Player GetNetworkedPlayer(Guid serial)
        {
            return _networkedPlayers.TryGetValue(serial, out var player) ? player : null;
        }

        public Player GetLocalPlayer()
        {
            foreach (var player in _networkedPlayers.Values)
            {
                if (player.IsLocalPlayer)
                    return player;
            }
            return null;
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.ClientManager.OnClientConnectionState -= OnClientConnectionState;
            }

            DebugLog("ZolianWorldClient destroyed and cleaned up");
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Force Connect")]
        public void DebugForceConnect()
        {
            ConnectToServer();
        }

        [ContextMenu("Force Disconnect")]
        public void DebugForceDisconnect()
        {
            DisconnectFromServer();
        }

        [ContextMenu("List Networked Players")]
        public void DebugListNetworkedPlayers()
        {
            Debug.Log($"Networked Players ({_networkedPlayers.Count}):");
            foreach (var kvp in _networkedPlayers)
            {
                var player = kvp.Value;
                Debug.Log($"  - {player.UserName} (Serial: {kvp.Key}, Local: {player.IsLocalPlayer})");
            }
        }

        #endregion
    }
}