using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    /// <summary>
    ///     Provides deserialization logic for <see cref="ServerMessageArgs" />
    /// </summary>
    public sealed class ServerMessageConverter : PacketConverterBase<ServerMessageArgs>
    {
        /// <inheritdoc />
        public override byte OpCode => (byte)ServerOpCode.ServerMessage;

        /// <inheritdoc />
        public override ServerMessageArgs Deserialize(ref SpanReader reader)
        {
            var messageType = reader.ReadByte();
            var message = reader.ReadString();

            return new ServerMessageArgs
            {
                ServerMessageType = (ServerMessageType)messageType,
                Message = message
            };
        }
    }
}