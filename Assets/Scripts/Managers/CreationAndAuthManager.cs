using Assets.Scripts.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class CreationAndAuthManager : MonoBehaviour
    {
        public Button createButton;
        public Button cancelButton;
        public Button exitButton;
        public Button loginButton;

        // GameObjects attached so they can be hidden through this manager
        public GameObject characterSelectionUI;
        public GameObject characterSelectionPanel;

        public static CreationAndAuthManager Instance;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            createButton.onClick.AddListener(OnCreateButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            exitButton.onClick.AddListener(OnExitButtonClick);
            loginButton.onClick.AddListener(OnLoginButtonClick);
            loginButton.gameObject.SetActive(false);
        }

        private void OnCreateButtonClick()
        {
            // Hide character selection
            characterSelectionUI.SetActive(false);
            characterSelectionPanel.SetActive(false);
            cancelButton.gameObject.SetActive(true);
            PopupManager.Instance.popupPanel.SetActive(false);
            // Show character creation
        }

        private void OnCancelButtonClick()
        {
            if (LoginClient.Instance.cachedPlayers.Count >= 1)
            {
                characterSelectionUI.SetActive(true);
                characterSelectionPanel.SetActive(true);
            }

            cancelButton.gameObject.SetActive(false);
        }

        private void OnLoginButtonClick()
        {
            Debug.Log($"Logging in {CharacterSelectionManager.Instance.selectedPlayer.Name}");
        }

        private void OnExitButtonClick() => MainThreadDispatcher.RunOnMainThread(Application.Quit);
    }
}