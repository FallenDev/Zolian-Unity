﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace AnyRPG {
    public class FishNetNewGameWizard : NewGameWizardBase {

        private const string pathToFishNetNetworkManagerPrefab = "/AnyRPG/Network/FishNet/GameManager/FishNetNetworkManager.prefab";
        private const string pathToPlayerUnitsTemplate = "/AnyRPG/Network/FishNet/Content/TemplatePackages/UnitProfile/Player/FishNetMecanimHumanPlayerUnitsTemplatePackage.asset";
        private const string pathToPhysicsSceneSync = "/AnyRPG/Network/FishNet/GameManager/FishNetPhysicsSceneSync.prefab";

        public override string PathToPlayerUnitsTemplate { get => pathToPlayerUnitsTemplate; }

        [MenuItem("Tools/AnyRPG/Wizard/FishNet/New Game Wizard")]
        public static void CreateWizard() {
            ScriptableWizard.DisplayWizard<FishNetNewGameWizard>("FishNet New Game Wizard", "Create");
        }

        protected override bool CheckFilesExist() {

            // check for presence of the FishNet Network Manager prefab
            if (WizardUtilities.CheckFileExists(pathToFishNetNetworkManagerPrefab, "FishNet Network Manager Prefab") == false) {
                return false;
            }

            return base.CheckFilesExist();
        }

        public override void ModifyScene() {
            string sceneConfigPrefabAssetPath = "Assets" + pathToPhysicsSceneSync;
            GameObject sceneSyncGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath(sceneConfigPrefabAssetPath);
            GameObject instantiatedGO = (GameObject)PrefabUtility.InstantiatePrefab(sceneSyncGameObject);
        }

        protected override void MakeOptionalContent(string fileSystemGameName, string prefabPath, GameObject gameManagerSceneVariant) {
            base.MakeOptionalContent(fileSystemGameName, prefabPath, gameManagerSceneVariant);

            // create a variant of the FishNet Network Manager prefab
            MakeFishNetNetworkManagerPrefabVariant(prefabPath + "/GameManager/FishNetNetworkManager.prefab", fileSystemGameName, gameManagerSceneVariant);
        }

        private GameObject MakeFishNetNetworkManagerPrefabVariant(string newPath, string gameName, GameObject gameManagerSceneVariant) {

            GameObject networkManagerGameObject = (GameObject)AssetDatabase.LoadMainAssetAtPath("Assets" + pathToFishNetNetworkManagerPrefab);

            // instantiate original
            GameObject instantiatedNetworkManagerGameObject = (GameObject)PrefabUtility.InstantiatePrefab(networkManagerGameObject);
            instantiatedNetworkManagerGameObject.name = gameName + instantiatedNetworkManagerGameObject.name;

            // make variant on disk
            GameObject savedNetworkManagerGameObject = PrefabUtility.SaveAsPrefabAsset(instantiatedNetworkManagerGameObject, newPath);

            // remove original from scene
            GameObject.DestroyImmediate(instantiatedNetworkManagerGameObject);

            // instantiate new variant in scene
            //PrefabUtility.InstantiatePrefab(variant);
            GameObject sceneNetworkManagerGameObject = (GameObject)PrefabUtility.InstantiatePrefab(savedNetworkManagerGameObject);
            sceneNetworkManagerGameObject.name = networkManagerGameObject.name;
            NetworkController networkController = sceneNetworkManagerGameObject.GetComponent<NetworkController>();
            if (networkController == null) {
                Debug.LogError($"NetworkController component not found on {sceneNetworkManagerGameObject.name}. Please ensure the prefab has a NetworkController component.");
            } else {
                Debug.Log($"NetworkController found on {sceneNetworkManagerGameObject.name}. Proceeding with configuration.");
            }
            // configure game manager scene variant to reference the network controller in the scene
            NetworkManagerClient networkManagerClient = gameManagerSceneVariant.GetComponent<NetworkManagerClient>();
            if (networkManagerClient != null) {
                Debug.Log($"Setting NetworkController on NetworkManagerClient in {gameManagerSceneVariant.name} to {networkController.name}");
                networkManagerClient.NetworkController = networkController;
            } else {
                Debug.LogWarning($"NetworkManagerClient component not found on {gameManagerSceneVariant.name}. NetworkController will not be set.");
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(networkManagerClient);
            NetworkManagerServer networkManagerServer = gameManagerSceneVariant.GetComponent<NetworkManagerServer>();
            if (networkManagerServer != null) {
                Debug.Log($"Setting NetworkController on NetworkManagerServer in {gameManagerSceneVariant.name} to {networkController.name}");
                networkManagerServer.NetworkController = networkController;
            } else {
               Debug.LogWarning($"NetworkManagerServer component not found on {gameManagerSceneVariant.name}. NetworkController will not be set.");
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(networkManagerServer);
            return savedNetworkManagerGameObject;
        }

        protected override void ConfigureGameOptions(SystemConfigurationManager systemConfigurationManager) {
            base.ConfigureGameOptions(systemConfigurationManager);

            systemConfigurationManager.DefaultPlayerUnitProfileName = "Mecanim Human Male";
            systemConfigurationManager.AllowOfflinePlay = false;
            systemConfigurationManager.AllowOnlinePlay = true;
        }

        protected override void ConfigureGameManager(GameObject instantiatedGameObject) {
            base.ConfigureGameManager(instantiatedGameObject);

        }

    }

}
