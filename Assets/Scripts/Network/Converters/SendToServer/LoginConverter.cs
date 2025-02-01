using Assets.Scripts.Network.OpCodes;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class LoginConverter : PacketConverterBase<LoginArgs>
    {
        public override byte OpCode => (byte)ClientOpCode.ClientRedirected;
        
        public override void Serialize(ref SpanWriter writer, LoginArgs args)
        {
            writer.WriteInt64(args.SteamId);
        }
    }
}