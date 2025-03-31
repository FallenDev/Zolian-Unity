using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network;

namespace Assets.Scripts.CharacterSelection
{
    public class CharacterSelectionUI : MonoBehaviour
    {
        [Header("UI Elements")] 
        public GameObject CharacterListPanel;
        public GameObject CharacterEntryPrefab;
        public Transform ContentListContainer;
        public TextMeshProUGUI CharacterNameText;
        public Button LeftArrowButton;
        public Button RightArrowButton;
        private readonly List<Button> _characterButtons = new();
        private Button _currentlySelectedButton;

        private List<PlayerSelection> _characters = new();
        private int _selectedCharacterIndex;

        public static CharacterSelectionUI Instance;

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
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                NavigateLeft();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                NavigateRight();
            }

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

            _characterButtons.Clear();

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
                var button = entry.GetComponent<Button>();

                button.onClick.AddListener(() =>
                {
                    _selectedCharacterIndex = index;
                    UpdateCharacterDisplay();
                });

                _characterButtons.Add(button);
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
            if (_characters.Count == 0 || _selectedCharacterIndex >= _characterButtons.Count) return;

            if (_currentlySelectedButton != null)
                _currentlySelectedButton.OnDeselect(null);

            _currentlySelectedButton = _characterButtons[_selectedCharacterIndex];
            if (_characterButtons[_selectedCharacterIndex] != null)
                _characterButtons[_selectedCharacterIndex].SetSelected();
            
            CharacterSelected.Instance.SelectCharacter(_characters[_selectedCharacterIndex]);
            var character = _characters[_selectedCharacterIndex];
            CharacterNameText.text = $"{character.Name} - Lvl {character.Level} {character.BaseClass}";
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