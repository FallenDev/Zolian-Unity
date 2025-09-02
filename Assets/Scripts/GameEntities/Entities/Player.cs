using System;
using Assets.Scripts.GameEntities.Abstractions;
using Assets.Scripts.GameEntities.Behaviors;
using Assets.Scripts.GameEntities.ScriptableObjects;
using Assets.Scripts.Managers;
using Assets.Scripts.Models;
using Assets.Scripts.Network;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.FishNetPro;
using JohnStairs.RCC;
using JohnStairs.RCC.Character;
using JohnStairs.RCC.Character.Cam;
using JohnStairs.RCC.Character.Motor;
using JohnStairs.RCC.Inputs;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.GameEntities.Entities
{
    public class Player : Damageable, IPlayer, IPointerInfo, IClient
    {
        [Header("Base Properties")]
        public WorldClient Client { get; set; }
        public RPGMotorMMO LocalMotor { get; set; }
        public RemoteRPGMotor RemoteMotor { get; set; }

        public bool IsLocalPlayer => CharacterGameManager.Instance.Serial == Serial;
        public Animator Animator { get; private set; }
        public CharacterController CharacterController { get; private set; }
        public FishNetMovementController FishNetMovement { get; private set; }
        
        private readonly SyncVar<DateTime> _lastLogged = new SyncVar<DateTime>();
        private readonly SyncVar<string> _userName = new SyncVar<string>();
        private readonly SyncVar<ClassStage> _stage = new SyncVar<ClassStage>();
        private readonly SyncVar<JobClass> _job = new SyncVar<JobClass>();
        private readonly SyncVar<uint> _jobLevel = new SyncVar<uint>();
        private readonly SyncVar<BaseClass> _firstClass = new SyncVar<BaseClass>();
        private readonly SyncVar<BaseClass> _secondClass = new SyncVar<BaseClass>();
        private readonly SyncVar<bool> _gameMaster = new SyncVar<bool>();
        private readonly SyncVar<float> _cameraYaw = new SyncVar<float>();
        public Vector3 LastServerPos;

        // Public properties for SyncVars
        public DateTime LastLogged 
        { 
            get => _lastLogged.Value; 
            set => _lastLogged.Value = value; 
        }
        
        public string UserName 
        { 
            get => _userName.Value; 
            set => _userName.Value = value; 
        }
        
        public ClassStage Stage 
        { 
            get => _stage.Value; 
            set => _stage.Value = value; 
        }
        
        public JobClass Job 
        { 
            get => _job.Value; 
            set => _job.Value = value; 
        }
        
        public uint JobLevel 
        { 
            get => _jobLevel.Value; 
            set => _jobLevel.Value = value; 
        }
        
        public BaseClass FirstClass 
        { 
            get => _firstClass.Value; 
            set => _firstClass.Value = value; 
        }
        
        public BaseClass SecondClass 
        { 
            get => _secondClass.Value; 
            set => _secondClass.Value = value; 
        }
        
        public bool GameMaster 
        { 
            get => _gameMaster.Value; 
            set => _gameMaster.Value = value; 
        }
        
        public float CameraYaw 
        { 
            get => _cameraYaw.Value; 
            set => _cameraYaw.Value = value; 
        }

        [Header("Character Looks")]
        public CharacterSO CharacterSo { get; set; }
        
        private readonly SyncVar<Race> _race = new SyncVar<Race>();
        private readonly SyncVar<Sex> _gender = new SyncVar<Sex>();
        public GameObject Hair { get; set; }
        private readonly SyncVar<Color> _hairColor = new SyncVar<Color>();
        private readonly SyncVar<Color> _hairHighlightColor = new SyncVar<Color>();
        private readonly SyncVar<Color> _skinColor = new SyncVar<Color>();
        private readonly SyncVar<Color> _eyeColor = new SyncVar<Color>();
        public GameObject Beard { get; set; }
        public GameObject Mustache { get; set; }
        public GameObject Bangs { get; set; }

        // Public properties for character looks
        public Race Race 
        { 
            get => _race.Value; 
            set => _race.Value = value; 
        }
        
        public Sex Gender 
        { 
            get => _gender.Value; 
            set => _gender.Value = value; 
        }
        
        public Color HairColor 
        { 
            get => _hairColor.Value; 
            set => _hairColor.Value = value; 
        }
        
        public Color HairHighlightColor 
        { 
            get => _hairHighlightColor.Value; 
            set => _hairHighlightColor.Value = value; 
        }
        
        public Color SkinColor 
        { 
            get => _skinColor.Value; 
            set => _skinColor.Value = value; 
        }
        
        public Color EyeColor 
        { 
            get => _eyeColor.Value; 
            set => _eyeColor.Value = value; 
        }

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

        [Header("Character Controller")]
        public bool EnableFlying;
        public bool EnableMovement = true;
        public bool EnableRotation = true;
        public bool AllowSprinting = true;
        public GameObject Target;
        public bool EnableTargetLock;
        public LayerMask UiLayers = 32;
        public Vector3 InputDirection;
        public float Speed;

        // Equipment
        // Add any additional properties needed for in-game representation
        private void Awake()
        {
            Animator = GetComponent<Animator>();
            CharacterController = GetComponent<CharacterController>();
            FishNetMovement = GetComponent<FishNetMovementController>();
        }

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            
            if (base.Owner.IsLocalClient)
            {
                // Local player setup
                SetupLocalPlayer();
            }
            else
            {
                // Remote player setup
                SetupRemotePlayer();
            }
        }

        private void SetupLocalPlayer()
        {
            if (CharacterGameManager.Instance != null)
            {
                CharacterGameManager.Instance.LocalPlayer = this;
            }
            
            Client = WorldClient.Instance;
        }

        private void SetupRemotePlayer()
        {
            ConfigureRemoteComponents();
        }

        private void Update()
        {
            if (IsLocalPlayer && Input.GetKeyDown(KeyCode.L))
                EnableTargetLock = !EnableTargetLock;
        }

        #region Character Controller

        public virtual bool CanFly() => EnableFlying;

        public virtual bool CanMove() => EnableMovement;

        public virtual bool CanRotate() => EnableRotation;

        public virtual bool CanSprint() => AllowSprinting;

        // Just return the default value if there is no movement speed impairment (1.0f == 100%)
        public virtual float GetMovementSpeedModifier() => 1.0f; 

        public virtual Vector3 GetTargetPosition() => Target?.transform.position ?? Vector3.zero;

        public virtual bool LockedOnTarget() => EnableTargetLock;

        public virtual bool IsPointerOverGUI() => Utils.IsPointerOverGUI(UiLayers);

        #endregion

        #region Character Display

        public void InitializeLocalPlayerFromData(CharacterDataArgs playerArgs)
        {
            Serial = playerArgs.Serial;
            CurrentZoneId = 0;
            Position = playerArgs.Position;
            CameraYaw = playerArgs.CameraYaw;
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

        public void InitializeFromSpawnData(EntitySpawnArgs data)
        {
            ConfigureRemoteComponents();
            Serial = data.Serial;
            CurrentZoneId = 0;
            Position = data.Position;
            LastServerPos = data.Position;
            CameraYaw = data.CameraYaw;
            EntityLevel = data.EntityLevel;
            CurrentHp = data.CurrentHealth;
            MaxHp = data.MaxHealth;
            CurrentMp = data.CurrentMana;
            MaxMp = data.MaxMana;
            UserName = data.UserName;
            Race = data.Race;
            Gender = data.Sex;
            CharacterSo = GetCurrentCharacterSO(Race);
            UpdateCharacterDisplay(data);
        }

        private void ConfigureRemoteComponents()
        {
            // Disable Local RCC components if they exist
            var localMotor = GetComponent<RPGController>();
            if (localMotor) localMotor.enabled = false;
            
            var rpgCamera = GetComponentInChildren<RPGCamera>();
            if (rpgCamera) rpgCamera.enabled = false;
            
            var frustum = GetComponentInChildren<RPGViewFrustum>();
            if (frustum) frustum.enabled = false;
            var input = GetComponent<PlayerInput>();
            if (input) input.enabled = false;
            var networkMovement = GetComponent<NetworkMovementSenderForLocal>();
            if (networkMovement) networkMovement.enabled = false;
            LocalMotor = GetComponent<RPGMotorMMO>();
            if (LocalMotor) LocalMotor.enabled = false;

            // Enabled Remote
            RemoteMotor = GetComponent<RemoteRPGMotor>();
            if (RemoteMotor) RemoteMotor.enabled = true;
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
        /// Create character prefab based on spawned player
        /// </summary>
        private void UpdateCharacterDisplay(EntitySpawnArgs characterData)
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
