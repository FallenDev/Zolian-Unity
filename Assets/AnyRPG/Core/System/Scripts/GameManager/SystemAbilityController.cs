using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemAbilityController : ConfiguredMonoBehaviour, IAbilityCaster {

        private AbilityManager abilityManager = null;

        // game manager references
        private ObjectPooler objectPooler = null;
        private SystemEventManager systemEventManager = null;
        private NetworkManagerServer networkManagerServer = null;

        public IAbilityManager AbilityManager { get => abilityManager; }
        public MonoBehaviour MonoBehaviour { get => this; }

        public override  void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            abilityManager = new AbilityManager(this, systemGameManager);
            systemEventManager.OnLevelUnload += HandleLevelUnload;
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();
            objectPooler = systemGameManager.ObjectPooler;
            systemEventManager = systemGameManager.SystemEventManager;
            networkManagerServer = systemGameManager.NetworkManagerServer;
        }

        // ensure that no coroutine continues or other spell effects exist past the end of a level
        public void HandleLevelUnload(int sceneHandle, string sceneName) {
            foreach (Coroutine coroutine in abilityManager.DestroyAbilityEffectObjectCoroutines) {
                StopCoroutine(coroutine);
            }
            abilityManager.DestroyAbilityEffectObjectCoroutines.Clear();

            foreach (GameObject go in abilityManager.AbilityEffectGameObjects) {
                if (go != null) {
                    objectPooler.ReturnObjectToPool(go);
                }
            }
            abilityManager.AbilityEffectGameObjects.Clear();
        }

        public void BeginDestroyAbilityEffectObject(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffectProperties fixedLengthEffect) {
            foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                foreach (GameObject go in gameObjectList) {
                    abilityManager.AbilityEffectGameObjects.Add(go);
                }
            }
            abilityManager.AddDestroyAbilityEffectObjectCoroutine(StartCoroutine(DestroyAbilityEffectObject(abilityEffectObjects, source, target, timer, abilityEffectInput, fixedLengthEffect)));
        }

        public IEnumerator DestroyAbilityEffectObject(Dictionary<PrefabProfile, List<GameObject>> abilityEffectObjects, IAbilityCaster source, Interactable target, float timer, AbilityEffectContext abilityEffectInput, FixedLengthEffectProperties fixedLengthEffect) {
            //Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(objectCount: {abilityEffectObjects.Count}, {(source == null ? "null" : source.AbilityManager.Name)}, {(target == null ? "null" : target.gameObject.name)}, {timer}, {fixedLengthEffect.ResourceName})");

            float timeRemaining = timer;
            // debug print instance ids of the objects we are about to destroy
            /*
            foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                foreach (GameObject go in gameObjectList) {
                    if (go != null) {
                        Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(): starting timer for {go.name} {go.GetInstanceID()}");
                    }
                }
            }
            Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(): fixedLengthEffect: {(fixedLengthEffect == null ? "null" : fixedLengthEffect.ResourceName)}");
            */

            // keep track of temporary elapsed time between ticks
            float elapsedTime = 0f;

            bool nullTarget = false;
            CharacterStats targetStats = null;
            if (target != null) {
                CharacterUnit _characterUnit = CharacterUnit.GetCharacterUnit(target);
                if (_characterUnit != null) {
                    targetStats = _characterUnit.UnitController.CharacterStats;
                }
            } else {
                nullTarget = true;
            }

            int milliseconds = (int)((fixedLengthEffect.TickRate - (int)fixedLengthEffect.TickRate) * 1000);
            float finalTickRate = fixedLengthEffect.TickRate;
            if (finalTickRate == 0) {
                finalTickRate = timer + 1;
            }
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() milliseconds: " + milliseconds);
            //TimeSpan tickRateTimeSpan = new TimeSpan(0, 0, 0, (int)finalTickRate, milliseconds);
            //Debug.Log(abilityEffectName + ".StatusEffect.Tick() tickRateTimeSpan: " + tickRateTimeSpan);
            //fixedLengthEffect.MyNextTickTime = System.DateTime.Now + tickRateTimeSpan;
            //Debug.Log(abilityEffectName + ".FixedLengthEffect.Tick() nextTickTime: " + nextTickTime);

            while (timeRemaining > 0f) {
                yield return null;

                if (nullTarget == false && (targetStats == null || fixedLengthEffect == null)) {
                    //Debug.Log("SystemAbilityController.DestroyAbilityEffectObject: BREAKING!: fixedLengthEffect: " + (fixedLengthEffect == null ? "null" : fixedLengthEffect.DisplayName) + "; targetstats: " + (targetStats == null ? "null" : targetStats.BaseCharacter.CharacterName));
                    break;
                }

                if (fixedLengthEffect.PrefabSpawnLocation != PrefabSpawnLocation.GroundTarget
                    && fixedLengthEffect.GetTargetOptions(source).RequireTarget == true
                    && (target == null || (targetStats.IsAlive == true && fixedLengthEffect.GetTargetOptions(source).RequireDeadTarget == true) || (targetStats.IsAlive == false && fixedLengthEffect.GetTargetOptions(source).RequireLiveTarget == true))) {
                    //Debug.Log("BREAKING!!!!!!!!!!!!!!!!!");
                    break;
                } else {
                    timeRemaining -= Time.deltaTime;
                    elapsedTime += Time.deltaTime;
                    if (elapsedTime > finalTickRate) {
                        //Debug.Log(fixedLengthEffect.DisplayName + ".FixedLengthEffect.Tick() TickTime!");
                        if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                            fixedLengthEffect.CastTick(source, target, abilityEffectInput);
                        }
                        elapsedTime -= finalTickRate;
                    }
                }
            }
            //Debug.Log(fixedLengthEffect.DisplayName + ".FixedLengthEffect.Tick() Done ticking and about to perform ability affects.");
            if (systemGameManager.GameMode == GameMode.Local || networkManagerServer.ServerModeActive == true) {
                fixedLengthEffect.CastComplete(source, target, abilityEffectInput);
            }
            foreach (List<GameObject> gameObjectList in abilityEffectObjects.Values) {
                foreach (GameObject go in gameObjectList) {
                    if (abilityManager.AbilityEffectGameObjects.Contains(go)) {
                        abilityManager.AbilityEffectGameObjects.Remove(go);
                    }
                    if (go != null) {
                        //Debug.Log($"SystemAbilityController.DestroyAbilityEffectObject(objectCount: {abilityEffectObjects.Count}, {(source == null ? "null" : source.AbilityManager.Name)}, {(target == null ? "null" : target.gameObject.name)}, {timer}, {fixedLengthEffect.ResourceName}) DESTROYING AT END OF TIMER : {go.name} ({go.GetInstanceID()})");
                        objectPooler.ReturnObjectToPool(go, fixedLengthEffect.PrefabDestroyDelay);
                    }
                }
            }
            abilityEffectObjects.Clear();
        }

        public static string GetTimeText(float durationSeconds) {
            string returnText = string.Empty;
            if (durationSeconds < 60f && durationSeconds >= 0f) {
                // less than 1 minute
                returnText = ((int)durationSeconds).ToString() + " second";
                if ((int)durationSeconds != 1) {
                    returnText += "s";
                }
            } else if (durationSeconds < 3600) {
                //less than 1 hour
                returnText = ((int)(durationSeconds / 60)).ToString() + " minute";
                if (((int)durationSeconds / 60) != 1) {
                    returnText += "s";
                }
            } else if (durationSeconds > 3600f) {
                //greater than 1 hour
                returnText = ((int)(durationSeconds / 3600)).ToString() + " hour";
                if (((int)durationSeconds / 3600) != 1) {
                    returnText += "s";
                }
            }
            return returnText;
        }

        public void OnDestroy() {
            StopAllCoroutines();
            systemEventManager.OnLevelUnload -= HandleLevelUnload;
        }
    }

}
