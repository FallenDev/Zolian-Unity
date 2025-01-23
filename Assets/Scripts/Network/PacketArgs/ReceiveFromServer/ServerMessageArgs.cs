namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record ServerMessageArgs : IPacketSerializable
    {
        public ServerMessageType ServerMessageType { get; set; }
        public string Message { get; set; } = null!;
    }
    public enum ServerMessageType : byte
    {
        Whisper = 0,
        ActiveMessage = 3,
        AdminMessage = 5,
        DamageScroll = 6,
        ScrollWindow = 8,
        NonScrollWindow = 9,
        WoodenBoard = 10,
        GroupChat = 11,
        GuildChat = 12,
        ClosePopup = 17,
        PersistentMessage = 18
    }
}