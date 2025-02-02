using Assets.Scripts.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class CreationAndAuthManager : MonoBehaviour
    {
        public TMP_InputField steamId;
        public Button loginButton;
        public Button createButton;
        public Button cancelButton;
        public Button exitButton;

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
            loginButton.onClick.AddListener(OnLoginButtonClick);
            createButton.onClick.AddListener(OnCreateButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            exitButton.onClick.AddListener(OnExitButtonClick);
        }

        private void OnLoginButtonClick()
        {
            int.TryParse(steamId.text, out var localSteamId);
            // Calls networking logic here for authentication
            LoginClient.Instance.SendLoginCredentials(localSteamId);
            createButton.gameObject.SetActive(true);
        }

        private void OnCreateButtonClick()
        {
            // Hide character selection
            characterSelectionUI.SetActive(false);
            characterSelectionPanel.SetActive(false);
            loginButton.gameObject.SetActive(false);
            steamId.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);
            // Show character creation
        }

        private void OnCancelButtonClick()
        {
            characterSelectionUI.SetActive(true);
            characterSelectionPanel.SetActive(true);
            cancelButton.gameObject.SetActive(false);
        }

        private void OnExitButtonClick() => MainThreadDispatcher.RunOnMainThread(Application.Quit);
    }
}