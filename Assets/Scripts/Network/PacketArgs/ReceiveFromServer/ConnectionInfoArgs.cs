using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record ConnectionInfoArgs : IPacketSerializable
    {
        public ushort PortNumber { get; set; }
    }
}