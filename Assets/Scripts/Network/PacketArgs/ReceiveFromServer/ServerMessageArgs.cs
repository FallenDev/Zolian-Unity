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
        Informational = 3,
        Popup = 5,
        WoodenBoard = 9,
        AdminMessage = 99
    }
}