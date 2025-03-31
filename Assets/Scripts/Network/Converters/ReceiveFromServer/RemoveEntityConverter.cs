using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class RemoveEntityConverter : PacketConverterBase<RemoveEntityArgs>
    {
        protected override RemoveEntityArgs Deserialize(ref SpanReader reader)
        {
            var sourceId = reader.ReadUInt32();

            return new RemoveEntityArgs
            {
                SourceId = sourceId
            };
        }
    }
}