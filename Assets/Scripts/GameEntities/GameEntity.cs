using System;
using Assets.Scripts.Models;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Assets.Scripts.GameEntities
{
    public class GameEntity : NetworkBehaviour
    {
        private readonly SyncVar<Guid> _serial = new SyncVar<Guid>();
        private readonly SyncVar<uint> _currentZoneId = new SyncVar<uint>();
        private readonly SyncVar<Vector3> _position = new SyncVar<Vector3>();
        private readonly SyncVar<uint> _entityLevel = new SyncVar<uint>();

        // Health & Mana
        private readonly SyncVar<long> _currentHp = new SyncVar<long>();
        private readonly SyncVar<long> _maxHp = new SyncVar<long>();
        private readonly SyncVar<long> _currentMp = new SyncVar<long>();
        private readonly SyncVar<long> _maxMp = new SyncVar<long>();

        // Resources
        private readonly SyncVar<uint> _currentStamina = new SyncVar<uint>();
        private readonly SyncVar<uint> _maxStamina = new SyncVar<uint>();
        private readonly SyncVar<uint> _currentRage = new SyncVar<uint>();
        private readonly SyncVar<uint> _maxRage = new SyncVar<uint>();

        // Saving Throws
        private readonly SyncVar<double> _reflex = new SyncVar<double>();
        private readonly SyncVar<double> _fortitude = new SyncVar<double>();
        private readonly SyncVar<double> _will = new SyncVar<double>();

        // Armor - How much damage you resist from all attacks (Physical, Magical, etc)
        private readonly SyncVar<int> _armorClass = new SyncVar<int>();
        private readonly SyncVar<Element> _offenseElement = new SyncVar<Element>();
        private readonly SyncVar<Element> _secondaryOffensiveElement = new SyncVar<Element>();
        private readonly SyncVar<Element> _defenseElement = new SyncVar<Element>();
        private readonly SyncVar<Element> _secondaryDefensiveElement = new SyncVar<Element>();

        // Stats
        private readonly SyncVar<int> _str = new SyncVar<int>();
        private readonly SyncVar<int> _int = new SyncVar<int>();
        private readonly SyncVar<int> _wis = new SyncVar<int>();
        private readonly SyncVar<int> _con = new SyncVar<int>();
        private readonly SyncVar<int> _dex = new SyncVar<int>();
        private readonly SyncVar<int> _luck = new SyncVar<int>();
        private readonly SyncVar<uint> _regen = new SyncVar<uint>();
        private readonly SyncVar<uint> _dmg = new SyncVar<uint>();

        // Public properties that access the SyncVars
        public Guid Serial 
        { 
            get => _serial.Value; 
            set => _serial.Value = value; 
        }
        
        public uint CurrentZoneId 
        { 
            get => _currentZoneId.Value; 
            set => _currentZoneId.Value = value; 
        }
        
        public Vector3 Position 
        { 
            get => _position.Value; 
            set => _position.Value = value; 
        }
        
        public uint EntityLevel 
        { 
            get => _entityLevel.Value; 
            set => _entityLevel.Value = value; 
        }

        // Health & Mana Properties
        public long CurrentHp 
        { 
            get => _currentHp.Value; 
            set => _currentHp.Value = value; 
        }
        
        public long MaxHp 
        { 
            get => _maxHp.Value; 
            set => _maxHp.Value = value; 
        }
        
        public long CurrentMp 
        { 
            get => _currentMp.Value; 
            set => _currentMp.Value = value; 
        }
        
        public long MaxMp 
        { 
            get => _maxMp.Value; 
            set => _maxMp.Value = value; 
        }

        // Resource Properties
        public uint CurrentStamina 
        { 
            get => _currentStamina.Value; 
            set => _currentStamina.Value = value; 
        }
        
        public uint MaxStamina 
        { 
            get => _maxStamina.Value; 
            set => _maxStamina.Value = value; 
        }
        
        public uint CurrentRage 
        { 
            get => _currentRage.Value; 
            set => _currentRage.Value = value; 
        }
        
        public uint MaxRage 
        { 
            get => _maxRage.Value; 
            set => _maxRage.Value = value; 
        }

        // Saving Throw Properties
        public double Reflex 
        { 
            get => _reflex.Value; 
            set => _reflex.Value = value; 
        }
        
        public double Fortitude 
        { 
            get => _fortitude.Value; 
            set => _fortitude.Value = value; 
        }
        
        public double Will 
        { 
            get => _will.Value; 
            set => _will.Value = value; 
        }

        // Armor Properties
        public int ArmorClass 
        { 
            get => _armorClass.Value; 
            set => _armorClass.Value = value; 
        }
        
        public Element OffenseElement 
        { 
            get => _offenseElement.Value; 
            set => _offenseElement.Value = value; 
        }
        
        public Element SecondaryOffensiveElement 
        { 
            get => _secondaryOffensiveElement.Value; 
            set => _secondaryOffensiveElement.Value = value; 
        }
        
        public Element DefenseElement 
        { 
            get => _defenseElement.Value; 
            set => _defenseElement.Value = value; 
        }
        
        public Element SecondaryDefensiveElement 
        { 
            get => _secondaryDefensiveElement.Value; 
            set => _secondaryDefensiveElement.Value = value; 
        }

        // Stat Properties
        public int Str 
        { 
            get => _str.Value; 
            set => _str.Value = value; 
        }
        
        public int Int 
        { 
            get => _int.Value; 
            set => _int.Value = value; 
        }
        
        public int Wis 
        { 
            get => _wis.Value; 
            set => _wis.Value = value; 
        }
        
        public int Con 
        { 
            get => _con.Value; 
            set => _con.Value = value; 
        }
        
        public int Dex 
        { 
            get => _dex.Value; 
            set => _dex.Value = value; 
        }
        
        public int Luck 
        { 
            get => _luck.Value; 
            set => _luck.Value = value; 
        }
        
        public uint Regen 
        { 
            get => _regen.Value; 
            set => _regen.Value = value; 
        }
        
        public uint Dmg 
        { 
            get => _dmg.Value; 
            set => _dmg.Value = value; 
        }

        private void Start()
        {
            if (IsServerInitialized)
            {
                // Server initialization logic
                InitializeServerSide();
            }
            
            if (IsClientInitialized)
            {
                // Client initialization logic
                InitializeClientSide();
            }
        }

        private void Update()
        {
            if (IsServerInitialized)
            {
                // Server update logic
                ServerUpdate();
            }
        }

        protected virtual void InitializeServerSide()
        {
            // Override in derived classes for server-specific initialization
        }

        protected virtual void InitializeClientSide()
        {
            // Override in derived classes for client-specific initialization
        }

        protected virtual void ServerUpdate()
        {
            // Override in derived classes for server-specific updates
        }
    }
}

