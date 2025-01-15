using Assets.Scripts.Network.PacketArgs;
using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network.Converters
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