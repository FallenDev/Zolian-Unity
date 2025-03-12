using System;
using Assets.Scripts.Models;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class PopupManager : MonoBehaviour
    {
        [Header("Screen Popups")]
        public GameObject screenPopup;
        [SerializeField] private TMP_Text screenMessageText;
        public Button screenAcceptButton;

        [Header("System Popups")]
        public GameObject systemPopup;
        [SerializeField] private TMP_Text systemMessageText;
        public Button systemAcceptButton;

        [Header("Confirmation Popups")]
        public GameObject confirmPopup;
        [SerializeField] private TMP_Text confirmMessageText;
        public Button confirmAcceptButton;
        [SerializeField] private TMP_Text confirmAcceptButtonText;
        public Button confirmCancelButton;
        public Action<int> onResult = (result) => { Debug.Log($"Result: {result}"); };

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
            confirmAcceptButton.onClick.AddListener(ClosePopup);
            confirmCancelButton.onClick.AddListener(ClosePopup);
            screenPopup.SetActive(false);
            systemPopup.SetActive(false);
            confirmPopup.SetActive(false);
        }


        public void ShowMessage(string message, PopupMessageType type, string buttonMessage = null)
        {
            switch (type)
            {
                case PopupMessageType.Confirmation:
                    {
                        confirmMessageText.text = message;
                        confirmAcceptButtonText.text = buttonMessage ?? "OK";

                        if (confirmPopup == null || confirmMessageText == null)
                        {
                            Debug.LogError("PopupManager is not set up correctly!");
                            return;
                        }
                        
                        confirmPopup.SetActive(true);
                    }
                    break;
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
            confirmPopup.SetActive(false);
        }

        public void OnResult(int result)
        {
            confirmPopup.SetActive(false);
            onResult(result);
        }
    }
}