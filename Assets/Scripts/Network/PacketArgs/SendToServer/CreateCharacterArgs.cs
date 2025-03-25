using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Models;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record CreateCharacterArgs : IPacketSerializable
    {
        public byte OpCode => (byte)ClientOpCode.CreateCharacter;
        public long SteamId;
        public string Name;
        public BaseClass Class;
        public Race Race;
        public Sex Sex;
        public short Hair;
        public short HairColor;
        public short HairHighlightColor;
        public short SkinColor;
        public short EyeColor;
        public short Beard;
        public short Mustache;
        public short Bangs;
    }
}