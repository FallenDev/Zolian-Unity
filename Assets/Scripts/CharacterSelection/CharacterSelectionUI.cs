using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts.Managers;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network;

public class CharacterSelectionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject characterListPanel;
    public GameObject characterEntryPrefab;
    public Transform contentListContainer;
    public RawImage characterPreview;
    public TextMeshProUGUI characterNameText;
    public Button leftArrowButton;
    public Button rightArrowButton;

    private List<PlayerSelection> characters = new();
    private int selectedCharacterIndex;

    public static CharacterSelectionUI Instance;

    private void Awake()
    {
        Instance = this;
        leftArrowButton.onClick.AddListener(NavigateLeft);
        rightArrowButton.onClick.AddListener(NavigateRight);
    }

    private void Start()
    {
        characterListPanel.SetActive(false);
        leftArrowButton.gameObject.SetActive(false);
        rightArrowButton.gameObject.SetActive(false);
    }

    public void PopulateCharacterList()
    {
        characters = LoginClient.Instance.cachedPlayers;

        List<Transform> childrenToDestroy = new List<Transform>();

        // Collect only character slot entries, NOT the Content object itself
        foreach (Transform child in contentListContainer)
        {
            if (child.CompareTag("CharacterSlot")) // Ensure only character slots are deleted
            {
                childrenToDestroy.Add(child);
            }
        }

        // Now, safely destroy only the character slots
        foreach (Transform child in childrenToDestroy)
        {
            DestroyImmediate(child.gameObject);
        }

        // Force UI to update after clearing
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentListContainer.GetComponent<RectTransform>());


        if (LoginClient.Instance.cachedPlayers.Count >= 1)
        {
            characterListPanel.SetActive(true);
            leftArrowButton.gameObject.SetActive(true);
            rightArrowButton.gameObject.SetActive(true);
        }

        for (var i = 0; i < characters.Count; i++)
        {
            var entry = Instantiate(characterEntryPrefab, contentListContainer);
            entry.transform.SetParent(contentListContainer, false);
            entry.GetComponentInChildren<TextMeshProUGUI>().text = $"{characters[i].Name} (Lvl {characters[i].Level} {characters[i].BaseClass})";
            var index = i;
            entry.GetComponent<Button>().onClick.AddListener(() =>
            {
                selectedCharacterIndex = index;
                UpdateCharacterDisplay();
            });
        }
    }

    public void SelectCharacter(int index)
    {
        if (characters.Count == 0) return;
        selectedCharacterIndex = index;
        UpdateCharacterDisplay();
    }

    private void UpdateCharacterDisplay()
    {
        if (characters.Count == 0) return;
        CharacterSelected.Instance.SelectCharacter(characters[selectedCharacterIndex]);
        var character = characters[selectedCharacterIndex];
        characterNameText.text = $"{character.Name} - Lvl {character.Level} {character.BaseClass}";

        // TODO: Update 3D character preview
    }

    public void EnterWorld()
    {
        Debug.Log($"Entering world as {characters[selectedCharacterIndex].Name}");
        CreationAndAuthManager.Instance.OnLoginButtonClick();
    }

    public void CreateCharacter()
    {
        CreationAndAuthManager.Instance.OnCreateButtonClick();
    }

    public void DeleteCharacter()
    {
        Debug.Log($"Deleting character {characters[selectedCharacterIndex].Name}");
        // TODO: Implement character deletion logic
    }

    public void NavigateLeft()
    {
        if (characters.Count == 0) return;
        selectedCharacterIndex = (selectedCharacterIndex - 1 + characters.Count) % characters.Count;
        SelectCharacter(selectedCharacterIndex);
    }

    public void NavigateRight()
    {
        if (characters.Count == 0) return;
        selectedCharacterIndex = (selectedCharacterIndex + 1) % characters.Count;
        SelectCharacter(selectedCharacterIndex);
    }
}
