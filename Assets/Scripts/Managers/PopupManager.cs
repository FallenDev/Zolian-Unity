using Assets.Scripts.Models;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class PopupManager : MonoBehaviour
    {
        public GameObject screenPopup;
        public GameObject systemPopup;
        [SerializeField] private TMP_Text screenMessageText;
        [SerializeField] private TMP_Text systemMessageText;
        public Button screenAcceptButton;
        public Button systemAcceptButton;

        private static PopupManager _instance;

        public static PopupManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindFirstObjectByType<PopupManager>();
                if (_instance != null) return _instance;

                Debug.LogError("PopupManager instance not found in the scene!");
                return null;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                // Remove from parent for persistence
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            screenAcceptButton.onClick.AddListener(ClosePopup);
            systemAcceptButton.onClick.AddListener(ClosePopup);
            screenPopup.SetActive(false);
            systemPopup.SetActive(false);
        }


        public void ShowMessage(string message, PopupMessageType type)
        {
            switch (type)
            {
                case PopupMessageType.Screen:
                    {
                        screenMessageText.text = message;

                        if (screenPopup == null || screenMessageText == null)
                        {
                            Debug.LogError("PopupManager is not set up correctly!");
                            return;
                        }

                        screenPopup.SetActive(true);
                    }
                    break;
                case PopupMessageType.System:
                    {
                        systemMessageText.text = message;

                        if (systemPopup == null || systemMessageText == null)
                        {
                            Debug.LogError("PopupManager is not set up correctly!");
                            return;
                        }

                        systemPopup.SetActive(true);
                    }
                    break;
                case PopupMessageType.Login:
                case PopupMessageType.WoodenBoard:
                case PopupMessageType.AdminMessage:
                default:
                    Debug.LogError($"PopupManager {type} type is not set up!");
                    break;
            }
        }

        public void ClosePopup()
        {
            screenPopup.SetActive(false);
            systemPopup.SetActive(false);
        }
    }
}