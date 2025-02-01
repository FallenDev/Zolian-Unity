using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class VersionConverter : PacketConverterBase<VersionArgs>
    {
        public override byte OpCode => (byte)ClientOpCode.Version;
        
        public override void Serialize(ref SpanWriter writer, VersionArgs args) => writer.WriteString(args.Version);
    }
}