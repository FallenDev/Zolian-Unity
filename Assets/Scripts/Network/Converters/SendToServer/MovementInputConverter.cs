using Assets.Scripts.Extensions;
using Assets.Scripts.Network.PacketArgs.SendToServer;
using Assets.Scripts.Network.PacketHandling;
using Assets.Scripts.Network.Span;

namespace Assets.Scripts.Network.Converters.SendToServer
{
    public sealed class MovementInputConverter : PacketConverterBase<MovementInputArgs>
    {
        public override void Serialize(ref SpanWriter writer, MovementInputArgs args)
        {
            writer.WriteGuid(args.Serial);
            writer.WriteVector3(args.MoveDirection.ToNumerics());
            writer.WriteFloat(args.CameraYaw);
        }
    }
}