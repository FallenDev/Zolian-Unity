using Assets.Scripts.Entity;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.CharacterSelection
{
    public class CharacterSelected : MonoBehaviour
    {
        public TextMeshProUGUI characterLevelText;
        public TextMeshProUGUI characterBaseClassText;
        public TextMeshProUGUI characterAdvClassText;
        public TextMeshProUGUI characterJobText;
        public TextMeshProUGUI characterHealthText;
        public TextMeshProUGUI characterManaText;

        public PlayerSelection selectedPlayer;
        private GameObject instantiatedCharacter;
        private Sex lastSexChosen;
        private const string characterSceneName = "CharacterSelectionDisplay";
        private bool isSceneLoaded = false;

        [Header("Character Body Parts")]
        private GameObject Hair;
        private GameObject HairBangs;
        private GameObject HairBeard;
        private GameObject HairMustache;
        private Color HairColor;
        private Color HairHighlightColor;
        private Color EyeColor;
        private Color SkinColor;

        [Header("Character BodyPart References")]
        private GameObject BaseHead;
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
        private GameObject Head;
        private SkinnedMeshRenderer _headBlendShapes;

        public static CharacterSelected Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void SelectCharacter(PlayerSelection player)
        {
            selectedPlayer = player;
            characterLevelText.text = $"Level: {player.Level}";
            characterBaseClassText.text = $"Class: {player.BaseClass}";
            characterAdvClassText.text = $"Adv Class: {player.AdvClass}";
            characterJobText.text = $"Job: {player.Job}";
            characterHealthText.text = $"Health: {player.Health}";
            characterManaText.text = $"Mana: {player.Mana}";
            CreationAndAuthManager.Instance.loginButton.gameObject.SetActive(true);
            LoadCharacterScene();
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
            else
            {
                UpdateCharacterDisplay();
            }
        }

        /// <summary>
        /// Unloads CharacterCreationDisplay Scene during character selection
        /// </summary>
        public void UnloadCharacterScene()
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
                UpdateCharacterDisplay();
        }

        /// <summary>
        /// Updates Character Prefab based on current selected values 
        /// </summary>
        private void UpdateCharacterDisplay()
        {
            if (CharacterDisplayManager.Instance == null) return;
            var characterSO = GetCurrentCharacterSO();
            if (characterSO == null) return;

            if (instantiatedCharacter != null)
                Destroy(instantiatedCharacter);
            
            var loadedCharacter = CharacterPrefabLoader.GetPrefabForSelection(selectedPlayer.Sex);
            instantiatedCharacter = CharacterDisplayManager.Instance.LoadCharacter(loadedCharacter);
            Hair = characterSO.Hair[selectedPlayer.Hair];
            HairBangs = characterSO.HairBangs[selectedPlayer.Bangs];
            HairBeard = characterSO.HairBeard[selectedPlayer.Beard];
            HairMustache = characterSO.HairMustache[selectedPlayer.Mustache];
            HairColor = characterSO.HairColor[selectedPlayer.HairColor];
            HairHighlightColor = characterSO.HairHighlightColor[selectedPlayer.HairHighlightColor];
            EyeColor = characterSO.EyeColor[selectedPlayer.EyeColor];
            SkinColor = characterSO.SkinColor[selectedPlayer.SkinColor];
            AssignCharacterBodyParts(instantiatedCharacter);
        }

        /// <summary>
        /// Sets the current Scriptable Object based on race
        /// </summary>
        public CharacterSO GetCurrentCharacterSO()
        {
            return selectedPlayer.Race switch
            {
                Race.Human => CreationAndAuthManager.Instance.HumanCharacterSO,
                Race.HalfElf => CreationAndAuthManager.Instance.HalfElfCharacterSO,
                Race.HighElf => CreationAndAuthManager.Instance.HighElfCharacterSO,
                Race.DarkElf => CreationAndAuthManager.Instance.DrowCharacterSO,
                Race.WoodElf => CreationAndAuthManager.Instance.WoodElfCharacterSO,
                Race.Orc => CreationAndAuthManager.Instance.OrcCharacterSO,
                Race.Dwarf => CreationAndAuthManager.Instance.DwarfCharacterSO,
                Race.Halfling => CreationAndAuthManager.Instance.HalflingCharacterSO,
                Race.Dragonkin => CreationAndAuthManager.Instance.DragonkinCharacterSO,
                Race.HalfBeast => CreationAndAuthManager.Instance.HalfBeastCharacterSO,
                Race.Merfolk => CreationAndAuthManager.Instance.MerfolkCharacterSO,
                _ => CreationAndAuthManager.Instance.HumanCharacterSO
            };
        }

        /// <summary>
        /// Assigns body parts to script's variables for easy access
        /// </summary>
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

            Head = character.transform.Find("Head").gameObject;
            if (Head != null)
            {
                _headBlendShapes = Head.GetComponent<SkinnedMeshRenderer>();
            }
            else
            {
                Debug.LogError("Head object with SkinnedMeshRenderer not found!");
            }
            
            AssignCharacterCustomizations();
            //AssignCharacterGearByClass(character);
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

                if (selectedPlayer.Race is Race.Merfolk && colorData.sharedMaterial.name.Contains("mat_scales"))
                {
                    colorData.mainColor_A = CreationAndAuthManager.Instance.ScalesSO.ScalesColor[0]; ;
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

            switch (selectedPlayer.Race)
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
    }
}