using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEventManager {

        private static Dictionary<string, Action<string, EventParamProperties>> singleEventDictionary = new Dictionary<string, Action<string, EventParamProperties>>();

        public event System.Action<UnitController> OnPlayerUnitSpawn = delegate { };
        public event System.Action<UnitController> OnPlayerUnitDespawn = delegate { };
        public event System.Action<UnitController, AbilityProperties> OnAbilityListChanged = delegate { };
        public event System.Action<UnitController, int> OnLevelChanged = delegate { };
        public event System.Action<UnitController, CharacterClass, CharacterClass> OnClassChange = delegate { };
        public event System.Action<UnitController, ClassSpecialization, ClassSpecialization> OnSpecializationChange = delegate { };
        public event System.Action<UnitController, Item> OnItemCountChanged = delegate { };
        public event System.Action<IAbilityCaster, UnitController, int, string> OnTakeDamage = delegate { };
        public event System.Action<UnitController> OnReputationChange = delegate { };
        public event System.Action<UnitController, QuestBase> OnAcceptQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnRemoveQuest = delegate { };
        public event System.Action<UnitController, QuestBase> OnMarkQuestComplete = delegate { };
        public event System.Action<UnitController, Quest> OnQuestObjectiveStatusUpdated = delegate { };
        public event System.Action<UnitController, Achievement> OnAchievementObjectiveStatusUpdated = delegate { };
        public event System.Action<UnitController, Skill> OnLearnSkill = delegate { };
        public event System.Action<UnitController, Skill> OnUnLearnSkill = delegate { };
        public event System.Action<UnitController, CraftAbilityProperties> OnSetCraftAbility = delegate { };
        public event System.Action OnCraftItem = delegate { };
        public event System.Action OnAddBag = delegate { };
        public event System.Action OnCurrencyChange = delegate { };
        public event System.Action<IUseable, int> OnSetGamepadActionButton = delegate { };
        public event System.Action<int> OnUnsetGamepadActionButton = delegate { };
        public event System.Action<IUseable, int> OnSetMouseActionButton = delegate { };
        public event System.Action<int> OnUnsetMouseActionButton = delegate { };
        public event System.Action<UnitProfile> OnRemoveActivePet = delegate { };
        public event System.Action<int> OnTakeLoot = delegate { };
        public event System.Action OnPlayerDeath = delegate { };
        public event System.Action OnCalculateRelativeTime = delegate { };
        public event System.Action OnStartServer = delegate { };
        public event System.Action OnStopServer = delegate { };
        public event System.Action OnLevelLoad = delegate { };
        public event System.Action<int, string> OnLevelUnload = delegate { };
        public event System.Action<int, string> OnRemoveLoadedScene = delegate { };
        public event System.Action<int, string> OnAddLoadedScene = delegate { };
        public event System.Action<int, WeatherProfile, bool> OnEndWeather = delegate { };
        public event System.Action<int, WeatherProfile> OnChooseWeather = delegate { };
        public event System.Action<int> OnStartWeather = delegate { };

        // equipment manager
        public System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnAddEquipment = delegate { };
        public System.Action<EquipmentSlotProfile, InstantiatedEquipment> OnRemoveEquipment = delegate { };

        public static void StartListening(string eventName, Action<string, EventParamProperties> listener) {
            Action<string, EventParamProperties> thisEvent;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {

                //Add more event to the existing one
                thisEvent += listener;

                //Update the Dictionary
                singleEventDictionary[eventName] = thisEvent;
            } else {
                //Add event to the Dictionary for the first time
                thisEvent += listener;

                singleEventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action<string, EventParamProperties> listener) {

            Action<string, EventParamProperties> thisEvent;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {

                //Remove event from the existing one
                thisEvent -= listener;

                //Update the Dictionary
                singleEventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, EventParamProperties eventParam) {
            Action<string, EventParamProperties> thisEvent = null;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {
                if (thisEvent != null) {
                    thisEvent.Invoke(eventName, eventParam);
                }
                // OR USE  instance.eventDictionary[eventName](eventParam);
            }
        }

        public void NotifyOnReputationChange(UnitController sourceUnitController) {
            OnReputationChange(sourceUnitController);
        }

        public void NotifyOnPlayerUnitSpawn(UnitController unitController) {
            OnPlayerUnitSpawn(unitController);
        }

        public void NotifyOnPlayerUnitDespawn(UnitController unitController) {
            OnPlayerUnitDespawn(unitController);
        }

        public void NotifyOnAddEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            OnAddEquipment(profile, equipment);
        }

        public void NotifyOnRemoveEquipment(EquipmentSlotProfile profile, InstantiatedEquipment equipment) {
            OnRemoveEquipment(profile, equipment);
        }

        public void NotifyOnClassChange(UnitController sourceUnitController, CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(sourceUnitController, newCharacterClass, oldCharacterClass);
        }

        public void NotifyOnSpecializationChange(UnitController sourceUnitController, ClassSpecialization newClassSpecialization, ClassSpecialization oldClassSpecialization) {
            OnSpecializationChange(sourceUnitController, newClassSpecialization, oldClassSpecialization);
        }

        public void NotifyOnTakeDamage(IAbilityCaster source, UnitController target, int damage, string abilityName) {
            OnTakeDamage(source, target, damage, abilityName);
        }

        public void NotifyOnLevelChanged(UnitController sourceUnitController, int newLevel) {
            OnLevelChanged(sourceUnitController, newLevel);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnAbilityListChanged(UnitController sourceUnitController, AbilityProperties newAbility) {
            //Debug.Log($"SystemEventManager.NotifyOnAbilityListChanged({newAbility})");

            OnAbilityListChanged(sourceUnitController, newAbility);
            //OnPrerequisiteUpdated();
        }
        
        public void NotifyOnItemCountChanged(UnitController sourceUnitController, Item item) {
            OnItemCountChanged(sourceUnitController, item);
        }

        public void NotifyOnAcceptQuest(UnitController sourceUnitController, QuestBase questBase) {
            OnAcceptQuest(sourceUnitController, questBase);
        }

        public void NotifyOnRemoveQuest(UnitController sourceUnitController, QuestBase questBase) {
            OnRemoveQuest(sourceUnitController, questBase);
        }

        public void NotifyOnMarkQuestComplete(UnitController sourceUnitController, QuestBase questBase) {
            OnMarkQuestComplete(sourceUnitController, questBase);
        }

        public void NotifyOnQuestObjectiveStatusUpdated(UnitController sourceUnitController, Quest quest) {
            OnQuestObjectiveStatusUpdated(sourceUnitController, quest);
        }

        public void NotifyOnAchievementObjectiveStatusUpdated(UnitController sourceUnitController, Achievement achievement) {
            OnAchievementObjectiveStatusUpdated(sourceUnitController, achievement);
        }

        public void NotifyOnLearnSkill(UnitController sourceUnitController, Skill skill) {
            OnLearnSkill(sourceUnitController, skill);
        }

        public void NotifyOnUnLearnSkill(UnitController sourceUnitController, Skill skill) {
            OnUnLearnSkill(sourceUnitController, skill);
        }

        public void NotifyOnSetCraftAbility(UnitController sourceUnitController, CraftAbilityProperties abilityProperties) {
            OnSetCraftAbility(sourceUnitController, abilityProperties);
        }

        public void NotifyOnCraftItem() {
            OnCraftItem();
        }

        public void NotifyOnAddBag() {
            OnAddBag();
        }

        public void NotifyOnCurrencyChange() {
            OnCurrencyChange();
        }

        public void NotifyOnUnsetGamepadActionButton(int buttonIndex) {
            OnUnsetGamepadActionButton(buttonIndex);
        }

        public void NotifyOnUnsetMouseActionButton(int buttonIndex) {
            OnUnsetMouseActionButton(buttonIndex);
        }

        public void NotifyOnSetMouseActionButton(IUseable useable, int buttonIndex) {
            OnSetMouseActionButton(useable, buttonIndex);
        }

        public void NotifyOnSetGamepadActionButton(IUseable useable, int buttonIndex) {
            OnSetGamepadActionButton(useable, buttonIndex);
        }

        public void NotifyOnRemoveActivePet(UnitProfile unitProfile) {
            OnRemoveActivePet(unitProfile);
        }

        public void NotifyOnTakeLoot(int accountId) {
            OnTakeLoot(accountId);
        }

        public void NotifyOnPlayerDeath() {
            OnPlayerDeath();
        }

        public void NotifyOnCalculateRelativeTime() {
            OnCalculateRelativeTime();
        }

        public void NotifyOnStartServer() {
            OnStartServer();
        }

        public void NotifyOnStopServer() {
            OnStopServer();
        }

        public void NotifyOnLevelLoad() {
            OnLevelLoad();
        }

        public void NotifyOnLevelUnload(int sceneHandle, string sceneName) {
            OnLevelUnload(sceneHandle, sceneName);
        }

        public void NotifyOnRemoveLoadedScene(int sceneHandle, string sceneName) {
            OnRemoveLoadedScene(sceneHandle, sceneName);
        }

        public void NotifyOnAddLoadedScene(int sceneHandle, string sceneName) {
            OnAddLoadedScene(sceneHandle, sceneName);
        }

        public void NotifyOnEndWeather(int sceneHandle, WeatherProfile previousWeather, bool immediate) {
            OnEndWeather(sceneHandle, previousWeather, immediate);
        }

        public void NotifyOnChooseWeather(int sceneHandle, WeatherProfile currentWeather) {
            OnChooseWeather(sceneHandle, currentWeather);
        }

        public void NotifyOnStartWeather(int sceneHandle) {
            OnStartWeather(sceneHandle);
        }
    }

    [System.Serializable]
    public class CustomParam {
        public EventParam eventParams = new EventParam();
        public ObjectConfigurationNode objectParam = new ObjectConfigurationNode();
    }

    [System.Serializable]
    public class EventParam {
        public string StringParam = string.Empty;
        public int IntParam = 0;
        public float FloatParam = 0f;
        public bool BoolParam = false;
    }

    [System.Serializable]
    public class EventParamProperties {
        public EventParam simpleParams = new EventParam();
        public ObjectConfigurationNode objectParam = new ObjectConfigurationNode();
    }

}