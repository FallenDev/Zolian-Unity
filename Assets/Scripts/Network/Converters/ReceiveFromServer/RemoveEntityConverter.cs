using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    /// <summary>
    ///     Provides deserialization logic for <see cref="RemoveEntityArgs" />
    /// </summary>
    public sealed class RemoveEntityConverter : PacketConverterBase<RemoveEntityArgs>
    {
        /// <inheritdoc />
        public override byte OpCode => (byte)ServerOpCode.RemoveEntity;

        /// <inheritdoc />
        public override RemoveEntityArgs Deserialize(ref SpanReader reader)
        {
            var sourceId = reader.ReadUInt32();

            return new RemoveEntityArgs
            {
                SourceId = sourceId
            };
        }
    }
}