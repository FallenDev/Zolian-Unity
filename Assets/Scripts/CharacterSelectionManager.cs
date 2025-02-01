using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using TMPro;
using UnityEngine.UI;

public class CharacterSelectionManager : MonoBehaviour
{
    public Scrollbar horizontalScrollbar;
    public GameObject characterSelectionUI;
    public GameObject characterSlotPrefab;
    public GameObject characterSelectionPanel;
    public Transform characterScrollContent;
    public TextMeshProUGUI characterNameText;
    public TextMeshProUGUI characterLevelText;
    public TextMeshProUGUI characterBaseClassText;
    public TextMeshProUGUI characterAdvClassText;
    public TextMeshProUGUI characterJobText;
    public TextMeshProUGUI characterHealthText;
    public TextMeshProUGUI characterManaText;

    private PlayerSelection selectedPlayer;

    public static CharacterSelectionManager Instance;

    private void Awake()
    {
        Instance = this;
        characterSelectionUI.SetActive(false);
        characterSelectionPanel.SetActive(false);
    }

    public void ShowCharacterSelection(List<PlayerSelection> players)
    {
        characterSelectionUI.SetActive(true);
        PopulateCharacterSelection(players);
    }

    private void PopulateCharacterSelection(List<PlayerSelection> players)
    {
        foreach (Transform child in characterScrollContent)
        {
            Destroy(child.gameObject);
        }

        foreach (var player in players)
        {
            GameObject slot = Instantiate(characterSlotPrefab, characterScrollContent);
            var button = slot.GetComponent<CharacterSelection>();

            if (button != null)
            {
                button.Initialize(player);
            }
        }

        ResetScrollPosition();
    }

    public void SelectCharacter(PlayerSelection player)
    {
        selectedPlayer = player;
        characterNameText.text = player.Name;
        characterLevelText.text = $"Level: {player.Level}";
        characterBaseClassText.text = $"Class: {player.BaseClass}";
        characterAdvClassText.text = $"Adv Class: {player.AdvClass}";
        characterJobText.text = $"Job: {player.Job}";
        characterHealthText.text = $"Health: {player.Health}";
        characterManaText.text = $"Mana: {player.Mana}";
        characterSelectionPanel.SetActive(true);

        // Future: Add logic to confirm selection or enter the game
    }

    private void ResetScrollPosition()
    {
        // Force the layout system to update
        Canvas.ForceUpdateCanvases();

        // Ensure the content starts at the leftmost position
        if (characterScrollContent != null)
        {
            RectTransform contentRect = characterScrollContent.GetComponent<RectTransform>();
            contentRect.anchoredPosition = new Vector2(0, contentRect.anchoredPosition.y); // Reset X position to 0
        }

        // Reset the horizontal scrollbar to the leftmost position
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.value = 0; // Start fully left (scrollbar value of 0)
        }
    }
}