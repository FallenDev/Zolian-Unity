using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class VersionConverter : PacketConverterBase<VersionArgs>
    {
        public override void Serialize(ref SpanWriter writer, VersionArgs args) => writer.WriteString(VersionArgs.Version);
    }
}