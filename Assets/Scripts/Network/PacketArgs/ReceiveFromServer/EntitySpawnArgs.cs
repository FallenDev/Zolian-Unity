using System;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketHandling;
using UnityEngine;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record EntitySpawnArgs : IPacketSerializable
    {
        public EntityType EntityType { get; set; }
        public Guid Serial { get; set; }
        public Vector3 Position { get; set; }
        public float CameraYaw { get; set; }
        public uint EntityLevel { get; set; }
        public long CurrentHealth { get; set; }
        public long MaxHealth { get; set; }
        public long CurrentMana { get; set; }
        public long MaxMana { get; set; }

        // Player Specific
        public string UserName { get; set; }
        public string Job { get; set; }
        public string FirstClass { get; set; }
        public string SecondClass { get; set; }
        public uint JobLevel { get; set; }
        public Race Race { get; set; }
        public Sex Sex { get; set; }
        public short Hair { get; set; }
        public short HairColor { get; set; }
        public short HairHighlightColor { get; set; }
        public short SkinColor { get; set; }
        public short EyeColor { get; set; }
        public short Beard { get; set; }
        public short Mustache { get; set; }
        public short Bangs { get; set; }

        // NPC Specific
        // Monster Specific
    }
}