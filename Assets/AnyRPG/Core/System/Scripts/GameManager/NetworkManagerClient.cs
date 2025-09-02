using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AnyRPG {
    public class NetworkManagerClient : ConfiguredMonoBehaviour {

        public event Action<string> OnClientVersionFailure = delegate { };
        public event Action<LobbyGame> OnCreateLobbyGame = delegate { };
        public event Action<int> OnCancelLobbyGame = delegate { };
        public event Action<int, int, string> OnJoinLobbyGame = delegate { };
        public event Action<int, int> OnLeaveLobbyGame = delegate { };
        public event Action<string> OnSendLobbyChatMessage = delegate { };
        public event Action<string, int> OnSendLobbyGameChatMessage = delegate { };
        public event Action<string, int> OnSendSceneChatMessage = delegate { };
        public event Action<int, string> OnLobbyLogin = delegate { };
        public event Action<int> OnLobbyLogout = delegate { };
        public event Action<List<LobbyGame>> OnSetLobbyGameList = delegate { };
        public event Action<Dictionary<int, string>> OnSetLobbyPlayerList = delegate { };
        public event Action<int, int, string> OnChooseLobbyGameCharacter = delegate { };
        public event Action<int, int, bool> OnSetLobbyGameReadyStatus = delegate { };
        public event Action<int> OnStartLobbyGame = delegate { };


        private string username = string.Empty;
        private string password = string.Empty;
        
        private bool isLoggingInOrOut = false;

        private NetworkClientMode clientMode = NetworkClientMode.Lobby;
        private int accountId;
        private LobbyGame lobbyGame;

        [SerializeField]
        private NetworkController networkController = null;

        private Dictionary<int, LoggedInAccount> lobbyGamePlayerList = new Dictionary<int, LoggedInAccount>();
        
        private Dictionary<int, LobbyGame> lobbyGames = new Dictionary<int, LobbyGame>();
        private Dictionary<int, string> lobbyPlayers = new Dictionary<int, string>();

        // game manager references
        private PlayerManager playerManager = null;
        private PlayerManagerServer playerManagerServer = null;
        private CharacterManager characterManager = null;
        private UIManager uIManager = null;
        private LevelManager levelManager = null;
        private LogManager logManager = null;
        private InteractionManager interactionManager = null;
        private MessageFeedManager messageFeedManager = null;
        private SystemItemManager systemItemManager = null;
        private LootManager lootManager = null;
        private CraftingManager craftingManager = null;
        private SystemEventManager systemEventManager = null;
        private TimeOfDayManagerServer timeOfDayManagerServer = null;
        private WeatherManagerClient weatherManagerClient = null;

        public string Username { get => username; }
        public string Password { get => password; }
        public NetworkClientMode ClientMode { get => clientMode; set => clientMode = value; }
        public Dictionary<int, LoggedInAccount> LobbyGamePlayerList { get => lobbyGamePlayerList; }
        public LobbyGame LobbyGame { get => lobbyGame; }
        public int AccountId { get => accountId; }
        public NetworkController NetworkController { get => networkController; set => networkController = value; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            playerManager = systemGameManager.PlayerManager;
            characterManager = systemGameManager.CharacterManager;
            levelManager = systemGameManager.LevelManager;
            uIManager = systemGameManager.UIManager;
            logManager = systemGameManager.LogManager;
            interactionManager = systemGameManager.InteractionManager;
            messageFeedManager = uIManager.MessageFeedManager;
            systemItemManager = systemGameManager.SystemItemManager;
            lootManager = systemGameManager.LootManager;
            craftingManager = systemGameManager.CraftingManager;
            systemEventManager = systemGameManager.SystemEventManager;
            playerManagerServer = systemGameManager.PlayerManagerServer;
            timeOfDayManagerServer = systemGameManager.TimeOfDayManagerServer;
            weatherManagerClient = systemGameManager.WeatherManagerClient;
        }

        public bool Login(string username, string password, string server) {
            //Debug.Log($"NetworkManagerClient.Login({username}, {password})");
            
            isLoggingInOrOut = true;

            this.username = username;
            this.password = password;
            return networkController.Login(username, password, server);
        }

        public void RequestLogout() {
            isLoggingInOrOut = true;
            networkController.RequestLogout();
        }

        public void RequestDisconnect() {
            isLoggingInOrOut = true;
            networkController.Disconnect();
        }

        public void LoadScene(string sceneName) {
            Debug.Log($"NetworkManagerClient.LoadScene({sceneName})");

            networkController.LoadScene(sceneName);
        }

        public void RequestSpawnPlayerUnit(string sceneName) {
            //Debug.Log($"NetworkManagerClient.RequestSpawnPlayerUnit({sceneName})");

            networkController.RequestSpawnPlayerUnit(sceneName);
        }

        public void RequestRespawnPlayerUnit() {
            networkController.RequestRespawnPlayerUnit();
        }

        public void RequestRevivePlayerUnit() {
            networkController.RequestRevivePlayerUnit();
        }

        public GameObject RequestSpawnModelPrefab(GameObject prefab, Transform parentTransform, Vector3 position, Vector3 forward) {
            //Debug.Log($"NetworkManagerClient.RequestSpawnModelPrefab({prefab.name}, {parentTransform.gameObject.name}, {position}, {forward})");

            return networkController.RequestSpawnModelPrefab(prefab, parentTransform, position, forward);
        }

        public void SendLobbyChatMessage(string messageText) {
            networkController.SendLobbyChatMessage(messageText);
        }

        public void SendLobbyGameChatMessage(string messageText, int gameId) {
            networkController.SendLobbyGameChatMessage(messageText, gameId);
        }

        public void SendSceneChatMessage(string chatMessage) {
            networkController.SendSceneChatMessage(chatMessage);
        }

        public void RequestLobbyPlayerList() {
            networkController.RequestLobbyPlayerList();
        }

        public void ToggleLobbyGameReadyStatus(int gameId) {
            networkController.ToggleLobbyGameReadyStatus(gameId);
        }

        public bool CanSpawnPlayerOverNetwork() {
            return networkController.CanSpawnCharacterOverNetwork();
        }

        public void ProcessStopNetworkUnitClient(UnitController unitController) {
            //if (playerManager.UnitController == unitController) {
                //playerManager.ProcessStopClient();
            //} else {
                characterManager.ProcessStopNetworkUnit(unitController);
            //}
            /*
            if (unitController.UnitControllerMode == UnitControllerMode.Player) {
                playerManagerServer.StopMonitoringPlayerUnit(unitController);
            }
            */
        }

        public void ProcessStopConnection() {
            //Debug.Log($"NetworkManagerClient.ProcessStopConnection()");

            systemGameManager.SetGameMode(GameMode.Local);
            if (levelManager.GetActiveSceneNode() != systemConfigurationManager.MainMenuSceneNode) {
                if (isLoggingInOrOut == false) {
                    uIManager.AddPopupWindowToQueue(uIManager.disconnectedWindow);
                }
                isLoggingInOrOut = false;
                levelManager.LoadMainMenu(false);
                return;
            }

            // don't open disconnected window if this was an expected logout;
            if (isLoggingInOrOut == true) {
                isLoggingInOrOut = false;
                return;
            }
            
            // main menu, close main menu windows and open the disconnected window
            uIManager.newGameWindow.CloseWindow();
            uIManager.loadGameWindow.CloseWindow();
            uIManager.clientLobbyWindow.CloseWindow();
            uIManager.clientLobbyGameWindow.CloseWindow();
            uIManager.createLobbyGameWindow.CloseWindow();
            uIManager.disconnectedWindow.OpenWindow();
        }

        public void ProcessClientVersionFailure(string requiredClientVersion) {
            Debug.Log($"NetworkManagerClient.ProcessClientVersionFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.wrongClientVersionWindow.OpenWindow();
            OnClientVersionFailure(requiredClientVersion);
        }

        public void ProcessAuthenticationFailure() {
            Debug.Log($"NetworkManagerClient.ProcessAuthenticationFailure()");

            uIManager.loginInProgressWindow.CloseWindow();
            uIManager.loginFailedWindow.OpenWindow();
        }

        public void ProcessLoginSuccess(int accountId) {
            //Debug.Log($"NetworkManagerClient.ProcessLoginSuccess({accountId})");

            // not doing this here because the connector has not spawned yet.
            //uIManager.ProcessLoginSuccess();
            this.accountId = accountId;
            isLoggingInOrOut = false;
        }

        public void CreatePlayerCharacter(AnyRPGSaveData anyRPGSaveData) {
            //Debug.Log($"NetworkManagerClient.CreatePlayerCharacterClient(AnyRPGSaveData)");

            networkController.CreatePlayerCharacter(anyRPGSaveData);
        }

        public void RequestLobbyGameList() {
            //Debug.Log($"NetworkManagerClient.RequestLobbyGameList()");

            networkController.RequestLobbyGameList();
        }

        public void LoadCharacterList() {
            //Debug.Log($"NetworkManagerClient.LoadCharacterList()");

            networkController.LoadCharacterList();
        }

        public void DeletePlayerCharacter(int playerCharacterId) {
            Debug.Log($"NetworkManagerClient.DeletePlayerCharacter({playerCharacterId})");

            networkController.DeletePlayerCharacter(playerCharacterId);
        }

        public void RequestCreateLobbyGame(string sceneResourceName, bool allowLateJoin) {
            networkController.RequestCreateLobbyGame(sceneResourceName, allowLateJoin);
        }

        public void AdvertiseCreateLobbyGame(LobbyGame lobbyGame) {
            //Debug.Log($"NetworkManagerClient.AdvertiseCreateLobbyGame({lobbyGame.leaderAccountId}) accountId: {accountId}");

            lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            if (lobbyGame.leaderAccountId == accountId) {
                this.lobbyGame = lobbyGame;
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
            OnCreateLobbyGame(lobbyGame);
        }

        public void SetLobbyGame(LobbyGame lobbyGame) {
            Debug.Log($"NetworkManagerClient.SetLobbyGame({lobbyGame.gameId}) accountId: {accountId}");

            this.lobbyGame = lobbyGame;
        }

        public void CancelLobbyGame(int gameId) {
            networkController.CancelLobbyGame(gameId);
        }

        public void AdvertiseCancelLobbyGame(int gameId) {
            OnCancelLobbyGame(gameId);
        }

        public void JoinLobbyGame(int gameId) {
            networkController.JoinLobbyGame(gameId);
        }

        public void LeaveLobbyGame(int gameId) {
            networkController.LeaveLobbyGame(gameId);
        }

        public void AdvertiseAccountJoinLobbyGame(int gameId, int accountId, string userName) {
            //Debug.Log($"NetworkManagerClient.AdvertiseAccountJoinLobbyGame({gameId}, {accountId}, {userName})");

            lobbyGames[gameId].AddPlayer(accountId, userName);
            OnJoinLobbyGame(gameId, accountId, userName);
            if (accountId == this.accountId) {
                // this client just joined a game
                lobbyGame = lobbyGames[gameId];
                uIManager.clientLobbyGameWindow.OpenWindow();
            }
        }

        public void AdvertiseAccountLeaveLobbyGame(int gameId, int accountId) {
            lobbyGames[gameId].RemovePlayer(accountId);
            OnLeaveLobbyGame(gameId, accountId);
        }

        public void AdvertiseSendLobbyChatMessage(string messageText) {
            OnSendLobbyChatMessage(messageText);
        }

        public void AdvertiseSendLobbyGameChatMessage(string messageText, int gameId) {
            OnSendLobbyGameChatMessage(messageText, gameId);
        }

        public void AdvertiseSendSceneChatMessage(string messageText, int accountId) {
            OnSendSceneChatMessage(messageText, accountId);
            logManager.WriteChatMessageClient(messageText);
        }

        public void AdvertiseLobbyLogin(int accountId, string userName) {
            OnLobbyLogin(accountId, userName);
        }

        public void AdvertiseLobbyLogout(int accountId) {
            OnLobbyLogout(accountId);
        }

        public void SetLobbyGameList(List<LobbyGame> lobbyGames) {
            //Debug.Log($"NetworkManagerClient.SetLobbyGameList({lobbyGames.Count})");

            this.lobbyGames.Clear();
            foreach (LobbyGame lobbyGame in lobbyGames) {
                this.lobbyGames.Add(lobbyGame.gameId, lobbyGame);
            }
            OnSetLobbyGameList(this.lobbyGames.Values.ToList<LobbyGame>());
        }

        public void SetLobbyPlayerList(Dictionary<int, string> lobbyPlayers) {
            this.lobbyPlayers.Clear();
            foreach (int loggedInClientId in lobbyPlayers.Keys) {
                this.lobbyPlayers.Add(loggedInClientId, lobbyPlayers[loggedInClientId]);
            }
            OnSetLobbyPlayerList(lobbyPlayers);
        }

        public void ChooseLobbyGameCharacter(string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            //Debug.Log($"NetworkManagerClient.ChooseLobbyGameCharacter({unitProfileName})");

            networkController.ChooseLobbyGameCharacter(unitProfileName, lobbyGame.gameId, appearanceString, swappableMeshSaveData);
        }

        public void AdvertiseChooseLobbyGameCharacter(int gameId, int accountId, string unitProfileName) {
            //Debug.Log($"NetworkManagerClient.AdvertiseChooseLobbyGameCharacter({gameId}, {accountId}, {unitProfileName})");

            if (lobbyGames.ContainsKey(gameId) == false) {
                Debug.LogWarning($"NetworkManagerClient.AdvertiseChooseLobbyGameCharacter: gameId {gameId} does not exist");
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].unitProfileName = unitProfileName;
            
            OnChooseLobbyGameCharacter(gameId, accountId, unitProfileName);

            if (lobbyGame != null && gameId == lobbyGame.gameId && accountId == this.accountId) {
                // the character was chosen for this client so close the new game window
                uIManager.newGameWindow.CloseWindow();
            }
        }

        public void RequestStartLobbyGame(int gameId) {
            networkController.RequestStartLobbyGame(gameId);
        }

        public void RequestJoinLobbyGameInProgress(int gameId) {
            networkController.RequestJoinLobbyGameInProgress(gameId);
        }

        public void AdvertiseJoinLobbyGameInProgress(int gameId) {
            //Debug.Log($"NetworkManagerClient.AdvertiseJoinLobbyGameInProgress({gameId})");

            if (lobbyGames.ContainsKey(gameId) == false) {
                // lobby game does not exist
                return;
            }

            LaunchLobbyGame(gameId);
        }

        public void AdvertiseStartLobbyGame(int gameId) {
            if (lobbyGames.ContainsKey(gameId) == false) {
                // lobby game does not exist
                return;
            }
            lobbyGames[gameId].inProgress = true;
            OnStartLobbyGame(gameId);

            LaunchLobbyGame(gameId);
        }

        public void LaunchLobbyGame(int gameId) {
            //Debug.Log($"NetworkManagerClient.LaunchLobbyGame({gameId})");

            if (lobbyGame == null || lobbyGame.gameId != gameId) {
                // have not joined lobby game, or joined different lobby game
                return;
            }

            // this is our lobby game
            uIManager.clientLobbyGameWindow.CloseWindow();
            systemItemManager.ClearInstantiatedItems();
            playerManager.SpawnPlayerConnection();
            //levelManager.LoadLevel(sceneName);
            levelManager.ProcessBeforeLevelUnload();
        }

        public void AdvertiseSetLobbyGameReadyStatus(int gameId, int accountId, bool ready) {
            //Debug.Log($"NetworkManagerClient.AdvertiseSetLobbyGameReadyStatus({gameId}, {accountId}, {ready})");

            if (lobbyGames.ContainsKey(gameId) == false || lobbyGames[gameId].PlayerList.ContainsKey(accountId) == false) {
                // game does not exist or player is not in game
                return;
            }
            lobbyGames[gameId].PlayerList[accountId].ready = ready;
            OnSetLobbyGameReadyStatus(gameId, accountId, ready);
        }

        public void AdvertiseLoadSceneClient(string sceneName) {
            //Debug.Log($"NetworkManagerClient.AdvertiseLoadSceneClient({sceneName})");

            levelManager.ProcessBeforeLevelUnload();
        }

        /*
        public void AdvertiseInteractWithQuestGiver(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithQuestGiverClient(interactable, optionIndex, playerManager.UnitController);
        }
        */

        public void InteractWithOption(UnitController sourceUnitController, Interactable targetInteractable, int componentIndex, int choiceIndex) {
            //Debug.Log($"NetworkManagerClient.InteractWithOption({targetInteractable.gameObject.name}, {componentIndex}, {choiceIndex})");

            networkController.InteractWithOption(sourceUnitController, targetInteractable, componentIndex, choiceIndex);
        }

        /*
        public void AdvertiseAddSpawnRequest(SpawnPlayerRequest loadSceneRequest) {
            levelManager.AddSpawnRequest(accountId, loadSceneRequest);
        }
        */

        public void HandleSceneLoadStart(string sceneName) {
            levelManager.NotifyOnBeginLoadingLevel(sceneName);
        }

        public void HandleSceneLoadPercentageChange(float percent) {
            levelManager.SetLoadingProgress(percent);
        }

        /*
        public void AdvertiseInteractWithClassChangeComponent(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithClassChangeComponentClient(interactable, optionIndex);
        }
        */

        public void RequestSetPlayerCharacterClass(Interactable interactable, int componentIndex) {
            networkController.RequestSetPlayerCharacterClass(interactable, componentIndex);
        }

        public void SetPlayerCharacterSpecialization(Interactable interactable, int componentIndex) {
            networkController.SetPlayerCharacterSpecialization(interactable, componentIndex);
        }

        public void RequestSetPlayerFaction(Interactable interactable, int componentIndex) {
            networkController.RequestSetPlayerFaction(interactable, componentIndex);
        }

        /*
        public void AdvertiseInteractWithSkillTrainerComponent(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithSkillTrainerComponentClient(interactable, optionIndex);
        }
        */

        public void RequestLearnSkill(Interactable interactable, int componentIndex, int skillId) {
            networkController.RequestLearnSkill(interactable, componentIndex, skillId);
        }

        public void RequestAcceptQuest(Interactable interactable, int componentIndex, Quest quest) {
            networkController.RequestAcceptQuest(interactable, componentIndex, quest);
        }

        public void RequestCompleteQuest(Interactable interactable, int componentIndex, Quest quest, QuestRewardChoices questRewardChoices) {
            networkController.RequestCompleteQuest(interactable, componentIndex, quest, questRewardChoices);
        }

        public void AdvertiseMessageFeedMessage(string message) {
            messageFeedManager.WriteMessage(message);
        }

        public void AdvertiseSystemMessage(string message) {
            logManager.WriteSystemMessage(message);
        }

        public void SellItemToVendor(Interactable interactable, int componentIndex, int itemInstanceId) {
            networkController.SellVendorItem(interactable, componentIndex, itemInstanceId);
        }

        public void RequestSpawnUnit(Interactable interactable, int componentIndex, int unitLevel, int extraLevels, bool useDynamicLevel, string unitProfileName, string unitToughnessName) {
            Debug.Log($"NetworkManagerClient.RequestSpawnUnit({unitLevel}, {extraLevels}, {useDynamicLevel}, {unitProfileName}, {unitToughnessName})");

            networkController.RequestSpawnUnit(interactable, componentIndex, unitLevel, extraLevels, useDynamicLevel, unitProfileName, unitToughnessName);
        }


        public void AdvertiseAddToBuyBackCollection(UnitController sourceUnitController, Interactable interactable, int componentIndex, int instantiatedItemId) {
            if (systemItemManager.InstantiatedItems.ContainsKey(instantiatedItemId) == false) {
                return;
            }
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                (currentInteractables[componentIndex] as VendorComponent).AddToBuyBackCollection(sourceUnitController, componentIndex, systemItemManager.InstantiatedItems[instantiatedItemId]);
            }

        }

        public void AdvertiseSellItemToPlayerClient(UnitController sourceUnitController, Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName, int remainingQuantity) {
            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables[componentIndex] is VendorComponent) {
                VendorComponent vendorComponent = (currentInteractables[componentIndex] as VendorComponent);
                List<VendorCollection> localVendorCollections = vendorComponent.GetLocalVendorCollections();
                if (localVendorCollections.Count > collectionIndex && localVendorCollections[collectionIndex].VendorItems.Count > itemIndex) {
                    VendorItem vendorItem = localVendorCollections[collectionIndex].VendorItems[itemIndex];
                    if (vendorItem.Item.ResourceName == resourceName) {
                        vendorComponent.ProcessQuantityNotification(vendorItem, remainingQuantity);
                    }
                }
            }
        }

        public void BuyItemFromVendor(Interactable interactable, int componentIndex, int collectionIndex, int itemIndex, string resourceName) {
            networkController.BuyItemFromVendor(interactable, componentIndex, collectionIndex, itemIndex, resourceName);
        }

        public void TakeAllLoot() {
            networkController.TakeAllLoot();
        }

        public void AddDroppedLoot(int lootDropId, int itemId) {
            //Debug.Log($"NetworkManagerClient.AddDroppedLoot({lootDropId}, {itemId})");

            lootManager.AddNetworkLootDrop(lootDropId, itemId);
        }

        public void AddAvailableDroppedLoot(List<int> lootDropIds) {
            Debug.Log($"NetworkManagerClient.AddAvailableDroppedLoot(count: {lootDropIds.Count})");

            lootManager.AddAvailableLoot(accountId, lootDropIds);
        }

        public void AdvertiseTakeLoot(int lootDropId) {
            Debug.Log($"NetworkManagerClient.AdvertiseTakeLoot({lootDropId})");

            lootManager.TakeLoot(accountId, lootDropId);
        }

        public void RequestTakeLoot(int lootDropId) {
            Debug.Log($"NetworkManagerClient.RequestTakeLoot({lootDropId})");

            networkController.RequestTakeLoot(lootDropId);
        }

        /*
        public void SetCraftingManagerAbility(CraftAbility craftAbility) {
            Debug.Log($"NetworkManagerClient.SetCraftingManagerAbility({craftAbility.DisplayName})");

            craftingManager.SetAbility(playerManager.UnitController, craftAbility.CraftAbilityProperties);
        }
        */

        public void RequestBeginCrafting(Recipe recipe, int craftAmount) {
            Debug.Log($"NetworkManagerClient.RequestBeginCrafting({recipe.DisplayName}, {craftAmount})");

            networkController.RequestBeginCrafting(recipe, craftAmount);
        }

        public void RequestCancelCrafting() {
            networkController.RequestCancelCrafting();
        }

        public void RequestUpdatePlayerAppearance(Interactable interactable, int componentIndex, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            networkController.RequestUpdatePlayerAppearance(interactable, componentIndex, unitProfileName, appearanceString, swappableMeshSaveData);
        }

        public void RequestChangePlayerName(Interactable interactable, int componentIndex, string newName) {
            networkController.RequestChangePlayerName(interactable, componentIndex, newName);
        }

        public void RequestSpawnPet(UnitProfile unitProfile) {
            networkController.RequestSpawnPet(unitProfile);
        }

        public void RequestDespawnPet(UnitProfile unitProfile) {
            networkController.RequestDespawnPet(unitProfile);
        }

        public void AdvertiseSpawnPlayerRequest(SpawnPlayerRequest spawnPlayerRequest) {
            //Debug.Log($"NetworkManagerClient.AdvertiseSpawnPlayerRequest()");

            playerManagerServer.AddSpawnRequest(accountId, spawnPlayerRequest);
        }

        public void SetStartTime(DateTime startTime) {
            timeOfDayManagerServer.SetStartTime(startTime);
        }

        public void ProcessStartClientConnector() {
            uIManager.ProcessLoginSuccess();
        }

        public void AdvertiseChooseWeather(WeatherProfile weatherProfile) {
            weatherManagerClient.ChooseWeather(weatherProfile);
        }

        public void AdvertiseEndWeather(WeatherProfile profile, bool immediate) {
            weatherManagerClient.EndWeather(profile, immediate);
        }

        public void AdvertiseStartWeather() {
            weatherManagerClient.StartWeather();
        }

        public void RequestSceneWeather() {
            networkController.RequestSceneWeather();
        }

        public void RequestDespawnPlayer() {
            Debug.Log($"NetworkManagerClient.RequestDespawnPlayer()");
            networkController.RequestDespawnPlayerUnit();
        }

        public void AdvertiseLoadCutscene(Cutscene cutscene) {
            levelManager.LoadCutSceneWithDelay(cutscene);

        }

        public void RequestTurnInDialog(Interactable interactable, int componentIndex, Dialog dialog) {
            networkController.RequestTurnInDialog(interactable, componentIndex, dialog);
        }

        public void RequestTurnInQuestDialog(Dialog dialog) {
            networkController.RequestTurnInQuestDialog(dialog);
        }

        /*
        public void AdvertiseInteractWithAnimatedObjectComponent(Interactable interactable, int optionIndex) {
            interactionManager.InteractWithAnimatedObjectComponentClient(interactable, optionIndex);
        }
        */
    }

    public enum NetworkClientMode { Lobby, MMO }

}