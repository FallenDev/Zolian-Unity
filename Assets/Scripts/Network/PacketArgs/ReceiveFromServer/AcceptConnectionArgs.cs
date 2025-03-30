using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record AcceptConnectionArgs : IPacketSerializable
    {
        public string Message { get; set; }
    }
}