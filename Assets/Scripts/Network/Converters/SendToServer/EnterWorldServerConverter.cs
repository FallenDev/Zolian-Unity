using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class EnterWorldServerConverter : PacketConverterBase<EnterWorldServerArgs>
    {
        public override void Serialize(ref SpanWriter writer, EnterWorldServerArgs args)
        {
            writer.WriteUInt16(args.Port);
        }
    }
}