using System;
using System.Collections.Generic;
using UnityEngine;
using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using FishNet.Connection;
using Assets.Scripts.GameEntities.Entities;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Managers;

namespace Assets.Scripts.Network.FishNetPro
{
    public class ZolianWorldServer : MonoBehaviour
    {
        [Header("Server Configuration")]
        public int maxConnections = 100;
        public ushort port = 7777;

        [Header("Physics Settings")]
        public float tickRate = 60f;
        public bool enablePhysics = true;

        private NetworkManager _networkManager;
        private WorldClient _worldClient;
        private readonly Dictionary<Guid, Player> _serverPlayers = new();
        private readonly Dictionary<int, Guid> _connectionToSerial = new();
        private readonly Dictionary<Guid, NetworkConnection> _serialToConnection = new();

        public static ZolianWorldServer Instance { get; private set; }

        public bool IsServerRunning => _networkManager != null && _networkManager.ServerManager.Started;
        public Dictionary<Guid, Player> ServerPlayers => _serverPlayers;

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

            InitializeServer();
        }

        private void InitializeServer()
        {
            _networkManager = FindFirstObjectByType<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("No NetworkManager found in scene. Please add one to use FishNet World Server.");
                return;
            }

            // Subscribe to FishNet events
            _networkManager.ServerManager.OnServerConnectionState += OnServerConnectionState;
            _networkManager.ServerManager.OnRemoteConnectionState += OnRemoteConnectionState;

