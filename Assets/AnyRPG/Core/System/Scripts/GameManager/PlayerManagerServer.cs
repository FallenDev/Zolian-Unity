using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace AnyRPG {
    public class PlayerManagerServer : ConfiguredMonoBehaviour, ICharacterRequestor {


        /// <summary>
        /// accountId, playerCharacterMonitor
        /// </summary>
        private Dictionary<int, PlayerCharacterMonitor> playerCharacterMonitors = new Dictionary<int, PlayerCharacterMonitor>();

        /// <summary>
        /// accountId, UnitController
        /// </summary>
        private Dictionary<int, UnitController> activePlayers = new Dictionary<int, UnitController>();

        // gameobject, accountId
        private Dictionary<GameObject, int> activePlayerGameObjects = new Dictionary<GameObject, int>();

        /// <summary>
        /// unitController, accountId
        /// </summary>
        private Dictionary<UnitController, int> activePlayerLookup = new Dictionary<UnitController, int>();

        /// <summary>
        /// accountId, LoadSceneRequest pairs for spawn requests
        /// </summary>
        private Dictionary<int, SpawnPlayerRequest> spawnRequests = new Dictionary<int, SpawnPlayerRequest>();

        private string defaultSpawnLocationTag = "DefaultSpawnLocation";

        protected bool eventSubscriptionsInitialized = false;

        private Coroutine monitorPlayerCharactersCoroutine = null;

        // game manager references
        protected SaveManager saveManager = null;
        protected SystemItemManager systemItemManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected LevelManager levelManager = null;
        protected InteractionManager interactionManager = null;
        protected PlayerManager playerManager = null;
        protected SystemAchievementManager systemAchievementManager = null;
        protected QuestGiverManagerClient questGiverManager = null;
        protected MessageFeedManager messageFeedManager = null;
        protected CharacterManager characterManager = null;
        protected SystemEventManager systemEventManager = null;

        public Dictionary<int, PlayerCharacterMonitor> PlayerCharacterMonitors { get => playerCharacterMonitors; }
        public Dictionary<int, UnitController> ActivePlayers { get => activePlayers; }
        public Dictionary<UnitController, int> ActivePlayerLookup { get => activePlayerLookup; }
        public Dictionary<GameObject, int> ActivePlayerGameObjects { get => activePlayerGameObjects; set => activePlayerGameObjects = value; }
        public Dictionary<int, SpawnPlayerRequest> SpawnRequests { get => spawnRequests; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            CreateEventSubscriptions();
            if (monitorPlayerCharactersCoroutine == null) {
                monitorPlayerCharactersCoroutine = StartCoroutine(MonitorPlayerCharacters());
            }

        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            saveManager = systemGameManager.SaveManager;
            systemItemManager = systemGameManager.SystemItemManager;
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            levelManager = systemGameManager.LevelManager;
            interactionManager = systemGameManager.InteractionManager;
            playerManager = systemGameManager.PlayerManager;
            systemAchievementManager = systemGameManager.SystemAchievementManager;
            questGiverManager = systemGameManager.QuestGiverManagerClient;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            characterManager = systemGameManager.CharacterManager;
            systemEventManager = systemGameManager.SystemEventManager;
        }


        private void CreateEventSubscriptions() {
            //Debug.Log("PlayerManager.CreateEventSubscriptions()");
            if (eventSubscriptionsInitialized) {
                return;
            }
            eventSubscriptionsInitialized = true;
        }

        private void CleanupEventSubscriptions() {
            //Debug.Log("PlayerManager.CleanupEventSubscriptions()");
            if (!eventSubscriptionsInitialized) {
                return;
            }
            /*
            systemEventManager.OnLevelUnload -= HandleLevelUnload;
            systemEventManager.OnLevelLoad -= HandleLevelLoad;
            systemEventManager.OnLevelChanged -= PlayLevelUpEffects;
            SystemEventManager.StopListening("OnPlayerDeath", HandlePlayerDeath);
            */
            eventSubscriptionsInitialized = false;
        }

        public void OnDisable() {
            //Debug.Log("PlayerManager.OnDisable()");
            if (SystemGameManager.IsShuttingDown) {
                return;
            }
            CleanupEventSubscriptions();
        }


        private void SavePlayerCharacter(PlayerCharacterMonitor playerCharacterMonitor) {
            networkManagerServer.SavePlayerCharacter(playerCharacterMonitor);
        }

        public void AddActivePlayer(int accountId, UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.AddActivePlayer({accountId}, {unitController.gameObject.name})");

            activePlayers.Add(accountId, unitController);
            activePlayerGameObjects.Add(unitController.gameObject, accountId);
            activePlayerLookup.Add(unitController, accountId);

        }

        public void MonitorPlayer(UnitController unitController) {
            if (activePlayerLookup.ContainsKey(unitController) == false) {
                return;
            }
            SubscribeToPlayerEvents(unitController);

            if (levelManager.SceneDictionary.ContainsKey(unitController.gameObject.scene.name) == false) {
                return;
            }
            SceneNode sceneNode = levelManager.SceneDictionary[unitController.gameObject.scene.name];
            if (sceneNode != null) {
                sceneNode.Visit(unitController);
            }
        }

        public void RemoveActivePlayer(int accountId) {
            //Debug.Log($"PlayerManagerServer.RemoveActivePlayer({accountId})");

            if (ActivePlayers.ContainsKey(accountId) == false) {
                return;
            }
            UnsubscribeFromPlayerEvents(activePlayers[accountId]);
            activePlayerGameObjects.Remove(activePlayers[accountId].gameObject);
            activePlayerLookup.Remove(activePlayers[accountId]);
            activePlayers.Remove(accountId);
        }

        public void SubscribeToPlayerEvents(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.SubscribeToPlayerEvents({unitController.gameObject.name})");

            unitController.UnitEventController.OnKillEvent += HandleKillEvent;
            unitController.UnitEventController.OnEnterInteractableTrigger += HandleEnterInteractableTrigger;
            unitController.UnitEventController.OnExitInteractableTrigger += HandleExitInteractableTrigger;
        }

        public void UnsubscribeFromPlayerEvents(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.UnsubscribeFromPlayerEvents({unitController.gameObject.name})");

            unitController.UnitEventController.OnKillEvent -= HandleKillEvent;
            unitController.UnitEventController.OnEnterInteractableTrigger -= HandleEnterInteractableTrigger;
            unitController.UnitEventController.OnExitInteractableTrigger -= HandleExitInteractableTrigger;
        }

        private void HandleEnterInteractableTrigger(UnitController unitController, Interactable interactable) {
            //Debug.Log($"PlayerManagerServer.HandleEnterInteractableTrigger({unitController.gameObject.name})");

            if (networkManagerServer.ServerModeActive || systemGameManager.GameMode == GameMode.Local) {
                interactionManager.InteractWithTrigger(unitController, interactable);
            }
        }

        private void HandleExitInteractableTrigger(UnitController unitController, Interactable interactable) {
            //Debug.Log($"PlayerManagerServer.HandleEnterInteractableTrigger({unitController.gameObject.name})");

            if (networkManagerServer.ServerModeActive || systemGameManager.GameMode == GameMode.Local) {
                interactionManager.InteractWithTrigger(unitController, interactable);
            }
        }

        public void HandleKillEvent(UnitController unitController, UnitController killedUnitController, float creditPercent) {
            if (creditPercent == 0) {
                return;
            }
            //Debug.Log($"{gameObject.name}: About to gain xp from kill with creditPercent: " + creditPercent);
            GainXP(unitController, (int)(LevelEquations.GetXPAmountForKill(unitController.CharacterStats.Level, killedUnitController, systemConfigurationManager) * creditPercent));
        }

        public void GainXP(int amount, int accountId) {
            if (activePlayers.ContainsKey(accountId) == true) {
                GainXP(activePlayers[accountId], amount);
            }
        }

        public void GainXP(UnitController unitController, int amount) {
            unitController.CharacterStats.GainXP(amount);
        }

        public void AddCurrency(Currency currency, int amount, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            activePlayers[accountId].CharacterCurrencyManager.AddCurrency(currency, amount);

        }

        public void AddItem(string itemName, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }

            InstantiatedItem tmpItem = activePlayers[accountId].CharacterInventoryManager.GetNewInstantiatedItem(itemName);
            if (tmpItem != null) {
                activePlayers[accountId].CharacterInventoryManager.AddItem(tmpItem, false);
            }
        }

        public void BeginAction(AnimatedAction animatedAction, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            activePlayers[accountId].UnitActionManager.BeginAction(animatedAction);

        }

        public void LearnAbility(string abilityName, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            Ability tmpAbility = systemDataFactory.GetResource<Ability>(abilityName);
            if (tmpAbility != null) {
                activePlayers[accountId].CharacterAbilityManager.LearnAbility(tmpAbility.AbilityProperties);
            }

        }

        public void SetLevel(int newLevel, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            CharacterStats characterStats = activePlayers[accountId].CharacterStats;
            newLevel = Mathf.Clamp(newLevel, characterStats.Level, systemConfigurationManager.MaxLevel);
            if (newLevel > characterStats.Level) {
                while (characterStats.Level < newLevel) {
                    characterStats.GainLevel();
                }
            }
        }

        public void LoadScene(string sceneName, UnitController sourceUnitController) {
            //Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {sourceUnitController.gameObject.name})");

            if (activePlayerLookup.ContainsKey(sourceUnitController)) {
                LoadScene(sceneName, activePlayerLookup[sourceUnitController]);
            }
        }

        public void LoadScene(string sceneName, int accountId) {
            //Debug.Log($"PlayerManagerServer.LoadScene({sceneName}, {accountId})");
            
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            //AddSpawnRequest(accountId, new SpawnPlayerRequest());
            if (systemGameManager.GameMode == GameMode.Local) {
                levelManager.LoadLevel(sceneName);
            } else if (networkManagerServer.ServerModeActive) {
                networkManagerServer.AdvertiseLoadScene(sceneName, accountId);
            }
        }

        public void Teleport(UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            StartCoroutine(TeleportDelay(unitController, teleportEffectProperties));
        }


        // delay the teleport by one frame so abilities can finish up without the source character becoming null
        public IEnumerator TeleportDelay (UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            yield return null;
            if (unitController != null) {
                TeleportInternal(unitController, teleportEffectProperties);
            }
        }

        private void TeleportInternal(UnitController unitController, TeleportEffectProperties teleportEffectProperties) {
            //Debug.Log($"PlayerManagerServer.TeleportInternal({unitController.gameObject.name}, {teleportEffectProperties.levelName})");

            if (activePlayerLookup.ContainsKey(unitController) == false) {
                return;
            }
            int accountId = activePlayerLookup[unitController];

            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest();
            if (teleportEffectProperties.overrideSpawnDirection == true) {
                loadSceneRequest.overrideSpawnDirection = true;
                loadSceneRequest.spawnForwardDirection = teleportEffectProperties.spawnForwardDirection;
            }
            if (teleportEffectProperties.overrideSpawnLocation == true) {
                loadSceneRequest.overrideSpawnLocation = true;
                loadSceneRequest.spawnLocation = teleportEffectProperties.spawnLocation;
            }
            if (teleportEffectProperties.locationTag != null && teleportEffectProperties.locationTag != string.Empty) {
                loadSceneRequest.locationTag = teleportEffectProperties.locationTag;
            }
            AddSpawnRequest(accountId, loadSceneRequest);

            // if the scene is already loaded, then just respawn the player
            if (unitController.gameObject.scene.name == teleportEffectProperties.levelName) {
                Debug.Log($"PlayerManagerServer.TeleportInternal({unitController.gameObject.name}, {teleportEffectProperties.levelName}) - already in scene, respawning");

                RespawnPlayerUnit(accountId);
                return;
            }

            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AdvertiseTeleport(accountId, teleportEffectProperties);
                return;
            }

            // local mode active, continue with teleport
            if (teleportEffectProperties.levelName != null) {
                DespawnPlayerUnit(accountId);
                levelManager.LoadLevel(teleportEffectProperties.levelName);
            }
        }

        public void DespawnPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManagerServer.DespawnPlayerUnit({accountId})");

            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            playerCharacterMonitors[accountId].ProcessBeforeDespawn();

            activePlayers[accountId].Despawn(0, false, true);
            RemoveActivePlayer(accountId);
        }

        public void AddSpawnRequest(UnitController unitController, SpawnPlayerRequest loadSceneRequest) {
            if (activePlayerLookup.ContainsKey(unitController)) {
                //if (networkManagerServer.ServerModeActive == true) {
                //    networkManagerServer.AdvertiseAddSpawnRequest(activePlayerLookup[unitController], loadSceneRequest);
                //} else {
                    AddSpawnRequest(activePlayerLookup[unitController], loadSceneRequest);
                //}
            }
        }

        public void SetPlayerCharacterClass(CharacterClass characterClass, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            activePlayers[accountId].BaseCharacter.ChangeCharacterClass(characterClass);
        }

        public void SetPlayerCharacterSpecialization(ClassSpecialization classSpecialization, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            activePlayers[accountId].BaseCharacter.ChangeClassSpecialization(classSpecialization);
        }

        public void SetPlayerFaction(Faction faction, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            activePlayers[accountId].BaseCharacter.ChangeCharacterFaction(faction);
        }


        public void LearnSkill(Skill skill, int accountId) {
            if (activePlayers.ContainsKey(accountId) == false) {
                return;
            }
            activePlayers[accountId].CharacterSkillManager.LearnSkill(skill);
        }

        public void AddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest) {
            //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId})");

            AddSpawnRequest(accountId, loadSceneRequest, true);
        }

        public void AddSpawnRequest(int accountId, SpawnPlayerRequest loadSceneRequest, bool advertise) {
            //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}, {advertise})");

            if (spawnRequests.ContainsKey(accountId)) {
                //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}, {advertise}) replacing request spawn location: {loadSceneRequest.spawnLocation}, forward direction: {loadSceneRequest.spawnForwardDirection}");
                spawnRequests[accountId] = loadSceneRequest;
            } else {
                //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}, {advertise}) adding new request spawn location: {loadSceneRequest.spawnLocation}, forward direction: {loadSceneRequest.spawnForwardDirection}");
                spawnRequests.Add(accountId, loadSceneRequest);
            }
            if (networkManagerServer.ServerModeActive == true && advertise == true) {
                networkManagerServer.AdvertiseAddSpawnRequest(accountId, loadSceneRequest);
            } else {
                //Debug.Log($"PlayerManagerServer.AddSpawnRequest({accountId}) not in server mode, not advertising");
            }
        }

        public void RemoveSpawnRequest(int accountId) {
            //Debug.Log($"PlayerManagerServer.RemoveSpawnRequest({accountId})");

            spawnRequests.Remove(accountId);
        }
        public SpawnPlayerRequest GetSpawnPlayerRequest(int accountId, string sceneName) {
            return GetSpawnPlayerRequest(accountId, sceneName, false);
        }

        public SpawnPlayerRequest GetSpawnPlayerRequest(int accountId, string sceneName, bool keepRequest) {
            //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}, {sceneName})");

            SpawnPlayerRequest inputLoadSceneRequest = null;
            SpawnPlayerRequest outputLoadSceneRequest = new SpawnPlayerRequest();

            if (spawnRequests.ContainsKey(accountId)) {
                //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}) found request");
                inputLoadSceneRequest = spawnRequests[accountId];
            } else {
                //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}) making new request");
                inputLoadSceneRequest = new SpawnPlayerRequest();
            }
            outputLoadSceneRequest.overrideSpawnLocation = inputLoadSceneRequest.overrideSpawnLocation;
            outputLoadSceneRequest.overrideSpawnDirection = inputLoadSceneRequest.overrideSpawnDirection;
            outputLoadSceneRequest.xOffset = inputLoadSceneRequest.xOffset;
            outputLoadSceneRequest.zOffset = inputLoadSceneRequest.zOffset;

            Scene accountScene;
            if (networkManagerServer.ServerModeActive == true ) {
                accountScene = networkManagerServer.GetAccountScene(accountId, sceneName);
            } else {
                accountScene = SceneManager.GetSceneByName(sceneName);
            }
            if (inputLoadSceneRequest.overrideSpawnLocation == true) {
                //Debug.Log("Levelmanager.GetSpawnLocation(). SpawnLocationOverride is set.  returning " + spawnLocationOverride);
                outputLoadSceneRequest.spawnLocation = inputLoadSceneRequest.spawnLocation;
            } else {
                GameObject spawnLocationMarker = null;
                if (inputLoadSceneRequest.locationTag != string.Empty) {
                    spawnLocationMarker = GetSceneObjectByTag(inputLoadSceneRequest.locationTag, accountScene);
                    if (spawnLocationMarker != null) {
                        outputLoadSceneRequest.spawnLocation = spawnLocationMarker.transform.position;
                        outputLoadSceneRequest.spawnForwardDirection = spawnLocationMarker.transform.forward;
                    }
                }
                if (spawnLocationMarker == null) {
                    spawnLocationMarker = GetSceneObjectByTag(defaultSpawnLocationTag, accountScene);
                    if (spawnLocationMarker != null) {
                        outputLoadSceneRequest.spawnLocation = spawnLocationMarker.transform.position;
                        outputLoadSceneRequest.spawnForwardDirection = spawnLocationMarker.transform.forward;
                    }
                }
            }

            if (inputLoadSceneRequest.overrideSpawnDirection == true) {
                outputLoadSceneRequest.spawnForwardDirection = inputLoadSceneRequest.spawnForwardDirection;
            }

            if (keepRequest == false) {
                RemoveSpawnRequest(accountId);
            }

            // debug print the spawn location
            //Debug.Log($"PlayerManagerServer.GetSpawnPlayerRequest({accountId}, {sceneName}) spawn location: {outputLoadSceneRequest.spawnLocation}, forward direction: {outputLoadSceneRequest.spawnForwardDirection}");

            return outputLoadSceneRequest;
        }

        public GameObject GetSceneObjectByTag(string locationTag, Scene scene) {
            //Debug.Log($"PlayerManagerServer.GetSpawnLocationMarker({locationTag})");

            List<GameObject> spawnLocationMarkers = new List<GameObject>();
            spawnLocationMarkers = GameObject.FindGameObjectsWithTag(locationTag).ToList();
            foreach (GameObject spawnLocationMarker in spawnLocationMarkers) {
                if (spawnLocationMarker.scene.handle == scene.handle) {
                    return spawnLocationMarker;
                }
            }
            return null;
        }

        public void RequestSpawnPlayerUnit(int accountId, string sceneName) {
            //Debug.Log($"PlayerManagerServer.RequestSpawnPlayerUnit({accountId}, {sceneName})");

            SpawnPlayerRequest spawnPlayerRequest = GetSpawnPlayerRequest(accountId, sceneName);

            if (systemGameManager.GameMode == GameMode.Local) {
                // load local player
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(systemDataFactory, playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
                CharacterRequestData characterRequestData = new CharacterRequestData(playerManager,
                    systemGameManager.GameMode,
                    characterConfigurationRequest);
                characterRequestData.isOwner = true;
                characterRequestData.saveData = playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData;
                UnitController unitController = characterManager.SpawnCharacterPrefab(characterRequestData, null, spawnPlayerRequest.spawnLocation, spawnPlayerRequest.spawnForwardDirection);
                playerCharacterMonitors[accountId].SetUnitController(unitController);
            } else {
                CharacterConfigurationRequest characterConfigurationRequest = new CharacterConfigurationRequest(systemDataFactory, playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData);
                characterConfigurationRequest.unitControllerMode = UnitControllerMode.Player;
                CharacterRequestData characterRequestData = new CharacterRequestData(this, GameMode.Network, characterConfigurationRequest);
                characterRequestData.isServer = true;
                characterRequestData.saveData = playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData;

                if (spawnPlayerRequest.overrideSpawnLocation == false) {
                    // we were loading the default location, so randomize the spawn position a bit so players don't all spawn in the same place
                    spawnPlayerRequest.spawnLocation = new Vector3(spawnPlayerRequest.spawnLocation.x + spawnPlayerRequest.xOffset, spawnPlayerRequest.spawnLocation.y, spawnPlayerRequest.spawnLocation.z + spawnPlayerRequest.zOffset);
                }
                networkManagerServer.SpawnPlayer(accountId, characterRequestData, spawnPlayerRequest.spawnLocation, spawnPlayerRequest.spawnForwardDirection, sceneName);
            }
        }

        public IEnumerator MonitorPlayerCharacters() {
            while (systemGameManager.GameMode == GameMode.Network) {
                foreach (PlayerCharacterMonitor playerCharacterMonitor in playerCharacterMonitors.Values) {
                    if (playerCharacterMonitor.unitController != null) {
                        SavePlayerCharacter(playerCharacterMonitor);
                    }
                }
                yield return new WaitForSeconds(10);
            }
        }

        public void AddPlayerMonitor(int accountId, PlayerCharacterSaveData playerCharacterSaveData) {

            if (playerCharacterMonitors.ContainsKey(accountId)) {
                return;
            } else {
                PlayerCharacterMonitor playerCharacterMonitor = new PlayerCharacterMonitor(
                    systemGameManager,
                    accountId,
                    playerCharacterSaveData,
                    null
                );
                playerCharacterMonitors.Add(accountId, playerCharacterMonitor);

                // this should not be needed.  Check for breakage before deleting.
                //AddSpawnRequest(accountId, new SpawnPlayerRequest());
            }
        }

        public void MonitorPlayerUnit(int accountId, UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.MonitorPlayerUnit({accountId}, {unitController.gameObject.name})");

            if (playerCharacterMonitors.ContainsKey(accountId) == false) {
                return;
            }
            playerCharacterMonitors[accountId].SetUnitController(unitController);
            AddActivePlayer(accountId, unitController);
        }

        /*
        public void StopMonitoringPlayerUnit(UnitController unitController) {
            Debug.Log($"PlayerManagerServer.StopMonitoringPlayerUnit({unitController.gameObject.name})");

            if (activePlayerLookup.ContainsKey(unitController)) {
                StopMonitoringPlayerUnit(activePlayerLookup[unitController]);
            }
        }
        */

        public void StopMonitoringPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManagerServer.StopMonitoringPlayerUnit({accountId})");

            if (playerCharacterMonitors.ContainsKey(accountId)) {
                PauseMonitoringPlayerUnit(accountId);

                //activePlayerCharactersByAccount.Remove(activePlayerCharacters[playerCharacterId].accountId);
                playerCharacterMonitors.Remove(accountId);
            }
        }

        public void ProcessDisconnect(int accountId) {
            //Debug.Log($"PlayerManagerServer.ProcessDisconnect({accountId})");

            if (playerCharacterMonitors.ContainsKey(accountId) && playerCharacterMonitors[accountId].unitController != null) {
                playerCharacterMonitors[accountId].SetDisconnected();
            }
            PauseMonitoringPlayerUnit(accountId);
        }

        public void PauseMonitoringPlayerUnit(int accountId) {
            //Debug.Log($"PlayerManagerServer.PauseMonitoringPlayerUnit({accountId})");

            //if (playerCharacterMonitors.ContainsKey(accountId) && playerCharacterMonitors[accountId].unitController != null) {
                RemoveActivePlayer(accountId);

                // flush data to database before stop monitoring
                if (systemGameManager.GameMode == GameMode.Network && playerCharacterMonitors.ContainsKey(accountId) == true) {
                    SavePlayerCharacter(playerCharacterMonitors[accountId]);
                }
            //}
        }

        public void RevivePlayerUnit(int accountId) {

            // get lobby game id, unitprofile, and scene name from the player character save data
            if (activePlayers.ContainsKey(accountId) == false) {
                //Debug.LogError($"PlayerManagerServer.RequestRespawnPlayerUnit: activePlayerCharacters does not contain accountId {accountId}");
                return;
            }
            activePlayers[accountId].CharacterStats.StatusEffectRevive();
        }

        public void RespawnPlayerUnit(int accountId) {
            
            // get lobby game id, unitprofile, and scene name from the player character save data
            if (playerCharacterMonitors.ContainsKey(accountId) == false) {
                //Debug.LogError($"PlayerManagerServer.RequestRespawnPlayerUnit: activePlayerCharacters does not contain accountId {accountId}");
                return;
            }
            string sceneName = playerCharacterMonitors[accountId].unitController.gameObject.scene.name;

            DespawnPlayerUnit(accountId);
            playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData.isDead = false;
            playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData.initializeResourceAmounts = true;
            RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void UpdatePlayerAppearance(int accountId, string unitProfileName, string appearanceString, List<SwappableMeshSaveData> swappableMeshSaveData) {
            Debug.Log($"PlayerManagerServer.UpdatePlayerAppearance({accountId}, {unitProfileName},swappableMeshSaveData.Count: {swappableMeshSaveData?.Count})");

            // Always despawn units if their appearance changes.
            SpawnPlayerRequest loadSceneRequest = new SpawnPlayerRequest() {
                overrideSpawnDirection = true,
                spawnForwardDirection = playerCharacterMonitors[accountId].unitController.transform.forward,
                overrideSpawnLocation = true,
                spawnLocation = playerCharacterMonitors[accountId].unitController.transform.position
            };
            string sceneName = playerCharacterMonitors[accountId].unitController.gameObject.scene.name;
            AddSpawnRequest(accountId, loadSceneRequest);
            DespawnPlayerUnit(accountId);
            playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData.appearanceString = appearanceString;
            playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData.swappableMeshSaveData = swappableMeshSaveData;
            playerCharacterMonitors[accountId].playerCharacterSaveData.SaveData.unitProfileName = unitProfileName;
            RequestSpawnPlayerUnit(accountId, sceneName);
        }

        public void ConfigureSpawnedCharacter(UnitController unitController) {
        }

        public void PostInit(UnitController unitController) {
            //Debug.Log($"PlayerManagerServer.PostInit({unitController.gameObject.name}, account: {unitController.CharacterRequestData.accountId})");

            /*
            // load player data from the active player characters dictionary
            if (!playerCharacterMonitors.ContainsKey(unitController.CharacterRequestData.accountId)) {
                //Debug.LogError($"PlayerManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            */
        }

        public void RequestSpawnPet(int accountId, UnitProfile unitProfile) {
            //Debug.Log($"PlayerManagerServer.RequestSpawnPet({accountId}, {unitProfile.ResourceName})");

            if (!playerCharacterMonitors.ContainsKey(accountId)) {
                //Debug.LogError($"NetworkManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            SpawnPet(playerCharacterMonitors[accountId].unitController, unitProfile);
        }

        public void SpawnPet(UnitController unitController, UnitProfile unitProfile) {
            //Debug.Log($"PlayerManagerServer.SpawnPet({unitController.gameObject.name}, {unitProfile.ResourceName})");

            unitController.CharacterPetManager.SpawnPet(unitProfile);
        }

        public void RequestDespawnPet(int accountId, UnitProfile unitProfile) {

            if (!playerCharacterMonitors.ContainsKey(accountId)) {
                //Debug.LogError($"NetworkManagerServer.PostInit: activePlayerCharacters does not contain accountId {characterRequestData.accountId}");
                return;
            }
            DespawnPet(playerCharacterMonitors[accountId].unitController, unitProfile);
        }

        public void DespawnPet(UnitController unitController, UnitProfile unitProfile) {
            unitController.CharacterPetManager.DespawnPet(unitProfile);
        }

        public void RequestSpawnRequest(int accountId) {
            //Debug.Log($"PlayerManagerServer.RequestSpawnRequest({accountId})");

            if (spawnRequests.ContainsKey(accountId)) {
                networkManagerServer.AdvertiseAddSpawnRequest(accountId, spawnRequests[accountId]);
            }
        }

        public void LoadCutscene(Cutscene cutscene, UnitController sourceUnitController) {
            //Debug.Log($"PlayerManagerServer.LoadCutscene({cutscene.ResourceName}, {sourceUnitController?.gameObject.name})");

            if (activePlayerLookup.ContainsKey(sourceUnitController)) {
                int accountId = activePlayerLookup[sourceUnitController];
                SpawnPlayerRequest spawnPlayerRequest = new SpawnPlayerRequest() {
                    overrideSpawnDirection = true,
                    spawnForwardDirection = sourceUnitController.transform.forward,
                    overrideSpawnLocation = true,
                    spawnLocation = sourceUnitController.transform.position
                };
                // debug print the spawn location
                //Debug.Log($"PlayerManagerServer.LoadCutscene({cutscene.ResourceName}, {accountId}) spawn location: {spawnPlayerRequest.spawnLocation}, forward direction: {spawnPlayerRequest.spawnForwardDirection}");
                DespawnPlayerUnit(accountId);
                AddSpawnRequest(accountId, spawnPlayerRequest, true);
                if (networkManagerServer.ServerModeActive) {
                    networkManagerServer.AdvertiseLoadCutscene(cutscene, accountId);
                } else {
                    // local mode active, continue with cutscene
                    levelManager.LoadCutSceneWithDelay(cutscene);
                }
            }
        }
    }

}