using Assets.Scripts.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class CreationAndAuthManager : MonoBehaviour
    {
        public Button createButton;
        public Button deleteButton;
        public Button cancelButton;
        public Button loginButton;
        public GameObject characterSelectionPanel;

        public static CreationAndAuthManager Instance;

        private void Awake()
        {
            Instance = this;
            createButton.onClick.AddListener(OnCreateButtonClick);
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            loginButton.onClick.AddListener(OnLoginButtonClick);
        }

        private void Start()
        {
            deleteButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);
        }

        public void OnCreateButtonClick()
        {
            // Hide popup if it exists
            PopupManager.Instance.popupPanel.SetActive(false);

            // Hide UI not needed for character creation
            characterSelectionPanel.SetActive(false);
            deleteButton.gameObject.SetActive(false);
            loginButton.gameObject.SetActive(false);
            createButton.gameObject.SetActive(false);
            CharacterSelectionUI.Instance.leftArrowButton.gameObject.SetActive(false);
            CharacterSelectionUI.Instance.rightArrowButton.gameObject.SetActive(false);

            // Show UI needed for character creation
            cancelButton.gameObject.SetActive(true);
        }

        private void OnDeleteButtonClick()
        {
            // Show character deletion
        }

        private void OnCancelButtonClick()
        {
            // Hide UI not needed for character selection
            cancelButton.gameObject.SetActive(false);

            // Show UI needed for character selection
            if (LoginClient.Instance.cachedPlayers.Count >= 1)
            {
                characterSelectionPanel.SetActive(true);
            }

            deleteButton.gameObject.SetActive(true);
            loginButton.gameObject.SetActive(true);
            createButton.gameObject.SetActive(true);
            CharacterSelectionUI.Instance.leftArrowButton.gameObject.SetActive(true);
            CharacterSelectionUI.Instance.rightArrowButton.gameObject.SetActive(true);
        }

        public void OnLoginButtonClick()
        {
            Debug.Log($"OnLoginButtonClick called");
        }
    }
}