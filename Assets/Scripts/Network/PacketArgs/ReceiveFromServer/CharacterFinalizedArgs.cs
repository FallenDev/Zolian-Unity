namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record CharacterFinalizedArgs : IPacketSerializable
    {
        public bool Finalized { get; set; }
    }
}