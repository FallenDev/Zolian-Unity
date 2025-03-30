using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class ConnectionInfoConverter : PacketConverterBase<ConnectionInfoArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.ConnectionInfo;

        protected override ConnectionInfoArgs Deserialize(ref SpanReader reader)
        {
            var port = reader.ReadUInt16();
            return new ConnectionInfoArgs { PortNumber = port };
        }
    }
}
