using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts.Managers;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network;

namespace Assets.Scripts.CharacterSelection
{
    public class CharacterSelectionUi : MonoBehaviour
    {
        [Header("UI Elements")] 
        public GameObject CharacterListPanel;
        public GameObject CharacterEntryPrefab;
        public Transform ContentListContainer;
        public TextMeshProUGUI CharacterNameText;
        public Button LeftArrowButton;
        public Button RightArrowButton;

        private List<PlayerSelection> _characters = new();
        private int _selectedCharacterIndex;

        public static CharacterSelectionUi Instance;

        private void Awake()
        {
            Instance = this;
            LeftArrowButton.onClick.AddListener(NavigateLeft);
            RightArrowButton.onClick.AddListener(NavigateRight);
        }

        private void Start()
        {
            CharacterListPanel.SetActive(false);
            LeftArrowButton.gameObject.SetActive(false);
            RightArrowButton.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_characters.Equals(LoginClient.Instance.CachedPlayers)) return;
            PopulateCharacterList();
        }

        public void PopulateCharacterList()
        {
            _characters = LoginClient.Instance.CachedPlayers;

            List<Transform> childrenToDestroy = new List<Transform>();

            // Collect only character slot entries, NOT the Content object itself
            foreach (Transform child in ContentListContainer)
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
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentListContainer.GetComponent<RectTransform>());


            if (LoginClient.Instance.CachedPlayers.Count >= 1)
            {
                CharacterListPanel.SetActive(true);
                LeftArrowButton.gameObject.SetActive(true);
                RightArrowButton.gameObject.SetActive(true);
            }

            for (var i = 0; i < _characters.Count; i++)
            {
                var entry = Instantiate(CharacterEntryPrefab, ContentListContainer);
                entry.transform.SetParent(ContentListContainer, false);
                entry.GetComponentInChildren<TextMeshProUGUI>().text = $"{_characters[i].Name} (Lvl {_characters[i].Level} {_characters[i].BaseClass})";
                var index = i;
                entry.GetComponent<Button>().onClick.AddListener(() =>
                {
                    _selectedCharacterIndex = index;
                    UpdateCharacterDisplay();
                });
            }
        }

        public void SelectCharacter(int index)
        {
            if (_characters.Count == 0) return;
            _selectedCharacterIndex = index;
            UpdateCharacterDisplay();
        }

        private void UpdateCharacterDisplay()
        {
            if (_characters.Count == 0) return;
            CharacterSelected.Instance.SelectCharacter(_characters[_selectedCharacterIndex]);
            var character = _characters[_selectedCharacterIndex];
            CharacterNameText.text = $"{character.Name} - Lvl {character.Level} {character.BaseClass}";

            // TODO: Update 3D character preview
        }

        public void EnterWorld()
        {
            Debug.Log($"Entering world as {_characters[_selectedCharacterIndex].Name}");
            CreationAndAuthManager.Instance.OnLoginButtonClick();
        }

        public void CreateCharacter()
        {
            CreationAndAuthManager.Instance.OnCreateButtonClick();
        }

        public void DeleteCharacter()
        {
            Debug.Log($"Deleting character {_characters[_selectedCharacterIndex].Name}");
            // TODO: Implement character deletion logic
        }

        private void NavigateLeft()
        {
            if (_characters.Count == 0) return;
            _selectedCharacterIndex = (_selectedCharacterIndex - 1 + _characters.Count) % _characters.Count;
            SelectCharacter(_selectedCharacterIndex);
        }

        private void NavigateRight()
        {
            if (_characters.Count == 0) return;
            _selectedCharacterIndex = (_selectedCharacterIndex + 1) % _characters.Count;
            SelectCharacter(_selectedCharacterIndex);
        }
    }
}