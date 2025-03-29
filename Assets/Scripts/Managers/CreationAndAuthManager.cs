using System;

using Assets.Scripts.CharacterSelection;
using Assets.Scripts.Entity;
using Assets.Scripts.Models;
using Assets.Scripts.Network;

using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Managers
{
    public class CreationAndAuthManager : MonoBehaviour
    {
        [Header("Action Buttons")]
        public Button createButton;
        public Button deleteButton;
        public Button cancelButton;
        public Button loginButton;
        public Button continueButton;

        [Header("Object References")]
        public GameObject CharacterListPanel;
        public TextMeshProUGUI RaceDescription;
        public TextMeshProUGUI ClassDescription;
        public TMP_InputField CharacterName;
        public Toggle CustomizeToggle;
        public GameObject CustomizePanel;
        public Toggle ClassSelectionToggle;
        public GameObject ClassSelectionPanel;
        public Button RandomizeLooks;
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
        public Slider HairSlider;
        public Slider HairBangsSlider;
        public Slider HairBeardSlider;
        public Slider HairMustacheSlider;
        public Slider HairColorSlider;
        public Slider HairHighlightSlider;
        public Slider EyeColorSlider;
        public Slider SkinToneSlider;
        private short lastHairIndex = -1;
        private short lastBangsIndex = -1;
        private short lastBeardIndex = -1;
        private short lastMustacheIndex = -1;
        private short lastHairColorIndex = -1;
        private short lastHairHighlightColorIndex = -1;
        private short lastEyeColorIndex = -1;
        private short lastSkinToneIndex = -1;
        private short randHairIndex;
        private short randBangsIndex;
        private short randBeardIndex;
        private short randMustacheIndex;
        private short randHairColorIndex;
        private short randHairHighlightColorIndex;
        private short randEyeColorIndex;
        private short randSkinToneIndex;

        // These are the groups that will be shown/hidden
        [Header("Character Screens")]
        public GameObject CharacterSelectionGroup;
        public GameObject CharacterCreationGroup;

        private BaseClass classChosen;
        private Race raceChosen;
        private Sex genderChosen;
        private Race lastRaceChosen;
        private Sex lastSexChosen;
        private GameObject instantiatedCharacter;
        private const string characterSceneName = "CharacterCreationDisplay";
        private bool isSceneLoaded = false;

        [Header("Character BodyPart References")]
        private GameObject BaseHead; // GameObject related to hair, hats, etc.
        private GameObject ArmsLower;
        private GameObject ArmsUpper;
        private GameObject Feet;
        private GameObject Hands;
        private GameObject Hips;
        private GameObject LegsLower;
        private GameObject LegsUpper;
        private GameObject LegsKnee;
        private GameObject Shoulders;
        private GameObject Neck;
        private GameObject Chest;
        private GameObject Abdomen;
        private GameObject Head; // BlendShapes parent, used for colorization
        private SkinnedMeshRenderer _headBlendShapes;

        [Header("Character Customizable Options")]
        private GameObject Hair;
        private GameObject HairBangs;
        private GameObject HairBeard;
        private GameObject HairMustache;
        private Color HairColor;
        private Color HairHighlightColor;
        private Color EyeColor;
        private Color SkinColor;

        [Header("Character Scriptable Object Data")]
        public CharacterSO HumanCharacterSO;
        public CharacterSO HalfElfCharacterSO;
        public CharacterSO HighElfCharacterSO;
        public CharacterSO DrowCharacterSO;
        public CharacterSO WoodElfCharacterSO;
        public CharacterSO OrcCharacterSO;
        public CharacterSO DwarfCharacterSO;
        public CharacterSO HalflingCharacterSO;
        public CharacterSO DragonkinCharacterSO;
        public CharacterSO HalfBeastCharacterSO;
        public CharacterSO MerfolkCharacterSO;
        public ScalesSO ScalesSO;

        public static CreationAndAuthManager Instance;

        private void Awake()
        {
            Instance = this;
            createButton.onClick.AddListener(OnCreateButtonClick);
            deleteButton.onClick.AddListener(OnDeleteButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
            loginButton.onClick.AddListener(OnLoginButtonClick);
            continueButton.onClick.AddListener(OnContinueButtonClick);
            RandomizeLooks.onClick.AddListener(OnRandomLookClick);
            CustomizeToggle.onValueChanged.AddListener(OnCustomizeToggle);
            ClassSelectionToggle.onValueChanged.AddListener(OnClassSelectionToggle);
            MaleToggle.onValueChanged.AddListener(OnGenderToggle);
            FemaleToggle.onValueChanged.AddListener(OnGenderToggle);
            BerserkerToggle.onValueChanged.AddListener(OnClassToggle);
            DefenderToggle.onValueChanged.AddListener(OnClassToggle);
            AssassinToggle.onValueChanged.AddListener(OnClassToggle);
            ClericToggle.onValueChanged.AddListener(OnClassToggle);
            ArcanusToggle.onValueChanged.AddListener(OnClassToggle);
            MonkToggle.onValueChanged.AddListener(OnClassToggle);
            RaceDropdown.onValueChanged.AddListener(delegate { OnRaceDropdownChange(RaceDropdown); });
            HairSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            HairBangsSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            HairBeardSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            HairMustacheSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            HairColorSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            HairHighlightSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            EyeColorSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
            SkinToneSlider.onValueChanged.AddListener(delegate { OnCustomizationSliderChanged(); });
        }

        private void Start()
        {
            OnClassSelectionToggle(false);
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
            CharacterSelected.Instance.UnloadCharacterScene();
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

        /// <summary>
        /// Toggles Character's Gender
        /// </summary>
        private void OnGenderToggle(bool toggled)
        {
            if (FemaleToggle.isOn)
            {
                genderChosen = Sex.Female;
            }
            else if (MaleToggle.isOn)
            {
                genderChosen = Sex.Male;
            }

            UpdateCharacterDisplay(false);
        }

        private void OnContinueButtonClick() => LoginClient.Instance.SendCharacterCreation(LoginClient.Instance.SteamId, CharacterName.text, classChosen, raceChosen, 
            genderChosen, lastHairIndex, lastBangsIndex, lastBeardIndex, lastMustacheIndex, lastHairColorIndex, lastHairHighlightColorIndex, lastEyeColorIndex, lastSkinToneIndex);
        public void CharacterFinalized() => OnCancelButtonClick();

        #region Customizations

        /// <summary>
        /// Toggles Character Visual Customizations Pane
        /// </summary>
        private void OnCustomizeToggle(bool toggled)
        {
            ClassSelectionPanel.SetActive(false);
            CustomizePanel.SetActive(true);
        }

        /// <summary>
        /// Randomizes a character's hair, facial hair, and other customizable options
        /// </summary>
        private void OnRandomLookClick()
        {
            var characterSO = GetCurrentCharacterSO();
            if (characterSO == null) return;
            ConfigureSliders(characterSO);

            randHairIndex = (short)UnityEngine.Random.Range(0, characterSO.Hair.Length);
            randBangsIndex = (short)UnityEngine.Random.Range(0, characterSO.HairBangs.Length);
            randBeardIndex = (short)UnityEngine.Random.Range(0, characterSO.HairBeard.Length);
            randMustacheIndex = (short)UnityEngine.Random.Range(0, characterSO.HairMustache.Length);
            randHairColorIndex = (short)UnityEngine.Random.Range(0, characterSO.HairColor.Length);
            randHairHighlightColorIndex = (short)UnityEngine.Random.Range(0, characterSO.HairHighlightColor.Length);
            randEyeColorIndex = (short)UnityEngine.Random.Range(0, characterSO.EyeColor.Length);
            randSkinToneIndex = (short)UnityEngine.Random.Range(0, characterSO.SkinColor.Length);
            Hair = characterSO.Hair[randHairIndex];
            HairBangs = characterSO.HairBangs[randBangsIndex];
            HairBeard = characterSO.HairBeard[randBeardIndex];
            HairMustache = characterSO.HairMustache[randMustacheIndex];
            HairColor = characterSO.HairColor[randHairColorIndex];
            HairHighlightColor = characterSO.HairHighlightColor[randHairHighlightColorIndex];
            EyeColor = characterSO.EyeColor[randEyeColorIndex];
            SkinColor = characterSO.SkinColor[randSkinToneIndex];

            SetSlidersAfterRandom();
            UpdateCharacterDisplay(true);
        }

        /// <summary>
        /// Updates character's customizations based on slider values
        /// </summary>
        private void OnCustomizationSliderChanged()
        {
            var characterSO = GetCurrentCharacterSO();
            if (characterSO == null) return;
            ConfigureSliders(characterSO);

            var hairIndex = (short)Mathf.Clamp(HairSlider.value, 0, characterSO.Hair.Length - 1);
            var bangsIndex = (short)Mathf.Clamp(HairBangsSlider.value, 0, characterSO.HairBangs.Length - 1);
            var beardIndex = (short)Mathf.Clamp(HairBeardSlider.value, 0, characterSO.HairBeard.Length - 1);
            var mustacheIndex = (short)Mathf.Clamp(HairMustacheSlider.value, 0, characterSO.HairMustache.Length - 1);
            var hairColorIndex = (short)Mathf.Clamp(HairColorSlider.value, 0, characterSO.HairColor.Length - 1);
            var hairHighlightIndex = (short)Mathf.Clamp(HairHighlightSlider.value, 0, characterSO.HairHighlightColor.Length - 1);
            var eyeColorIndex = (short)Mathf.Clamp(EyeColorSlider.value, 0, characterSO.EyeColor.Length - 1);
            var skinToneIndex = (short)Mathf.Clamp(SkinToneSlider.value, 0, characterSO.SkinColor.Length - 1);

            if (hairIndex != lastHairIndex)
            {
                Hair = characterSO.Hair[hairIndex];
                lastHairIndex = hairIndex;
            }

            if (bangsIndex != lastBangsIndex)
            {
                HairBangs = characterSO.HairBangs[bangsIndex];
                lastBangsIndex = bangsIndex;
            }

            if (beardIndex != lastBeardIndex)
            {
                HairBeard = characterSO.HairBeard[beardIndex];
                lastBeardIndex = beardIndex;
            }

            if (mustacheIndex != lastMustacheIndex)
            {
                HairMustache = characterSO.HairMustache[mustacheIndex];
                lastMustacheIndex = mustacheIndex;
            }

            if (hairColorIndex != lastHairColorIndex)
            {
                HairColor = characterSO.HairColor[hairColorIndex];
                lastHairColorIndex = hairColorIndex;
            }

            if (hairHighlightIndex != lastHairHighlightColorIndex)
            {
                HairHighlightColor = characterSO.HairHighlightColor[hairHighlightIndex];
                lastHairHighlightColorIndex = hairHighlightIndex;
            }

            if (eyeColorIndex != lastEyeColorIndex)
            {
                EyeColor = characterSO.EyeColor[eyeColorIndex];
                lastEyeColorIndex = eyeColorIndex;
            }

            if (skinToneIndex != lastSkinToneIndex)
            {
                SkinColor = characterSO.SkinColor[skinToneIndex];
                lastSkinToneIndex = skinToneIndex;
            }

            UpdateCharacterDisplay(true);
        }

        /// <summary>
        /// Sets slider's max values on CharacterSO change
        /// </summary>
        private void ConfigureSliders(CharacterSO characterSO)
        {
            HairSlider.maxValue = characterSO.Hair.Length - 1;
            HairBangsSlider.maxValue = characterSO.HairBangs.Length - 1;
            HairBeardSlider.maxValue = characterSO.HairBeard.Length - 1;
            HairMustacheSlider.maxValue = characterSO.HairMustache.Length - 1;

            HairColorSlider.maxValue = characterSO.HairColor.Length - 1;
            HairHighlightSlider.maxValue = characterSO.HairHighlightColor.Length - 1;
            EyeColorSlider.maxValue = characterSO.EyeColor.Length - 1;
            SkinToneSlider.maxValue = characterSO.SkinColor.Length - 1;
        }

        /// <summary>
        /// Set slider's current values on RandomButton()
        /// </summary>
        private void SetSlidersAfterRandom()
        {
            HairSlider.value = randHairIndex;
            HairBangsSlider.value = randBangsIndex;
            HairBeardSlider.value = randBeardIndex;
            HairMustacheSlider.value = randMustacheIndex;
            HairColorSlider.value = randHairColorIndex;
            HairHighlightSlider.value = randHairHighlightColorIndex;
            EyeColorSlider.value = randEyeColorIndex;
            SkinToneSlider.value = randSkinToneIndex;
        }

        #endregion

        /// <summary>
        /// Toggles Class Selection Pane
        /// </summary>
        private void OnClassSelectionToggle(bool toggled)
        {
            CustomizePanel.SetActive(false);
            ClassSelectionPanel.SetActive(true);
        }

        /// <summary>
        /// Sets the current Scriptable Object based on race
        /// </summary>
        private CharacterSO GetCurrentCharacterSO()
        {
            return raceChosen switch
            {
                Race.Human => HumanCharacterSO,
                Race.HalfElf => HalfElfCharacterSO,
                Race.HighElf => HighElfCharacterSO,
                Race.DarkElf => DrowCharacterSO,
                Race.WoodElf => WoodElfCharacterSO,
                Race.Orc => OrcCharacterSO,
                Race.Dwarf => DwarfCharacterSO,
                Race.Halfling => HalflingCharacterSO,
                Race.Dragonkin => DragonkinCharacterSO,
                Race.HalfBeast => HalfBeastCharacterSO,
                Race.Merfolk => MerfolkCharacterSO,
                _ => HumanCharacterSO
            };
        }

        /// <summary>
        /// Toggles Character's Class Selection
        /// </summary>
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

            UpdateCharacterDisplay(false);
        }

        /// <summary>
        /// Sets Character's Race Selection
        /// </summary>
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

            UpdateCharacterDisplay(false);
        }

        /// <summary>
        /// Loads CharacterCreationDisplay Scene for visually seeing character during character creation
        /// </summary>
        private void LoadCharacterScene()
        {
            if (!isSceneLoaded)
            {
                SceneManager.LoadScene(characterSceneName, LoadSceneMode.Additive);
                isSceneLoaded = true;
                Invoke(nameof(FindCharacterDisplayManager), 0.15f);
            }
        }

        /// <summary>
        /// Unloads CharacterCreationDisplay Scene during character selection
        /// </summary>
        private void UnloadCharacterScene()
        {
            if (isSceneLoaded)
            {
                // Destroy the instantiated character to free up resources
                if (instantiatedCharacter != null)
                {
                    Destroy(instantiatedCharacter);
                    instantiatedCharacter = null;
                }
                SceneManager.UnloadSceneAsync(characterSceneName);
                isSceneLoaded = false;
            }
        }

        /// <summary>
        /// Updates Character Display if CharacterDisplayManager is available
        /// </summary>
        private void FindCharacterDisplayManager()
        {
            if (CharacterDisplayManager.Instance != null)
                UpdateCharacterDisplay(false);
        }

        /// <summary>
        /// Updates Character Prefab based on current selected values 
        /// </summary>
        private void UpdateCharacterDisplay(bool customized)
        {
            if (CharacterDisplayManager.Instance == null) return;

            if (lastSexChosen != genderChosen || lastRaceChosen != raceChosen || instantiatedCharacter == null)
            {
                var selectedPrefab = CharacterPrefabLoader.GetPrefabForCreation(genderChosen);
                instantiatedCharacter = CharacterDisplayManager.Instance.LoadCharacter(selectedPrefab);
                lastSexChosen = genderChosen;
                lastRaceChosen = raceChosen;
                OnPrefabSetup();
                return;
            }

            AssignCharacterBodyParts(instantiatedCharacter, customized);
        }

        private void OnPrefabSetup()
        {
            var characterSO = GetCurrentCharacterSO();
            if (characterSO == null) return;
            ConfigureSliders(characterSO);

            randHairIndex = (short)UnityEngine.Random.Range(1, characterSO.Hair.Length);
            randBangsIndex = (short)UnityEngine.Random.Range(0, characterSO.HairBangs.Length);
            randHairColorIndex = (short)UnityEngine.Random.Range(0, characterSO.HairColor.Length);
            randHairHighlightColorIndex = (short)UnityEngine.Random.Range(0, characterSO.HairHighlightColor.Length);
            randEyeColorIndex = (short)UnityEngine.Random.Range(0, characterSO.EyeColor.Length);
            randSkinToneIndex = (short)UnityEngine.Random.Range(0, characterSO.SkinColor.Length);
            Hair = characterSO.Hair[randHairIndex];
            HairBangs = characterSO.HairBangs[randBangsIndex];
            HairBeard = characterSO.HairBeard[0];
            HairMustache = characterSO.HairMustache[0];
            HairColor = characterSO.HairColor[randHairColorIndex];
            HairHighlightColor = characterSO.HairHighlightColor[randHairHighlightColorIndex];
            EyeColor = characterSO.EyeColor[randEyeColorIndex];
            SkinColor = characterSO.SkinColor[randSkinToneIndex];

            SetSlidersAfterRandom();
            UpdateCharacterDisplay(true);
        }

        /// <summary>
        /// Assigns body parts to script's variables for easy access
        /// </summary>
        private void AssignCharacterBodyParts(GameObject character, bool customized)
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

            Head = character.transform.Find("Head").gameObject;
            if (Head != null)
            {
                _headBlendShapes = Head.GetComponent<SkinnedMeshRenderer>();
            }
            else
            {
                Debug.LogError("Head object with SkinnedMeshRenderer not found!");
            }

            if (customized)
                AssignCharacterCustomizations();
            AssignCharacterGearByClass(character);
        }

        /// <summary>
        /// Assigns character customizations like hair, beard, etc.
        /// </summary>
        private void AssignCharacterCustomizations()
        {
            if (BaseHead == null)
            {
                Debug.LogError("BaseHead reference not set!");
                return;
            }

            // Clear existing child objects from BaseHead
            foreach (Transform child in BaseHead.transform)
            {
                Destroy(child.gameObject);
            }

            // Instantiate and parent the new customizable hair parts
            if (Hair != null)
                InstantiateHairPart(Hair, BaseHead.transform);
            if (HairBangs != null)
                InstantiateHairPart(HairBangs, BaseHead.transform);
            if (HairBeard != null)
                InstantiateHairPart(HairBeard, BaseHead.transform);
            if (HairMustache != null)
                InstantiateHairPart(HairMustache, BaseHead.transform);
            ConfigureSkinColor();
            ConfigureHeadColor();
            ConfigureFacialBlendShapes();
        }

        /// <summary>
        /// Instantiates a hair part and sets its color
        /// </summary>
        private void InstantiateHairPart(GameObject hairPrefab, Transform parent)
        {
            if (hairPrefab == null) return;
            var hairPart = Instantiate(hairPrefab, parent);
            SetLayerRecursively(hairPart, LayerMask.NameToLayer("Player"));
            var colorCustomization = hairPart.GetComponent<ColorCustomization>();
            if (colorCustomization == null)
            {
                colorCustomization = hairPart.AddComponent<ColorCustomization>();
            }

            ConfigureHairColors(colorCustomization);
        }

        /// <summary>
        /// Sets the layer recursively for all child objects
        /// </summary>
        private void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (null == obj)
                return;

            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (null == child)
                    continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        /// <summary>
        /// Configures hair colors for the character
        /// </summary>
        private void ConfigureHairColors(ColorCustomization customization)
        {
            if (customization == null) return;

            foreach (var colorData in customization.m_Colors)
            {
                if (colorData.mainColor_A == colorData.mainColor_B)
                {
                    if (colorData.mainColor_A == colorData.mainColor_C)
                        colorData.mainColor_C = HairColor;
                    colorData.mainColor_B = HairColor;
                }
                else
                {
                    if (colorData.mainColor_A != colorData.mainColor_C)
                        colorData.mainColor_C = HairHighlightColor;
                    colorData.mainColor_B = HairHighlightColor;
                }

                colorData.mainColor_A = HairColor;
            }

            customization.ApplyColors();
        }

        private void ConfigureSkinColor()
        {
            SetSkinColor(ArmsLower);
            SetSkinColor(Hands);
            SetSkinColor(Feet);
            SetSkinColor(LegsLower);
            SetSkinColor(LegsUpper);
            SetSkinColor(LegsKnee);
            SetSkinColor(Hips);
            SetSkinColor(Chest);
            SetSkinColor(Abdomen);
            SetSkinColor(Shoulders);
            SetSkinColor(Neck);
            SetSkinColor(ArmsUpper);
        }

        private void SetSkinColor(GameObject bodyPart)
        {
            if (bodyPart == null || !bodyPart.gameObject.activeSelf)
                return;

            var colorCustomization = bodyPart.GetComponent<ColorCustomization>();
            if (colorCustomization == null)
                colorCustomization = bodyPart.AddComponent<ColorCustomization>();

            foreach (var colorData in colorCustomization.m_Colors)
            {
                colorData.mainColor_A = SkinColor;
            }

            colorCustomization.ApplyColors();
        }

        private void ConfigureHeadColor()
        {
            if (Head == null)
                return;

            var colorCustomization = Head.GetComponent<ColorCustomization>();
            if (colorCustomization == null)
            {
                colorCustomization = Head.AddComponent<ColorCustomization>();
            }

            foreach (var colorData in colorCustomization.m_Colors)
            {
                if (colorData.sharedMaterial == null)
                    continue;

                if (colorData.sharedMaterial.name.Contains("mat_face"))
                {
                    colorData.mainColor_A = SkinColor;
                }
                else if (colorData.sharedMaterial.name.Contains("mat_eye.002"))
                {
                    colorData.mainColor_B = EyeColor;
                }

                if (raceChosen is Race.Merfolk && colorData.sharedMaterial.name.Contains("mat_scales"))
                {
                    colorData.mainColor_A = ScalesSO.ScalesColor[0];;
                    colorData.metallic = 0.8f;
                    colorData.smoothness = 0.5f;
                }
                else
                {
                    colorData.mainColor_A = SkinColor;
                    colorData.metallic = 0.2f;
                    colorData.smoothness = 0.2f;
                }
            }

            colorCustomization.ApplyColors();
        }

        private void ConfigureFacialBlendShapes()
        {
            var tuckedEars = _headBlendShapes.sharedMesh.GetBlendShapeIndex("tuck_ears");
            var halfElfEars = _headBlendShapes.sharedMesh.GetBlendShapeIndex("half_elf_ears");
            var elvenEars = _headBlendShapes.sharedMesh.GetBlendShapeIndex("elf_ears");
            var merfolkEars = _headBlendShapes.sharedMesh.GetBlendShapeIndex("merfolk_ears");
            var scales = _headBlendShapes.sharedMesh.GetBlendShapeIndex("scales");
            var elvenFace = _headBlendShapes.sharedMesh.GetBlendShapeIndex("elf_eyes");

            switch (raceChosen)
            {
                case Race.UnDecided:
                case Race.Human:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
                case Race.HalfElf:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 100);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 50);
                    break;
                case Race.HighElf:
                case Race.DarkElf:
                case Race.WoodElf:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 100);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 100);
                    break;
                case Race.Orc:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 100);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 25);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 25);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
                case Race.Dwarf:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 75);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
                case Race.Halfling:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 100);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 40);
                    break;
                case Race.Dragonkin:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 100);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
                case Race.HalfBeast:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
                case Race.Merfolk:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 25);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
                default:
                    if (tuckedEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(tuckedEars, 0);
                    if (halfElfEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(halfElfEars, 0);
                    if (elvenEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenEars, 0);
                    if (merfolkEars != -1)
                        _headBlendShapes.SetBlendShapeWeight(merfolkEars, 0);
                    if (scales != -1)
                        _headBlendShapes.SetBlendShapeWeight(scales, 0);
                    if (elvenFace != -1)
                        _headBlendShapes.SetBlendShapeWeight(elvenFace, 0);
                    break;
            }
        }

        /// <summary>
        /// Assigns character gear based on the selected class
        /// </summary>
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

        /// <summary>
        /// UnEquips all character gear
        /// </summary>
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