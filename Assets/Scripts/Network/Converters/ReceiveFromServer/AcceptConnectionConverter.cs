using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.ReceiveFromServer
{
    public sealed class AcceptConnectionConverter : PacketConverterBase<AcceptConnectionArgs>
    {
        public override byte OpCode => (byte)ServerOpCode.AcceptConnection;

        protected override AcceptConnectionArgs Deserialize(ref SpanReader reader)
        {
            var message = reader.ReadString();
            return new AcceptConnectionArgs { Message = message };
        }
    }
}
