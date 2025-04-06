using System;

using Assets.Scripts.Entity.Abstractions;
using Assets.Scripts.Entity.ScriptableObjects;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;

using UnityEngine;

namespace Assets.Scripts.Entity.Entities
{
    public class Player : Damageable, IPlayer
    {
        public static Player PlayerInstance { get; private set; }

        // Base properties
        public WorldClient Client { get; set; }
        public DateTime LastLogged { get; set; }
        public string UserName { get; set; }
        public ClassStage Stage { get; set; }
        public JobClass Job { get; set; }
        public uint JobLevel { get; set; }
        public BaseClass FirstClass { get; set; }
        public BaseClass SecondClass { get; set; }
        public bool GameMaster { get; set; }

        [Header("Character Looks")]
        public CharacterSO CharacterSo { get; set; }
        public Race Race { get; set; }
        public Sex Gender { get; set; }
        public GameObject Hair { get; set; }
        public Color HairColor { get; set; }
        public Color HairHighlightColor { get; set; }
        public Color SkinColor { get; set; }
        public Color EyeColor { get; set; }
        public GameObject Beard { get; set; }
        public GameObject Mustache { get; set; }
        public GameObject Bangs { get; set; }

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

        // Equipment


        // Add any additional properties needed for in-game representation

        public Player() { }

        private void Awake()
        {
            PlayerInstance = this;
        }

        public void InitializeFromData(CharacterDataArgs playerArgs)
        {
            Serial = playerArgs.Serial;
            CurrentZoneId = 0;
            Position = playerArgs.Position;
            EntityLevel = playerArgs.EntityLevel;
            CurrentHp = playerArgs.CurrentHealth;
            MaxHp = playerArgs.MaxHealth;
            CurrentMp = playerArgs.CurrentMana;
            MaxMp = playerArgs.MaxMana;
            CurrentStamina = playerArgs.CurrentStamina;
            MaxStamina = playerArgs.MaxStamina;
            CurrentRage = playerArgs.CurrentRage;
            MaxRage = playerArgs.MaxRage;
            Reflex = playerArgs.Reflex;
            Fortitude = playerArgs.Fortitude;
            Will = playerArgs.Will;
            ArmorClass = playerArgs.ArmorClass;
            OffenseElement = playerArgs.OffenseElement;
            SecondaryOffensiveElement = playerArgs.SecondaryOffenseElement;
            DefenseElement = playerArgs.DefenseElement;
            SecondaryDefensiveElement = playerArgs.SecondaryDefenseElement;
            Str = playerArgs.Str;
            Int = playerArgs.Int;
            Wis = playerArgs.Wis;
            Con = playerArgs.Con;
            Dex = playerArgs.Dex;
            Luck = playerArgs.Luck;
            Regen = playerArgs.Regen;
            Dmg = playerArgs.Dmg;
            LastLogged = DateTime.Now;
            UserName = playerArgs.UserName;
            Stage = playerArgs.Stage;
            Job = playerArgs.Job;
            JobLevel = playerArgs.JobLevel;
            FirstClass = playerArgs.FirstClass;
            SecondClass = playerArgs.SecondClass;
            GameMaster = playerArgs.GameMaster;
            Race = playerArgs.Race;
            Gender = playerArgs.Sex;
            CharacterSo = GetCurrentCharacterSO(Race);
            UpdateCharacterDisplay(playerArgs);
        }

        /// <summary>
        /// Sets the current Scriptable Object based on race
        /// </summary>
        private CharacterSO GetCurrentCharacterSO(Race race)
        {
            return race switch
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

        #region Character Display

        /// <summary>
        /// Creates character prefab based on initial data
        /// </summary>
        private void UpdateCharacterDisplay(CharacterDataArgs characterData)
        {
            if (CharacterSo == null) return;
            Hair = CharacterSo.Hair[characterData.Hair];
            Bangs = CharacterSo.HairBangs[characterData.Bangs];
            Beard = CharacterSo.HairBeard[characterData.Beard];
            Mustache = CharacterSo.HairMustache[characterData.Mustache];
            HairColor = CharacterSo.HairColor[characterData.HairColor];
            HairHighlightColor = CharacterSo.HairHighlightColor[characterData.HairHighlightColor];
            EyeColor = CharacterSo.EyeColor[characterData.EyeColor];
            SkinColor = CharacterSo.SkinColor[characterData.SkinColor];
            AssignCharacterBodyParts(gameObject);
        }

        /// <summary>
        /// Assigns body parts to script's variables for easy access
        /// </summary>
        public void AssignCharacterBodyParts(GameObject character)
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

            AssignCharacterCustomizations(character);
        }

        /// <summary>
        /// Assigns character customizations like hair, beard, etc.
        /// </summary>
        private void AssignCharacterCustomizations(GameObject character)
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

            try
            {
                var hair = character.GetComponent<Player>().Hair;
                var bangs = character.GetComponent<Player>().Bangs;
                var beard = character.GetComponent<Player>().Beard;
                var mustache = character.GetComponent<Player>().Mustache;

                // Instantiate and parent the new customizable hair parts
                if (hair != null)
                    InstantiateHairPart(hair, BaseHead.transform);
                if (bangs != null)
                    InstantiateHairPart(bangs, BaseHead.transform);
                if (beard != null)
                    InstantiateHairPart(beard, BaseHead.transform);
                if (mustache != null)
                    InstantiateHairPart(mustache, BaseHead.transform);
                ConfigureSkinColor();
                ConfigureHeadColor();
                ConfigureFacialBlendShapes();
            }
            catch
            {
                // ignored
            }
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
                if (colorData.sharedMaterial.name.Contains("eye")) continue;
                if (colorData.sharedMaterial.name.Contains("mouth")) continue;
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

                if (colorData.sharedMaterial.name.Contains("face"))
                {
                    colorData.mainColor_A = SkinColor;
                }
                else if (colorData.sharedMaterial.name.Contains("eye"))
                {
                    colorData.mainColor_B = EyeColor;
                }

                if (Race is Race.Merfolk && colorData.sharedMaterial.name.Contains("scales"))
                {
                    colorData.mainColor_A = CreationAndAuthManager.Instance.ScalesSO.ScalesColor[0]; ;
                    colorData.metallic = 0.8f;
                    colorData.smoothness = 0.5f;
                }
                else if (colorData.sharedMaterial.name.Contains("scales"))
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

            switch (Race)
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

        #endregion
    }
}
