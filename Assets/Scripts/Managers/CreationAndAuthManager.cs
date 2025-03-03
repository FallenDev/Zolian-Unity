using Assets.Scripts.Network;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class CreationAndAuthManager : MonoBehaviour
    {
        [Header("Action Buttons")] public Button createButton;
        public Button deleteButton;
        public Button cancelButton;
        public Button loginButton;
        public Button continueButton;

        [Header("Object References")] public GameObject CharacterListPanel;
        public TextMeshProUGUI RaceDescription;
        public TextMeshProUGUI ClassDescription;
        public TextMeshProUGUI CharacterName;
        public Toggle MaleToggle;
        public Toggle FemaleToggle;
        public Toggle BerserkerToggle;
        public Toggle DefenderToggle;
        public Toggle AssassinToggle;
        public Toggle ClericToggle;
        public Toggle ArcanusToggle;
        public Toggle MonkToggle;
        public Image BerserkerImage;
        public Image DefenderImage;
        public Image AssassinImage;
        public Image ClericImage;
        public Image ArcanusImage;
        public Image MonkImage;
        public TMP_Dropdown RaceDropdown;

        // These are the groups that will be shown/hidden
        [Header("Character Screens")] public GameObject CharacterSelectionGroup;
        public GameObject CharacterCreationGroup;

        public static CreationAndAuthManager Instance;

        private void Awake()
        {
            Instance = this;
            createButton.onClick.AddListener(OnCreateButtonClick);
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            loginButton.onClick.AddListener(OnLoginButtonClick);
            continueButton.onClick.AddListener(OnContinueButtonClick);
            MaleToggle.onValueChanged.AddListener(OnGenderToggle);
            FemaleToggle.onValueChanged.AddListener(OnGenderToggle);
            BerserkerToggle.onValueChanged.AddListener(OnClassToggle);
            DefenderToggle.onValueChanged.AddListener(OnClassToggle);
            AssassinToggle.onValueChanged.AddListener(OnClassToggle);
            ClericToggle.onValueChanged.AddListener(OnClassToggle);
            ArcanusToggle.onValueChanged.AddListener(OnClassToggle);
            MonkToggle.onValueChanged.AddListener(OnClassToggle);
            RaceDropdown.onValueChanged.AddListener(delegate { OnRaceDropdownChange(RaceDropdown); });
        }

        private void Start()
        {
            OnClassToggle(false);
            OnRaceDropdownChange(RaceDropdown);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelButtonClick();
            }

            if (CharacterName.text != "")
            {
                continueButton.interactable = true;
            }
            else
            {
                continueButton.interactable = false;
            }
        }

        public void OnCreateButtonClick()
        {
            PopupManager.Instance.popupPanel.SetActive(false);
            CharacterSelectionGroup.gameObject.SetActive(false);
            CharacterCreationGroup.gameObject.SetActive(true);
        }

        private void OnDeleteButtonClick()
        {
            // Show character deletion
        }

        private void OnCancelButtonClick()
        {
            CharacterCreationGroup.gameObject.SetActive(false);
            // Only show SelectionPanel if characters exist to select from
            CharacterListPanel.SetActive(LoginClient.Instance.cachedPlayers.Count >= 1);
            CharacterSelectionGroup.gameObject.SetActive(true);
        }

        public void OnLoginButtonClick()
        {
            Debug.Log($"OnLoginButtonClick called");
        }

        private void OnGenderToggle(bool toggled)
        {
            //if (MaleToggle.isOn)
            //{

            //}
            //else
            //{

            //}
        }

        private void OnContinueButtonClick()
        {
            // Send character creation packet
        }

        private void OnClassToggle(bool toggled)
        {
            if (BerserkerToggle.isOn)
            {
                ClassDescription.text = "Berserkers are known for their high burst damage. They can dual-wield weapons and can wear medium to heavy armor.";
                BerserkerImage.gameObject.SetActive(true);
                DefenderImage.gameObject.SetActive(false);
                AssassinImage.gameObject.SetActive(false);
                ClericImage.gameObject.SetActive(false);
                ArcanusImage.gameObject.SetActive(false);
                MonkImage.gameObject.SetActive(false);
            }
            else if (DefenderToggle.isOn)
            {
                ClassDescription.text = "Defenders are protectors, they have heavy armor, unique shields, and deal moderate damage.";
                BerserkerImage.gameObject.SetActive(false);
                DefenderImage.gameObject.SetActive(true);
                AssassinImage.gameObject.SetActive(false);
                ClericImage.gameObject.SetActive(false);
                ArcanusImage.gameObject.SetActive(false);
                MonkImage.gameObject.SetActive(false);
            }
            else if (AssassinToggle.isOn)
            {
                ClassDescription.text = "Assassins are known for their quick combinations, traps, and dual-wielding. They can wear medium armor.";
                BerserkerImage.gameObject.SetActive(false);
                DefenderImage.gameObject.SetActive(false);
                AssassinImage.gameObject.SetActive(true);
                ClericImage.gameObject.SetActive(false);
                ArcanusImage.gameObject.SetActive(false);
                MonkImage.gameObject.SetActive(false);
            }
            else if (ClericToggle.isOn)
            {
                ClassDescription.text = "Clerics are a support class with heavy armor. They also deal moderate damage and can control low-level undead/rodents.";
                BerserkerImage.gameObject.SetActive(false);
                DefenderImage.gameObject.SetActive(false);
                AssassinImage.gameObject.SetActive(false);
                ClericImage.gameObject.SetActive(true);
                ArcanusImage.gameObject.SetActive(false);
                MonkImage.gameObject.SetActive(false);
            }
            else if (ArcanusToggle.isOn)
            {
                ClassDescription.text = "Arcanus class focuses heavily on magic and has a high defense to magic, while wearing light armor.";
                BerserkerImage.gameObject.SetActive(false);
                DefenderImage.gameObject.SetActive(false);
                AssassinImage.gameObject.SetActive(false);
                ClericImage.gameObject.SetActive(false);
                ArcanusImage.gameObject.SetActive(true);
                MonkImage.gameObject.SetActive(false);
            }
            else if (MonkToggle.isOn)
            {
                ClassDescription.text = "Monks are well rounded, they can deliver moderate to high damage, while wearing medium armor.";
                BerserkerImage.gameObject.SetActive(false);
                DefenderImage.gameObject.SetActive(false);
                AssassinImage.gameObject.SetActive(false);
                ClericImage.gameObject.SetActive(false);
                ArcanusImage.gameObject.SetActive(false);
                MonkImage.gameObject.SetActive(true);
            }
        }

        private void OnRaceDropdownChange(TMP_Dropdown dropdown)
        {
            switch (dropdown.options[dropdown.value].text)
            {
                case "Human":
                    RaceDescription.text = "Humans are well-rounded and can adapt to any class. They have no specific strengths or weaknesses.";
                    break;
                case "Half-Elf":
                    RaceDescription.text = "Half-Elves are a mix of human and elf. They have a higher resistance to magic and can adapt to any class.";
                    break;
                case "High Elf":
                    RaceDescription.text = "High Elves are known for their high intelligence and magic resistance. They are best suited for magic classes.";
                    break;
                case "Drow":
                    RaceDescription.text = "Drow are known for their high agility and dexterity. They are best suited for rogue classes.";
                    break;
                case "Wood Elf":
                    RaceDescription.text = "Wood Elves are known for their high agility and dexterity. They are best suited for rogue classes.";
                    break;
                case "Orc":
                    RaceDescription.text = "Orcs are known for their high strength and constitution. They are best suited for warrior classes.";
                    break;
                case "Dwarf":
                    RaceDescription.text = "Dwarves are known for their high constitution and strength. They are best suited for warrior classes.";
                    break;
                case "Halfling":
                    RaceDescription.text = "Halflings are known for their high agility and dexterity. They are best suited for rogue classes.";
                    break;
                case "Dragonkin":
                    RaceDescription.text = "Dragonkin are known for their high strength and constitution. They are best suited for warrior classes.";
                    break;
                case "Half-Beast":
                    RaceDescription.text = "Half-Beasts are known for their high strength and constitution. They are best suited for warrior classes.";
                    break;
                case "Merfolk":
                    RaceDescription.text = "Merfolk are known for their high agility and dexterity. They are best suited for rogue classes.";
                    break;
            }
        }
    }
}