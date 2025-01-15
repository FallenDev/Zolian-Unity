namespace Assets.Scripts.Network
{
    /// <summary>
    /// Defines a pattern to deserialize a span of bytes into an object.
    /// </summary>
    public interface IPacketConverter
    {
        /// <summary>
        /// The opcode associated with the converter.
        /// </summary>
        byte OpCode { get; }

        /// <summary>
        /// Deserializes a span of bytes into an object.
        /// </summary>
        /// <param name="reader">A reference to an object that reads a span of bytes.</param>
        object Deserialize(ref SpanReader reader);
    }

    /// <inheritdoc />
    /// <typeparam name="T">A type that inherits from <see cref="IPacketSerializable" />.</typeparam>
    public interface IPacketConverter<T> : IPacketConverter where T : IPacketSerializable
    {
        /// <summary>
        /// Deserializes a span of bytes into an object.
        /// </summary>
        /// <param name="reader">A reference to an object that reads a span of bytes.</param>
        new T Deserialize(ref SpanReader reader);
    }
}