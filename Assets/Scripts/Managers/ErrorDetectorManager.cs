using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class ErrorDetectorManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI errorTextMesh;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button copyToClipboardButton;

        private readonly List<string> _ignoreErrorStringList = new();

        private void Awake()
        {
            closeButton.onClick.AddListener(Hide);

            copyToClipboardButton.onClick.AddListener(() =>
            {
                try
                {
                    GUIUtility.systemCopyBuffer = errorTextMesh.text;
                }
                catch (Exception e)
                {
                    Debug.LogException(new Exception("Failed to copy to Clipboard!\n" + e));
                }
            });
        }

        private void Start()
        {
            Application.logMessageReceived += Application_logMessageReceived;
            Hide();
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (type is not (LogType.Error or LogType.Exception)) return;
            errorTextMesh.text = "Error: " + condition + "\n" + stackTrace;
            if (_ignoreErrorStringList.Contains(errorTextMesh.text)) return;
            _ignoreErrorStringList.Add(errorTextMesh.text);
            Show();
        }

        private void OnDestroy() => Application.logMessageReceived -= Application_logMessageReceived;
        private void Show() => gameObject.SetActive(true);
        private void Hide() => gameObject.SetActive(false);
    }
}