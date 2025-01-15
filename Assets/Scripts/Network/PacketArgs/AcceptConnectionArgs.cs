namespace Assets.Scripts.Network.PacketArgs
{
    /// <summary>
    ///     Represents the serialization of the <see cref="ServerOpCode.AcceptConnection" /> packet
    /// </summary>
    public sealed record AcceptConnectionArgs : IPacketSerializable
    {
        /// <summary>
        ///     The message to send to the client. In official DA, this is "CONNECTED SERVER", but it doesnt really matter what you
        ///     send
        /// </summary>
        public string Message { get; set; }
    }
}