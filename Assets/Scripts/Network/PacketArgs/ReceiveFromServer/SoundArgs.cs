using Assets.Scripts.Network.PacketHandling;

namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record SoundArgs : IPacketSerializable
    {
        public bool IsMusic { get; set; }
        public byte Sound { get; set; }
    }
}