            // Configure physics if enabled
            if (enablePhysics)
            {
                // Note: In FishNet, TickRate is often configured in the NetworkManager's inspector
                // or may be read-only at runtime. We'll set up physics simulation instead.
                Physics.simulationMode = SimulationMode.Script;

                // Log the current tick rate for reference
                Debug.Log($"FishNet TimeManager TickRate: {_networkManager.TimeManager.TickRate}");
            }
        }

        private void Start()
        {
            // Initialize World Client for hybrid server mode
            _worldClient = WorldClient.Instance;

            // Start server automatically if this is a server build
            if (IsServerBuild())
            {
                StartServer();
            }
        }

        public void StartServer()
        {
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager not found. Cannot start server.");
                return;
            }

            try
            {
                _networkManager.ServerManager.StartConnection(port);
                Debug.Log($"Zolian World Server starting on port {port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start server: {e.Message}");
            }
        }

        public void StopServer()
        {
            if (_networkManager != null && _networkManager.ServerManager.Started)
            {
                _networkManager.ServerManager.StopConnection(true);
                Debug.Log("Zolian World Server stopped");
            }
        }

        private void OnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Started)
            {
                Debug.Log("Zolian World Server started successfully");
                OnServerStarted();
            }
            else if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                Debug.Log("Zolian World Server stopped");
                OnServerStopped();
            }
        }

        private void OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Started)
            {
                Debug.Log($"Client connected: {conn.ClientId}");
                OnClientConnected(conn);
            }
            else if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                Debug.Log($"Client disconnected: {conn.ClientId}");
                OnClientDisconnected(conn);
            }
        }

        private void OnServerStarted()
        {
            // Enable physics simulation for server
            if (enablePhysics)
            {
                Physics.autoSimulation = false;
            }
        }

        private void OnServerStopped()
        {
            // Clear all server data
            _serverPlayers.Clear();
            _connectionToSerial.Clear();

            // Reset physics simulation
            if (enablePhysics)
            {
                Physics.autoSimulation = true;
                Physics.simulationMode = SimulationMode.FixedUpdate;
            }
        }

        /// <summary>Used by ZolianNetworkManager to find the FishNet connection for a Serial.</summary>
        public bool TryGetConnection(Guid serial, out NetworkConnection connection)
        {
            return _serialToConnection.TryGetValue(serial, out connection);
        }

        private void OnClientConnected(NetworkConnection connection)
        {
            // Handle new client connection
            // We'll wait for the client to send their authentication info
        }

        private void OnClientDisconnected(NetworkConnection connection)
        {
            // Handle client disconnection
            if (_connectionToSerial.TryGetValue(connection.ClientId, out var serial))
            {
                _serialToConnection.Remove(serial);

                if (_serverPlayers.TryGetValue(serial, out var player))
                {
                    // Despawn the player
                    if (player != null && player.gameObject != null)
                    {
                        _networkManager.ServerManager.Despawn(player.gameObject);
                    }

                    _serverPlayers.Remove(serial);
                }

                _connectionToSerial.Remove(connection.ClientId);
            }
        }

        public void HandleClientEnterGame(NetworkConnection connection, EnterGameArgs args)
        {
            if (_connectionToSerial.ContainsKey(connection.ClientId))
            {
                Debug.LogWarning($"Client {connection.ClientId} already entered game");
                return;
            }

            try
            {
                // Store connection mapping
                _connectionToSerial[connection.ClientId] = args.Serial;
                _serialToConnection[args.Serial] = connection;

                // Spawn player for this client
                SpawnPlayerForClient(connection, args);

                Debug.Log($"Client {connection.ClientId} entered game with serial {args.Serial}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to handle client enter game: {e.Message}");
            }
        }

        private void SpawnPlayerForClient(NetworkConnection connection, EnterGameArgs args)
        {
            // Get or create player prefab
            var playerPrefab = GetPlayerPrefab();
            if (playerPrefab == null)
            {
                Debug.LogError("Player prefab not found");
                return;
            }

            // Spawn player object
            var playerObject = Instantiate(playerPrefab);
            var player = playerObject.GetComponent<Player>();

            if (player == null)
            {
                Debug.LogError("Player component not found on prefab");
                Destroy(playerObject);
                return;
            }

            // Initialize player data (this would normally come from your database)
            // For now, we'll use placeholder data
            InitializePlayerData(player, args);

            // Spawn the network object for the specific client
            _networkManager.ServerManager.Spawn(playerObject, connection);

            // Store in server players collection
            _serverPlayers[args.Serial] = player;

            // Notify other clients about this player
            NotifyOtherPlayersAboutNewPlayer(player, connection);
        }

        private GameObject GetPlayerPrefab()
        {
            // Try to find the player prefab
            var prefab = Resources.Load<GameObject>("Player");
            if (prefab == null)
            {
                prefab = GameObject.FindGameObjectWithTag("Player");
            }

            return prefab;
        }

        private void InitializePlayerData(Player player, EnterGameArgs args)
        {
            // Set basic properties
            player.Serial = args.Serial;
            player.UserName = args.UserName;

            // Set default position (this should come from your database)
            player.Position = Vector3.zero;
            player.transform.position = player.Position;

            // Set default stats (these should come from your database)
            player.EntityLevel = 1;
            player.CurrentHp = 100;
            player.MaxHp = 100;
            player.CurrentMp = 50;
            player.MaxMp = 50;

            // Initialize other default values
            player.CurrentStamina = 100;
            player.MaxStamina = 100;
            player.ArmorClass = 10;

            // Basic stats
            player.Str = 10;
            player.Int = 10;
            player.Wis = 10;
            player.Con = 10;
            player.Dex = 10;
            player.Luck = 10;
        }

        private void NotifyOtherPlayersAboutNewPlayer(Player newPlayer, NetworkConnection excludeConnection)
        {
            // Create spawn data for other clients
            var spawnArgs = new EntitySpawnArgs
            {
                Serial = newPlayer.Serial,
                EntityType = EntityType.Player,
                Position = newPlayer.Position,
                CameraYaw = newPlayer.CameraYaw,
                EntityLevel = newPlayer.EntityLevel,
                CurrentHealth = newPlayer.CurrentHp,
                MaxHealth = newPlayer.MaxHp,
                CurrentMana = newPlayer.CurrentMp,
                MaxMana = newPlayer.MaxMp,
                UserName = newPlayer.UserName,
                Race = newPlayer.Race,
                Sex = newPlayer.Gender
            };

            // Send to all other connected clients except the one who just joined
            foreach (var connection in _networkManager.ServerManager.Clients.Values)
            {
                if (connection != excludeConnection)
                {
                    // Send spawn notification to client
                    // You would implement this using your existing packet system
                    // or FishNet RPCs
                }
            }
        }

        public void HandleClientMovement(NetworkConnection connection, MovementInputArgs args)
        {
            if (!_connectionToSerial.TryGetValue(connection.ClientId, out var serial))
            {
                Debug.LogWarning($"Received movement from unregistered client {connection.ClientId}");
                return;
            }

            if (!_serverPlayers.TryGetValue(serial, out var player))
            {
                Debug.LogWarning($"Player not found for serial {serial}");
                return;
            }

            // Validate and process movement on server
            ProcessPlayerMovement(player, args);

            // Broadcast movement to other clients
            BroadcastPlayerMovement(player, connection);
        }

        private void ProcessPlayerMovement(Player player, MovementInputArgs args)
        {
            // Server-side movement validation and processing
            var newPosition = args.Position;

            // Validate movement (anti-cheat checks)
            if (IsValidMovement(player.Position, newPosition, args.Speed))
            {
                player.Position = newPosition;
                player.transform.position = newPosition;
                player.CameraYaw = args.CameraYaw;
                player.Speed = args.Speed;
                player.InputDirection = args.InputDirection;
            }
            else
            {
                Debug.LogWarning($"Invalid movement detected for player {player.Serial}");
                // Optionally send correction to client
            }
        }

        private bool IsValidMovement(Vector3 oldPos, Vector3 newPos, float speed)
        {
            // Basic movement validation
            var distance = Vector3.Distance(oldPos, newPos);
            var maxDistance = speed * Time.fixedDeltaTime * 2f; // Allow some tolerance

            return distance <= maxDistance;
        }

        private void BroadcastPlayerMovement(Player player, NetworkConnection excludeConnection)
        {
            var movementArgs = new EntityMovementArgs
            {
                Serial = player.Serial,
                EntityType = EntityType.Player,
                Position = player.Position,
                CameraYaw = player.CameraYaw,
                Speed = player.Speed,
                InputDirection = player.InputDirection
            };

            // Broadcast to all clients except the sender
            foreach (var connection in _networkManager.ServerManager.Clients.Values)
            {
                if (connection != excludeConnection)
                {
                    // Send movement update to client
                    // Implement using your packet system or FishNet RPCs
                }
            }
        }

        private void FixedUpdate()
        {
            // Handle physics simulation on server
            if (enablePhysics && IsServerRunning)
            {
                Physics.Simulate(Time.fixedDeltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
            {
                _networkManager.ServerManager.OnServerConnectionState -= OnServerConnectionState;
                _networkManager.ServerManager.OnRemoteConnectionState -= OnRemoteConnectionState;
            }
        }

        private bool IsServerBuild()
        {
            // Check if this is a server build (you can implement this based on your build configuration)
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null ||
                   Application.isBatchMode ||
                   CommandLineHelper.HasArgument("-server");
        }
    }

    public static class CommandLineHelper
    {
        public static bool HasArgument(string argument)
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals(argument, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}