using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace AnyRPG {
    public class NetworkManagerServer : ConfiguredMonoBehaviour {

        public event Action<int, int, bool, bool> OnAuthenticationResult = delegate { };
        public event Action<int, List<PlayerCharacterSaveData>> OnLoadCharacterList = delegate { };
        public event Action<int> OnDeletePlayerCharacter = delegate { };
        public event Action<int> OnCreatePlayerCharacter = delegate { };
        public event Action<int> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int> OnStartLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };

        [SerializeField]
        private NetworkController networkController = null;

        // jwt for each client so the server can make API calls to the api server on their behalf
        //private Dictionary<int, string> clientTokens = new Dictionary<int, string>();

        // cached list of player character save data from client lookups used for loading games
        /// <summary>
        /// accountId, playerCharacterId, playerCharacterSaveData
        /// </summary>
        private Dictionary<int, Dictionary<int, PlayerCharacterSaveData>> playerCharacterDataDict = new Dictionary<int, Dictionary<int, PlayerCharacterSaveData>>();

        /// <summary>
        /// clientId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccountsByClient = new Dictionary<int, LoggedInAccount>();

        /// <summary>
        /// accountId, loggedInAccount
        /// </summary>
        private Dictionary<int, LoggedInAccount> loggedInAccounts = new Dictionary<int, LoggedInAccount>();


        /// <summary>
        /// clientId, username
        /// </summary>
        private Dictionary<int, string> loginRequests = new Dictionary<int, string>();

        /// <summary>
        /// username, temporarylobbyaccount
        /// </summary>
        private Dictionary<string, TemporaryLobbyAccount> lobbyAccounts = new Dictionary<string, TemporaryLobbyAccount>();

        // list of lobby games
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();

        /// <summary>
        /// accountId, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameAccountLookup = new Dictionary<int, int>();

        /// <summary>
        /// hashcode, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameLoadRequestHashCodes = new Dictionary<int, int>();

        /// <summary>
        /// gameId, sceneFileName, sceneHandle
        /// </summary>
        private Dictionary<int, Dictionary<string, int>> lobbyGameSceneHandles = new Dictionary<int, Dictionary<string, int>>();

        /// <summary>
        /// sceneHandle, gameId
        /// </summary>
        private Dictionary<int, int> lobbyGameSceneHandleLookup = new Dictionary<int, int>();

        private int lobbyGameCounter = 0;
        private int lobbyGameAccountCounter = 0;
        private int maxLobbyChatTextSize = 64000;

        // lobby chat
        private string lobbyChatText = string.Empty;
        private Dictionary<int, string> lobbyGameChatText = new Dictionary<int, string>();


        private GameServerClient gameServerClient = null;
        private bool serverModeActive = false;
        private NetworkClientMode clientMode = NetworkClientMode.Lobby;

        // game manager references
        private SaveManager saveManager = null;
        private ChatCommandManager chatCommandManager = null;
        private LogManager logManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private InteractionManager interactionManager = null;
        private LevelManagerServer levelManagerServer = null;
        private SystemDataFactory systemDataFactory = null;
        private VendorManagerServer vendorManagerServer = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;
        private CraftingManager craftingManager = null;
        private UnitSpawnManager unitSpawnManager = null;
        private LevelManager levelManager = null;
        private TimeOfDayManagerServer timeOfDayManagerServer = null;
        private SystemEventManager systemEventManager = null;
        private WeatherManagerServer weatherManagerServer = null;
        private ClassChangeManagerServer classChangeManagerServer = null;
        private FactionChangeManagerServer factionChangeManagerServer = null;
        private SpecializationChangeManagerServer specializationChangeManagerServer = null;
        private SkillTrainerManagerServer skillTrainerManagerServer = null;
        private NameChangeManagerServer nameChangeManagerServer = null;
        private QuestGiverManagerServer questGiverManagerServer = null;
        private CharacterAppearanceManagerServer characterAppearanceManagerServer = null;
        private DialogManagerServer dialogManagerServer = null;

        public bool ServerModeActive { get => serverModeActive; }
        public NetworkClientMode ClientMode { get => clientMode; set => clientMode = value; }
        public Dictionary<int, LoggedInAccount> LoggedInAccounts { get => loggedInAccounts; }
        public Dictionary<int, LoggedInAccount> LoggedInAccountsByClient { get => loggedInAccountsByClient; }
        public Dictionary<int, LobbyGame> LobbyGames { get => lobbyGames; }
        public Dictionary<int, Dictionary<string, int>> LobbyGameSceneHandles { get => lobbyGameSceneHandles; }
        public Dictionary<int, int> LobbyGameAccountLookup { get => lobbyGameAccountLookup; set => lobbyGameAccountLookup = value; }
        public NetworkController NetworkController { get => networkController; set => networkController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            networkController?.Configure(systemGameManager);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            chatCommandManager = systemGameManager.ChatCommandManager;
            logManager = systemGameManager.LogManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            characterManager = systemGameManager.CharacterManager;
            interactionManager = systemGameManager.InteractionManager;
            levelManagerServer = systemGameManager.LevelManagerServer;
            systemDataFactory = systemGameManager.SystemDataFactory;
            vendorManagerServer = systemGameManager.VendorManagerServer;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
            craftingManager = systemGameManager.CraftingManager;
            unitSpawnManager = systemGameManager.UnitSpawnManager;
            levelManager = systemGameManager.LevelManager;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            systemEventManager = systemGameManager.SystemEventManager;
            weatherManagerServer = systemGameManager.WeatherManagerServer;
            classChangeManagerServer = systemGameManager.ClassChangeManagerServer;
            factionChangeManagerServer = systemGameManager.FactionChangeManagerServer;
            specializationChangeManagerServer = systemGameManager.SpecializationChangeManagerServer;
            skillTrainerManagerServer = systemGameManager.SkillTrainerManagerServer;
            nameChangeManagerServer = systemGameManager.NameChangeManagerServer;
            characterAppearanceManagerServer = systemGameManager.CharacterAppearanceManagerServer;
            dialogManagerServer = systemGameManager.DialogManagerServer;
            questGiverManagerServer = systemGameManager.QuestGiverManagerServer;
        }

        public void AddLoggedInAccount(int clientId, int accountId, string token) {
            //Debug.Log($"NetworkManagerServer.AddLoggedInAccount({clientId}, {accountId}, {token})");

            if (loginRequests.ContainsKey(clientId)) {
                if (loggedInAccounts.ContainsKey(accountId)) {
                    Debug.Log($"NetworkManagerServer.AddLoggedInAccount({clientId}, {accountId}, {token}) : updating existing object");
                    int oldClientId = loggedInAccounts[accountId].clientId;
                    loggedInAccounts[accountId].clientId = clientId;
                    loggedInAccounts[accountId].token = token;
                    loggedInAccounts[accountId].ipAddress = GetClientIPAddress(clientId);
                    loggedInAccountsByClient.Remove(oldClientId);
                    loggedInAccountsByClient.Add(clientId, loggedInAccounts[accountId]);
                } else {
                    LoggedInAccount loggedInAccount = new LoggedInAccount(clientId, accountId, loginRequests[clientId], token, GetClientIPAddress(clientId));
                    loggedInAccounts.Add(accountId, loggedInAccount);
                    loggedInAccountsByClient.Add(clientId, loggedInAccount);
                }
            }
        }

        public void OnSetGameMode(GameMode gameMode) {
            //Debug.Log($"NetworkManagerServer.OnSetGameMode({gameMode})");
            
            if (gameMode == GameMode.Network) {
                // create instance of GameServerClient
                gameServerClient = new GameServerClient(systemGameManager, systemConfigurationManager.ApiServerAddress);
                return;
            }

        }

        public void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            if (playerCharacterMonitor.unitController != null) {
                playerCharacterMonitor.SavePlayerLocation();
            }
            if (playerCharacterMonitor.saveDataDirty == true) {
                if (clientMode == NetworkClientMode.MMO) {
                    if (loggedInAccounts.ContainsKey(playerCharacterMonitor.accountId) == false) {
                        // can't do anything without a token
                        return;
                    }
                    gameServerClient.SavePlayerCharacter(
                        playerCharacterMonitor.accountId,
                        loggedInAccounts[playerCharacterMonitor.accountId].token,
                        playerCharacterMonitor.playerCharacterSaveData.PlayerCharacterId,
                        playerCharacterMonitor.playerCharacterSaveData.SaveData);
                }
            }
        }

        public void GetLoginToken(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.GetLoginToken({clientId}, {username}, {password})");

            loginRequests.Add(clientId, username);
            if (clientMode == NetworkClientMode.MMO) {
                gameServerClient.Login(clientId, username, password);
            } else {
                LobbyLogin(clientId, username, password);
            }
        }

        public void LobbyLogin(int clientId, string username, string password) {
            //Debug.Log($"NetworkManagerServer.LobbyLogin({clientId}, {username}, {password})");
            if (lobbyAccounts.ContainsKey(username) == false) {
                int accountId = CreateLobbyAccount(username, password);
                ProcessLoginResponse(clientId, accountId, true, string.Empty);
                return;
            } else {
                if (lobbyAccounts[username].password == password) {
                    // password correct
                    ProcessLoginResponse(clientId, lobbyAccounts[username].accountId, true, string.Empty);
                    return;
                } else {
                    // password incorrect
                    ProcessLoginResponse(clientId, -1, false, string.Empty);
                    return;
                }
            }
        }

        private int CreateLobbyAccount(string username, string password) {
            TemporaryLobbyAccount temporaryLobbyAccount = new TemporaryLobbyAccount(GetNewLobbyAccountId(), username, password);
            lobbyAccounts.Add(username, temporaryLobbyAccount);
            return temporaryLobbyAccount.accountId;
        }

        private int GetNewLobbyAccountId() {
            int returnValue = lobbyGameAccountCounter;
            lobbyGameAccountCounter++;
            return returnValue;
        }

        public void ProcessLoginResponse(int clientId, int accountId, bool correctPassword, string token) {
            //Debug.Log($"NetworkManagerServer.ProcessLoginResponse({clientId}, {accountId}, {correctPassword}, {token})");

            SpawnPlayerRequest spawnPlayerRequest = null;
            if (correctPassword == true) {
                if (loggedInAccounts.ContainsKey(accountId)) {
                    if (playerManagerServer.ActivePlayers.ContainsKey(accountId)) {
                        // if the player is already logged in, we need to add a spawn request to match the current position and direction of the player
                        spawnPlayerRequest = new SpawnPlayerRequest() {
                            overrideSpawnDirection = true,
                            spawnForwardDirection = playerManagerServer.ActivePlayers[accountId].transform.forward,
                            overrideSpawnLocation = true,
                            spawnLocation = playerManagerServer.ActivePlayers[accountId].transform.position
                        };
                    }
                    // if the account is already logged in, kick the old client
                    playerManagerServer.DespawnPlayerUnit(accountId);
                    KickPlayer(accountId);
                } else if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId)) {
                    // if the account is disconnected but was already logged in, add a spawn request to match the saved position and direction of the player
                    AnyRPGSaveData saveData = playerManagerServer.PlayerCharacterMonitors[accountId].playerCharacterSaveData.SaveData;
                    spawnPlayerRequest = new SpawnPlayerRequest() {
                        overrideSpawnDirection = true,
                        spawnForwardDirection = new Vector3(saveData.PlayerRotationX, saveData.PlayerRotationY, saveData.PlayerRotationZ),
                        overrideSpawnLocation = true,
                        spawnLocation = new Vector3(saveData.PlayerLocationX, saveData.PlayerLocationY, saveData.PlayerLocationZ)
                    };
                }
                if (spawnPlayerRequest != null) {
                    playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest, false);
                }
                AddLoggedInAccount(clientId, accountId, token);
            }
            loginRequests.Remove(clientId);
            OnAuthenticationResult(clientId, accountId, true, correctPassword);
            
            if (correctPassword == false) {
                return;
            }
            //if (spawnPlayerRequest != null) {
            //}

            OnLobbyLogin(accountId);
            networkController.AdvertiseLobbyLogin(accountId, loggedInAccounts[accountId].username);
        }

        public void CreatePlayerCharacter(int accountId, AnyRPGSaveData anyRPGSaveData) {
            Debug.Log($"NetworkManagerServer.CreatePlayerCharacter(AnyRPGSaveData)");
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.CreatePlayerCharacter(accountId, loggedInAccounts[accountId].token, anyRPGSaveData);
        }

        public void ProcessCreatePlayerCharacterResponse(int accountId) {
            Debug.Log($"NetworkManagerServer.ProcessCreatePlayerCharacterResponse({accountId})");

            OnCreatePlayerCharacter(accountId);
        }


        public void DeletePlayerCharacter(int accountId, int playerCharacterId) {
            Debug.Log($"NetworkManagerServer.DeletePlayerCharacter({playerCharacterId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                return;
            }

            gameServerClient.DeletePlayerCharacter(accountId, loggedInAccounts[accountId].token, playerCharacterId);
        }

        public void ProcessStopNetworkUnitServer(UnitController unitController) {
            //Debug.Log($"NetworkManagerServer.ProcessStopNetworkUnitServer({unitController.gameObject.name})");

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            characterManager.ProcessStopNetworkUnit(unitController);
            //if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                //playerManagerServer.StopMonitoringPlayerUnit(unitController);
            //}
        }

        public void ProcessDeletePlayerCharacterResponse(int accountId) {
            Debug.Log($"NetworkManagerServer.ProcessDeletePlayerCharacterResponse({accountId})");

            OnDeletePlayerCharacter(accountId);
        }

        public void LoadCharacterList(int accountId) {
            Debug.Log($"NetworkManagerServer.LoadCharacterList({accountId})");
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                // can't do anything without a token
                //return new List<PlayerCharacterSaveData>();
                return;
            }
            gameServerClient.LoadCharacterList(accountId, loggedInAccounts[accountId].token);
        }

        public void ProcessLoadCharacterListResponse(int accountId, List<PlayerCharacterData> playerCharacters) {
            //Debug.Log($"NetworkManagerServer.ProcessLoadCharacterListResponse({accountId})");

            List<PlayerCharacterSaveData> playerCharacterSaveDataList = new List<PlayerCharacterSaveData>();
            foreach (PlayerCharacterData playerCharacterData in playerCharacters) {
                playerCharacterSaveDataList.Add(new PlayerCharacterSaveData() {
                    PlayerCharacterId = playerCharacterData.id,
                    SaveData = saveManager.LoadSaveDataFromString(playerCharacterData.saveData)
                });
            }
            Dictionary<int, PlayerCharacterSaveData> playerCharacterSaveDataDict = new Dictionary<int, PlayerCharacterSaveData>();
            foreach (PlayerCharacterSaveData playerCharacterSaveData in playerCharacterSaveDataList) {
                playerCharacterSaveDataDict.Add(playerCharacterSaveData.PlayerCharacterId, playerCharacterSaveData);
            }
            if (playerCharacterDataDict.ContainsKey(accountId)) {
                playerCharacterDataDict[accountId] = playerCharacterSaveDataDict;
            } else {
                playerCharacterDataDict.Add(accountId, playerCharacterSaveDataDict);
            }

            OnLoadCharacterList(accountId, playerCharacterSaveDataList);
        }

        public PlayerCharacterSaveData GetPlayerCharacterSaveData(int accountId, int playerCharacterId) {
            if (playerCharacterDataDict.ContainsKey(accountId) == false) {
                return null;
            }
            if (playerCharacterDataDict[accountId].ContainsKey(playerCharacterId) == false) {
                return null;
            }
            return playerCharacterDataDict[accountId][playerCharacterId];
        }

        public string GetAccountToken(int accountId) {
            //Debug.Log($"NetworkManagerServer.GetClientToken({accountId})");

            if (loggedInAccounts.ContainsKey(accountId)) {
                return loggedInAccounts[accountId].token;
            }
            return string.Empty;
        }

        public void ProcessClientDisconnect(int clientId) {
            //Debug.Log($"NetworkManagerServer.ProcessClientDisconnect({clientId})");

            if (loggedInAccountsByClient.ContainsKey(clientId) == false) {
                return;
            }
            int accountId = loggedInAccountsByClient[clientId].accountId;
            ProcessClientLogout(accountId);
            playerManagerServer.ProcessDisconnect(accountId);
        }

        public void ProcessClientLogout(int accountId) {
            //Debug.Log($"NetworkManagerServer.ProcessClientLogout({accountId})");

            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            int clientId = loggedInAccounts[accountId].clientId;
            if (lobbyGameAccountLookup.ContainsKey(accountId) == true) {
                int gameId = lobbyGameAccountLookup[accountId];
                LeaveLobbyGame(gameId, accountId);
            }

            loggedInAccounts.Remove(accountId);
            loggedInAccountsByClient.Remove(clientId);

            OnLobbyLogout(accountId);
            networkController?.AdvertiseLobbyLogout(accountId);
        }

        public void ActivateServerMode() {
            //Debug.Log($"NetworkManagerServer.ActivateServerMode()");

            serverModeActive = true;
            systemEventManager.NotifyOnStartServer();
            systemEventManager.OnChooseWeather += HandleChooseWeather;
            systemEventManager.OnStartWeather += HandleStartWeather;
            systemEventManager.OnEndWeather += HandleEndWeather;
        }

        public void DeactivateServerMode() {
            Debug.Log($"NetworkManagerServer.DeactivateServerMode()");

            serverModeActive = false;

            loggedInAccountsByClient.Clear();
            lobbyGames.Clear();
            lobbyGameChatText.Clear();

            systemEventManager.NotifyOnStopServer();
            systemEventManager.OnChooseWeather -= HandleChooseWeather;
            systemEventManager.OnStartWeather -= HandleStartWeather;
            systemEventManager.OnEndWeather -= HandleEndWeather;

        }

        private void HandleStartWeather(int sceneHandle) {
            networkController.AdvertiseStartWeather(sceneHandle);
        }

        private void HandleEndWeather(int sceneHandle, WeatherProfile profile, bool immediate) {
            networkController?.AdvertiseEndWeather(sceneHandle, profile, immediate);
        }

        private void HandleChooseWeather(int sceneHandle, WeatherProfile profile) {
            networkController?.AdvertiseChooseWeather(sceneHandle, profile);
        }

        public void StartServer() {
            //Debug.Log($"NetworkManagerServer.StartServer()");

            if (serverModeActive == true) {
                return;
            }

            networkController?.StartServer();
        }

        public void StopServer() {
            Debug.Log($"NetworkManagerServer.StartServer()");

            if (serverModeActive == false) {
                return;
            }

            networkController?.StopServer();
        }

        public void KickPlayer(int accountId) {
            Debug.Log($"NetworkManagerServer.KickPlayer({accountId})");
            networkController?.KickPlayer(accountId);
        }

        public string GetClientIPAddress(int clientId) {
            return networkController?.GetClientIPAddress(clientId);
        }

        public void CreateLobbyGame(string sceneResourceName, int accountId, bool allowLateJoin) {
            
            LobbyGame lobbyGame = new LobbyGame(accountId, lobbyGameCounter, sceneResourceName, loggedInAccounts[accountId].username, allowLateJoin);
            lobbyGameCounter++;
            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            lobbyGameAccountLookup.Add(accountId, lobbyGame.gameId);
            lobbyGameChatText.Add(lobbyGame.gameId, string.Empty);
            OnCreateLobbyGame(lobbyGame);
            networkController.AdvertiseCreateLobbyGame(lobbyGame);
        }

        public void CancelLobbyGame(int accountId, int gameId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId) {
                // game not found, or requesting client is not leader
                return;
            }
            foreach (int accountIdInGame in lobbyGames[gameId].PlayerList.Keys) {
                lobbyGameAccountLookup.Remove(accountIdInGame);
            }
            lobbyGames.Remove(gameId);
            lobbyGameChatText.Remove(gameId);
            OnCancelLobbyGame(gameId);
            networkController.AdvertiseCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            lobbyGames[gameId].AddPlayer(accountId, loggedInAccounts[accountId].username);
            lobbyGameAccountLookup.Add(accountId, gameId);
            OnJoinLobbyGame(gameId, accountId, loggedInAccounts[accountId].username);
            networkController.AdvertiseAccountJoinLobbyGame(gameId, accountId, loggedInAccounts[accountId].username);
        }

        public void RequestLobbyGameList(int accountId) {
            networkController.SetLobbyGameList(accountId, lobbyGames.Values.ToList<LobbyGame>());
        }

        public void RequestLobbyPlayerList(int accountId) {
            Dictionary<int, string> lobbyPlayerList = new Dictionary<int, string>();
            foreach (int loggedInClientId in loggedInAccountsByClient.Keys) {
                lobbyPlayerList.Add(loggedInClientId, loggedInAccountsByClient[loggedInClientId].username);
            }
            networkController.SetLobbyPlayerList(accountId, lobbyPlayerList);
        }

        public void ChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
                Debug.LogWarning($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName}) - lobby game or client does not exist");
                return;
            }
            if (lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                Debug.LogWarning($"NetworkManagerServer.ChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName}) - client not in lobby game");
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].unitProfileName = unitProfileName;
            lobbyGames[gameId].PlayerList[accountId].appearanceString = appearanceString;
            lobbyGames[gameId].PlayerList[accountId].swappableMeshSaveData = swappableMeshSaveData;
            networkController.AdvertiseChooseLobbyGameCharacter(gameId, accountId, unitProfileName);
        }

        public void ToggleLobbyGameReadyStatus(int gameId, int accountId) {
            //Debug.Log($"NetworkManagerClient.ToggleLobbyGameReadyStatus({gameId}, {accountId})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // game did not exist or client was not in game
                return;
            }

            lobbyGames[gameId].PlayerList[accountId].ready = !lobbyGames[gameId].PlayerList[accountId].ready;
            networkController.AdvertiseSetLobbyGameReadyStatus(gameId, accountId, lobbyGames[gameId].PlayerList[accountId].ready);
        }

        public void RequestStartLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].leaderAccountId != accountId || lobbyGames[gameId].inProgress == true) {
                // game did not exist, non leader tried to start, or already in progress, nothing to do
                return;
            }
            StartLobbyGame(gameId);
        }

        public void RequestJoinLobbyGameInProgress(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].inProgress == false || lobbyGames[gameId].allowLateJoin == false) {
                // game did not exist or not in progress or does not allow late joins
                return;
            }
            JoinLobbyGameInProgress(gameId, accountId);
        }

        public void JoinLobbyGameInProgress(int gameId, int accountId) {
            Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId})");

            string sceneName = string.Empty;
            if (playerManagerServer.SpawnRequests.ContainsKey(accountId) == false) {
                Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId}) - creating new spawn request");
                sceneName = lobbyGames[gameId].sceneResourceName;
                PlayerCharacterSaveData playerCharacterSaveData = GetNewLobbyGamePlayerCharacterSaveData(gameId, accountId, lobbyGames[gameId].PlayerList[accountId].unitProfileName);
                playerCharacterSaveData.SaveData.appearanceString = lobbyGames[gameId].PlayerList[accountId].appearanceString;
                playerCharacterSaveData.SaveData.swappableMeshSaveData = lobbyGames[gameId].PlayerList[accountId].swappableMeshSaveData;

                playerManagerServer.AddPlayerMonitor(accountId, playerCharacterSaveData);
            } else {
                // player already has a spawn request, so this is a rejoin.  Leave it alone because it contains the last correct position and direction
                Debug.Log($"NetworkManagerServer.JoinLobbyGameInProgress({gameId}, {accountId}) - reusing existing spawn request");
                sceneName = playerManagerServer.PlayerCharacterMonitors[accountId].playerCharacterSaveData.SaveData.CurrentScene;
                if (levelManager.SceneDictionary.ContainsKey(sceneName)) {
                    sceneName = levelManager.SceneDictionary[sceneName].ResourceName;
                }
            }
            networkController.AdvertiseJoinLobbyGameInProgress(gameId, accountId, sceneName);
        }

        public void StartLobbyGame(int gameId) {
            //Debug.Log($"NetworkManagerServer.StartLobbyGame({gameId})");

            lobbyGames[gameId].inProgress = true;
            OnStartLobbyGame(gameId);
            // create spawn requests for all players in the game
            foreach (KeyValuePair<int, LobbyGamePlayerInfo> playerInfo in lobbyGames[gameId].PlayerList) {
                PlayerCharacterSaveData playerCharacterSaveData = GetNewLobbyGamePlayerCharacterSaveData(gameId, playerInfo.Key, playerInfo.Value.unitProfileName);
                playerCharacterSaveData.SaveData.appearanceString = playerInfo.Value.appearanceString;
                playerCharacterSaveData.SaveData.swappableMeshSaveData = playerInfo.Value.swappableMeshSaveData;

                playerManagerServer.AddPlayerMonitor(playerInfo.Key, playerCharacterSaveData);

            }
            networkController.StartLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId, int accountId) {
            if (lobbyGames.ContainsKey(gameId) == false || loggedInAccounts.ContainsKey(accountId) == false) {
                // game or client doesn't exist
                return;
            }
            if (lobbyGames[gameId].leaderAccountId == accountId && lobbyGames[gameId].inProgress == false) {
                CancelLobbyGame(accountId, gameId);
            } else {
                lobbyGames[gameId].RemovePlayer(accountId);
                lobbyGameAccountLookup.Remove(accountId);
                OnLeaveLobbyGame(gameId, accountId);
                networkController.AdvertiseAccountLeaveLobbyGame(gameId, accountId);
            }
        }

        public void SendLobbyChatMessage(string messageText, int accountId) {
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";
            lobbyChatText += addedText;
            lobbyChatText = ShortenStringOnNewline(lobbyChatText, maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyChatMessage(addedText);
        }

        public void SendLobbyGameChatMessage(string messageText, int accountId, int gameId) {
            if (loggedInAccountsByClient.ContainsKey(accountId) == false) {
                return;
            }
            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";
            lobbyGameChatText[gameId] += addedText;
            lobbyGameChatText[gameId] = ShortenStringOnNewline(lobbyGameChatText[gameId], maxLobbyChatTextSize);

            networkController.AdvertiseSendLobbyGameChatMessage(addedText, gameId);
        }

        public void SendSceneChatMessage(string messageText, int accountId) {
            if (loggedInAccounts.ContainsKey(accountId) == false) {
                return;
            }
            logManager.WriteChatMessageServer(accountId, messageText);
        }

        public void AdvertiseSceneChatMessage(string messageText, int accountId) {
            networkController.AdvertiseSendSceneChatMessage(messageText, accountId);

            if (playerManagerServer.PlayerCharacterMonitors.ContainsKey(accountId) == false) {
                // no unit logged in
                return;
            }
            string addedText = $"{loggedInAccounts[accountId].username}: {messageText}\n";

            playerManagerServer.PlayerCharacterMonitors[accountId].unitController.UnitEventController.NotifyOnBeginChatMessage(addedText);
        }

        public void AdvertiseLoadScene(string sceneName, int accountId) {
            Debug.Log($"NetworkManagerServer.AdvertiseLoadScene({sceneName}, {accountId})");
            
            DespawnPlayerUnit(accountId);
            networkController.AdvertiseLoadScene(sceneName, accountId);
        }

        public void DespawnPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.DespawnPlayerUnit({accountId})");

            playerManagerServer.DespawnPlayerUnit(accountId);
        }

        public void RequestDespawnPlayerUnit(int accountId) {
            //Debug.Log($"NetworkManagerServer.RequestDespawnPlayerUnit({accountId})");

            // this method is only called when loading a cutscene, so we need to create a spawn request so the player spawns
            // at the correct position and direction when the cutscene ends
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId)) {
                SpawnPlayerRequest spawnPlayerRequest = new SpawnPlayerRequest() {
                    overrideSpawnDirection = true,
                    spawnForwardDirection = playerManagerServer.ActivePlayers[accountId].transform.forward,
                    overrideSpawnLocation = true,
                    spawnLocation = playerManagerServer.ActivePlayers[accountId].transform.position
                };
                DespawnPlayerUnit(accountId);
                playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest, true);
            }
        }

        public static string ShortenStringOnNewline(string message, int messageLength) {
            // if the chat text is greater than the max size, keep splitting it on newlines until reaches an acceptable size
            while (message.Length > messageLength && message.Contains("\n")) {
                message = message.Split("\n", 1)[1];
            }
            return message;
        }

        public int GetServerPort() {
            return networkController.GetServerPort();
        }

        public void AdvertiseTeleport(int accountId, TeleportEffectProperties teleportEffectProperties) {
            //Debug.Log($"NetworkManagerServer.AdvertiseTeleport({accountId}, {teleportEffectProperties.LevelName})");

            DespawnPlayerUnit(accountId);
            networkController.AdvertiseLoadScene(teleportEffectProperties.LevelName, accountId);
        }

        public void ReturnObjectToPool(GameObject returnedObject) {
            //Debug.Log($"NetworkManagerServer.ReturnObjectToPool({returnedObject.name})");

            networkController.ReturnObjectToPool(returnedObject);
        }

        /*
        public void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex, UnitController sourceUnitController) {
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController)) {
                networkController.AdvertiseInteractWithQuestGiver(interactable, optionIndex, playerManagerServer.ActivePlayerLookup[sourceUnitController]);
            }
        }
        */

        public void InteractWithOption(UnitController sourceUnitController, Interactable interactable, int componentIndex, int choiceIndex) {
            interactionManager.InteractWithOptionServer(sourceUnitController, interactable, componentIndex, choiceIndex);
        }

        /*
        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            networkController.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
        }
        */

        /*
        public void AdvertiseInteractWithClassChangeComponent(int accountId, Interactable interactable, int optionIndex) {
            networkController.AdvertiseInteractWithClassChangeComponentServer(accountId, interactable, optionIndex);
        }
        */

        public void HandleSceneLoadEnd(Scene scene, int loadRequestHashCode) {
            //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode})");

            if (lobbyGameLoadRequestHashCodes.ContainsKey(loadRequestHashCode) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneLoadEnd({scene.name}, {loadRequestHashCode}) - lobby game load request");
                AddLobbyGameSceneHandle(lobbyGameLoadRequestHashCodes[loadRequestHashCode], scene);
                lobbyGameLoadRequestHashCodes.Remove(loadRequestHashCode);
            }
            levelManagerServer.AddLoadedScene(scene);
            levelManagerServer.ProcessLevelLoad(scene);
        }

        private void AddLobbyGameSceneHandle(int lobbyGameId, Scene scene) {
            if (lobbyGameSceneHandles.ContainsKey(lobbyGameId) == false) {
                lobbyGameSceneHandles.Add(lobbyGameId, new Dictionary<string, int>());
            }
            if (lobbyGameSceneHandles[lobbyGameId].ContainsKey(scene.name) == false) {
                lobbyGameSceneHandles[lobbyGameId].Add(scene.name, scene.handle);
            }
            if (lobbyGameSceneHandleLookup.ContainsKey(scene.handle) == false) {
                lobbyGameSceneHandleLookup.Add(scene.handle, lobbyGameId);
            }
        }

        public void HandleSceneUnloadEnd(int sceneHandle, string sceneName) {
            //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle})");

            if (lobbyGameSceneHandleLookup.ContainsKey(sceneHandle) == true) {
                //Debug.Log($"NetworkManagerServer.HandleSceneUnloadEnd({sceneName}, {sceneHandle}) - lobby game unload request");
                int lobbyGameId = lobbyGameSceneHandleLookup[sceneHandle];
                if (lobbyGameSceneHandles.ContainsKey(lobbyGameId) == true && lobbyGameSceneHandles[lobbyGameId].ContainsKey(sceneName) == true) {
                    lobbyGameSceneHandles[lobbyGameId].Remove(sceneName);
                }
                lobbyGameSceneHandleLookup.Remove(sceneHandle);
            }
            levelManagerServer.RemoveLoadedScene(sceneHandle, sceneName);
        }

        public UnitController SpawnCharacterPrefab(CharacterRequestData characterRequestData, Transform parentTransform, Vector3 position, Vector3 forward, Scene scene) {
            return networkController.SpawnCharacterPrefab(characterRequestData, parentTransform, position, forward, scene);
        }

        public GameObject SpawnModelPrefab(GameObject spawnPrefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerServer.SpawnModelPrefab({spawnRequestId})");

            return networkController.SpawnModelPrefabServer(spawnPrefab, parentTransform, position, forward);
        }

        public void TurnInDialog(Interactable interactable, int componentIndex, Dialog dialog, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            dialogManagerServer.TurnInDialog(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, dialog);
        }

        public void TurnInQuestDialog(Dialog dialog, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            playerManagerServer.ActivePlayers[accountId].CharacterDialogManager.TurnInDialog(dialog);
        }


        public void SetPlayerCharacterClass(Interactable interactable, int componentIndex, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            classChangeManagerServer.ChangeCharacterClass(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex);
        }

        public void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            specializationChangeManagerServer.ChangeCharacterSpecialization(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex);
        }

        public void SetPlayerFaction(Interactable interactable, int componentIndex, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            factionChangeManagerServer.ChangeCharacterFaction(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex);
        }

        public void LearnSkill(Interactable interactable, int componentIndex, int skillId, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            skillTrainerManagerServer.LearnSkill(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, skillId);
        }

        public void AcceptQuest(Interactable interactable, int componentIndex, Quest quest, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }

            questGiverManagerServer.AcceptQuest(interactable, componentIndex, playerManagerServer.ActivePlayers[accountId], quest);
        }

        public void CompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }

            questGiverManagerServer.CompleteQuest(interactable, componentIndex, playerManagerServer.ActivePlayers[accountId], quest, questRewardChoices);
        }


        public void AdvertiseMessageFeedMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseMessageFeedMessage(playerManagerServer.ActivePlayerLookup[sourceUnitController], message);
        }

        public void AdvertiseSystemMessage(UnitController sourceUnitController, string message) {
            networkController.AdvertiseSystemMessage(playerManagerServer.ActivePlayerLookup[sourceUnitController], message);
        }

        public void SellVendorItem(Interactable interactable, int componentIndex, int itemInstanceId, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            if (systemItemManager.InstantiatedItems.ContainsKey(itemInstanceId) == false) {
                return;
            }
            vendorManagerServer.SellItemToVendor(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, systemItemManager.InstantiatedItems[itemInstanceId]);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, UnitProfile unitProfile, UnitToughness unitToughness, int accountId) {
            Debug.Log($"NetworkManagerServer.RequestSpawnUnit({interactable.gameObject.name}, {componentIndex}, {unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfile.ResourceName}, {unitToughness?.ResourceName}, {accountId})");

            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            unitSpawnManager.SpawnUnit(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfile, unitToughness);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, InstantiatedItem newInstantiatedItem) {
            networkController.AdvertiseAddToBuyBackCollection(sourceUnitController, playerManagerServer.ActivePlayerLookup[sourceUnitController], interactable, componentIndex, newInstantiatedItem);
        }

        public void AdvertiseSellItemToPlayer(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            //Debug.Log($"NetworkManagerServer.AdvertiseSellItemToPlayer({sourceUnitController.gameObject.name}, {interactable.gameObject.name}, {componentIndex}, {collectionIndex}, {itemIndex}, {resourceName}, {remainingQuantity})");
            networkController.AdvertiseSellItemToPlayer(sourceUnitController, interactable, componentIndex, collectionIndex, itemIndex, resourceName, remainingQuantity);
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            vendorManagerServer.BuyItemFromVendor(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, collectionIndex, itemIndex, resourceName, accountId);
        }

        public void TakeAllLoot(int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                lootManager.TakeAllLootInternal(accountId, playerManagerServer.ActivePlayers[accountId]);
            }
        }

        public void RequestTakeLoot(int lootDropId, int accountId) {
            Debug.Log($"NetworkManagerServer.RequestTakeLoot({lootDropId}, {accountId})");

            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                //lootManager.TakeLoot(accountId, lootDropId);
                if (lootManager.LootDropIndex.ContainsKey(lootDropId) == false) {
                    return;
                }
                lootManager.LootDropIndex[lootDropId].TakeLoot(playerManagerServer.ActivePlayers[accountId]);
            }
        }

        public void RequestBeginCrafting(Recipe recipe, int craftAmount, int accountId) {
            Debug.Log($"NetworkManagerServer.RequestBeginCrafting({recipe.DisplayName}, {craftAmount}, {accountId})");

            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                craftingManager.BeginCrafting(playerManagerServer.ActivePlayers[accountId], recipe, craftAmount);
            }
        }

        public void RequestCancelCrafting(int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == true) {
                craftingManager.CancelCrafting(playerManagerServer.ActivePlayers[accountId]);
            }
        }


        public void AddAvailableDroppedLoot(int accountId, List<LootDrop> items) {
            Debug.Log($"NetworkManagerServer.AddAvailableDroppedLoot({accountId}, count: {items.Count})");

            networkController.AddAvailableDroppedLoot(accountId, items);
        }

        public void AddLootDrop(int accountId, int lootDropId, int itemId) {
            networkController.AddLootDrop(accountId, lootDropId, itemId);
        }

        public void AdvertiseTakeLoot(int accountId, int lootDropId) {
            networkController.AdvertiseTakeLoot(accountId, lootDropId);
        }

        public void SetLobbyGameLoadRequestHashcode(int gameId, int hashCode) {
            if (lobbyGameLoadRequestHashCodes.ContainsKey(hashCode) == false) {
                lobbyGameLoadRequestHashCodes.Add(hashCode, gameId);
            }
        }

        public void RequestSpawnLobbyGamePlayer(int accountId, int gameId, string sceneName) {
            //Debug.Log($"NetworkManagerServer.RequestSpawnLobbyGamePlayer({accountId}, {gameId}, {sceneName})");

            playerManagerServer.RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void SpawnPlayer(int accountId, CharacterRequestData characterRequestData, Vector3 position, Vector3 forward, string sceneName) {
            networkController.SpawnLobbyGamePlayer(accountId, characterRequestData, position, forward, sceneName);
        }

        private PlayerCharacterSaveData GetNewLobbyGamePlayerCharacterSaveData(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"NetworkManagerServer.GetNewLobbyGamePlayerCharacterSaveData({gameId}, {accountId}, {unitProfileName})");

            UnitProfile unitProfile = systemDataFactory.GetResource<UnitProfile>(unitProfileName);
            return GetNewLobbyGamePlayerCharacterSaveData(gameId, accountId, unitProfile);
        }

        private PlayerCharacterSaveData GetNewLobbyGamePlayerCharacterSaveData(int gameId, int accountId, UnitProfile unitProfile) {
            //Debug.Log($"NetworkManagerServer.GetNewLobbyGamePlayerCharacterSaveData({gameId}, {accountId}, {unitProfile.ResourceName})");

            PlayerCharacterSaveData playerCharacterSaveData = saveManager.CreateSaveData();
            playerCharacterSaveData.PlayerCharacterId = accountId;
            playerCharacterSaveData.SaveData.playerName = lobbyGames[gameId].PlayerList[accountId].userName;
            playerCharacterSaveData.SaveData.unitProfileName = unitProfile.ResourceName;
            playerCharacterSaveData.SaveData.CurrentScene = lobbyGames[gameId].sceneResourceName;
            return playerCharacterSaveData;
        }

        public Scene GetAccountScene(int accountId, string sceneName) {
            return networkController.GetAccountScene(accountId, sceneName);
        }

        public void RequestRespawnPlayerUnit(int accountId) {
            Debug.Log($"NetworkManagerServer.RequestRespawnPlayerUnit({accountId})");
            
            playerManagerServer.RespawnPlayerUnit(accountId);
        }

        public void RequestRevivePlayerUnit(int accountId) {
            Debug.Log($"NetworkManagerServer.RequestRevivePlayerUnit({accountId})");

            playerManagerServer.RevivePlayerUnit(accountId);
        }

        public void MonitorPlayerUnit(int accountId, UnitController unitController) {
            playerManagerServer.MonitorPlayerUnit(accountId, unitController);
        }

        public void RequestUpdatePlayerAppearance(int accountId, Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }

            characterAppearanceManagerServer.UpdatePlayerAppearance(playerManagerServer.ActivePlayers[accountId], accountId, interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName, int accountId) {
            if (playerManagerServer.ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            nameChangeManagerServer.SetPlayerName(playerManagerServer.ActivePlayers[accountId], interactable, componentIndex, newName);
        }

        public void RequestSpawnPet(int accountId, UnitProfile unitProfile) {
            Debug.Log($"NetworkManagerServer.RequestSpawnPet({accountId}, {unitProfile.ResourceName})");

            playerManagerServer.RequestSpawnPet(accountId, unitProfile);
        }

        public void RequestDespawnPet(int accountId, UnitProfile unitProfile) {
            playerManagerServer.RequestDespawnPet(accountId, unitProfile);
        }

        public void AdvertiseAddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"NetworkManagerServer.AdvertiseAddSpawnRequest({accountId})");

            networkController.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
        }



        public void Logout(int accountId) {
            Debug.Log($"NetworkManagerServer.Logout({accountId})");

            playerManagerServer.StopMonitoringPlayerUnit(accountId);
            KickPlayer(accountId);
            ProcessClientLogout(accountId);
        }

        public void RequestSpawnRequest(int accountId) {
            playerManagerServer.RequestSpawnRequest(accountId);
        }

        public DateTime GetServerStartTime() {
            return timeOfDayManagerServer.StartTime;
        }

        public WeatherProfile GetSceneWeatherProfile(int handle) {
            return weatherManagerServer.GetSceneWeatherProfile(handle);
        }

        public void AdvertiseLoadCutscene(Cutscene cutscene, int accountId) {
            networkController.AdvertiseLoadCutscene(cutscene, accountId);
        }
    }

}