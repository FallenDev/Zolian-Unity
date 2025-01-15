namespace Assets.Scripts.Network.PacketArgs
{
    /// <summary>
    ///     Represents the serialization of the <see cref="ServerOpCode.ServerMessage" /> packet
    /// </summary>
    public sealed record ServerMessageArgs : IPacketSerializable
    {
        /// <summary>
        ///     The type of the server message.
        /// </summary>
        public ServerMessageType ServerMessageType { get; set; }

        /// <summary>
        ///     The message content to be displayed.
        /// </summary>
        public string Message { get; set; } = null!;
    }

    /// <summary>
    ///     Enum representing the types of server messages.
    /// </summary>
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