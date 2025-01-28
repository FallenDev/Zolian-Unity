using TMPro;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TMP_Text messageText;

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

    private void Start() => popupPanel.SetActive(false);
    

    public void ShowMessage(string message)
    {
        if (popupPanel == null || messageText == null)
        {
            Debug.LogError("PopupManager is not set up correctly!");
            return;
        }
        
        messageText.text = message;
        popupPanel.SetActive(true);
    }

    public void ClosePopup() => popupPanel.SetActive(false);
}