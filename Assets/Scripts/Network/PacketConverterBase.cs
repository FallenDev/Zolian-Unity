using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network
{
    /// <summary>
    /// A base packet converter that provides shared logic for packet deserialization.
    /// </summary>
    /// <typeparam name="T">The serializable type the converter is for.</typeparam>
    public abstract class PacketConverterBase<T> : IPacketConverter<T> where T : IPacketSerializable
    {
        public abstract byte OpCode { get; }

        object IPacketConverter.Deserialize(ref SpanReader reader) => Deserialize(ref reader);
        public virtual T Deserialize(ref SpanReader reader) => default;
        void IPacketConverter.Serialize(ref SpanWriter writer, object args) => Serialize(ref writer, (T)args);
        public virtual void Serialize(ref SpanWriter writer, T args) { }
    }
}