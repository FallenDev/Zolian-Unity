using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network.PacketArgs.SendToServer
{
    public sealed record ConfirmConnectionArgs : IPacketSerializable
    {
        public byte OpCode => (byte)ClientOpCode.ClientRedirected;
        public string Message => "Redirect Successful";
    }
}