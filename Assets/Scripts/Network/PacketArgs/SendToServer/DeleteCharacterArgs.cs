using System;
using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record DeleteCharacterArgs : IPacketSerializable
    {
        public static byte OpCode => (byte)ClientOpCode.DeleteCharacter;
        public Guid Serial;
        public long SteamId;
        public string Name;
    }
}