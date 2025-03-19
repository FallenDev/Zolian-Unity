using Assets.Scripts.CharacterSelection;
using Assets.Scripts.Entity;
using Assets.Scripts.Models;
using Assets.Scripts.Network;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
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
        public TMP_InputField CharacterName;
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

        private BaseClass classChosen;
        private Race raceChosen;
        private Sex genderChosen;
        private const string characterSceneName = "CharacterCreationDisplay";
        private bool isSceneLoaded = false;

        [Header("Character BodyParts")] public GameObject BaseHead;
        public GameObject ArmsLower;
        public GameObject ArmsUpper;
        public GameObject Feet;
        public GameObject Hands;
        public GameObject Hips;
        public GameObject LegsLower;
        public GameObject LegsUpper;
        public GameObject LegsKnee;
        public GameObject Shoulders;
        public GameObject Neck;
        public GameObject Chest;
        public GameObject Abdomen;
        public SkinnedMeshRenderer HeadBlendShapes;

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

            if (CharacterName == null || string.IsNullOrEmpty(CharacterName.text))
            {
                continueButton.gameObject.SetActive(false);
            }
            else
            {
                continueButton.gameObject.SetActive(CharacterName.text.Length >= 3);
            }
        }

        public void OnCreateButtonClick()
        {
            PopupManager.Instance.systemPopup.SetActive(false);
            CharacterSelectionGroup.gameObject.SetActive(false);
            CharacterCreationGroup.gameObject.SetActive(true);
            LoadCharacterScene();
        }

        private void OnDeleteButtonClick()
        {
            // Show character deletion
        }

        private void OnCancelButtonClick()
        {
            CharacterName.text = "";
            BerserkerToggle.isOn = true;
            MaleToggle.isOn = true;
            RaceDropdown.value = 0;
            CharacterCreationGroup.gameObject.SetActive(false);
            // Only show SelectionPanel if characters exist to select from
            CharacterListPanel.SetActive(LoginClient.Instance.cachedPlayers.Count >= 1);
            CharacterSelectionGroup.gameObject.SetActive(true);
            UnloadCharacterScene();
        }

        public void OnLoginButtonClick()
        {
            Debug.Log($"OnLoginButtonClick called");
        }

        private void OnGenderToggle(bool toggled)
        {
            // ToDo: Switch model to show male or female based on which is chosen. 
            if (FemaleToggle.isOn)
            {
                genderChosen = Sex.Female;
            }
            else if (MaleToggle.isOn)
            {
                genderChosen = Sex.Male;
            }

            UpdateCharacterDisplay();
        }

        private void OnContinueButtonClick() => LoginClient.Instance.SendCharacterCreation(LoginClient.Instance.SteamId, CharacterName.text, classChosen, raceChosen, genderChosen);
        public void CharacterFinalized() => OnCancelButtonClick();

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
                classChosen = BaseClass.Berserker;
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
                classChosen = BaseClass.Defender;
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
                classChosen = BaseClass.Assassin;
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
                classChosen = BaseClass.Cleric;
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
                classChosen = BaseClass.Arcanus;
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
                classChosen = BaseClass.Monk;
            }

            UpdateCharacterDisplay();
        }

        private void OnRaceDropdownChange(TMP_Dropdown dropdown)
        {
            switch (dropdown.options[dropdown.value].text)
            {
                case "Human":
                    RaceDescription.text = "Humans are well-rounded and can adapt to any class. They have no specific strengths or weaknesses.";
                    raceChosen = Race.Human;
                    break;
                case "Half-Elf":
                    RaceDescription.text = "Half-Elves are a mix of human and elf. They have a higher resistance to magic and can adapt to any class.";
                    raceChosen = Race.HalfElf;
                    break;
                case "High Elf":
                    RaceDescription.text = "High Elves are known for their high intelligence and magic resistance. They are best suited for magic classes.";
                    raceChosen = Race.HighElf;
                    break;
                case "Drow":
                    RaceDescription.text = "Drow are known for their high agility and dexterity. They are best suited for rogue classes.";
                    raceChosen = Race.DarkElf;
                    break;
                case "Wood Elf":
                    RaceDescription.text = "Wood Elves are known for their high agility and dexterity. They are best suited for rogue classes.";
                    raceChosen = Race.WoodElf;
                    break;
                case "Orc":
                    RaceDescription.text = "Orcs are known for their high strength and constitution. They are best suited for warrior classes.";
                    raceChosen = Race.Orc;
                    break;
                case "Dwarf":
                    RaceDescription.text = "Dwarves are known for their high constitution and strength. They are best suited for warrior classes.";
                    raceChosen = Race.Dwarf;
                    break;
                case "Halfling":
                    RaceDescription.text = "Halflings are known for their high agility and dexterity. They are best suited for rogue classes.";
                    raceChosen = Race.Halfling;
                    break;
                case "Dragonkin":
                    RaceDescription.text = "Dragonkin are known for their high strength and constitution. They are best suited for warrior classes.";
                    raceChosen = Race.Dragonkin;
                    break;
                case "Half-Beast":
                    RaceDescription.text = "Half-Beasts are known for their high strength and constitution. They are best suited for warrior classes.";
                    raceChosen = Race.HalfBeast;
                    break;
                case "Merfolk":
                    RaceDescription.text = "Merfolk are known for their high agility and dexterity. They are best suited for rogue classes.";
                    raceChosen = Race.Merfolk;
                    break;
            }

            UpdateCharacterDisplay();
        }

        private void LoadCharacterScene()
        {
            if (!isSceneLoaded)
            {
                SceneManager.LoadScene(characterSceneName, LoadSceneMode.Additive);
                isSceneLoaded = true;
                Invoke(nameof(FindCharacterDisplayManager), 0.5f);
            }
        }

        private void UnloadCharacterScene()
        {
            if (isSceneLoaded)
            {
                SceneManager.UnloadSceneAsync(characterSceneName);
                isSceneLoaded = false;
            }
        }

        private void FindCharacterDisplayManager()
        {
            if (CharacterDisplayManager.Instance != null)
                UpdateCharacterDisplay();
        }

        private void UpdateCharacterDisplay()
        {
            if (CharacterDisplayManager.Instance == null) return;
            GameObject selectedPrefab = CharacterPrefabLoader.GetPrefab(raceChosen, genderChosen);
            GameObject instantiatedCharacter = CharacterDisplayManager.Instance.LoadCharacter(selectedPrefab);
            AssignCharacterBodyParts(instantiatedCharacter);
        }

        private void AssignCharacterBodyParts(GameObject character)
        {
            if (character == null)
            {
                Debug.LogError("Character prefab not instantiated correctly.");
                return;
            }

            var headTransform = character.transform.Find("Armature_M/RL_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02/CC_Base_NeckTwist01/CC_Base_NeckTwist02/CC_Base_Head");
            if (headTransform == null)
            {
                headTransform = character.transform.Find("Armature_F/RL_BoneRoot/CC_Base_Hip/CC_Base_Waist/CC_Base_Spine01/CC_Base_Spine02/CC_Base_NeckTwist01/CC_Base_NeckTwist02/CC_Base_Head");
                if (headTransform == null)
                {
                    Debug.LogError("Head transform not found!");
                    return;
                }
            }

            BaseHead = headTransform.gameObject;
            ArmsLower = character.transform.Find("CC_body_arms_lower").gameObject;
            ArmsUpper = character.transform.Find("CC_body_arms_upper").gameObject;
            Feet = character.transform.Find("CC_body_feet").gameObject;
            Hands = character.transform.Find("CC_body_hands").gameObject;
            Hips = character.transform.Find("CC_body_hips").gameObject;
            LegsLower = character.transform.Find("CC_body_legs_lower").gameObject;
            LegsUpper = character.transform.Find("CC_body_legs_upper").gameObject;
            LegsKnee = character.transform.Find("CC_body_legs_knee").gameObject;
            Shoulders = character.transform.Find("CC_body_shoulders").gameObject;
            Neck = character.transform.Find("CC_body_neck").gameObject;
            Chest = character.transform.Find("CC_body_chest").gameObject;
            Abdomen = character.transform.Find("CC_body_abdomen").gameObject;

            var headObject = character.transform.Find("Head");
            if (headObject != null)
            {
                HeadBlendShapes = headObject.GetComponent<SkinnedMeshRenderer>();
            }
            else
            {
                Debug.LogError("Head object with SkinnedMeshRenderer not found!");
            }

            AssignCharacterGearByClass(character);
        }

        private void AssignCharacterGearByClass(GameObject character)
        {
            UnEquip(character);

            switch (classChosen)
            {
                case BaseClass.Berserker:
                    {
                        character.transform.Find("Equipment/top.04_knight").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.02").gameObject.SetActive(true);
                    }
                    break;
                case BaseClass.Defender:
                    {
                        character.transform.Find("Equipment/top.05_knight_full").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.08_knight").gameObject.SetActive(true);
                    }
                    break;
                case BaseClass.Assassin:
                    {
                        character.transform.Find("Equipment/top.08_ranger_coat").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.05").gameObject.SetActive(true);
                    }
                    break;
                case BaseClass.Cleric:
                    {
                        character.transform.Find("Equipment/dress.05_cleric").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.01").gameObject.SetActive(true);
                    }
                    break;
                case BaseClass.Arcanus:
                    {
                        character.transform.Find("Equipment/dress.04_witch").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.01").gameObject.SetActive(true);
                    }
                    break;
                case BaseClass.Monk:
                    {
                        character.transform.Find("Equipment/top.06_warrior").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.06").gameObject.SetActive(true);
                    }
                    break;
                default:
                    {
                        character.transform.Find("Equipment/top.03_sleeveless_shirt").gameObject.SetActive(true);
                        character.transform.Find("Equipment/bottoms.02").gameObject.SetActive(true);
                    }
                    break;
            }

        }

        private void UnEquip(GameObject character)
        {
            character.transform.Find("Equipment/top.01_long_sleeve_shirt").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.02_short_sleeve_shirt").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.03_sleeveless_shirt").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.04_knight").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.05_knight_full").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.06_warrior").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.07_rogue-coat").gameObject.SetActive(false);
            character.transform.Find("Equipment/top.08_ranger_coat").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.01").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.02").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.03").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.04").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.05").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.06").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.07").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.08_knight").gameObject.SetActive(false);
            character.transform.Find("Equipment/bottoms.09").gameObject.SetActive(false);
            character.transform.Find("Equipment/dress.01").gameObject.SetActive(false);
            character.transform.Find("Equipment/dress.02").gameObject.SetActive(false);
            character.transform.Find("Equipment/dress.03").gameObject.SetActive(false);
            character.transform.Find("Equipment/dress.04_witch").gameObject.SetActive(false);
            character.transform.Find("Equipment/dress.05_cleric").gameObject.SetActive(false);
        }
    }
}