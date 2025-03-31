using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class ConnectionInfoConverter : PacketConverterBase<ConnectionInfoArgs>
    {
        protected override ConnectionInfoArgs Deserialize(ref SpanReader reader)
        {
            var port = reader.ReadUInt16();
            return new ConnectionInfoArgs { PortNumber = port };
        }
    }
}
