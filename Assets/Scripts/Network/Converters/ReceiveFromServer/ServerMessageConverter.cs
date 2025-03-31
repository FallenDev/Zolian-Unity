using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class ServerMessageConverter : PacketConverterBase<ServerMessageArgs>
    {
        protected override ServerMessageArgs Deserialize(ref SpanReader reader)
        {
            var messageType = reader.ReadByte();
            var message = reader.ReadString();

            return new ServerMessageArgs
            {
                ServerMessageType = (PopupMessageType)messageType,
                Message = message
            };
        }
    }
}