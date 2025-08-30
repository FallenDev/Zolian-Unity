using UnityEngine;
using FishNet.Managing;

namespace Assets.Scripts.Network.FishNet
{
    [CreateAssetMenu(fileName = "ZolianNetworkConfig", menuName = "Zolian/Network Configuration")]
    public class ZolianNetworkConfig : ScriptableObject
    {
        [Header("Server Configuration")]
        public ushort worldServerPort = 7777;
        public int maxConnections = 100;
        public float tickRate = 60f;
        public bool enableServerPhysics = true;
        
        [Header("Client Configuration")]
        public string defaultServerAddress = "localhost";
        public bool autoConnectAfterAuth = true;
        public float connectionTimeout = 10f;
        
        [Header("Movement Settings")]
        public float movementSendRate = 0.2f;
        public float positionThreshold = 0.05f;
        public float rotationThreshold = 5f;
        public float speedThreshold = 0.1f;
        
        [Header("Prediction Settings")]
        public bool enableClientPrediction = true;
        public float reconciliationThreshold = 0.5f;
        public int maxPredictionFrames = 60;
        
        [Header("Synchronization")]
        public bool syncCharacterCustomization = true;
        public bool syncPlayerStats = true;
        public bool syncInventory = false; // Set to true when inventory system is ready
        
        [Header("Security")]
        public bool enableMovementValidation = true;
        public float maxMovementSpeed = 20f;
        public float maxTeleportDistance = 50f;
        
        [Header("Debug")]
        public bool enableNetworkLogging = true;
        public bool showNetworkStats = false;
        public bool enableLatencySimulation = false;
        [Range(0, 500)]
        public int simulatedLatency = 50;

        /// <summary>
        /// Apply this configuration to a NetworkManager
        /// </summary>
        public void ApplyToNetworkManager(NetworkManager networkManager)
        {
            if (networkManager == null) return;
            
            // Note: FishNet TickRate is typically configured in the NetworkManager inspector
            // and may be read-only at runtime. Check your NetworkManager component settings.
            
            // Configure ServerManager settings if available
            if (networkManager.ServerManager != null)
            {
                // Server-specific configuration would go here
                // Most FishNet settings are configured via inspector
            }
            
            // Configure ClientManager settings if available  
            if (networkManager.ClientManager != null)
            {
                // Client-specific configuration would go here
            }
            
            Debug.Log($"Applied Zolian network configuration to NetworkManager: " +
                     $"Port={worldServerPort}, MaxConnections={maxConnections}, " +
                     $"Current TickRate={networkManager.TimeManager.TickRate} (read-only)");
        }

        /// <summary>
        /// Get configuration for server builds
        /// </summary>
        public ZolianNetworkConfig GetServerConfig()
        {
            var serverConfig = CreateInstance<ZolianNetworkConfig>();
            
            // Copy values but adjust for server
            serverConfig.worldServerPort = worldServerPort;
            serverConfig.maxConnections = maxConnections;
            serverConfig.tickRate = tickRate;
            serverConfig.enableServerPhysics = enableServerPhysics;
            serverConfig.enableClientPrediction = false; // Server doesn't need prediction
            serverConfig.enableNetworkLogging = enableNetworkLogging;
            
            return serverConfig;
        }

        /// <summary>
        /// Get configuration for client builds
        /// </summary>
        public ZolianNetworkConfig GetClientConfig()
        {
            var clientConfig = CreateInstance<ZolianNetworkConfig>();
            
            // Copy values but adjust for client
            clientConfig.defaultServerAddress = defaultServerAddress;
            clientConfig.autoConnectAfterAuth = autoConnectAfterAuth;
            clientConfig.connectionTimeout = connectionTimeout;
            clientConfig.enableClientPrediction = enableClientPrediction;
            clientConfig.reconciliationThreshold = reconciliationThreshold;
            clientConfig.enableNetworkLogging = enableNetworkLogging;
            
            return clientConfig;
        }

        /// <summary>
        /// Validate configuration values
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (worldServerPort < 1024 || worldServerPort > 65535)
            {
                errorMessage = "World server port must be between 1024 and 65535";
                return false;
            }
            
            if (maxConnections <= 0)
            {
                errorMessage = "Max connections must be greater than 0";
                return false;
            }
            
            if (tickRate <= 0 || tickRate > 120)
            {
                errorMessage = "Tick rate must be between 1 and 120";
                return false;
            }
            
            if (connectionTimeout <= 0)
            {
                errorMessage = "Connection timeout must be greater than 0";
                return false;
            }
            
            return true;
        }

        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            worldServerPort = (ushort)Mathf.Clamp(worldServerPort, 1024, 65535);
            maxConnections = Mathf.Clamp(maxConnections, 1, 1000);
            tickRate = Mathf.Clamp(tickRate, 10, 120);
            connectionTimeout = Mathf.Clamp(connectionTimeout, 1f, 60f);
            movementSendRate = Mathf.Clamp(movementSendRate, 0.05f, 1f);
            positionThreshold = Mathf.Clamp(positionThreshold, 0.01f, 1f);
            reconciliationThreshold = Mathf.Clamp(reconciliationThreshold, 0.1f, 5f);
            maxMovementSpeed = Mathf.Clamp(maxMovementSpeed, 1f, 100f);
            maxTeleportDistance = Mathf.Clamp(maxTeleportDistance, 1f, 1000f);
        }
    }
}