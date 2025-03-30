using System;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record PlayerListArgs : IPacketSerializable
    {
        public List<PlayerSelection> Players { get; set; }
    }

    public sealed record PlayerSelection
    {
        public Guid Serial { get; set; }
        public bool Disabled { get; set; }
        public string Name { get; set; }
        public uint Level { get; set; }
        public BaseClass BaseClass { get; set; }
        public BaseClass AdvClass { get; set; }
        public JobClass Job { get; set; }
        public long Health { get; set; }
        public long Mana { get; set; }

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
    }
}
