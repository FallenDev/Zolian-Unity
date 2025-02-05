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
            if (CharacterSelectionManager.Instance.cachedPlayers.Count >= 1)
            {
                characterSelectionUI.SetActive(true);
                characterSelectionPanel.SetActive(true);
            }

            cancelButton.gameObject.SetActive(false);
        }

        private void OnExitButtonClick() => MainThreadDispatcher.RunOnMainThread(Application.Quit);
    }
}