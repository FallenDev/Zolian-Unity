using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public class LoginMessageConverter : PacketConverterBase<LoginMessageArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.LoginMessage;

        public override LoginMessageArgs Deserialize(ref SpanReader reader)
        {
            var loginMessageType = (LoginMessageType)reader.ReadByte();
            var message = reader.ReadString();
            return new LoginMessageArgs { LoginMessageType = loginMessageType, Message = message };
        }
    }

}