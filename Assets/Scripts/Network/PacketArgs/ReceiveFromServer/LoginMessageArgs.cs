namespace Assets.Scripts.Network.PacketArgs.ReceiveFromServer
{
    public sealed record LoginMessageArgs : IPacketSerializable
    {
        public LoginMessageType LoginMessageType { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    ///     Enum for login message types
    /// </summary>
    public enum LoginMessageType : byte
    {
        Confirm = 0,
        WrongPassword = 1,
        CheckName = 2,
        CheckPassword = 3
    }
}