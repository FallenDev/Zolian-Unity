using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    /// <summary>
    ///     Provides serialization and deserialization logic for <see cref="ConnectionInfoArgs" />
    /// </summary>
    public sealed class ConnectionInfoConverter : PacketConverterBase<ConnectionInfoArgs>
    {
        /// <inheritdoc />
        public override byte OpCode => (byte)ServerOpCode.ConnectionInfo;

        /// <inheritdoc />
        public override ConnectionInfoArgs Deserialize(ref SpanReader reader)
        {
            var port = reader.ReadUInt16();
            return new ConnectionInfoArgs { PortNumber = port };
        }
    }
}
