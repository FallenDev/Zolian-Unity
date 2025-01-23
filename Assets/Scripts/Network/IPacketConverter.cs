using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// Defines a pattern to deserialize a span of bytes into an object.
    /// </summary>
    public interface IPacketConverter
    {
        byte OpCode { get; }
        object Deserialize(ref SpanReader reader);
        void Serialize(ref SpanWriter writer, object args);
    }

    /// <inheritdoc />
    /// <typeparam name="T">A type that inherits from <see cref="IPacketSerializable" />.</typeparam>
    public interface IPacketConverter<T> : IPacketConverter where T : IPacketSerializable
    {
        new T Deserialize(ref SpanReader reader);
        void Serialize(ref SpanWriter writer, T args);
    }
}