using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class ConfirmConnectionConverter : PacketConverterBase<ConfirmConnectionArgs>
    {
        public override byte OpCode => (byte)ClientOpCode.ClientRedirected;
        
        public override void Serialize(ref SpanWriter writer, ConfirmConnectionArgs args) => writer.WriteString(args.Message);
    }
}