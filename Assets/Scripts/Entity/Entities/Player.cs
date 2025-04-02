using System;
using Assets.Scripts.Entity.Abstractions;
using Assets.Scripts.Models;
using Assets.Scripts.Network;

namespace Assets.Scripts.Entity.Entities
{
    public class Player : Damageable, IPlayer
    {
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


        // In-Game Properties
        // Add any additional properties needed for in-game representation


        public Player()
        {
            // Initialize any default values if needed
        }
    }
}
