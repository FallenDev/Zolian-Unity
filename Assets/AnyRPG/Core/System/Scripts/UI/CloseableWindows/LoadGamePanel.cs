using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AnyRPG {

    public class LoadGamePanel : WindowContentController {

        public override event Action<CloseableWindowContents> OnCloseWindow = delegate { };

        [SerializeField]
        private CharacterPreviewPanel characterPreviewPanel = null;

        [SerializeField]
        private GameObject buttonPrefab = null;

        [SerializeField]
        private ScrollRect scrollRect = null;

        [SerializeField]
        private RectTransform buttonArea = null;

        
        [SerializeField]
        private HighlightButton returnButton = null;

        [SerializeField]
        private HighlightButton logoutButton = null;

        [SerializeField]
        private HighlightButton loadGameButton = null;

        /*
        [SerializeField]
        private HighlightButton newGameButton = null;
        */

        [SerializeField]
        private HighlightButton deleteGameButton = null;

        [SerializeField]
        private HighlightButton copyGameButton = null;

        [SerializeField]
        private TextMeshProUGUI nameText = null;

        private List<LoadGameButton> loadGameButtons = new List<LoadGameButton>();

        private LoadGameButton selectedLoadGameButton = null;

        // game manager references
        private SaveManager saveManager = null;
        private ObjectPooler objectPooler = null;
        private CharacterCreatorManager characterCreatorManager = null;
        private UIManager uIManager = null;
        private LoadGameManager loadGameManager = null;
        private NetworkManagerClient networkManager = null;

        public LoadGameButton SelectedLoadGameButton { get => selectedLoadGameButton; }

        public override void Configure(SystemGameManager systemGameManager) {
            base.Configure(systemGameManager);

            loadGameManager.OnDeleteGame += HandleDeleteGame;
            loadGameManager.OnCopyGame += HandleCopyGame;

            characterPreviewPanel.Configure(systemGameManager);
            characterPreviewPanel.SetParentPanel(this);
        }

        public override void SetGameManagerReferences() {
            //Debug.Log("LoadGamePanel.SetGameManagerReferences()");
            base.SetGameManagerReferences();
            saveManager = systemGameManager.SaveManager;
            objectPooler = systemGameManager.ObjectPooler;
            characterCreatorManager = systemGameManager.CharacterCreatorManager;
            uIManager = systemGameManager.UIManager;
            loadGameManager = systemGameManager.LoadGameManager;
            networkManager = systemGameManager.NetworkManagerClient;
        }

        public override void ReceiveClosedWindowNotification() {
            //Debug.Log("LoadGamePanel.RecieveClosedWindowNotification()");

            characterPreviewPanel.OnUnitCreated -= HandleUnitCreated;
            loadGameManager.OnLoadCharacterList -= HandleLoadCharacterList;
            characterPreviewPanel.CharacterConfigurationProvider = null;
            characterPreviewPanel.ReceiveClosedWindowNotification();
            //saveManager.ClearSharedData();

            ClearLoadButtons();

            characterCreatorManager.DisableLight();

            base.ReceiveClosedWindowNotification();
            OnCloseWindow(this);
        }

        public override void ProcessOpenWindowNotification() {
            //Debug.Log("LoadGamePanel.ProcessOpenWindowNotification()");

            base.ProcessOpenWindowNotification();

            loadGameManager.LoadCharacterList();

            // inform the preview panel so the character can be rendered
            characterPreviewPanel.OnUnitCreated += HandleUnitCreated;
            loadGameManager.OnLoadCharacterList += HandleLoadCharacterList;
            characterPreviewPanel.CharacterConfigurationProvider = loadGameManager;
            characterPreviewPanel.ReceiveOpenWindowNotification();


            // This is down here so re-used UMA units don't trigger handleTargetCreated before we can subscribe to it
            ShowLoadButtonsCommon();

            // this needs to be run here because the initial run in ShowLoadButtonsCommon will have done nothing because the preview panel wasn't open yet
            //LoadSavedAppearanceSettings();

            characterCreatorManager.EnableLight();

            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons() {
            if (systemGameManager.GameMode == GameMode.Local) {
                returnButton.gameObject.SetActive(true);
                //deleteGameButton.gameObject.SetActive(true);
                copyGameButton.gameObject.SetActive(true);
                logoutButton.gameObject.SetActive(false);
            } else {
                returnButton.gameObject.SetActive(false);
                //deleteGameButton.gameObject.SetActive(false);
                copyGameButton.gameObject.SetActive(false);
                logoutButton.gameObject.SetActive(true);
            }
            uINavigationControllers[1].UpdateNavigationList();
        }

        public void ShowSavedGame(LoadGameButton loadButton) {
            //Debug.Log($"LoadGamePanel.ShowSavedGame({loadButton.gameObject.name})");

            selectedLoadGameButton = loadButton;

            loadGameManager.SetSavedGame(loadButton.PlayerCharacterSaveData);

            // ensure the correct unit and character model is spawned
            characterPreviewPanel.ReloadUnit();

            // testing get proper appearance
            //LoadSavedAppearanceSettings();

            // apply capabilities to it so equipment can work
            //characterCreatorManager.PreviewUnitController.BaseCharacter.ApplyCapabilityConsumerSnapshot(capabilityConsumerSnapshot);

            loadGameButton.Button.interactable = true;
            copyGameButton.Button.interactable = true;
            deleteGameButton.Button.interactable = true;
            uINavigationControllers[1].UpdateNavigationList();
            uINavigationControllers[0].UnHightlightButtonBackgrounds(loadButton);

            nameText.text = loadGameManager.PlayerCharacterSaveData.SaveData.playerName;
        }


        public void ClearLoadButtons() {
            //Debug.Log("LoadGamePanel.ClearLoadButtons()");

            // clear the quest list so any quests left over from a previous time opening the window aren't shown
            foreach (LoadGameButton loadGameButton in loadGameButtons) {
                if (loadGameButton != null) {
                    loadGameButton.DeSelect();
                    objectPooler.ReturnObjectToPool(loadGameButton.gameObject);
                }
            }
            loadGameButtons.Clear();
            uINavigationControllers[0].ClearActiveButtons(true);
            selectedLoadGameButton = null;
            loadGameButton.Button.interactable = false;
            copyGameButton.Button.interactable = false;
            deleteGameButton.Button.interactable = false;
            nameText.text = "";
            loadGameManager.ResetData();
        }


        public void ShowLoadButtonsCommon() {
            //Debug.Log($"LoadGamePanel.ShowLoadButtonsCommon() buttonArea height: {buttonArea.sizeDelta.y}");

            ClearLoadButtons();
            characterPreviewPanel.ClearPreviewTarget();
            int count = 0;
            foreach (PlayerCharacterSaveData playerCharacterSaveData in loadGameManager.CharacterList) {
                //Debug.Log("LoadGamePanel.ShowLoadButtonsCommon(): setting a button with saved game data");
                AddLoadButton(playerCharacterSaveData);
                count++;
            }
            uINavigationControllers[1].UpdateNavigationList();
            /*
            Debug.Log($"LoadGamePanel.ShowLoadButtonsCommon() buttonArea height after adds: {buttonArea.sizeDelta.y}");
            Canvas.ForceUpdateCanvases();
            Debug.Log($"LoadGamePanel.ShowLoadButtonsCommon() buttonArea height after recalculate 1: {buttonArea.sizeDelta.y}");
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonArea);
            Debug.Log($"LoadGamePanel.ShowLoadButtonsCommon() buttonArea height after recalculate 2: {buttonArea.sizeDelta.y}");
            LayoutRebuilder.MarkLayoutForRebuild(buttonArea);
            Debug.Log($"LoadGamePanel.ShowLoadButtonsCommon() buttonArea height after recalculate 3: {buttonArea.sizeDelta.y}");
            */

            if (loadGameButtons.Count > 0) {
                SetNavigationController(uINavigationControllers[0]);
            } else {
                SetNavigationController(uINavigationControllers[1]);
            }
            //SetPreviewTarget();
            StartCoroutine(RefreshContentLayout());
            //scrollRect.Rebuild(CanvasUpdate.PostLayout);
        }

        /// <summary>
        /// This is some janky **** here but Unity's layout system is not cooperating and no matter what you do,
        /// it doesn't want to update the scroll rect content size properly on the first frame. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator RefreshContentLayout() {
            //Debug.Log($"LoadGamePanel.RefreshContentLayout() buttonArea height first frame: {buttonArea.sizeDelta.y}");
            yield return null;
            scrollRect.Rebuild(CanvasUpdate.PostLayout);
        }

        private void AddLoadButton(PlayerCharacterSaveData playerCharacterSaveData) {
            GameObject go = objectPooler.GetPooledObject(buttonPrefab, buttonArea.transform);
            LoadGameButton loadGameButton = go.GetComponent<LoadGameButton>();
            loadGameButton.Configure(systemGameManager);
            loadGameButton.AddSaveData(this, playerCharacterSaveData);
            loadGameButtons.Add(loadGameButton);
            uINavigationControllers[0].AddActiveButton(loadGameButton);
        }

        public void HandleLoadCharacterList() {
            //Debug.Log("LoadGamePanel.HandleLoadCharacterList()");

            ShowLoadButtonsCommon();
        }

        public void HandleUnitCreated() {
            //Debug.Log("LoadGamePanel.HandleTargetCreated()");

            LoadEquipmentData();
        }

        /*
        public void LoadSavedAppearanceSettings() {
            if (characterCreatorManager.PreviewUnitController?.UnitModelController  != null) {
                characterCreatorManager.PreviewUnitController?.UnitModelController.LoadSavedAppearanceSettings();
            }

            // testing - try this here to allow the unit to equip itself naturally when it finishes spawning
            LoadEquipmentData();
        }
        */

        public void LoadEquipmentData() {
            //Debug.Log("LoadGamePanel.LoadEquipmentData()");

            if (characterCreatorManager.PreviewUnitController != null) {
                //Debug.Log("LoadGamePanel.LoadEquipmentData(): preview controller found");

                characterCreatorManager.PreviewUnitController.CharacterSaveManager.LoadEquipmentData(loadGameManager.PlayerCharacterSaveData.SaveData);
            }
        }

        /*
        public void ClosePanel() {
            //Debug.Log("LoadGamePanel.ClosePanel()");
            uIManager.loadGameWindow.CloseWindow();
        }
        */

        public void LoadGame() {
            //Debug.Log("LoadGamePanel.LoadGame()");
            if (selectedLoadGameButton != null) {
                loadGameManager.LoadGame(selectedLoadGameButton.PlayerCharacterSaveData);
                //Close();
            }
        }

        public void NewGame() {
            //Debug.Log("LoadGamePanel.NewGame()");
            if (systemConfigurationManager.UseNewGameWindow == true) {
                Close();
                uIManager.newGameWindow.OpenWindow();
            } else {
                uIManager.confirmNewGameMenuWindow.OpenWindow();
            }
        }

        public void DeleteGame() {
            if (selectedLoadGameButton != null) {
                uIManager.deleteGameMenuWindow.OpenWindow();
            }
        }

        public void HandleDeleteGame() {
            //Debug.Log("LoadGamePanel.HandleDeleteGame()");

            loadGameManager.LoadCharacterList();
            //ShowLoadButtonsCommon();
        }

        public void CopyGame() {
            if (selectedLoadGameButton != null) {
                uIManager.copyGameMenuWindow.OpenWindow();
            }
        }

        public void HandleCopyGame() {
            uIManager.copyGameMenuWindow.CloseWindow();
            loadGameManager.LoadCharacterList();
            //ShowLoadButtonsCommon();
        }

        public void Logout() {
            networkManager.RequestLogout();
            Close();
        }

    }

}