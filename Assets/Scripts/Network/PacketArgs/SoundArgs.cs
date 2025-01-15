namespace Assets.Scripts.Network.PacketArgs
{
    /// <summary>
    ///     Represents the serialization of the <see cref="ServerOpCode.Sound" /> packet
    /// </summary>
    public sealed record SoundArgs : IPacketSerializable
    {
        /// <summary>
        ///     Whether or not the sound is a music track
        /// </summary>
        public bool IsMusic { get; set; }

        /// <summary>
        ///     The sound or music track index to play
        /// </summary>
        public byte Sound { get; set; }
    }
}