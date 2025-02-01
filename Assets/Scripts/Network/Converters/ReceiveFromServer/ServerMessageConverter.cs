using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class ServerMessageConverter : PacketConverterBase<ServerMessageArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.ServerMessage;

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