using FishNet.Component.Transforming;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace AnyRPG {
    public class FishNetInteractable : SpawnedNetworkObject {

        private Interactable interactable = null;

        private bool eventRegistrationComplete = false;

        // game manager references
        protected SystemGameManager systemGameManager = null;
        protected SystemDataFactory systemDataFactory = null;
        protected NetworkManagerServer networkManagerServer = null;
        protected SystemItemManager systemItemManager = null;

        public Interactable Interactable { get => interactable; }

        protected virtual void Awake() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.Awake() position: { gameObject.transform.position}");
        }

        protected virtual void Configure() {
            systemGameManager = GameObject.FindAnyObjectByType<SystemGameManager>();
            systemDataFactory = systemGameManager.SystemDataFactory;
            networkManagerServer = systemGameManager.NetworkManagerServer;
            systemItemManager = systemGameManager.SystemItemManager;
            
            interactable = GetComponent<Interactable>();
        }

        public override void OnStartClient() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStartClient()");

            base.OnStartClient();

            Configure();
            if (systemGameManager == null) {
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            List<AutoConfiguredMonoBehaviour> autoConfiguredMonoBehaviours = GetComponents<AutoConfiguredMonoBehaviour>().ToList();
            foreach (AutoConfiguredMonoBehaviour autoConfiguredMonoBehaviour in autoConfiguredMonoBehaviours) {
               autoConfiguredMonoBehaviour.AutoConfigure(systemGameManager);
            }
            //interactable.AutoConfigure(systemGameManager);

            SubscribeToClientInteractableEvents();
        }

        public override void OnStopClient() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStopClient()");

            base.OnStopClient();
            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }

            UnsubscribeFromClientInteractableEvents();
            //systemGameManager.NetworkManagerClient.ProcessStopClient(unitController);
        }

        public override void OnStartServer() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStartServer()");

            base.OnStartServer();

            Configure();
            if (systemGameManager == null) {
                Debug.Log($"{gameObject.name}.NetworkInteractable.OnStartServer(): systemGameManager is null");
                return;
            }

            // network objects will not be active on clients when the autoconfigure runs, so they must configure themselves
            //interactable.AutoConfigure(systemGameManager);

            SubscribeToServerInteractableEvents();
        }

        public override void OnStopServer() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.OnStopServer()");

            base.OnStopServer();

            if (SystemGameManager.IsShuttingDown == true) {
                return;
            }
            //UnsubscribeFromServerInteractableEvents();
            //systemGameManager.NetworkManagerServer.ProcessStopServer(unitController);
        }

        public void SubscribeToServerInteractableEvents() {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.SubscribeToServerInteractableEvents()");

            if (eventRegistrationComplete == true) {
                Debug.Log($"{gameObject.name}.NetworkInteractable.SubscribeToServerInteractableEvents(): already registered");
                return;
            }

            if (interactable == null) {
                Debug.Log($"{gameObject.name}.NetworkInteractable.SubscribeToServerInteractableEvents(): interactable is null");
                // something went wrong
                return;
            }

            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement += HandleAnimatedObjectChooseMovementServer;
            interactable.OnInteractionWithOptionStarted += HandleInteractionWithOptionStarted;
            interactable.InteractableEventController.OnPlayDialogNode += HandlePlayDialogNode;
            interactable.OnInteractableDisable += HandleInteractableDisableServer;
            interactable.InteractableEventController.OnDropLoot += HandleDropLoot;
            interactable.InteractableEventController.OnRemoveDroppedItem += HandleRemoveDroppedItem;
            interactable.InteractableEventController.OnStopMovementSound += HandleStopMovementSound;
            interactable.InteractableEventController.OnPlayMovementSound += HandlePlayMovementSound;
            interactable.InteractableEventController.OnStopCastSound += HandleStopCastSound;
            interactable.InteractableEventController.OnPlayCastSound += HandlePlayCastSound;
            interactable.InteractableEventController.OnStopEffectSound += HandleStopEffectSound;
            interactable.InteractableEventController.OnPlayEffectSound += HandlePlayEffectSound;
            interactable.InteractableEventController.OnStopVoiceSound += HandleStopVoiceSound;
            interactable.InteractableEventController.OnPlayVoiceSound += HandlePlayVoiceSound;


            eventRegistrationComplete = true;
        }

        public void HandleInteractableDisableServer() {
            UnsubscribeFromServerInteractableEvents();
        }

        public void UnsubscribeFromServerInteractableEvents() {
            if (interactable == null) {
                return;
            }
            if (eventRegistrationComplete == false) {
                //Debug.Log($"{gameObject.name}.NetworkInteractable.UnsubscribeFromServerInteractableEvents(): not registered");
                return;
            }
            //interactable.InteractableEventController.OnAnimatedObjectChooseMovement -= HandleAnimatedObjectChooseMovementServer;
            interactable.OnInteractionWithOptionStarted -= HandleInteractionWithOptionStarted;
            interactable.InteractableEventController.OnPlayDialogNode -= HandlePlayDialogNode;
            interactable.OnInteractableDisable -= HandleInteractableDisableServer;
            interactable.InteractableEventController.OnDropLoot -= HandleDropLoot;
            interactable.InteractableEventController.OnRemoveDroppedItem -= HandleRemoveDroppedItem;
            interactable.InteractableEventController.OnStopMovementSound -= HandleStopMovementSound;
            interactable.InteractableEventController.OnPlayMovementSound -= HandlePlayMovementSound;
            interactable.InteractableEventController.OnStopCastSound -= HandleStopCastSound;
            interactable.InteractableEventController.OnPlayCastSound -= HandlePlayCastSound;
            interactable.InteractableEventController.OnStopEffectSound -= HandleStopEffectSound;
            interactable.InteractableEventController.OnPlayEffectSound -= HandlePlayEffectSound;
            interactable.InteractableEventController.OnStopVoiceSound -= HandleStopVoiceSound;
            interactable.InteractableEventController.OnPlayVoiceSound -= HandlePlayVoiceSound;

            eventRegistrationComplete = false;
        }

        private void HandlePlayVoiceSound(AudioClip clip) {
            HandlePlayVoiceSoundClient(clip.name);
        }

        [ObserversRpc]
        public void HandlePlayVoiceSoundClient(string clipName) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayVoiceSound(clip);
            }
        }

        private void HandleStopVoiceSound() {
            HandleStopVoiceSoundClient();
        }

        [ObserversRpc]
        public void HandleStopVoiceSoundClient() {
            interactable.UnitComponentController.StopVoiceSound();
        }

        private void HandlePlayEffectSound(AudioClip clip, bool loop) {
            HandlePlayEffectSoundClient(clip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayEffectSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayEffectSound(clip, loop);
            }
        }

        private void HandleStopEffectSound() {
            HandleStopEffectSoundClient();
        }

        [ObserversRpc]
        public void HandleStopEffectSoundClient() {
            interactable.UnitComponentController.StopEffectSound();
        }

        private void HandlePlayCastSound(AudioClip clip, bool loop) {
            HandlePlayCastSoundClient(clip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayCastSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayCastSound(clip, loop);
            }
        }

        private void HandleStopCastSound() {
            HandleStopCastSoundClient();
        }

        [ObserversRpc]
        public void HandleStopCastSoundClient() {
            interactable.UnitComponentController.StopCastSound();
        }

        private void HandlePlayMovementSound(AudioClip clip, bool loop) {
            HandlePlayMovementSoundClient(clip.name, loop);
        }

        [ObserversRpc]
        public void HandlePlayMovementSoundClient(string clipName, bool loop) {
            AudioClip clip = systemGameManager.AudioManager.GetAudioClip(clipName);
            if (clip != null) {
                interactable.UnitComponentController.PlayMovementSound(clip, loop);
            }
        }

        private void HandleStopMovementSound(bool stopLoops) {
            HandleStopMovementSoundClient(stopLoops);
        }

        [ObserversRpc]
        public void HandleStopMovementSoundClient(bool stopLoops) {
            interactable.UnitComponentController.StopMovementSound(stopLoops);
        }

        public void SubscribeToClientInteractableEvents() {
            if (interactable == null) {
                // something went wrong
                return;
            }

            //unitController.UnitEventController.OnBeginChatMessage += HandleBeginChatMessageServer;
        }

        public void UnsubscribeFromClientInteractableEvents() {
            if (interactable == null) {
                return;
            }
            //unitController.UnitEventController.OnBeginChatMessage -= HandleBeginChatMessageServer;
        }

        [ObserversRpc]
        public void HandlePlayDialogNode(string dialogName, int dialogIndex) {
            Debug.Log($"{gameObject.name}.NetworkInteractable.HandlePlayDialogNode({dialogName}, {dialogIndex})");
            interactable.DialogController.PlayDialogNode(dialogName, dialogIndex);
        }


        /*
        public void HandleAnimatedObjectChooseMovementServer(UnitController sourceUnitController, int optionIndex) {
            
            NetworkCharacterUnit targetNetworkCharacterUnit = null;
            if (sourceUnitController != null) {
                targetNetworkCharacterUnit = sourceUnitController.GetComponent<NetworkCharacterUnit>();
            }
            HandleAnimatedObjectChooseMovementClient(targetNetworkCharacterUnit, optionIndex);
        }

        [ObserversRpc]
        public void HandleAnimatedObjectChooseMovementClient(NetworkCharacterUnit sourceNetworkCharacterUnit, int optionIndex) {
            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit != null) {
                sourceUnitController = sourceNetworkCharacterUnit.UnitController;
            }

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetCurrentInteractables(sourceUnitController);
            if (currentInteractables.ContainsKey(optionIndex)) {
                if (currentInteractables[optionIndex] is AnimatedObjectComponent) {
                    (currentInteractables[optionIndex] as AnimatedObjectComponent).ChooseMovement(sourceUnitController, optionIndex);
                }
            }
        }
        */

        public void HandleInteractionWithOptionStarted(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.HandleInteractionWithOptionStarted({(sourceUnitController == null ? "null" : sourceUnitController.gameObject.name)}, {componentIndex}, {choiceIndex})");

            FishNetUnitController targetNetworkCharacterUnit = null;
            if (sourceUnitController != null) {
                targetNetworkCharacterUnit = sourceUnitController.GetComponent<FishNetUnitController>();
            }
            HandleInteractionWithOptionStartedClient(targetNetworkCharacterUnit, componentIndex, choiceIndex);
        }

        /// <summary>
        /// this triggers an event that results in ClientInteract() on players on their own clients
        /// </summary>
        /// <param name="sourceNetworkCharacterUnit"></param>
        /// <param name="componentIndex"></param>
        /// <param name="choiceIndex"></param>
        [ObserversRpc]
        public void HandleInteractionWithOptionStartedClient(FishNetUnitController sourceNetworkCharacterUnit, int componentIndex, int choiceIndex) {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.HandleInteractionWithOptionStartedClient({(sourceNetworkCharacterUnit == null ? "null" : sourceNetworkCharacterUnit.gameObject.name)}, {componentIndex}, {choiceIndex})");

            UnitController sourceUnitController = null;
            if (sourceNetworkCharacterUnit == null) {
                return;
            }
            sourceUnitController = sourceNetworkCharacterUnit.UnitController;

            Dictionary<int, InteractableOptionComponent> currentInteractables = interactable.GetSwitchInteractables(sourceUnitController);
            if (currentInteractables.ContainsKey(componentIndex)) {
                //currentInteractables[componentIndex].ClientInteract(sourceUnitController, componentIndex, choiceIndex);
                sourceUnitController.UnitEventController.NotifyOnStartInteractWithOption(currentInteractables[componentIndex], componentIndex, choiceIndex);
            }
            
        }

        private void HandleDropLoot(Dictionary<int, List<int>> lootDropIdLookup) {
            foreach (KeyValuePair<int, List<int>> kvp in lootDropIdLookup) {
                int accountId = kvp.Key;
                List<int> lootDropIds = kvp.Value;
                Dictionary<int, List<int>> targetLootDropIdLookup = new Dictionary<int, List<int>>();
                targetLootDropIdLookup.Add(accountId, lootDropIds);
                if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) && base.NetworkManager.ServerManager.Clients.ContainsKey(networkManagerServer.LoggedInAccounts[accountId].clientId)) {
                    NetworkConnection networkConnection = base.NetworkManager.ServerManager.Clients[networkManagerServer.LoggedInAccounts[accountId].clientId];
                    HandleDropLootTarget(networkConnection, targetLootDropIdLookup);
                }
            }
        }

        [TargetRpc]
        public void HandleDropLootTarget(NetworkConnection networkConnection, Dictionary<int, List<int>> lootDropIdLookup) {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.HandleDropLootTarget()");
            if (interactable == null) {
                Debug.Log($"{gameObject.name}.NetworkInteractable.HandleDropLootTarget(): interactable is null");
                return;
            }
            interactable.InteractableEventController.NotifyOnDropLoot(lootDropIdLookup);
        }

        private void HandleRemoveDroppedItem(int lootDropId, int accountId) {
            if (networkManagerServer.LoggedInAccounts.ContainsKey(accountId) && base.NetworkManager.ServerManager.Clients.ContainsKey(networkManagerServer.LoggedInAccounts[accountId].clientId)) {
                NetworkConnection networkConnection = base.NetworkManager.ServerManager.Clients[networkManagerServer.LoggedInAccounts[accountId].clientId];
                HandleRemoveDroppedItemTarget(networkConnection, lootDropId, accountId);
            }
        }

        [TargetRpc]
        public void HandleRemoveDroppedItemTarget(NetworkConnection networkConnection, int lootDropId, int accountId) {
            //Debug.Log($"{gameObject.name}.NetworkInteractable.HandleRemoveDroppedItemTarget({lootDropId}, {accountId})");
            if (interactable == null) {
                Debug.Log($"{gameObject.name}.NetworkInteractable.HandleRemoveDroppedItemTarget(): interactable is null");
                return;
            }
            interactable.InteractableEventController.NotifyOnRemoveDroppedItemClient(lootDropId, accountId);
        }

    }
}

