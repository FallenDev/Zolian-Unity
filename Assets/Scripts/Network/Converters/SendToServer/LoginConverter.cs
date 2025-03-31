using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class LoginConverter : PacketConverterBase<LoginArgs>
    {
        public override void Serialize(ref SpanWriter writer, LoginArgs args)
        {
            writer.WriteInt64(args.SteamId);
        }
    }
}