using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class ConfirmConnectionConverter : PacketConverterBase<ConfirmConnectionArgs>
    {
        public override void Serialize(ref SpanWriter writer, ConfirmConnectionArgs args) => writer.WriteString(ConfirmConnectionArgs.Message);
    }
}