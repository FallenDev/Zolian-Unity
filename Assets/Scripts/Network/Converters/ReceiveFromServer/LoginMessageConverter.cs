using Assets.Scripts.Models;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class LoginMessageConverter : PacketConverterBase<LoginMessageArgs>
    {
        protected override LoginMessageArgs Deserialize(ref SpanReader reader)
        {
            var loginMessageType = (PopupMessageType)reader.ReadByte();
            var message = reader.ReadString();
            return new LoginMessageArgs { LoginMessageType = loginMessageType, Message = message };
        }
    }
}