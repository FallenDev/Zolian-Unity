using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record DeleteCharacterArgs : IPacketSerializable
    {
        public byte OpCode => (byte)ClientOpCode.DeleteCharacter;
        public long SteamId;
        public string Name;
    }
}