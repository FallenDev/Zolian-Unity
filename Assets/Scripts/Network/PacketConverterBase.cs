namespace Assets.Scripts.Network
{
    /// <summary>
    /// A base packet converter that provides shared logic for packet deserialization.
    /// </summary>
    /// <typeparam name="T">The serializable type the converter is for.</typeparam>
    public abstract class PacketConverterBase<T> : IPacketConverter<T> where T : IPacketSerializable
    {
        /// <inheritdoc />
        public abstract byte OpCode { get; }

        /// <inheritdoc />
        object IPacketConverter.Deserialize(ref SpanReader reader)
        {
            return Deserialize(ref reader);
        }

        /// <inheritdoc />
        public abstract T Deserialize(ref SpanReader reader);
    }
}