using Assets.Scripts.Network.PacketArgs;
using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network.Converters
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