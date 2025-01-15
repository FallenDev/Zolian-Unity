using Assets.Scripts.Network.PacketArgs;
using Assets.Scripts.Network.OpCodes;

namespace Assets.Scripts.Network.Converters
{
    /// <summary>
    ///     Provides serialization and deserialization logic for <see cref="LoginMessageArgs" />
    /// </summary>
    public sealed class LoginMessageConverter : PacketConverterBase<LoginMessageArgs>
    {
        /// <inheritdoc />
        public override byte OpCode => (byte)ServerOpCode.LoginMessage;

        /// <inheritdoc />
        public override LoginMessageArgs Deserialize(ref SpanReader reader)
        {
            var type = reader.ReadByte();
            var message = reader.ReadString();

            return new LoginMessageArgs
            {
                LoginMessageType = (LoginMessageType)type,
                Message = message
            };
        }
    }
}