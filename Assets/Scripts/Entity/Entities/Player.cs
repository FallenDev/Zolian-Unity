using System;
using Assets.Scripts.Entity.Abstractions;
using Assets.Scripts.Models;
using Assets.Scripts.Network;
using UnityEngine;

namespace Assets.Scripts.Entity.Entities
{
    public class Player : Damageable, IPlayer
    {
        private static Player _playerInstance;

        public static Player PlayerInstance
        {
            get
            {
                if (_playerInstance != null) return _playerInstance;
                _playerInstance = FindFirstObjectByType<Player>();
                if (_playerInstance != null) return _playerInstance;

                Debug.LogError("Player cannot be found!");
                return null;
            }
        }
        // Base properties
        public WorldClient Client { get; set; }
        public long SteamId { get; set; }
        public DateTime LastLogged { get; set; }
        public string UserName { get; set; }
        public ClassStage Stage { get; set; }
        public JobClass Job { get; set; }
        public uint JobLevel { get; set; }
        public BaseClass FirstClass { get; set; }
        public BaseClass SecondClass { get; set; }
        public bool GameMaster { get; set; }

        // Character Customized Looks
        public Race Race { get; set; }
        public Sex Gender { get; set; }
        public short Hair { get; set; }
        public short HairColor { get; set; }
        public short HairHighlightColor { get; set; }
        public short SkinColor { get; set; }
        public short EyeColor { get; set; }
        public short Beard { get; set; }
        public short Mustache { get; set; }
        public short Bangs { get; set; }

        // Equipment


        // Add any additional properties needed for in-game representation
        
        public Player()
        {
            // Initialize any default values if needed
        }

        private void Awake()
        {
            if (_playerInstance == null)
            {
                _playerInstance = this;
                // Remove from parent for persistence
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            else if (_playerInstance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {

        }
    }
}
