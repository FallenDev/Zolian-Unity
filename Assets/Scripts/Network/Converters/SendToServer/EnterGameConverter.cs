using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class EnterGameConverter : PacketConverterBase<EnterGameArgs>
    {
        public override void Serialize(ref SpanWriter writer, EnterGameArgs args)
        {
            writer.WriteGuid(args.Serial);
            writer.WriteInt64(args.SteamId);
            writer.WriteString(args.UserName);
        }
    }
}