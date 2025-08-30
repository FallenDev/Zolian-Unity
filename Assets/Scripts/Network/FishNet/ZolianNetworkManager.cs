using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Connection;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Models;
using Assets.Scripts.Managers;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Network.FishNet
{
    /// <summary>
    /// Unified network manager that bridges the existing Zolian packet system with FishNet
    /// Handles both the auth server communication and FishNet world server communication
    /// </summary>
    public class ZolianNetworkManager : MonoBehaviour
    {
        [Header("Network Configuration")]
        public bool isServerBuild = false;
        public bool autoStartServer = true;
        public bool autoConnectClient = true;
        
        [Header("Server Settings")]
        public ushort worldServerPort = 7777;
        public int maxConnections = 100;
        
        [Header("Client Settings")]
        public string worldServerAddress = "localhost";
        
        private NetworkManager _fishNetManager;
        private ZolianWorldServer _worldServer;
        private ZolianWorldClient _worldClient;
        private WorldClient _legacyWorldClient;
        
        public static ZolianNetworkManager Instance { get; private set; }
        
        public bool IsWorldServerRunning => _worldServer != null && _worldServer.IsServerRunning;
        public bool IsWorldClientConnected => _worldClient != null && _worldClient.IsClientConnected;
        
        // Events for UI and game systems
        public event Action OnWorldServerStarted;
        public event Action OnWorldServerStopped;
        public event Action OnWorldClientConnected;
        public event Action OnWorldClientDisconnected;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeNetworking();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Determine if this is a server build
            isServerBuild = IsServerBuild();
            
            if (isServerBuild && autoStartServer)
            {
                StartWorldServer();
            }
            else if (!isServerBuild && autoConnectClient)
            {
                // Client will connect after auth process completes
                SetupClientMode();
            }
        }

        private void InitializeNetworking()
        {
            _fishNetManager = GetComponent<NetworkManager>();
            if (_fishNetManager == null)
            {
                Debug.LogError("NetworkManager component not found on ZolianNetworkManager!");
                return;
            }

            // Get or create world server/client components
            _worldServer = GetComponent<ZolianWorldServer>();
            if (_worldServer == null)
            {
                _worldServer = gameObject.AddComponent<ZolianWorldServer>();
            }

            _worldClient = GetComponent<ZolianWorldClient>();
            if (_worldClient == null)
            {
                _worldClient = gameObject.AddComponent<ZolianWorldClient>();
            }

            // Get reference to legacy WorldClient for bridging
            _legacyWorldClient = FindFirstObjectByType<WorldClient>();
        }

        #region Server Methods

        public void StartWorldServer()
        {
            if (!isServerBuild)
            {
                Debug.LogWarning("Cannot start world server on client build!");
                return;
            }

            if (_worldServer != null)
            {
                _worldServer.port = worldServerPort;
                _worldServer.maxConnections = maxConnections;
                _worldServer.StartServer();
                
                Debug.Log("Zolian World Server starting...");
                OnWorldServerStarted?.Invoke();
            }
        }

        public void StopWorldServer()
        {
            if (_worldServer != null)
            {
                _worldServer.StopServer();
                OnWorldServerStopped?.Invoke();
            }
        }

        #endregion

        #region Client Methods

        private void SetupClientMode()
        {
            if (_worldClient != null)
            {
                _worldClient.serverAddress = worldServerAddress;
                _worldClient.serverPort = worldServerPort;
                _worldClient.autoConnect = false; // We'll control when to connect
            }
        }

        public void ConnectToWorldServer()
        {
            if (isServerBuild)
            {
                Debug.LogWarning("Cannot connect to world server from server build!");
                return;
            }

            if (_worldClient != null)
            {
                _worldClient.ConnectToServer();
                Debug.Log("Connecting to Zolian World Server...");
            }
        }

        public void DisconnectFromWorldServer()
        {
            if (_worldClient != null)
            {
                _worldClient.DisconnectFromServer();
                OnWorldClientDisconnected?.Invoke();
            }
        }

        #endregion

        #region Packet System Bridge

        /// <summary>
        /// Called by the legacy WorldClient when a character enters the world
        /// This bridges the old auth system with the new FishNet world system
        /// </summary>
        public void OnCharacterEnteredWorld(CharacterDataArgs characterData)
        {
            if (isServerBuild)
            {
                // On server, handle the character entering via existing auth flow
                HandleServerCharacterEntry(characterData);
            }
            else
            {
                // On client, connect to FishNet world server after auth
                ConnectToWorldServer();
            }
        }

        private void HandleServerCharacterEntry(CharacterDataArgs characterData)
        {
            // Server-side handling when a character enters the world
            // This is called after the auth server validates the character
            if (_worldServer != null && _worldServer.IsServerRunning)
            {
                // The character is already authenticated, create them in the FishNet world
                Debug.Log($"Character {characterData.UserName} entering FishNet world server");
                
                // Store character data for when the client connects to FishNet
                // This will be handled by the ZolianWorldServer when the client sends EnterGame
            }
        }

        /// <summary>
        /// Bridges movement packets from legacy system to FishNet
        /// </summary>
        public void OnMovementReceived(MovementInputArgs movementArgs)
        {
            if (isServerBuild && _worldServer != null)
            {
                // Find the connection for this player and forward the movement
                // This bridges the legacy packet system to FishNet
                var connection = GetConnectionForSerial(movementArgs.Serial);
                if (connection != null)
                {
                    _worldServer.HandleClientMovement(connection, movementArgs);
                }
            }
        }

        /// <summary>
        /// Bridges entity spawn packets from legacy system to FishNet
        /// </summary>
        public void OnEntitySpawnReceived(EntitySpawnArgs spawnArgs)
        {
            if (!isServerBuild && _worldClient != null)
            {
                // Client-side entity spawn handling
                _worldClient.HandleServerPlayerSpawn(spawnArgs);
            }
        }

        /// <summary>
        /// Bridges entity movement packets from legacy system to FishNet
        /// </summary>
        public void OnEntityMovementReceived(EntityMovementArgs movementArgs)
        {
            if (!isServerBuild && _worldClient != null)
            {
                // Client-side movement update handling
                _worldClient.HandleServerMovementUpdate(movementArgs);
            }
        }

        #endregion

        #region Utility Methods

        private NetworkConnection GetConnectionForSerial(Guid serial)
        {
            // Find the FishNet connection associated with a character serial
            // This would need to be implemented based on your connection tracking
            if (_worldServer != null && _worldServer.ServerPlayers.ContainsKey(serial))
            {
                // You'd need to track connection IDs to serials in ZolianWorldServer
                // Return the appropriate NetworkConnection
            }
            return null;
        }

        private bool IsServerBuild()
        {
            #if ZOLIAN_SERVER
            return true;
            #elif ZOLIAN_CLIENT
            return false;
            #else
            // Fallback detection
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null ||
                   Application.isBatchMode ||
                   HasCommandLineArgument("-server");
            #endif
        }

        private bool HasCommandLineArgument(string argument)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(argument, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// Handles scene transitions for the networking system
        /// </summary>
        public void OnSceneChanged(string sceneName)
        {
            if (sceneName.Contains("World") || sceneName.Contains("Game"))
            {
                // Entering the game world
                if (isServerBuild)
                {
                    // Server: Ensure world server is running
                    if (!IsWorldServerRunning)
                        StartWorldServer();
                }
                else
                {
                    // Client: Will connect after character auth completes
                    SetupClientMode();
                }
            }
            else if (sceneName.Contains("Lobby"))
            {
                // Entering lobby - use traditional auth system
                if (!isServerBuild)
                {
                    // Disconnect from world server if connected
                    if (IsWorldClientConnected)
                        DisconnectFromWorldServer();
                }
            }
        }

        #endregion

        private void OnDestroy()
        {
            OnWorldServerStarted = null;
            OnWorldServerStopped = null;
            OnWorldClientConnected = null;
            OnWorldClientDisconnected = null;
        }

        #region Debug Methods

        [ContextMenu("Force Start World Server")]
        public void DebugStartWorldServer()
        {
            isServerBuild = true;
            StartWorldServer();
        }

        [ContextMenu("Force Connect World Client")]
        public void DebugConnectWorldClient()
        {
            isServerBuild = false;
            ConnectToWorldServer();
        }

        #endregion
    }
}