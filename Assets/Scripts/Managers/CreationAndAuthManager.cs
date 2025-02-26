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
        public GameObject characterCreationPanel;

        // These are the groups that will be shown/hidden
        public GameObject characterSelectionGroup;
        public GameObject characterCreationGroup;

        public static CreationAndAuthManager Instance;

        private void Awake()
        {
            Instance = this;
            createButton.onClick.AddListener(OnCreateButtonClick);
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            loginButton.onClick.AddListener(OnLoginButtonClick);
        }

        private void Start() { }

        public void OnCreateButtonClick()
        {
            PopupManager.Instance.popupPanel.SetActive(false);
            characterSelectionGroup.gameObject.SetActive(false);
            characterCreationGroup.gameObject.SetActive(true);
        }

        private void OnDeleteButtonClick()
        {
            // Show character deletion
        }

        private void OnCancelButtonClick()
        {
            characterCreationGroup.gameObject.SetActive(false);
            // Only show SelectionPanel if characters exist to select from
            characterSelectionPanel.SetActive(LoginClient.Instance.cachedPlayers.Count >= 1);
            characterSelectionGroup.gameObject.SetActive(true);
        }

        public void OnLoginButtonClick()
        {
            Debug.Log($"OnLoginButtonClick called");
        }
    }
}