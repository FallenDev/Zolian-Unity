using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record LoginArgs : IPacketSerializable
    {
        public static byte OpCode => (byte)ClientOpCode.Login;
        public long SteamId;
    }
}