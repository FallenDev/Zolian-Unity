using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record LoginArgs : IPacketSerializable
    {
        public byte OpCode => (byte)ClientOpCode.Login;
        public string Username;
        public string Password;
    }
}