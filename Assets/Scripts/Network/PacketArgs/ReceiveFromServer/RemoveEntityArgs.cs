namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record RemoveEntityArgs : IPacketSerializable
    {
        public uint SourceId { get; set; }
    }
}